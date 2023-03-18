namespace LibPlcTag.FSharp

open System
open System.Runtime.CompilerServices
open System.Threading
open System.Threading.Tasks
open LibPlcTag.FSharp.Native

type private TagEventArgs = (struct (Event * Status))

[<IsReadOnly; Struct>]
type private TagMessage<'T> =
    | Read
    | Write of data: 'T
    | Status of reply: AsyncReplyChannel<Status>
    | TagEvent of event: TagEventArgs

/// <summary>
/// A tag is a local reference to a region of PLC memory.
/// Depending on the PLC type and protocol the region may be named.
/// For some protocols, the region is simply a type and register number (e.g. Modbus).
/// For other protocols, it is a name, possible array element, field names etc. (e.g. a CIP-based PLC).
/// </summary>
[<Sealed>]
type Tag<'T> private (id, mapper) =
    let readStarted = Event<_>()
    let readCompleted = Event<_>()
    let writeStarted = Event<_>()
    let writeCompleted = Event<_>()
    let operationAborted = Event<_>()

    let body (inbox: MailboxProcessor<TagMessage<'T>>) =
        let rec loop () =
            async {
                match! inbox.Receive() with
                | Read -> plc_tag_read (id, 0) |> ignore
                | Write data ->
                    mapper.Encode id data
                    plc_tag_write (id, 0) |> ignore
                | Status channel -> plc_tag_status id |> enum |> channel.Reply
                | TagEvent(event, status) ->
                    match event with
                    | Event.ReadStarted -> readStarted.Trigger status
                    | Event.ReadCompleted -> readCompleted.Trigger status
                    | Event.WriteStarted -> writeStarted.Trigger status
                    | Event.WriteCompleted -> writeCompleted.Trigger status
                    | Event.Aborted -> operationAborted.Trigger status
                    | _ -> ()

                return! loop ()
            }

        loop ()

    let agent = new MailboxProcessor<_>(body)

    let cb _id event status =
        TagEvent(enum event, enum status) |> agent.Post

    do
        agent.Start()
        plc_tag_register_callback (id, cb) |> ignore

    interface IDisposable with
        member this.Dispose() =
            (agent :> IDisposable).Dispose()
            plc_tag_destroy id |> ignore

    member val ReadStarted = readStarted.Publish
    member val ReadCompleted = readCompleted.Publish
    member val WriteStarted = writeStarted.Publish
    member val WriteCompleted = writeCompleted.Publish
    member val OperationAborted = operationAborted.Publish

    /// <summary>Creates a new generic Tag instance</summary>
    /// <param name="attributes">Tag string attributes -
    /// <a href="https://github.com/libplctag/libplctag/wiki/Tag-String-Attributes">reference</a></param>
    /// <param name="mapper">Custom data mapper</param>
    /// <exception cref="T:LibPlcTag.FSharp.LibPlcTagError">The tag could not be created.</exception>
    static member Create<'T>(attributes, mapper) =
        let id = plc_tag_create (attributes, 0)

        if id < 0 then
            raise <| LibPlcTagError(enum id)

        new Tag<'T>(id, mapper)

    /// <summary>Decodes the underlying data using the provided mapping function.</summary>
    /// <remarks>Data does not update automatically. You must call one of the Read methods.</remarks>
    member this.GetData() = mapper.Decode id

    /// Fetches the current tag status.
    member this.GetStatus() = Status |> agent.PostAndReply

    /// Reads data from the PLC asynchronously without waiting for the operation to complete.
    member this.BeginRead() = Read |> agent.Post

    /// <summary>Reads data from the PLC synchronously.</summary>
    /// <param name="timeout">The number of milliseconds to wait for the operation to complete before timing out.
    /// The default value is 5000 ms.</param>
    /// <exception cref="T:LibPlcTag.FSharp.LibPlcTagError">The read operation was not successful.</exception>
    member this.Read(?timeout) =
        let timeout = defaultArg timeout 5000

        if timeout <= 0 then
            invalidArg (nameof timeout) "must be positive"

        let status = plc_tag_read (id, timeout) |> enum

        if status <> Status.Ok then
            raise <| LibPlcTagError status

        this.GetData()

    /// <summary>Reads data from the PLC asynchronously.</summary>
    /// <param name="pollingInterval">The rate in milliseconds in which status checks are performed while waiting
    /// for the operation to complete. The default value is 100 ms.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <exception cref="T:LibPlcTag.FSharp.LibPlcTagError">The read operation was not successful.</exception>
    member this.ReadAsync(?pollingInterval, ?cancellationToken) =
        task {
            let pollingInterval = defaultArg pollingInterval 100
            let cancellationToken = defaultArg cancellationToken CancellationToken.None

            if pollingInterval < 0 then
                invalidArg (nameof pollingInterval) "must not be negative"

            let mutable status = plc_tag_read (id, 0) |> enum

            while status = Status.Pending do
                cancellationToken.ThrowIfCancellationRequested()
                do! Task.Delay pollingInterval
                status <- plc_tag_status id |> enum

            if int status < 0 then
                raise <| LibPlcTagError status

            return this.GetData()
        }

    /// <summary>Writes data to the PLC asynchronously without waiting for the operation to complete.</summary>
    /// <param name="data">The data to be encoded and written to the PLC.</param>
    member this.BeginWrite(data) = Write data |> agent.Post

    /// <summary>Writes data to the PLC synchronously.</summary>
    /// <param name="data">The data to be encoded and written to the PLC.</param>
    /// <param name="timeout">The number of milliseconds to wait for the operation to complete before timing out.
    /// The default value is 5000 ms.</param>
    /// <exception cref="T:LibPlcTag.FSharp.LibPlcTagError">The write operation was not successful.</exception>
    member this.Write(data, ?timeout) =
        let timeout = defaultArg timeout 5000

        if timeout <= 0 then
            invalidArg (nameof timeout) "must be positive"

        mapper.Encode id data
        let status = plc_tag_write (id, timeout) |> enum

        if status <> Status.Ok then
            raise <| LibPlcTagError status

    /// <summary>Writes data to the PLC asynchronously.</summary>
    /// <param name="data">The data to be encoded and written to the PLC.</param>
    /// <param name="pollingInterval">The rate in milliseconds in which status checks are performed while waiting
    /// for the operation to complete. The default value is 100 ms.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <exception cref="T:LibPlcTag.FSharp.LibPlcTagError">The write operation was not successful.</exception>
    member this.WriteAsync(data, ?pollingInterval, ?cancellationToken) =
        task {
            let pollingInterval = defaultArg pollingInterval 100
            let cancellationToken = defaultArg cancellationToken CancellationToken.None

            if pollingInterval < 0 then
                invalidArg (nameof pollingInterval) "must not be negative"

            mapper.Encode id data
            let mutable status = plc_tag_write (id, 0) |> enum

            while status = Status.Pending do
                cancellationToken.ThrowIfCancellationRequested()
                do! Task.Delay pollingInterval
                status <- plc_tag_status id |> enum

            if int status < 0 then
                raise <| LibPlcTagError status
        }

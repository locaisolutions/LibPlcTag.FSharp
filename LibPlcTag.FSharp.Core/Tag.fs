namespace LibPlcTag.FSharp

open System
open System.Runtime.CompilerServices
open System.Threading
open System.Threading.Tasks
open LibPlcTag.FSharp.Native

exception LibPlcTagError of Status

[<IsReadOnly; Struct>]
type private TagMessage<'T> =
    | Read
    | Write of data: 'T
    | Status of reply: AsyncReplyChannel<Status>
    | TagEvent of event: struct (Event * Status)

[<Sealed>]
type Tag<'T> private (id, mapper: TagMapper<'T>, ?cancellationToken) =
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

    let agent = new MailboxProcessor<_>(body, ?cancellationToken = cancellationToken)

    let cb _id event status =
        TagEvent(enum event, enum status) |> agent.Post

    do
        agent.Start()
        plc_tag_register_callback (id, cb) |> ignore

    interface IDisposable with
        member this.Dispose() =
            (agent :> IDisposable).Dispose()
            plc_tag_destroy id |> ignore

    member val ReadStarted =
        readStarted.Publish
        |> Event.map (fun status -> struct (status, DateTimeOffset.UtcNow))

    member val ReadCompleted =
        readCompleted.Publish
        |> Event.map (fun status -> struct (status, DateTimeOffset.UtcNow))

    member val WriteStarted =
        writeStarted.Publish
        |> Event.map (fun status -> struct (status, DateTimeOffset.UtcNow))

    member val WriteCompleted =
        writeCompleted.Publish
        |> Event.map (fun status -> struct (status, DateTimeOffset.UtcNow))

    member val OperationAborted =
        operationAborted.Publish
        |> Event.map (fun status -> struct (status, DateTimeOffset.UtcNow))

    static member Create<'T>(attributes, mapper, ?cancellationToken) =
        let id = plc_tag_create (attributes, 0)

        if id < 0 then
            raise <| LibPlcTagError(enum id)

        new Tag<'T>(id, mapper, ?cancellationToken = cancellationToken)

    member this.GetData() = mapper.Decode id

    member this.GetStatus() = Status |> agent.PostAndReply

    member this.BeginRead() = Read |> agent.Post

    member this.Read(?timeout) =
        let timeout = defaultArg timeout 5000

        if timeout <= 0 then
            invalidArg (nameof timeout) "must be positive"

        let status = plc_tag_read (id, timeout) |> enum

        if status <> Status.Ok then
            raise <| LibPlcTagError status

        this.GetData()

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

    member this.BeginWrite(data) = Write data |> agent.Post

    member this.Write(data, ?timeout) =
        let timeout = defaultArg timeout 5000

        if timeout <= 0 then
            invalidArg (nameof timeout) "must be positive"

        mapper.Encode id data
        let status = plc_tag_write (id, timeout) |> enum

        if status <> Status.Ok then
            raise <| LibPlcTagError status

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

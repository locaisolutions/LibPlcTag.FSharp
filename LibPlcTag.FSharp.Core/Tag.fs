namespace LibPlcTag.FSharp

open System
open System.Runtime.CompilerServices
open LibPlcTag.FSharp.Native

[<IsReadOnly; Struct>]
type private TagMessage<'T> =
    | Read
    | Write of data: 'T
    | Status of reply: AsyncReplyChannel<STATUS_CODE>
    | TagEvent of event: struct (EVENT_CODE * STATUS_CODE)

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
                    | EVENT_CODE.PLCTAG_EVENT_READ_STARTED -> readStarted.Trigger status
                    | EVENT_CODE.PLCTAG_EVENT_READ_COMPLETED -> readCompleted.Trigger status
                    | EVENT_CODE.PLCTAG_EVENT_WRITE_STARTED -> writeStarted.Trigger status
                    | EVENT_CODE.PLCTAG_EVENT_WRITE_COMPLETED -> writeCompleted.Trigger status
                    | EVENT_CODE.PLCTAG_EVENT_ABORTED -> operationAborted.Trigger status
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

    member val ReadStarted = readStarted.Publish

    member val ReadCompleted =
        readCompleted.Publish
        |> Event.map (fun status -> struct (status, mapper.Decode id))

    member val WriteStarted = writeStarted.Publish
    member val WriteCompleted = writeCompleted.Publish
    member val OperationAborted = operationAborted.Publish

    static member Create<'T>(attributes, mapper, ?cancellationToken) =
        let id = plc_tag_create (attributes, 0)

        if id < 0 then
            raise <| LibPlcTagError(enum id)

        new Tag<'T>(id, mapper, ?cancellationToken = cancellationToken)

    member this.BeginRead() = Read |> agent.Post
    member this.BeginWrite(data) = Write data |> agent.Post
    member this.GetStatus() = Status |> agent.PostAndReply

module LibPlcTag.Events

open System
open System.Collections.Concurrent
open System.Runtime.CompilerServices
open System.Runtime.InteropServices
open System.Threading
open System.Threading.Channels
open LibPlcTag.Native

type 'T IEventStream =
    inherit IDisposable
    abstract Consume: unit -> 'T ChannelReader

let private createProducer
    (channel: _ Channel)
    (consumers: ConcurrentDictionary<int, _ ChannelWriter>)
    (token: CancellationToken)
    =
    backgroundTask {
        let mutable notDone = true
        let mutable eventRef = Unchecked.defaultof<_>
        let mutable consumerRef = Unchecked.defaultof<_>

        while notDone && not token.IsCancellationRequested do
            let! canRead = channel.Reader.WaitToReadAsync token

            if canRead && channel.Reader.TryRead &eventRef then
                for KeyValue(id, consumer) in consumers do
                    (consumer.TryWrite eventRef || consumers.TryRemove(id, &consumerRef)) |> ignore
            else
                notDone <- false

        channel.Writer.TryComplete() |> ignore

        for KeyValue(_, writer) in consumers do
            writer.TryComplete() |> ignore
    }
    |> Async.AwaitTask
    |> Async.Start

let private options =
    UnboundedChannelOptions(SingleReader = true, SingleWriter = true)

let inline private createConsumer (consumers: ConcurrentDictionary<int, _ ChannelWriter>) id =
    let channel = Channel.CreateUnbounded options

    if not <| consumers.TryAdd(id, channel.Writer) then
        failwith "Couldn't create consumer"

    channel.Reader

type TagEventArgs = (struct (int * TagStatus))

[<IsReadOnly; Struct>]
type TagEvent =
    | ReadStarted of readStarted: TagEventArgs
    | ReadCompleted of readCompleted: TagEventArgs
    | WriteStarted of writeStarted: TagEventArgs
    | WriteCompleted of writeCompleted: TagEventArgs
    | Aborted of aborted: TagEventArgs
    | Destroyed of destroyed: TagEventArgs
    | Created of created: TagEventArgs

module private TagEvent =
    let create =
        function
        | PLCTAG_EVENT_READ_STARTED -> ReadStarted
        | PLCTAG_EVENT_READ_COMPLETED -> ReadCompleted
        | PLCTAG_EVENT_WRITE_STARTED -> WriteStarted
        | PLCTAG_EVENT_WRITE_COMPLETED -> WriteCompleted
        | PLCTAG_EVENT_ABORTED -> Aborted
        | PLCTAG_EVENT_DESTROYED -> Destroyed
        | PLCTAG_EVENT_CREATED -> Created
        | x -> failwith $"Invalid tag event '{x}'"

type TagEventStream = TagEvent IEventStream

let tagEventStream id token : TagEventStream =
    let channel = Channel.CreateUnbounded options
    let consumers = ConcurrentDictionary()
    let mutable maxId = 0

    let inline func tagId event status =
        TagEvent.create event (tagId, enum status) |> channel.Writer.TryWrite |> ignore

    let callback = tag_callback_func func

    // prevents the delegate from being garbage collected
    let handle = GCHandle.Alloc callback

    if plc_tag_register_callback (id, callback) <> PLCTAG_STATUS_OK then
        failwithf "Failed to register callback for tag %d!" id

    do createProducer channel consumers token

    { new TagEventStream with
        member _.Dispose() =
            channel.Writer.TryComplete() |> ignore
            handle.Free()

        member _.Consume() =
            Interlocked.Increment &maxId |> createConsumer consumers }

[<IsReadOnly; Struct>]
type LogEventArgs =
    { TagId: int
      DebugLevel: DebugLevel
      Message: string }

type LogEventStream = LogEventArgs IEventStream

let logEventStream token : LogEventStream =
    let channel = Channel.CreateUnbounded options
    let consumers = ConcurrentDictionary()
    let mutable maxId = 0

    let inline func id level message =
        channel.Writer.TryWrite
            { TagId = id
              DebugLevel = enum level
              Message = message }
        |> ignore

    let callback = log_callback_func func

    // prevents the delegate from being garbage collected
    let handle = GCHandle.Alloc callback

    if plc_tag_register_logger callback <> PLCTAG_STATUS_OK then
        failwith "Failed to register logger!"

    do createProducer channel consumers token

    { new LogEventStream with
        member _.Dispose() =
            channel.Writer.TryComplete() |> ignore
            handle.Free()

        member _.Consume() =
            Interlocked.Increment &maxId |> createConsumer consumers }

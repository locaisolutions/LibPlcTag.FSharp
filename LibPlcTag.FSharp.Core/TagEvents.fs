module LibPlcTag.TagEvents

open System
open System.Collections.Concurrent
open System.Collections.Generic
open System.Threading
open System.Threading.Channels
open System.Threading.Tasks
open LibPlcTag.Native
open LibPlcTag.Utils

type private TagEventArgs = { Event: TagEvent; Status: TagStatus }

type TagEventCallback = TagStatus -> CancellationToken -> ValueTask

[<Sealed>]
type TagEventReader(tagId: int, ?cancellationToken: CancellationToken) =

    static let unboundedChannelOptions: UnboundedChannelOptions =
        UnboundedChannelOptions(SingleReader = true, SingleWriter = true)

    static let voidTagEventCallback = fun _ _ -> ValueTask.CompletedTask

    let cancellationToken = defaultArg cancellationToken CancellationToken.None

    let unboundedChannel: Channel<TagEventArgs> =
        Channel.CreateUnbounded(unboundedChannelOptions)

    let mutable onReadStarted = voidTagEventCallback
    let mutable onReadCompleted = voidTagEventCallback
    let mutable onWriteStarted = voidTagEventCallback
    let mutable onWriteCompleted = voidTagEventCallback
    let mutable onAborted = voidTagEventCallback
    let mutable onDestroyed = voidTagEventCallback
    let mutable onCreated = voidTagEventCallback

    let tagEventCallback tagEventArgs =
        match tagEventArgs.Event with
        | TagEvent.ReadStarted -> onReadStarted tagEventArgs.Status
        | TagEvent.ReadCompleted -> onReadCompleted tagEventArgs.Status
        | TagEvent.WriteStarted -> onWriteStarted tagEventArgs.Status
        | TagEvent.WriteCompleted -> onWriteCompleted tagEventArgs.Status
        | TagEvent.Aborted -> onAborted tagEventArgs.Status
        | TagEvent.Destroyed -> onDestroyed tagEventArgs.Status
        | TagEvent.Created -> onCreated tagEventArgs.Status
        | tagEvent -> failwithf "Unknown tag event: %A" tagEvent

    do
        TagEventStream.RegisterCallback(tagId, unboundedChannel.Writer)

        unboundedChannel
            .Reader
            .ReadAllAsync(cancellationToken)
            .ForEachAsync(cancellationToken, tagEventCallback)
        |> ignore

    interface IDisposable with
        member this.Dispose() =
            TagEventStream.UnregisterCallback(tagId, unboundedChannel.Writer)

    member this.OnReadStarted
        with get () = onReadStarted
        and set value = onReadStarted <- value

    member this.OnReadCompleted
        with get () = onReadCompleted
        and set value = onReadCompleted <- value

    member this.OnWriteStarted
        with get () = onWriteStarted
        and set value = onWriteStarted <- value

    member this.OnWriteCompleted
        with get () = onWriteCompleted
        and set value = onWriteCompleted <- value

    member this.OnAborted
        with get () = onAborted
        and set value = onAborted <- value

    member this.OnDestroyed
        with get () = onDestroyed
        and set value = onDestroyed <- value

    member this.OnCreated
        with get () = onCreated
        and set value = onCreated <- value

and [<AbstractClass; Sealed>] private TagEventStream private () =

    static let channelWriterLookup =
        ConcurrentDictionary<int, HashSet<ChannelWriter<TagEventArgs>>>()

    static let tagCallbackFunc tagId event status =
        let tagEventArgs =
            { Event = enum event
              Status = enum status }

        let mutable channelWriters = Unchecked.defaultof<_>

        if channelWriterLookup.TryGetValue(tagId, &channelWriters) then
            for channelWriter in channelWriters do
                channelWriter.TryWrite tagEventArgs |> ignore

    static member val private s_tagCallbackFunc: tag_callback_func = tag_callback_func tagCallbackFunc

    static member RegisterCallback(tagId: int, channelWriter: ChannelWriter<TagEventArgs>) =
        channelWriterLookup.AddOrUpdate(
            tagId,
            (fun tagId ->
                plc_tag_register_callback (tagId, TagEventStream.s_tagCallbackFunc) |> ignore
                let rval = HashSet()
                rval.Add channelWriter |> ignore
                rval),
            (fun _tagId channelWriters ->
                channelWriters.Add(channelWriter) |> ignore
                channelWriters)
        )
        |> ignore

    static member UnregisterCallback(tagId: int, channelWriter: ChannelWriter<TagEventArgs>) =
        let mutable channelWriters = Unchecked.defaultof<_>

        if
            channelWriterLookup.TryGetValue(tagId, &channelWriters)
            && channelWriters.Remove(channelWriter)
            && Seq.isEmpty channelWriters
        then
            channelWriterLookup.TryRemove(tagId) |> ignore
            plc_tag_unregister_callback tagId |> ignore

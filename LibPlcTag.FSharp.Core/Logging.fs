module LibPlcTag.Logging

open System
open System.Collections.Concurrent
open System.Threading
open System.Threading.Channels
open System.Threading.Tasks
open LibPlcTag.Native
open LibPlcTag.Utils

type LogEventArgs =
    { TagId: int
      DebugLevel: DebugLevel
      Message: string }

type LogEventCallback = LogEventArgs -> CancellationToken -> ValueTask

[<Sealed>]
type LogEventReader(logEventCallback: LogEventCallback, ?cancellationToken: CancellationToken) =

    static let unboundedChannelOptions: UnboundedChannelOptions =
        UnboundedChannelOptions(SingleReader = true, SingleWriter = true)

    let cancellationToken = defaultArg cancellationToken CancellationToken.None

    let unboundedChannel: Channel<LogEventArgs> =
        Channel.CreateUnbounded(unboundedChannelOptions)

    let mutable channelWriterId = Guid.Empty

    do
        LogEventStream.InitializeIfNotInitialized()
        channelWriterId <- LogEventStream.RegisterLogger(unboundedChannel.Writer)

        unboundedChannel
            .Reader
            .ReadAllAsync(cancellationToken)
            .ForEachAsync(cancellationToken, logEventCallback)
        |> ignore

    interface IDisposable with
        member this.Dispose() =
            LogEventStream.UnregisterLogger(channelWriterId) |> ignore

and [<AbstractClass; Sealed>] private LogEventStream private () =

    static let mutable isInitialized = false

    static let channelWriterLookup = ConcurrentDictionary<Guid, ChannelWriter<LogEventArgs>>()

    static let logCallbackFunc tagId debugLevel message =
        let logEventArgs =
            { TagId = tagId
              DebugLevel = enum debugLevel
              Message = message }

        for KeyValue(_, channelWriter) in channelWriterLookup do
            channelWriter.TryWrite logEventArgs |> ignore

    static member val private s_logCallbackFunc: log_callback_func = log_callback_func logCallbackFunc

    static member InitializeIfNotInitialized() : unit =
        if not <| isInitialized then
            isInitialized <- true
            plc_tag_register_logger LogEventStream.s_logCallbackFunc |> ignore

    static member RegisterLogger(channelWriter: ChannelWriter<LogEventArgs>) : Guid =
        let channelWriterId = Guid.NewGuid()
        channelWriterLookup.TryAdd(channelWriterId, channelWriter) |> ignore
        channelWriterId

    static member UnregisterLogger(channelWriterId: Guid) : bool =
        let mutable channelWriter = Unchecked.defaultof<_>

        channelWriterLookup.TryRemove(channelWriterId, &channelWriter)
        && channelWriter.TryComplete()

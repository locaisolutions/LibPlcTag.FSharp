namespace LibPlcTag.FSharp

open System
open System.Runtime.CompilerServices
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open LibPlcTag.FSharp.Native

type private LogEventArgs = (struct (DebugLevel * string))

[<IsReadOnly; Struct>]
type private LogActorMessage = LogEvent of event: LogEventArgs

/// <summary>
/// Logs messages received from the base API.
/// </summary>
[<Sealed>]
type LibPlcTagLogger(logger: LibPlcTagLogger ILogger) =
    static let logEvent = Event<_>()

    static let debugLevel2LogLevel (debugLevel: DebugLevel) =
        match debugLevel with
        | DebugLevel.None -> LogLevel.None
        | DebugLevel.Error -> LogLevel.Error
        | DebugLevel.Warn -> LogLevel.Warning
        | DebugLevel.Info -> LogLevel.Information
        | DebugLevel.Detail -> LogLevel.Debug
        | DebugLevel.Spew -> LogLevel.Trace
        | level -> failwithf "Unknown debug level %A{0}" level

    static let logEventHandler (logger: LibPlcTagLogger ILogger) ((debugLevel, message): LogEventArgs) =
        logger.Log(debugLevel2LogLevel debugLevel, message)

    static let agent =
        new MailboxProcessor<LogActorMessage>(fun inbox ->
            let rec loop () =
                async {
                    match! inbox.Receive() with
                    | LogEvent event -> logEvent.Trigger event

                    return! loop ()
                }

            loop ())

    static let cb _ debugLevel message =
        LogEvent(enum debugLevel, message) |> agent.Post

    let handler = logEventHandler logger |> logEvent.Publish.Subscribe

    static do plc_tag_register_logger cb |> ignore

    interface IDisposable with
        member this.Dispose() = handler.Dispose()

/// <summary>
/// Extension methods for <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection"/>
/// </summary>
[<Extension>]
type IServiceCollectionExtensions =
    /// <summary>
    /// Registers a shared <c>LibPlcTagLogger</c> instance for dependency injection using the default service provider.
    /// </summary>
    /// <param name="services"><see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection"/></param>
    [<Extension>]
    static member inline AddLibPlcTagLogger(services: IServiceCollection) =
        services.AddSingleton<LibPlcTagLogger>(fun container ->
            new LibPlcTagLogger(container.GetRequiredService<ILogger<LibPlcTagLogger>>()))

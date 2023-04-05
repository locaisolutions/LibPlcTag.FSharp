open System
open System.Diagnostics
open System.Threading
open System.Threading.Tasks
open LibPlcTag

module LoggingTests =

    open LibPlcTag.Logging

    let testLogEventCallback (numberOfLoggers: int) =

        printfn "Number of loggers: %i" numberOfLoggers

        let mutable logEventCount = 0
        let monitor = Object()

        let incrementLogEventCount _ _ =
            lock monitor (fun () -> logEventCount <- logEventCount + 1) |> ValueTask

        let logEventReaders =
            Array.init numberOfLoggers (fun _ -> new LogEventReader(incrementLogEventCount))

        // Generate some logging noise.

        Tag.SetDebugLevel(DebugLevel.Spew)

        try
            Tag.Create("OBVIOUSLY_BAD_TAG_ATTRIBUTES_STRING") |> ignore
        with Failure _ ->
            ()

        try
            Tag.Read(69, 420)
        with Failure _ ->
            ()

        try
            Tag.GetFloat32(6900, -1) |> ignore
        with Failure _ ->
            ()

        try
            Tag.SetString(8675309, -1, null) |> ignore
        with Failure _ ->
            ()

        try
            Tag.Write(420, 69)
        with Failure _ ->
            ()

        try
            Tag.Destroy(123456789)
        with Failure _ ->
            ()

        Thread.Sleep 1000

        for logEventReader in logEventReaders do
            (logEventReader :> IDisposable).Dispose()

        Tag.SetDebugLevel(DebugLevel.None)

        printfn "Log event count: %i" logEventCount

        assert (logEventCount > 0)

module TagEventTests =

    open LibPlcTag.TagEvents

    let testTagEventCallback (numberOfConcurrentReaders: int) =

        printfn "Number of concurrent readers: %i" numberOfConcurrentReaders

        let monitor = Object()

        let mutable readStartedCount = 0

        let onReadStarted _ _ =
            lock monitor (fun () -> readStartedCount <- readStartedCount + 1) |> ValueTask

        let mutable readCompletedCount = 0

        let onReadCompleted _ _ =
            lock monitor (fun () -> readCompletedCount <- readCompletedCount + 1)
            |> ValueTask

        let mutable writeStartedCount = 0

        let onWriteStarted _ _ =
            lock monitor (fun () -> writeStartedCount <- writeStartedCount + 1) |> ValueTask

        let mutable writeCompletedCount = 0

        let onWriteCompleted _ _ =
            lock monitor (fun () -> writeCompletedCount <- writeCompletedCount + 1)
            |> ValueTask

        let tagAttributes =
            "name=MyTag&gateway=127.0.0.1&protocol=ab_eip&plc=controllogix&path=1,0&elem_count=100"

        let tagId = Tag.Create(tagAttributes)

        let tagEventReaders =
            Array.init numberOfConcurrentReaders (fun _ ->
                let tagEventReader = new TagEventReader(tagId)
                tagEventReader.OnReadStarted <- onReadStarted
                tagEventReader.OnReadCompleted <- onReadCompleted
                tagEventReader.OnWriteStarted <- onWriteStarted
                tagEventReader.OnWriteCompleted <- onWriteCompleted
                tagEventReader)

        // Generate read and write events.

        Tag.Read(tagId)
        Tag.Write(tagId)

        Thread.Sleep 1000

        for tagEventReader in tagEventReaders do
            (tagEventReader :> IDisposable).Dispose()

        printfn "Read started count: %i" readStartedCount
        printfn "Read completed count: %i" readCompletedCount
        printfn "Write started count: %i" writeStartedCount
        printfn "Write completed count: %i" writeCompletedCount

        assert (readStartedCount = numberOfConcurrentReaders)
        assert (readCompletedCount = numberOfConcurrentReaders)
        assert (writeStartedCount = numberOfConcurrentReaders)
        assert (writeCompletedCount = numberOfConcurrentReaders)

open LoggingTests
open TagEventTests

let timer = Stopwatch.StartNew()

testLogEventCallback 1_000
testTagEventCallback 1_000_000

timer.Stop()
printfn "Elapsed ms: %i" timer.ElapsedMilliseconds

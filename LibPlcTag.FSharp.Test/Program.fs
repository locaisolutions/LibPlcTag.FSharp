open System.Runtime.CompilerServices
open System.Text
open System.Threading
open System.Threading.Channels
open LibPlcTag
open LibPlcTag.Events
open Microsoft.FSharp.NativeInterop

let attributes =
    TagAttributes.create AbEip "127.0.0.1" "MyTag"
    |> withPlc ControlLogix
    |> withPath "1,0"
    |> withElemCount 100

let createAndDestroyTag () =
    let tag = Tag.create attributes 5000
    Tag.destroy tag

let readAndWriteString () =
    let tag = Tag.create attributes 5000
    let value = "Test"
    let offset = 7
    Tag.read tag 5000
    Tag.setString tag offset value
    Tag.write tag 5000
    Tag.read tag 5000
    let result = Tag.getString tag offset
    Tag.destroy tag
    assert (result = value)

[<IsReadOnly; Struct>]
type MyStruct =
    { Field1: int32
      Field2: int32
      Field3: int16
      Field4: int16 }

let readAndWriteStruct () =
    let tag = Tag.create attributes 5000

    let value =
        { Field1 = 1
          Field2 = 2
          Field3 = 3s
          Field4 = 4s }

    let offset = 1
    Tag.setStruct tag offset &value
    Tag.write tag 5000
    Tag.read tag 5000
    let result = Tag.getStruct tag offset
    Tag.destroy tag
    assert (result = value)

#nowarn "9"

let readAndWriteBytes () =
    let tag = Tag.create attributes 5000
    let message = "Hello, World!"
    let chars = fixed message
    let bytes = NativePtr.stackalloc message.Length
    Encoding.ASCII.GetBytes(chars, message.Length, bytes, message.Length) |> ignore
    Tag.setRawBytes tag 0 bytes message.Length
    Tag.write tag 5000
    Tag.read tag 5000
    Tag.getRawBytes tag 0 bytes message.Length
    let result = Encoding.ASCII.GetString(bytes, message.Length)
    Tag.destroy tag
    assert (result = message)

let consumeEventStream () =
    let tag = Tag.create attributes 5000
    let mutable readStarted = false
    let mutable readCompleted = false
    let mutable writeStarted = false
    let mutable writeCompleted = false
    let token = CancellationToken.None
    use stream = tagEventStream tag token

    let readEventListener (reader: ChannelReader<_>) =
        task {
            let mutable value = Unchecked.defaultof<_>
            let mutable notDone = true

            while notDone && not token.IsCancellationRequested do
                let! canRead = reader.WaitToReadAsync token

                if canRead && reader.TryRead &value then
                    match value with
                    | ReadStarted _ -> readStarted <- true
                    | ReadCompleted _ -> readCompleted <- true
                    | _ -> ()
                else
                    notDone <- false
        }
        |> Async.AwaitTask
        |> Async.Start

    let writeEventListener (reader: ChannelReader<_>) =
        task {
            let mutable value = Unchecked.defaultof<_>
            let mutable notDone = true

            while notDone && not token.IsCancellationRequested do
                let! canRead = reader.WaitToReadAsync token

                if canRead && reader.TryRead &value then
                    match value with
                    | WriteStarted _ -> writeStarted <- true
                    | WriteCompleted _ -> writeCompleted <- true
                    | _ -> ()
                else
                    notDone <- false
        }
        |> Async.AwaitTask
        |> Async.Start

    // Create two independent consumers
    stream.Consume() |> readEventListener
    stream.Consume() |> writeEventListener
    Tag.read tag 5000
    Tag.write tag 5000
    Thread.Sleep 100
    Tag.destroy tag

    assert
        (readStarted = true
         && readCompleted = true
         && writeStarted = true
         && writeCompleted = true)

// Run test cases
for _ in 1..10 do
    do createAndDestroyTag ()
    do readAndWriteString ()
    do readAndWriteStruct ()
    do readAndWriteBytes ()
    do consumeEventStream ()

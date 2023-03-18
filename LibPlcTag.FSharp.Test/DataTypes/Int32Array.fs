module LibPlcTag.FSharp.Test.DataTypes.Int32Array

open System.Threading
open Expecto
open LibPlcTag.FSharp
open LibPlcTag.FSharp.Test.Utils

[<Tests>]
let int32ArrayTests =
    testList
        "int32 array tests"
        [ yield!
              testFixture
                  (withTag
                      "protocol=ab-eip&gateway=127.0.0.1&path=1,0&plc=controllogix&elem_count=50&name=MyTag&debug=1"
                      (int32ArrayMapper 50))
                  [ "test #1",
                    fun tag ->
                        let mutable readCount = 0

                        let expected = [| Array.zeroCreate 50; [| 1..50 |] |]

                        use _ =
                            tag.WriteCompleted.Subscribe(fun status ->
                                status ==? Status.Ok
                                tag.BeginRead())

                        use _ =
                            tag.ReadCompleted.Subscribe(fun status ->
                                status ==? Status.Ok
                                (tag.GetData(), expected[readCount]) ||> Flip.Expect.sequenceEqual ""
                                readCount <- readCount + 1)

                        tag.BeginWrite expected[0]
                        SpinWait.SpinUntil((fun () -> readCount = 1), 5000) ==? true
                        tag.BeginWrite expected[1]
                        SpinWait.SpinUntil((fun () -> readCount = 2), 5000) ==? true ] ]

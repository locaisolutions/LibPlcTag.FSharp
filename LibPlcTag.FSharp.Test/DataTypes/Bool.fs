module LibPlcTag.FSharp.Test.DataTypes.Bool

open System.Threading
open Expecto
open LibPlcTag.FSharp
open LibPlcTag.FSharp.Test.Utils

[<Tests>]
let boolTests =
    testList
        "bool tests"
        [ yield!
              testFixture
                  (withTag
                      "protocol=ab-eip&gateway=127.0.0.1&path=1,0&plc=controllogix&elem_count=1&elem_size=1&name=MyTag&debug=1"
                      boolMapper)
                  [ "toggle bool",
                    fun tag ->
                        let mutable readCount = 0

                        use _ =
                            tag.WriteCompleted.Subscribe(fun status ->
                                status ==? Status.Ok
                                tag.BeginRead())

                        use _ =
                            tag.ReadCompleted.Subscribe(fun status ->
                                status ==? Status.Ok
                                // Expected values:
                                // Read #1 - true
                                // Read #2 - false
                                tag.GetData() ==? (readCount = 0)
                                readCount <- readCount + 1)

                        tag.BeginWrite true
                        SpinWait.SpinUntil((fun () -> readCount = 1), 5000) ==? true
                        tag.BeginWrite false
                        SpinWait.SpinUntil((fun () -> readCount = 2), 5000) ==? true ] ]

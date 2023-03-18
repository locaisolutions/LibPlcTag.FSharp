module LibPlcTag.FSharp.Test.DataTypes.String

open System.Threading
open Expecto
open LibPlcTag.FSharp
open LibPlcTag.FSharp.Test.Utils

[<Tests>]
let stringTests =
    testList
        "string tests"
        [ yield!
              testFixture
                  (withTag
                      "protocol=ab-eip&gateway=127.0.0.1&path=1,0&plc=controllogix&elem_count=50&name=MyTag&debug=1"
                      stringMapper)
                  [ "hello world",
                    fun tag ->
                        let message = "Hello, World!"
                        let mutable readCompleted = false

                        use _ =
                            tag.WriteCompleted.Subscribe(fun status ->
                                status ==? Status.Ok
                                tag.BeginRead())

                        use _ =
                            tag.ReadCompleted.Subscribe(fun status ->
                                status ==? Status.Ok
                                tag.GetData() ==? message
                                readCompleted <- true)

                        tag.BeginWrite message
                        SpinWait.SpinUntil((fun () -> readCompleted), 5000) ==? true ] ]

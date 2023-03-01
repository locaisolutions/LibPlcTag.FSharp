module LibPlcTag.FSharp.Test.DataTypes.Int32Array

open System.Threading
open Expecto
open LibPlcTag.FSharp
open LibPlcTag.FSharp.Native
open LibPlcTag.FSharp.Test.Utils

// let attributes =
//     "protocol=ab-eip&gateway=127.0.0.1&path=1,0&plc=controllogix&elem_count=50&name=MyTag&debug=1"
//
// [<Tests>]
// let int32ArrayTests =
//     testList
//         "int32 array tests"
//         [ testCase "scenario 1"
//           <| fun () ->
//               let sut = [| 1..50 |]
//               let expected = [| 1..50 |]
//               let mapper = int32ArrayMapper sut
//               use tag = Tag.Create(attributes, mapper)
//               let mutable readCompleted = false
//
//               use _ =
//                   tag.WriteCompleted.Subscribe(fun status ->
//                       status =? STATUS_CODE.PLCTAG_STATUS_OK
//                       // set array elements to 0
//                       for i in 0 .. sut.Length - 1 do
//                           sut[i] <- 0
//
//                       tag.BeginRead())
//
//               use _ =
//                   tag.ReadStarted.Subscribe(fun status ->
//                       status =? STATUS_CODE.PLCTAG_STATUS_OK
//                       // array should still be zeroed out at this point
//                       Expect.allEqual sut 0 "")
//
//               use _ =
//                   tag.ReadCompleted.Subscribe(fun status ->
//                       status =? STATUS_CODE.PLCTAG_STATUS_OK
//                       mapper.Decode tag.Id
//                       // array should now contain the expected values (1 .. 50)
//                       sut =? expected
//                       readCompleted <- true)
//
//               tag.BeginWrite()
//               SpinWait.SpinUntil((fun () -> readCompleted), 2000) =? true ]


[<Tests>]
let int32ArrayTests =
    testList
        "int32 array tests"
        [ yield!
              testFixture
                  (withTag
                      "protocol=ab-eip&gateway=127.0.0.1&path=1,0&plc=controllogix&elem_count=50&name=MyTag&debug=1"
                      (int32ArrayMapper 50))
                  [ "toggle bool",
                    fun tag ->
                        let mutable readCount = 0
                        let expected = [| Array.zeroCreate 50; [| 1..50 |] |]

                        use _ =
                            tag.WriteCompleted.Subscribe(fun status ->
                                status ==? STATUS_CODE.PLCTAG_STATUS_OK
                                tag.BeginRead())

                        use _ =
                            tag.ReadCompleted.Subscribe(fun struct (status, value) ->
                                status ==? STATUS_CODE.PLCTAG_STATUS_OK
                                value ==? expected[readCount]
                                readCount <- readCount + 1)

                        tag.BeginWrite expected[0]
                        SpinWait.SpinUntil((fun () -> readCount = 1), 5000) ==? true
                        tag.BeginWrite expected[1]
                        SpinWait.SpinUntil((fun () -> readCount = 2), 5000) ==? true ] ]

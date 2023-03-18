namespace LibPlcTag.FSharp

open System
open System.Collections.Generic
open System.Runtime.CompilerServices
open System.Text
open LibPlcTag.FSharp.Native
open Microsoft.FSharp.NativeInterop

/// <summary>
/// Used to define custom data marshalling behavior.
/// </summary>
[<IsReadOnly; NoComparison; NoEquality; Struct>]
type TagMapper<'T> =
    { Decode: int -> 'T
      Encode: int -> 'T -> unit }

/// <summary>
/// Contains a set of <see cref="T:LibPlcTag.FSharp.TagMapper"/> instances for common scenarios.
/// </summary>
[<AutoOpen>]
module TagMapper =
    let boolMapper: TagMapper<bool> =
        let inline decode id = plc_tag_get_uint8 (id, 0) <> 0uy

        let inline encode id value =
            plc_tag_set_uint8 (id, 0, (if value then 255uy else 0uy)) |> ignore

        { Decode = decode; Encode = encode }

    let stringMapper: TagMapper<string> =
        let inline decode id =
            let strlen = plc_tag_get_string_length (id, 0)
            let buffer = NativePtr.stackalloc strlen
            plc_tag_get_string (id, 0, NativePtr.toNativeInt buffer, strlen) |> ignore
            Encoding.ASCII.GetString(buffer, strlen)

        let inline encode id (value: string) =
            let buffer = NativePtr.stackalloc<byte> value.Length

            Encoding.ASCII.GetBytes(value, Span(NativePtr.toVoidPtr buffer, value.Length))
            |> ignore

            plc_tag_set_string (id, 0, NativePtr.toNativeInt buffer) |> ignore

        { Decode = decode; Encode = encode }

    let uint8ArrayMapper size : TagMapper<uint8 IList> =
        let array = Array.zeroCreate size
        let dataView = array.AsReadOnly() :> _ IList

        let inline decode id =
            for i in 0 .. array.Length - 1 do
                array[i] <- plc_tag_get_uint8 (id, i * 4)

            dataView

        let inline encode id (source: uint8 IList) =
            for i in 0 .. source.Count - 1 do
                plc_tag_set_uint8 (id, i, source[i]) |> ignore

        { Decode = decode; Encode = encode }

    let int8ArrayMapper size : TagMapper<int8 IList> =
        let array = Array.zeroCreate size
        let dataView = array.AsReadOnly() :> _ IList

        let inline decode id =
            for i in 0 .. array.Length - 1 do
                array[i] <- plc_tag_get_int8 (id, i * 4)

            dataView

        let inline encode id (source: int8 IList) =
            for i in 0 .. source.Count - 1 do
                plc_tag_set_int8 (id, i, source[i]) |> ignore

        { Decode = decode; Encode = encode }

    let uint16ArrayMapper size : TagMapper<uint16 IList> =
        let array = Array.zeroCreate size
        let dataView = array.AsReadOnly() :> _ IList

        let inline decode id =
            for i in 0 .. array.Length - 1 do
                array[i] <- plc_tag_get_uint16 (id, i * 4)

            dataView

        let inline encode id (source: uint16 IList) =
            for i in 0 .. source.Count - 1 do
                plc_tag_set_uint16 (id, i * 2, source[i]) |> ignore

        { Decode = decode; Encode = encode }

    let int16ArrayMapper size : TagMapper<int16 IList> =
        let array = Array.zeroCreate size
        let dataView = array.AsReadOnly() :> _ IList

        let inline decode id =
            for i in 0 .. array.Length - 1 do
                array[i] <- plc_tag_get_int16 (id, i * 4)

            dataView

        let inline encode id (source: int16 IList) =
            for i in 0 .. source.Count - 1 do
                plc_tag_set_int16 (id, i * 2, source[i]) |> ignore

        { Decode = decode; Encode = encode }

    let uint32ArrayMapper size : TagMapper<uint32 IList> =
        let array = Array.zeroCreate size
        let dataView = array.AsReadOnly() :> _ IList

        let inline decode id =
            for i in 0 .. array.Length - 1 do
                array[i] <- plc_tag_get_uint32 (id, i * 4)

            dataView

        let inline encode id (source: uint32 IList) =
            for i in 0 .. source.Count - 1 do
                plc_tag_set_uint32 (id, i * 4, source[i]) |> ignore

        { Decode = decode; Encode = encode }

    let int32ArrayMapper size : TagMapper<int32 IList> =
        let array = Array.zeroCreate size
        let dataView = array.AsReadOnly() :> _ IList

        let inline decode id =
            for i in 0 .. array.Length - 1 do
                array[i] <- plc_tag_get_int32 (id, i * 4)

            dataView

        let inline encode id (source: int32 IList) =
            for i in 0 .. source.Count - 1 do
                plc_tag_set_int32 (id, i * 4, source[i]) |> ignore

        { Decode = decode; Encode = encode }

    let uint64ArrayMapper size : TagMapper<uint64 IList> =
        let array = Array.zeroCreate size
        let dataView = array.AsReadOnly() :> _ IList

        let inline decode id =
            for i in 0 .. array.Length - 1 do
                array[i] <- plc_tag_get_uint64 (id, i * 4)

            dataView

        let inline encode id (source: uint64 IList) =
            for i in 0 .. source.Count - 1 do
                plc_tag_set_uint64 (id, i * 8, source[i]) |> ignore

        { Decode = decode; Encode = encode }

    let int64ArrayMapper size : TagMapper<int64 IList> =
        let array = Array.zeroCreate size
        let dataView = array.AsReadOnly() :> _ IList

        let inline decode id =
            for i in 0 .. array.Length - 1 do
                array[i] <- plc_tag_get_int64 (id, i * 4)

            dataView

        let inline encode id (source: int64 IList) =
            for i in 0 .. source.Count - 1 do
                plc_tag_set_int64 (id, i * 8, source[i]) |> ignore

        { Decode = decode; Encode = encode }

    let float32ArrayMapper size : TagMapper<float32 IList> =
        let array = Array.zeroCreate size
        let dataView = array.AsReadOnly() :> _ IList

        let inline decode id =
            for i in 0 .. array.Length - 1 do
                array[i] <- plc_tag_get_float32 (id, i * 4)

            dataView

        let inline encode id (source: float32 IList) =
            for i in 0 .. source.Count - 1 do
                plc_tag_set_float32 (id, i * 4, source[i]) |> ignore

        { Decode = decode; Encode = encode }

    let float64ArrayMapper size : TagMapper<float IList> =
        let array = Array.zeroCreate size
        let dataView = array.AsReadOnly() :> _ IList

        let inline decode id =
            for i in 0 .. array.Length - 1 do
                array[i] <- plc_tag_get_float64 (id, i * 4)

            dataView

        let inline encode id (source: float IList) =
            for i in 0 .. source.Count - 1 do
                plc_tag_set_float64 (id, i * 8, source[i]) |> ignore

        { Decode = decode; Encode = encode }

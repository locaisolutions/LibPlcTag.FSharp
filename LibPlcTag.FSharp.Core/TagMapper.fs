namespace LibPlcTag.FSharp

open System
open System.Runtime.CompilerServices
open System.Text
open LibPlcTag.FSharp.Native
open Microsoft.FSharp.NativeInterop

/// Used to define custom data marshalling behavior
[<IsReadOnly; NoComparison; NoEquality; Struct>]
type TagMapper<'T> =
    {
        /// Reads/unpacks the underlying value of the tag and returns it as an F# value
        Decode: int -> 'T
        /// Transforms the F# value into the underlying value of the tag
        Encode: int -> 'T -> unit
    }

[<AutoOpen>]
module TagMapper =
    let boolMapper: TagMapper<bool> =
        let inline decode id = plc_tag_get_bit (id, 0) = 1

        let inline encode id value =
            plc_tag_set_bit (id, 0, (if value then 1 else 0)) |> ignore

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

    let int32ArrayMapper count : TagMapper<int32[]> =
        let array = Array.zeroCreate count

        let inline decode id =
            for i in 0 .. count - 1 do
                array[i] <- plc_tag_get_int32 (id, i * 4)

            array

        let inline encode id (value: int32[]) =
            for i in 0 .. value.Length - 1 do
                plc_tag_set_int32 (id, i * 4, value[i]) |> ignore

        { Decode = decode; Encode = encode }

    let int64ArrayMapper count : TagMapper<int64[]> =
        let array = Array.zeroCreate count

        let inline decode id =
            for i in 0 .. count - 1 do
                array[i] <- plc_tag_get_int64 (id, i * 8)

            array

        let inline encode id (value: int64[]) =
            for i in 0 .. value.Length - 1 do
                plc_tag_set_int64 (id, i * 8, value[i]) |> ignore

        { Decode = decode; Encode = encode }

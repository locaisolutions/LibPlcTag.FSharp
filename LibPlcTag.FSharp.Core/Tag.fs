namespace LibPlcTag

open System.Text
open System.Threading
open System.Threading.Tasks
open LibPlcTag.Native
open Microsoft.FSharp.NativeInterop

[<AbstractClass; Sealed>]
type Tag private () =

    static member DecodeError(statusCode) =
        let bytes = plc_tag_decode_error statusCode
        let mutable byteCount = 0

        while NativePtr.get bytes byteCount <> 0uy do
            byteCount <- byteCount + 1

        Encoding.ASCII.GetString(bytes, byteCount)

    static member WaitForOkStatus(tagId, ?cancellationToken: CancellationToken) =
        task {
            let cancellationToken = defaultArg cancellationToken CancellationToken.None
            let mutable status = plc_tag_status tagId

            while status = PLCTAG_STATUS_PENDING do
                cancellationToken.ThrowIfCancellationRequested()
                do! Task.Yield()
                status <- plc_tag_status tagId

            if status <> PLCTAG_STATUS_OK then
                failwith (Tag.DecodeError(status))
        }
        :> Task

    static member Create(attributes, ?timeout) =
        let rval = plc_tag_create (attributes, defaultArg timeout 5000)

        if rval < 0 then
            failwith (Tag.DecodeError(rval))

        rval

    static member CreateAsync(attributes, ?cancellationToken: CancellationToken) =
        let tagId = Tag.Create(attributes, 0)
        Tag.WaitForOkStatus(tagId, defaultArg cancellationToken CancellationToken.None)

    static member Read(tagId, ?timeout) =
        let status = plc_tag_read (tagId, defaultArg timeout 5000)

        if status <> PLCTAG_STATUS_OK then
            failwith (Tag.DecodeError(status))

    static member ReadAsync(tagId, ?cancellationToken: CancellationToken) =
        plc_tag_read (tagId, 0) |> ignore
        Tag.WaitForOkStatus(tagId, defaultArg cancellationToken CancellationToken.None)

    static member inline BeginRead(tagId) = plc_tag_read (tagId, 0) |> ignore

    static member Write(tagId, ?timeout) =
        let status = plc_tag_write (tagId, defaultArg timeout 5000)

        if status <> PLCTAG_STATUS_OK then
            failwith (Tag.DecodeError(status))

    static member WriteAsync(tagId, ?cancellationToken: CancellationToken) =
        plc_tag_write (tagId, 0) |> ignore
        Tag.WaitForOkStatus(tagId, defaultArg cancellationToken CancellationToken.None)

    static member inline BeginWrite(tagId) = plc_tag_write (tagId, 0) |> ignore

    static member GetString(tagId, offset) =
        let byteCount = plc_tag_get_string_length (tagId, offset)
        let bytes = NativePtr.stackalloc byteCount
        plc_tag_get_string (tagId, offset, bytes, byteCount) |> ignore
        let mutable byteCount = 0

        while NativePtr.get bytes byteCount <> 0uy do
            byteCount <- byteCount + 1

        Encoding.ASCII.GetString(bytes, byteCount)

    static member inline SetString(tagId, offset, value) =
        plc_tag_set_string (tagId, offset, value)

    static member inline GetSize(tagId) = plc_tag_get_size tagId

    static member inline SetSize(tagId, newSize) = plc_tag_set_size (tagId, newSize)

    static member inline GetIntAttribute(tagId, attributeName, defaultValue) =
        plc_tag_get_int_attribute (tagId, attributeName, defaultValue)

    static member inline SetIntAttribute(tagId, attributeName, newValue) =
        plc_tag_set_int_attribute (tagId, attributeName, newValue)

    static member inline GetUInt8(tagId, offset) = plc_tag_get_uint8 (tagId, offset)

    static member inline SetUInt8(tagId, offset, value) =
        plc_tag_set_uint8 (tagId, offset, value)

    static member inline GetInt8(tagId, offset) = plc_tag_get_int8 (tagId, offset)

    static member inline SetInt8(tagId, offset, value) = plc_tag_set_int8 (tagId, offset, value)

    static member inline GetUInt16(tagId, offset) = plc_tag_get_uint16 (tagId, offset)

    static member inline SetUInt16(tagId, offset, value) =
        plc_tag_set_uint16 (tagId, offset, value)

    static member inline GetInt16(tagId, offset) = plc_tag_get_int16 (tagId, offset)

    static member inline SetInt16(tagId, offset, value) =
        plc_tag_set_int16 (tagId, offset, value)

    static member inline GetUInt32(tagId, offset) = plc_tag_get_uint32 (tagId, offset)

    static member inline SetUInt32(tagId, offset, value) =
        plc_tag_set_uint32 (tagId, offset, value)

    static member inline GetInt32(tagId, offset) = plc_tag_get_int32 (tagId, offset)

    static member inline SetInt32(tagId, offset, value) =
        plc_tag_set_int32 (tagId, offset, value)

    static member inline GetUInt64(tagId, offset) = plc_tag_get_uint64 (tagId, offset)

    static member inline SetUInt64(tagId, offset, value) =
        plc_tag_set_uint64 (tagId, offset, value)

    static member inline GetInt64(tagId, offset) = plc_tag_get_int64 (tagId, offset)

    static member inline SetInt64(tagId, offset, value) =
        plc_tag_set_int64 (tagId, offset, value)

    static member inline GetFloat32(tagId, offset) = plc_tag_get_float32 (tagId, offset)

    static member inline SetFloat32(tagId, offset, value) =
        plc_tag_set_float32 (tagId, offset, value)

    static member inline GetFloat64(tagId, offset) = plc_tag_get_float64 (tagId, offset)

    static member inline SetFloat64(tagId, offset, value) =
        plc_tag_set_float64 (tagId, offset, value)

    static member inline SetDebugLevel(debugLevel: DebugLevel) =
        plc_tag_set_debug_level (int debugLevel)

    static member inline Destroy(tagId) = plc_tag_destroy tagId |> ignore

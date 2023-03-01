module LibPlcTag.Tag

open System
open System.Diagnostics
open System.Runtime.InteropServices
open System.Text
open System.Threading.Tasks
open LibPlcTag.Native
open Microsoft.FSharp.NativeInterop

let inline private tagError err =
    let ptr = plc_tag_decode_error err
    let mutable idx = 0

    while NativePtr.get ptr idx <> 0uy do
        idx <- idx + 1

    failwith <| Encoding.ASCII.GetString(ptr, idx)

let inline waitForOkStatus id timeout =
    backgroundTask {
        let mutable err = plc_tag_status id

        if err <> PLCTAG_STATUS_OK then
            let timer = Stopwatch.StartNew()

            while err = PLCTAG_STATUS_PENDING do
                // ensures that this will run asynchronously
                do! Task.Yield()

                if timer.ElapsedMilliseconds > timeout then
                    raise <| TimeoutException()

                err <- plc_tag_status id

            if err <> PLCTAG_STATUS_OK then
                tagError err
    }
    |> Async.AwaitTask

let inline create attributes timeout =
    let err = plc_tag_create (TagAttributes.value attributes, timeout)

    if err < 0 then
        tagError err

    err

let inline createAsync attributes timeout =
    async {
        let id = create attributes 0
        do! waitForOkStatus id timeout
        return id
    }

let inline read id timeout =
    let err = plc_tag_read (id, timeout)

    if err <> PLCTAG_STATUS_OK then
        tagError err

let inline readAsync id timeout =
    plc_tag_read (id, 0) |> ignore
    waitForOkStatus id timeout

let inline beginRead id = plc_tag_read (id, 0) |> ignore

let inline write id timeout =
    let err = plc_tag_write (id, timeout)

    if err <> PLCTAG_STATUS_OK then
        tagError err

let inline writeAsync id timeout =
    plc_tag_write (id, 0) |> ignore
    waitForOkStatus id timeout

let inline beginWrite id = plc_tag_write (id, 0) |> ignore

let inline getRawBytes id offset ptr count =
    plc_tag_get_raw_bytes (id, offset, NativePtr.toNativeInt ptr, count) |> ignore

let inline setRawBytes id offset ptr count =
    plc_tag_set_raw_bytes (id, offset, NativePtr.toNativeInt ptr, count) |> ignore

let inline getStruct<'T when 'T: unmanaged and 'T: struct> id offset =
    let size = Marshal.SizeOf<'T>()
    let ptr = NativePtr.stackalloc<'T> size
    plc_tag_get_raw_bytes (id, offset, NativePtr.toNativeInt ptr, size) |> ignore
    NativePtr.read ptr

let inline setStruct<'T when 'T: unmanaged> id offset (value: inref<'T>) =
    let size = Marshal.SizeOf<'T>()
    let ptr = &&value
    NativePtr.write ptr value

    plc_tag_set_raw_bytes (id, offset, NativePtr.toNativeInt ptr, size) |> ignore

let getString id offset =
    let size = plc_tag_get_string_length (id, offset)

    if size = 0 then
        null
    else
        let ptr = NativePtr.stackalloc size
        plc_tag_get_string (id, offset, ptr, size) |> ignore
        let mutable idx = 0

        while NativePtr.get ptr idx <> 0uy do
            idx <- idx + 1

        Encoding.ASCII.GetString(ptr, idx)

let inline setString id offset value =
    let err = plc_tag_set_string (id, offset, value)

    if err <> PLCTAG_STATUS_OK then
        tagError err

let inline getSize id = plc_tag_get_size id
let inline setSize id value = plc_tag_set_size (id, value) |> ignore

let inline getIntAttribute id name defVal =
    plc_tag_get_int_attribute (id, name, defVal)

let inline setIntAttribute id name value =
    plc_tag_set_int_attribute (id, name, value) |> ignore

let inline getUInt8 id offset = plc_tag_get_uint8 (id, offset)

let inline setUInt8 id offset value =
    plc_tag_set_uint8 (id, offset, value) |> ignore

let inline getInt8 id offset = plc_tag_get_int8 (id, offset)

let inline setInt8 id offset value =
    plc_tag_set_int8 (id, offset, value) |> ignore

let inline getUInt16 id offset = plc_tag_get_uint16 (id, offset)

let inline setUInt16 id offset value =
    plc_tag_set_uint16 (id, offset, value) |> ignore

let inline getInt16 id offset = plc_tag_get_int16 (id, offset)

let inline setInt16 id offset value =
    plc_tag_set_int16 (id, offset, value) |> ignore

let inline getUInt32 id offset = plc_tag_get_uint32 (id, offset)

let inline setUInt32 id offset value =
    plc_tag_set_uint32 (id, offset, value) |> ignore

let inline getInt32 id offset = plc_tag_get_int32 (id, offset)

let inline setInt32 id offset value =
    plc_tag_set_int32 (id, offset, value) |> ignore

let inline getUInt64 id offset = plc_tag_get_uint64 (id, offset)

let inline setUInt64 id offset value =
    plc_tag_set_uint64 (id, offset, value) |> ignore

let inline getInt64 id offset = plc_tag_get_int64 (id, offset)

let inline setInt64 id offset value =
    plc_tag_set_int64 (id, offset, value) |> ignore

let inline getFloat32 id offset = plc_tag_get_float32 (id, offset)

let inline setFloat32 id offset value =
    plc_tag_set_float32 (id, offset, value) |> ignore

let inline getFloat64 id offset = plc_tag_get_float64 (id, offset)

let inline setFloat64 id offset value =
    plc_tag_set_float64 (id, offset, value) |> ignore

let inline setDebugLevel (value: DebugLevel) = plc_tag_set_debug_level (int value)
let inline destroy id = plc_tag_destroy id |> ignore

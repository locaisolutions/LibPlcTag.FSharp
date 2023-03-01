module LibPlcTag.FSharp.Native

open System.Runtime.InteropServices

type DEBUG_LEVEL =
    | PLCTAG_DEBUG_NONE = 0
    | PLCTAG_DEBUG_ERROR = 1
    | PLCTAG_DEBUG_WARN = 2
    | PLCTAG_DEBUG_INFO = 3
    | PLCTAG_DEBUG_DETAIL = 4
    | PLCTAG_DEBUG_SPEW = 5

type EVENT_CODE =
    | PLCTAG_EVENT_CREATED = 7
    | PLCTAG_EVENT_READ_STARTED = 1
    | PLCTAG_EVENT_READ_COMPLETED = 2
    | PLCTAG_EVENT_WRITE_STARTED = 3
    | PLCTAG_EVENT_WRITE_COMPLETED = 4
    | PLCTAG_EVENT_ABORTED = 5
    | PLCTAG_EVENT_DESTROYED = 6

type STATUS_CODE =
    | PLCTAG_STATUS_PENDING = 1
    | PLCTAG_STATUS_OK = 0
    | PLCTAG_ERR_ABORT = -1
    | PLCTAG_ERR_BAD_CONFIG = -2
    | PLCTAG_ERR_BAD_CONNECTION = -3
    | PLCTAG_ERR_BAD_DATA = -4
    | PLCTAG_ERR_BAD_DEVICE = -5
    | PLCTAG_ERR_BAD_GATEWAY = -6
    | PLCTAG_ERR_BAD_PARAM = -7
    | PLCTAG_ERR_BAD_REPLY = -8
    | PLCTAG_ERR_BAD_STATUS = -9
    | PLCTAG_ERR_CLOSE = -10
    | PLCTAG_ERR_CREATE = -11
    | PLCTAG_ERR_DUPLICATE = -12
    | PLCTAG_ERR_ENCODE = -13
    | PLCTAG_ERR_MUTEX_DESTROY = -14
    | PLCTAG_ERR_MUTEX_INIT = -15
    | PLCTAG_ERR_MUTEX_LOCK = -16
    | PLCTAG_ERR_MUTEX_UNLOCK = -17
    | PLCTAG_ERR_NOT_ALLOWED = -18
    | PLCTAG_ERR_NOT_FOUND = -19
    | PLCTAG_ERR_NOT_IMPLEMENTED = -20
    | PLCTAG_ERR_NO_DATA = -21
    | PLCTAG_ERR_NO_MATCH = -22
    | PLCTAG_ERR_NO_MEM = -23
    | PLCTAG_ERR_NO_RESOURCES = -24
    | PLCTAG_ERR_NULL_PTR = -25
    | PLCTAG_ERR_OPEN = -26
    | PLCTAG_ERR_OUT_OF_BOUNDS = -27
    | PLCTAG_ERR_READ = -28
    | PLCTAG_ERR_REMOTE_ERR = -29
    | PLCTAG_ERR_THREAD_CREATE = -30
    | PLCTAG_ERR_THREAD_JOIN = -31
    | PLCTAG_ERR_TIMEOUT = -32
    | PLCTAG_ERR_TOO_LARGE = -33
    | PLCTAG_ERR_TOO_SMALL = -34
    | PLCTAG_ERR_UNSUPPORTED = -35
    | PLCTAG_ERR_WINSOCK = -36
    | PLCTAG_ERR_WRITE = -37
    | PLCTAG_ERR_PARTIAL = -38
    | PLCTAG_ERR_BUSY = -39

[<UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
type log_callback_func = delegate of tag_id: int * debug_level: int * message: string -> unit

[<UnmanagedFunctionPointer(CallingConvention.Cdecl)>]
type tag_callback_func = delegate of tag_id: int * event: int * status: int -> unit

[<UnmanagedFunctionPointer(CallingConvention.Cdecl)>]
type tag_callback_func_ex = delegate of tag_id: int * event: int * status: int * userdata: nativeint -> unit


[<Literal>]
let dllName = "plctag"

[<DllImport(dllName)>]
extern string plc_tag_decode_error(int err)

[<DllImport(dllName)>]
extern int plc_tag_check_lib_version(int req_major, int req_minor, int req_patch)

[<DllImport(dllName)>]
extern int plc_tag_create([<MarshalAs(UnmanagedType.LPStr)>] string attrib_str, int timeout)

[<DllImport(dllName)>]
extern int plc_tag_create_ex(
    [<MarshalAs(UnmanagedType.LPStr)>] string attrib_str,
    tag_callback_func_ex callback,
    nativeint userdata,
    int timeout
)

[<DllImport(dllName)>]
extern int plc_tag_destroy(int tag)

[<DllImport(dllName)>]
extern unit plc_tag_shutdown()

[<DllImport(dllName)>]
extern int plc_tag_register_callback(
    int tag_id,
    [<MarshalAsAttribute(UnmanagedType.FunctionPtr)>] tag_callback_func callback
)

[<DllImport(dllName)>]
extern int plc_tag_register_callback_ex(
    int tag_id,
    [<MarshalAsAttribute(UnmanagedType.FunctionPtr)>] tag_callback_func_ex callback,
    nativeint userdata
)

[<DllImport(dllName)>]
extern int plc_tag_unregister_callback(int tag_id)

[<DllImport(dllName)>]
extern int plc_tag_register_logger([<MarshalAsAttribute(UnmanagedType.FunctionPtr)>] log_callback_func collback)

[<DllImport(dllName)>]
extern int plc_tag_unregister_logger()

[<DllImport(dllName)>]
extern int plc_tag_lock(int tag)

[<DllImport(dllName)>]
extern int plc_tag_unlock(int tag)

[<DllImport(dllName)>]
extern int plc_tag_abort(int tag)

[<DllImport(dllName)>]
extern int plc_tag_read(int tag, int timeout)

[<DllImport(dllName)>]
extern int plc_tag_status(int tag)

[<DllImport(dllName)>]
extern int plc_tag_write(int tag, int timeout)

[<DllImport(dllName)>]
extern int plc_tag_get_size(int tag)

[<DllImport(dllName)>]
extern int plc_tag_set_size(int tag, int new_size)

[<DllImport(dllName)>]
extern uint64 plc_tag_get_uint64(int tag, int offset)

[<DllImport(dllName)>]
extern int plc_tag_set_uint64(int tag, int offset, uint64 value)

[<DllImport(dllName)>]
extern int64 plc_tag_get_int64(int tag, int offset)

[<DllImport(dllName)>]
extern int plc_tag_set_int64(int, int offset, int64 value)

[<DllImport(dllName)>]
extern uint32 plc_tag_get_uint32(int tag, int offset)

[<DllImport(dllName)>]
extern int plc_tag_set_uint32(int tag, int offset, uint32 value)

[<DllImport(dllName)>]
extern int32 plc_tag_get_int32(int tag, int offset)

[<DllImport(dllName)>]
extern int plc_tag_set_int32(int, int offset, int32 value)

[<DllImport(dllName)>]
extern uint16 plc_tag_get_uint16(int tag, int offset)

[<DllImport(dllName)>]
extern int plc_tag_set_uint16(int tag, int offset, uint16 value)

[<DllImport(dllName)>]
extern int16 plc_tag_get_int16(int tag, int offset)

[<DllImport(dllName)>]
extern int plc_tag_set_int16(int, int offset, int16 value)

[<DllImport(dllName)>]
extern uint8 plc_tag_get_uint8(int tag, int offset)

[<DllImport(dllName)>]
extern int plc_tag_set_uint8(int tag, int offset, uint8 value)

[<DllImport(dllName)>]
extern int8 plc_tag_get_int8(int tag, int offset)

[<DllImport(dllName)>]
extern int plc_tag_set_int8(int, int offset, int8 value)

[<DllImport(dllName)>]
extern int plc_tag_get_bit(int tag, int offset_bit)

[<DllImport(dllName)>]
extern int plc_tag_set_bit(int tag, int offset_bit, int value)

[<DllImport(dllName)>]
extern float plc_tag_get_float64(int tag, int offset)

[<DllImport(dllName)>]
extern int plc_tag_set_float64(int tag, int offset, float value)

[<DllImport(dllName)>]
extern single plc_tag_get_float32(int tag, int offset)

[<DllImport(dllName)>]
extern int plc_tag_set_float32(int tag, int offset, single value)

[<DllImport(dllName)>]
extern int plc_tag_get_string(int tag_id, int string_start_offset, nativeint buffer, int buffer_length)

[<DllImport(dllName)>]
extern int plc_tag_set_string(int tag_id, int string_start_offset, nativeint buffer)

[<DllImport(dllName)>]
extern int plc_tag_get_string_length(int tag_id, int string_start_offset)

[<DllImport(dllName)>]
extern int plc_tag_get_string_capacity(int tag_id, int string_start_offset)

[<DllImport(dllName)>]
extern int plc_tag_get_string_total_length(int tag_id, int string_start_offset)

[<DllImport(dllName)>]
extern int plc_tag_get_raw_bytes(int tag_id, int start_offset, nativeint buffer, int buffer_length)

[<DllImport(dllName)>]
extern int plc_tag_set_raw_bytes(int tag_id, int start_offset, nativeint buffer, int buffer_length)

[<DllImport(dllName)>]
extern int plc_tag_get_int_attribute(int tag, [<MarshalAs(UnmanagedType.LPStr)>] string attrib_name, int default_value)

[<DllImport(dllName)>]
extern int plc_tag_set_int_attribute(int tag, [<MarshalAs(UnmanagedType.LPStr)>] string attrib_name, int new_value)

[<DllImport(dllName)>]
extern unit plc_tag_set_debug_level(int debug_level)

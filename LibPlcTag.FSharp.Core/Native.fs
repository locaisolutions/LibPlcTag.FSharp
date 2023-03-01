module LibPlcTag.Native

open System.Runtime.InteropServices

[<Literal>]
let dllName = "plctag"

[<UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
type log_callback_func = delegate of tag_id: int * debug_level: int * message: string -> unit

[<UnmanagedFunctionPointer(CallingConvention.Cdecl)>]
type tag_callback_func = delegate of tag_id: int * event: int * status: int -> unit

[<UnmanagedFunctionPointer(CallingConvention.Cdecl)>]
type tag_callback_func_ex = delegate of tag_id: int * event: int * status: int * userdata: nativeint -> unit

[<Literal>]
let PLCTAG_STATUS_PENDING = 1

[<Literal>]
let PLCTAG_STATUS_OK = 0

[<Literal>]
let PLCTAG_ERR_ABORT = -1

[<Literal>]
let PLCTAG_ERR_BAD_CONFIG = -2

[<Literal>]
let PLCTAG_ERR_BAD_CONNECTION = -3

[<Literal>]
let PLCTAG_ERR_BAD_DATA = -4

[<Literal>]
let PLCTAG_ERR_BAD_DEVICE = -5

[<Literal>]
let PLCTAG_ERR_BAD_GATEWAY = -6

[<Literal>]
let PLCTAG_ERR_BAD_PARAM = -7

[<Literal>]
let PLCTAG_ERR_BAD_REPLY = -8

[<Literal>]
let PLCTAG_ERR_BAD_STATUS = -9

[<Literal>]
let PLCTAG_ERR_CLOSE = -10

[<Literal>]
let PLCTAG_ERR_CREATE = -11

[<Literal>]
let PLCTAG_ERR_DUPLICATE = -12

[<Literal>]
let PLCTAG_ERR_ENCODE = -13

[<Literal>]
let PLCTAG_ERR_MUTEX_DESTROY = -14

[<Literal>]
let PLCTAG_ERR_MUTEX_INIT = -15

[<Literal>]
let PLCTAG_ERR_MUTEX_LOCK = -16

[<Literal>]
let PLCTAG_ERR_MUTEX_UNLOCK = -17

[<Literal>]
let PLCTAG_ERR_NOT_ALLOWED = -18

[<Literal>]
let PLCTAG_ERR_NOT_FOUND = -19

[<Literal>]
let PLCTAG_ERR_NOT_IMPLEMENTED = -20

[<Literal>]
let PLCTAG_ERR_NO_DATA = -21

[<Literal>]
let PLCTAG_ERR_NO_MATCH = -22

[<Literal>]
let PLCTAG_ERR_NO_MEM = -23

[<Literal>]
let PLCTAG_ERR_NO_RESOURCES = -24

[<Literal>]
let PLCTAG_ERR_NULL_PTR = -25

[<Literal>]
let PLCTAG_ERR_OPEN = -26

[<Literal>]
let PLCTAG_ERR_OUT_OF_BOUNDS = -27

[<Literal>]
let PLCTAG_ERR_READ = -28

[<Literal>]
let PLCTAG_ERR_REMOTE_ERR = -29

[<Literal>]
let PLCTAG_ERR_THREAD_CREATE = -30

[<Literal>]
let PLCTAG_ERR_THREAD_JOIN = -31

[<Literal>]
let PLCTAG_ERR_TIMEOUT = -32

[<Literal>]
let PLCTAG_ERR_TOO_LARGE = -33

[<Literal>]
let PLCTAG_ERR_TOO_SMALL = -34

[<Literal>]
let PLCTAG_ERR_UNSUPPORTED = -35

[<Literal>]
let PLCTAG_ERR_WINSOCK = -36

[<Literal>]
let PLCTAG_ERR_WRITE = -37

[<Literal>]
let PLCTAG_ERR_PARTIAL = -38

[<Literal>]
let PLCTAG_ERR_BUSY = -39

(*
 * helper function for errors.
 *
 * This takes one of the above errors and turns it into a const char * suitable
 * for printing.
 *)

[<DllImport(dllName, CallingConvention = CallingConvention.Cdecl)>]
extern byte * plc_tag_decode_error(int err)

(*
 * Set the debug level.
 *
 * This function takes values from the defined debug levels below.  It sets
 * the debug level to the passed value.  Higher numbers output increasing amounts
 * of information.   Input values not defined below will be ignored.
 *)

[<Literal>]
let PLCTAG_DEBUG_NONE = 0

[<Literal>]
let PLCTAG_DEBUG_ERROR = 1

[<Literal>]
let PLCTAG_DEBUG_WARN = 2

[<Literal>]
let PLCTAG_DEBUG_INFO = 3

[<Literal>]
let PLCTAG_DEBUG_DETAIL = 4

[<Literal>]
let PLCTAG_DEBUG_SPEW = 5

[<DllImport(dllName, CallingConvention = CallingConvention.Cdecl)>]
extern void plc_tag_set_debug_level(int debug_level)

(*
 * Check that the library supports the required API version.
 *
 * The version is passed as integers.   The three arguments are:
 *
 * ver_major - the major version of the library.  This must be an exact match.
 * ver_minor - the minor version of the library.   The library must have a minor
 *             version greater than or equal to the requested version.
 * ver_patch - the patch version of the library.   The library must have a patch
 *             version greater than or equal to the requested version if the minor
 *             version is the same as that requested.   If the library minor version
 *             is greater than that requested, any patch version will be accepted.
 *
 * PLCTAG_STATUS_OK is returned if the version is compatible.  If it does not,
 * PLCTAG_ERR_UNSUPPORTED is returned.
 *
 * Examples:
 *
 * To match version 2.1.4, call plc_tag_check_lib_version(2, 1, 4).
 *
 *)

[<DllImport(dllName, CallingConvention = CallingConvention.Cdecl)>]
extern int plc_tag_check_lib_version(int req_major, int req_minor, int req_patch)

(*
 * tag functions
 *
 * The following is the public API for tag operations.
 *
 * These are implemented in a protocol-specific manner.
 *)

(*
 * plc_tag_create
 *
 * Create a new tag based on the passed attributed string.  The attributes
 * are protocol-specific.  The only required part of the string is the key-
 * value pair "protocol=XXX" where XXX is one of the supported protocol
 * types.
 *
 * Wait for timeout milliseconds for the tag to finish the creation process.
 * If this is zero, return immediately.  The application program will need to
 * poll the tag status with plc_tag_status() while the status is PLCTAG_STATUS_PENDING
 * until the status changes to PLCTAG_STATUS_OK if the creation was successful or
 * another PLCTAG_ERR_xyz if it was not.
 *
 * An opaque handle is returned. If the value is greater than zero, then
 * the operation was a success.  If the value is less than zero then the
 * tag was not created and the failure error is one of the PLCTAG_ERR_xyz
 * errors.
 *)

[<DllImport(dllName, CallingConvention = CallingConvention.Cdecl)>]
extern int32 plc_tag_create([<MarshalAs(UnmanagedType.LPStr)>] string attrib_str, int timeout)

(*
 * plc_tag_create_ex
 *
 * As for plc_tag_create with the addition of a callback and user-supplied data pointer.
 * 
 * The callback will be set as early as possible in the callback process.  This allows sending
 * of early creation time events to user code.
 *)

[<DllImport(dllName, CallingConvention = CallingConvention.Cdecl)>]
extern int32 plc_tag_create_ex(
    [<MarshalAs(UnmanagedType.LPStr)>] string attrib_str,
    [<MarshalAsAttribute(UnmanagedType.FunctionPtr)>] tag_callback_func_ex tag_callback_func,
    nativeint userdata,
    int timeout
)

(*
 * plc_tag_shutdown
 *
 * Some systems may not call the atexit() handlers.  In those cases, wrappers should
 * call this function before unloading the library or terminating.   Most OSes will cleanly
 * recover all system resources when a process is terminated and this will not be necessary.
 *
 * THIS IS NOT THREAD SAFE!   Do not call this if you have multiple threads running against
 * the library.  You have been warned.   Close all tags first with plc_tag_destroy() and make
 * sure that nothing can call any library functions until this function returns.
 *
 * Normally you do not need to call this function.   This is only for certain wrappers or
 * operating environments that use libraries in ways that prevent the normal exit handlers
 * from working.
 *)

[<DllImport(dllName, CallingConvention = CallingConvention.Cdecl)>]
extern void plc_tag_shutdown()

(*
 * plc_tag_register_callback
 *
 * This function registers the passed callback function with the tag.  Only one callback function
 * may be registered on a tag at a time!
 *
 * Once registered, any of the following operations on or in the tag will result in the callback
 * being called:
 *
 *      * starting a tag read operation.
 *      * a tag read operation ending.
 *      * a tag read being aborted.
 *      * starting a tag write operation.
 *      * a tag write operation ending.
 *      * a tag write being aborted.
 *      * a tag being destroyed
 *
 * The callback is called outside of the internal tag mutex so it can call any tag functions safely.   However,
 * the callback is called in the context of the internal tag helper thread and not the client library thread(s).
 * This means that YOU are responsible for making sure that all client application data structures the callback
 * function touches are safe to access by the callback!
 *
 * Do not do any operations in the callback that block for any significant time.   This will cause library
 * performance to be poor or even to start failing!
 *
 * When the callback is called with the PLCTAG_EVENT_DESTROY_STARTED, do not call any tag functions.  It is
 * not guaranteed that they will work and they will possibly hang or fail.
 *
 * Return values:
 *
 * If there is already a callback registered, the function will return PLCTAG_ERR_DUPLICATE.   Only one callback
 * function may be registered at a time on each tag.
 *
 * If all is successful, the function will return PLCTAG_STATUS_OK.
 *)

[<Literal>]
let PLCTAG_EVENT_READ_STARTED = 1

[<Literal>]
let PLCTAG_EVENT_READ_COMPLETED = 2

[<Literal>]
let PLCTAG_EVENT_WRITE_STARTED = 3

[<Literal>]
let PLCTAG_EVENT_WRITE_COMPLETED = 4

[<Literal>]
let PLCTAG_EVENT_ABORTED = 5

[<Literal>]
let PLCTAG_EVENT_DESTROYED = 6

[<Literal>]
let PLCTAG_EVENT_CREATED = 7

[<Literal>]
let PLCTAG_EVENT_MAX = 8

[<DllImport(dllName, CallingConvention = CallingConvention.Cdecl)>]
extern int plc_tag_register_callback(
    int32 tag_id,
    [<MarshalAsAttribute(UnmanagedType.FunctionPtr)>] tag_callback_func tag_callback_func
)

[<DllImport(dllName, CallingConvention = CallingConvention.Cdecl)>]
extern int plc_tag_register_callback_ex(
    int32 tag_id,
    [<MarshalAsAttribute(UnmanagedType.FunctionPtr)>] tag_callback_func_ex tag_callback_func,
    nativeint userdata
)

(*
 * plc_tag_unregister_callback
 *
 * This function removes the callback already registered on the tag.
 *
 * Return values:
 *
 * The function returns PLCTAG_STATUS_OK if there was a registered callback and removing it went well.
 * An error of PLCTAG_ERR_NOT_FOUND is returned if there was no registered callback.
 *)

[<DllImport(dllName, CallingConvention = CallingConvention.Cdecl)>]
extern int plc_tag_unregister_callback(int32 tag_id)

(*
 * plc_tag_register_logger
 *
 * This function registers the passed callback function with the library.  Only one callback function
 * may be registered with the library at a time!
 *
 * Once registered, the function will be called with any logging message that is normally printed due
 * to the current log level setting.
 *
 * WARNING: the callback will usually be called when the internal tag API mutex is held.   You cannot
 * call any tag functions within the callback!
 *
 * Return values:
 *
 * If there is already a callback registered, the function will return PLCTAG_ERR_DUPLICATE.   Only one callback
 * function may be registered at a time on each tag.
 *
 * If all is successful, the function will return PLCTAG_STATUS_OK.
 *)

[<DllImport(dllName, CallingConvention = CallingConvention.Cdecl)>]
extern int plc_tag_register_logger(
    [<MarshalAsAttribute(UnmanagedType.FunctionPtr)>] log_callback_func log_callback_func
)

(*
 * plc_tag_unregister_logger
 *
 * This function removes the logger callback already registered for the library.
 *
 * Return values:
 *
 * The function returns PLCTAG_STATUS_OK if there was a registered callback and removing it went well.
 * An error of PLCTAG_ERR_NOT_FOUND is returned if there was no registered callback.
 *)

[<DllImport(dllName, CallingConvention = CallingConvention.Cdecl)>]
extern int plc_tag_unregister_logger()

(*
 * plc_tag_lock
 *
 * Lock the tag against use by other threads.  Because operations on a tag are
 * very much asynchronous, actions like getting and extracting the data from
 * a tag take more than one API call.  If more than one thread is using the same tag,
 * then the internal state of the tag will get broken and you will probably experience
 * a crash.
 *
 * This should be used to initially lock a tag when starting operations with it
 * followed by a call to plc_tag_unlock when you have everything you need from the tag.
 *)

[<DllImport(dllName, CallingConvention = CallingConvention.Cdecl)>]
extern int plc_tag_lock(int32 tag)

(*
 * plc_tag_unlock
 *
 * The opposite action of plc_tag_unlock.  This allows other threads to access the
 * tag.
 *)

[<DllImport(dllName, CallingConvention = CallingConvention.Cdecl)>]
extern int plc_tag_unlock(int32 tag)

(*
 * plc_tag_abort
 *
 * Abort any outstanding IO to the PLC.  If there is something in flight, then
 * it is marked invalid.  Note that this does not abort anything that might
 * be still processing in the report PLC.
 *
 * The status will be PLCTAG_STATUS_OK unless there is an error such as
 * a null pointer.
 *
 * This is a function provided by the underlying protocol implementation.
 *)

[<DllImport(dllName, CallingConvention = CallingConvention.Cdecl)>]
extern int plc_tag_abort(int32 tag)

(*
 * plc_tag_destroy
 *
 * This frees all resources associated with the tag.  Internally, it may result in closed
 * connections etc.   This calls through to a protocol-specific function.
 *
 * This is a function provided by the underlying protocol implementation.
 *)

[<DllImport(dllName, CallingConvention = CallingConvention.Cdecl)>]
extern int plc_tag_destroy(int32 tag)

(*
 * plc_tag_read
 *
 * Start a read.  If the timeout value is zero, then wait until the read
 * returns or the timeout occurs, whichever is first.  Return the status.
 * If the timeout value is zero, then plc_tag_read will normally return
 * PLCTAG_STATUS_PENDING.
 *
 * This is a function provided by the underlying protocol implementation.
 *)

[<DllImport(dllName, CallingConvention = CallingConvention.Cdecl)>]
extern int plc_tag_read(int32 tag, int timeout)

(*
 * plc_tag_status
 *
 * Return the current status of the tag.  This will be PLCTAG_STATUS_PENDING if there is
 * an uncompleted IO operation.  It will be PLCTAG_STATUS_OK if everything is fine.  Other
 * errors will be returned as appropriate.
 *
 * This is a function provided by the underlying protocol implementation.
 *)

[<DllImport(dllName, CallingConvention = CallingConvention.Cdecl)>]
extern int plc_tag_status(int32 tag)

(*
 * plc_tag_write
 *
 * Start a write.  If the timeout value is zero, then wait until the write
 * returns or the timeout occurs, whichever is first.  Return the status.
 * If the timeout value is zero, then plc_tag_write will usually return
 * PLCTAG_STATUS_PENDING.  The write is considered done
 * when it has been written to the socket.
 *
 * This is a function provided by the underlying protocol implementation.
 *)

[<DllImport(dllName, CallingConvention = CallingConvention.Cdecl)>]
extern int plc_tag_write(int32 tag, int timeout)

(*
 * Tag data accessors.
 *)

[<DllImport(dllName, CallingConvention = CallingConvention.Cdecl)>]
extern int plc_tag_get_int_attribute(
    int32 tag,
    [<MarshalAs(UnmanagedType.LPStr)>] string attrib_name,
    int default_value
)

[<DllImport(dllName, CallingConvention = CallingConvention.Cdecl)>]
extern int plc_tag_set_int_attribute(int32 tag, [<MarshalAs(UnmanagedType.LPStr)>] string attrib_name, int new_value)

[<DllImport(dllName, CallingConvention = CallingConvention.Cdecl)>]
extern int plc_tag_get_size(int32 tag)

(* return the old size or negative for errors. *)
[<DllImport(dllName, CallingConvention = CallingConvention.Cdecl)>]
extern int plc_tag_set_size(int32 tag, int new_size)

[<DllImport(dllName, CallingConvention = CallingConvention.Cdecl)>]
extern int plc_tag_get_bit(int32 tag, int offset_bit)

[<DllImport(dllName, CallingConvention = CallingConvention.Cdecl)>]
extern int plc_tag_set_bit(int32 tag, int offset_bit, int value)

[<DllImport(dllName, CallingConvention = CallingConvention.Cdecl)>]
extern uint64 plc_tag_get_uint64(int32 tag, int offset)

[<DllImport(dllName, CallingConvention = CallingConvention.Cdecl)>]
extern int plc_tag_set_uint64(int32 tag, int offset, uint64 value)

[<DllImport(dllName, CallingConvention = CallingConvention.Cdecl)>]
extern int64 plc_tag_get_int64(int32 tag, int offset)

[<DllImport(dllName, CallingConvention = CallingConvention.Cdecl)>]
extern int plc_tag_set_int64(int32, int offset, int64 value)

[<DllImport(dllName, CallingConvention = CallingConvention.Cdecl)>]
extern uint32 plc_tag_get_uint32(int32 tag, int offset)

[<DllImport(dllName, CallingConvention = CallingConvention.Cdecl)>]
extern int plc_tag_set_uint32(int32 tag, int offset, uint32 value)

[<DllImport(dllName, CallingConvention = CallingConvention.Cdecl)>]
extern int32 plc_tag_get_int32(int32 tag, int offset)

[<DllImport(dllName, CallingConvention = CallingConvention.Cdecl)>]
extern int plc_tag_set_int32(int32, int offset, int32 value)

[<DllImport(dllName, CallingConvention = CallingConvention.Cdecl)>]
extern uint16 plc_tag_get_uint16(int32 tag, int offset)

[<DllImport(dllName, CallingConvention = CallingConvention.Cdecl)>]
extern int plc_tag_set_uint16(int32 tag, int offset, uint16 value)

[<DllImport(dllName, CallingConvention = CallingConvention.Cdecl)>]
extern int16 plc_tag_get_int16(int32 tag, int offset)

[<DllImport(dllName, CallingConvention = CallingConvention.Cdecl)>]
extern int plc_tag_set_int16(int32, int offset, int16 value)

[<DllImport(dllName, CallingConvention = CallingConvention.Cdecl)>]
extern uint8 plc_tag_get_uint8(int32 tag, int offset)

[<DllImport(dllName, CallingConvention = CallingConvention.Cdecl)>]
extern int plc_tag_set_uint8(int32 tag, int offset, uint8 value)

[<DllImport(dllName, CallingConvention = CallingConvention.Cdecl)>]
extern int8 plc_tag_get_int8(int32 tag, int offset)

[<DllImport(dllName, CallingConvention = CallingConvention.Cdecl)>]
extern int plc_tag_set_int8(int32, int offset, int8 value)

[<DllImport(dllName, CallingConvention = CallingConvention.Cdecl)>]
extern double plc_tag_get_float64(int32 tag, int offset)

[<DllImport(dllName, CallingConvention = CallingConvention.Cdecl)>]
extern int plc_tag_set_float64(int32 tag, int offset, double value)

[<DllImport(dllName, CallingConvention = CallingConvention.Cdecl)>]
extern single plc_tag_get_float32(int32 tag, int offset)

[<DllImport(dllName, CallingConvention = CallingConvention.Cdecl)>]
extern int plc_tag_set_float32(int32 tag, int offset, single value)

(* raw byte bulk access *)

[<DllImport(dllName, CallingConvention = CallingConvention.Cdecl)>]
extern int plc_tag_set_raw_bytes(int32 id, int offset, nativeint buffer, int buffer_length)

[<DllImport(dllName, CallingConvention = CallingConvention.Cdecl)>]
extern int plc_tag_get_raw_bytes(int32 id, int offset, nativeint buffer, int buffer_length)

(* string accessors *)

[<DllImport(dllName, CallingConvention = CallingConvention.Cdecl)>]
extern int plc_tag_get_string(int32 tag_id, int string_start_offset, byte* buffer, int buffer_length)

[<DllImport(dllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern int plc_tag_set_string(
    int32 tag_id,
    int string_start_offset,
    [<MarshalAs(UnmanagedType.LPStr)>] string string_val
)

[<DllImport(dllName, CallingConvention = CallingConvention.Cdecl)>]
extern int plc_tag_get_string_length(int32 tag_id, int string_start_offset)

[<DllImport(dllName, CallingConvention = CallingConvention.Cdecl)>]
extern int plc_tag_get_string_capacity(int32 tag_id, int string_start_offset)

[<DllImport(dllName, CallingConvention = CallingConvention.Cdecl)>]
extern int plc_tag_get_string_total_length(int32 tag_id, int string_start_offset)

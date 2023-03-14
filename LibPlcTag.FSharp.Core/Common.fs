namespace LibPlcTag.FSharp

type DebugLevel =
    /// Disables debugging output.
    | None = 0
    /// Only output errors. Generally these are fatal to the functioning of the library.
    | Error = 1
    /// Outputs warnings such as error found when checking a malformed tag attribute string or when unexpected problems are reported from the PLC.
    | Warn = 2
    /// Outputs diagnostic information about the internal calls within the library.
    /// Includes some packet dumps.
    | Info = 3
    /// Outputs detailed diagnostic information about the code executing within the library including packet dumps.
    | Detail = 4
    /// Outputs extremely detailed information.
    /// Do not use this unless you are trying to debug detailed information about every mutex lock and release.
    /// Will output many lines of output per millisecond.
    /// You have been warned!
    | Spew = 5

type Event =
    /// The callback is called after a tag creation completes.
    /// The final status of the creation is passed to the callback as well.
    /// This is not as well supported in some cases, so only depend on this for normal tags and not tags like @tags.
    | Created = 7
    /// A read of the tag has been requested.
    /// The callback is called immediately before the underlying protocol implementation is called.
    | ReadStarted = 1
    /// The callback is called after a read completes.
    /// The final status of the read is passed to the callback as well.
    | ReadCompleted = 2
    /// As with reads, the callback is called when a write is requested.
    /// The callback can change the data in the tag and the changes will be sent to the PLC.
    | WriteStarted = 3
    /// The callback is called when the PLC indicates that the write has completed.
    /// The status of the write is passed to the callback.
    | WriteCompleted = 4
    /// The callback function is called when something calls <see cref="T:LibPlcTag.FSharp.Native.plc_tag_abort"/> on the tag.
    | Aborted = 5
    /// The callback function is called when the tag is being destroyed.
    /// It is not safe to call any API functions on the tag at this time.
    /// This is purely for the callback to manage any application state.
    | Destroyed = 6

type Status =
    /// Operation in progress. Not an error.
    | Pending = 1
    /// No error.
    | Ok = 0
    /// The operation was aborted.
    | ErrorAbort = -1
    /// The operation failed due to incorrect configuration. Usually returned from a remote system.
    | ErrorBadConfig = -2
    /// The connection failed for some reason. This can mean that the remote PLC was power cycled, for instance.
    | ErrorBadConnection = -3
    /// The data received from the remote PLC was undecipherable or otherwise not able to be processed.
    /// Can also be returned from a remote system that cannot process the data sent to it.
    | ErrorBadData = -4
    /// Usually returned from a remote system when something addressed does not exist.
    | ErrorBadDevice = -5
    /// Usually returned when the library is unable to connect to a remote system.
    | ErrorBadGateway = -6
    /// A common error return when something is not correct with the tag creation attribute string.
    | ErrorBadParam = -7
    /// Usually returned when the remote system returned an unexpected response.
    | ErrorBadReply = -8
    /// Usually returned by a remote system when something is not in a good state.
    | ErrorBadStatus = -9
    /// An error occurred trying to close some resource.
    | ErrorClose = -10
    /// An error occurred trying to create some internal resource.
    | ErrorCreate = -11
    /// An error returned by a remote system when something is incorrectly duplicated (i.e. a duplicate connection ID).
    | ErrorDuplicate = -12
    /// An error was returned when trying to encode some data such as a tag name.
    | ErrorEncode = -13
    /// An internal library error. It would be very unusual to see this.
    | ErrorMutexDestroy = -14
    /// An internal library error. It would be very unusual to see this.
    | ErrorMutexInit = -15
    /// An internal library error. It would be very unusual to see this.
    | ErrorMutexLock = -16
    /// An internal library error. It would be very unusual to see this.
    | ErrorMutexUnlock = -17
    /// Often returned from the remote system when an operation is not permitted.
    | ErrorNotAllowed = -18
    /// Often returned from the remote system when something is not found.
    | ErrorNotFound = -19
    /// returned when a valid operation is not implemented.
    | ErrorNotImplemented = -20
    /// Returned when expected data is not present.
    | ErrorNoData = -21
    /// Similar to <see cref="T:Status.ErrorNotFound"/>
    | ErrorNoMatch = -22
    /// Returned by the library when memory allocation fails.
    | ErrorNoMem = -23
    /// Returned by the remote system when some resource allocation fails.
    | ErrorNoResources = -24
    /// Usually an internal error, but can be returned when an invalid handle is used with an API call.
    | ErrorNullPtr = -25
    /// Returned when an error occurs opening a resource such as a socket.
    | ErrorOpen = -26
    /// Usually returned when trying to write a value into a tag outside of the tag data bounds.
    | ErrorOutOfBounds = -27
    /// Returned when an error occurs during a read operation. Usually related to socket problems.
    | ErrorRead = -28
    /// An unspecified or untranslatable remote error causes this.
    | ErrorRemoteErr = -29
    /// An internal library error. If you see this, it is likely that everything is about to crash.
    | ErrorThreadCreate = -30
    /// Another internal library error. It is very unlikely that you will see this.
    | ErrorThreadJoin = -31
    /// An operation took too long and timed out.
    | ErrorTimeout = -32
    /// More data was returned than was expected.
    | ErrorTooLarge = -33
    /// Insufficient data was returned from the remote system.
    | ErrorTooSmall = -34
    /// The operation is not supported on the remote system.
    | ErrorUnsupported = -35
    /// A Winsock-specific error occurred (only on Windows).
    | ErrorWinsock = -36
    /// An error occurred trying to write, usually to a socket.
    | ErrorWrite = -37
    /// Partial data was received or something was unexpectedly incomplete.
    | ErrorPartial = -38
    /// The operation cannot be performed as some other operation is taking place.
    | ErrorBusy = -39

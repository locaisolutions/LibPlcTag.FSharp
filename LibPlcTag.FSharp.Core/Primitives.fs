namespace LibPlcTag

open System.Runtime.CompilerServices

[<IsReadOnly; Struct>]
type DebugLevel =
    | None = 0
    | Error = 1
    | Warn = 2
    | Info = 3
    | Detail = 4
    | Spew = 5

[<IsReadOnly; Struct>]
type TagEvent =
    | ReadStarted = 1
    | ReadCompleted = 2
    | WriteStarted = 3
    | WriteCompleted = 4
    | Aborted = 5
    | Destroyed = 6
    | Created = 7

[<IsReadOnly; Struct>]
type TagStatus =
    | StatusPending = 1
    | StatusOk = 0
    | ErrorAbort = -1
    | ErrorBadConfig = -2
    | ErrorBadConnection = -3
    | ErrorBadData = -4
    | ErrorBadDevice = -5
    | ErrorBadGateway = -6
    | ErrorBadParam = -7
    | ErrorBadReply = -8
    | ErrorBadStatus = -9
    | ErrorClose = -10
    | ErrorCreate = -11
    | ErrorDuplicate = -12
    | ErrorEncode = -13
    | ErrorMutexDestroy = -14
    | ErrorMutexInit = -15
    | ErrorMutexLock = -16
    | ErrorMutexUnlock = -17
    | ErrorNotAllowed = -18
    | ErrorNotFound = -19
    | ErrorNotImplemented = -20
    | ErrorNoData = -21
    | ErrorNoMatch = -22
    | ErrorNoMem = -23
    | ErrorNoResources = -24
    | ErrorNullPtr = -25
    | ErrorOpen = -26
    | ErrorOutOfBounds = -27
    | ErrorRead = -28
    | ErrorRemoteErr = -29
    | ErrorThreadCreate = -30
    | ErrorThreadJoin = -31
    | ErrorTimeout = -32
    | ErrorTooLarge = -33
    | ErrorTooSmall = -34
    | ErrorUnsupported = -35
    | ErrorWinsock = -36
    | ErrorWrite = -37
    | ErrorPartial = -38
    | ErrorBusy = -39

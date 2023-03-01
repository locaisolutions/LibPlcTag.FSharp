namespace LibPlcTag

open System
open System.Runtime.CompilerServices
open System.Text
open System.Text.Json.Serialization

[<IsReadOnly; Struct>]
type DebugLevel =
    | None = 0
    | Error = 1
    | Warn = 2
    | Info = 3
    | Detail = 4
    | Spew = 5

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

[<IsReadOnly; Struct>]
[<JsonConverter(typeof<PlcProtocolConverter>)>]
type PlcProtocol =
    | AbEip
    | ModbusTcp

and PlcProtocolConverter() =
    inherit JsonConverter<PlcProtocol>()

    override this.Read(reader, _typeToConvert, _options) =
        match reader.GetString().ToLower() with
        | "ab_eip"
        | "ab-eip"
        | "abeip" -> AbEip
        | "modbus_tcp"
        | "modbus-tcp"
        | "modbustcp" -> ModbusTcp
        | x -> failwith $"Invalid PLC protocol '{x}'"

    override this.Write(writer, value, _options) =
        match value with
        | AbEip -> "ab_eip"
        | ModbusTcp -> "modbus_tcp"
        |> writer.WriteStringValue

module PlcProtocol =
    let value =
        function
        | AbEip -> "ab_eip"
        | ModbusTcp -> "modbus_tcp"

[<IsReadOnly; Struct>]
[<JsonConverter(typeof<PlcTypeConverter>)>]
type PlcType =
    | ControlLogix
    | Plc5
    | Slc500
    | LogixPccc
    | Micro800
    | MicroLogix
    | OmronNjnx

and PlcTypeConverter() =
    inherit JsonConverter<PlcType>()

    override this.Read(reader, _typeToConvert, _options) =
        match reader.GetString().ToLower() with
        | "controllogix" -> ControlLogix
        | "plc5" -> Plc5
        | "slc500" -> Slc500
        | "logixpccc" -> LogixPccc
        | "micro800" -> Micro800
        | "micrologix" -> MicroLogix
        | "omron-njnx"
        | "omron_njnx"
        | "omronnjnx" -> OmronNjnx
        | x -> failwith $"Invalid PLC type '{x}'"

    override this.Write(writer, value, _options) =
        match value with
        | ControlLogix -> "controllogix"
        | Plc5 -> "plc5"
        | Slc500 -> "slc500"
        | LogixPccc -> "logixpccc"
        | Micro800 -> "micro800"
        | MicroLogix -> "micrologix"
        | OmronNjnx -> "omron-njnx"
        |> writer.WriteStringValue

module PlcType =
    let value =
        function
        | ControlLogix -> "controllogix"
        | Plc5 -> "plc5"
        | Slc500 -> "slc500"
        | LogixPccc -> "logixpccc"
        | Micro800 -> "micro800"
        | MicroLogix -> "micrologix"
        | OmronNjnx -> "omron-njnx"

[<CLIMutable; IsReadOnly; Struct>]
type TagAttributes =
    { Protocol: PlcProtocol
      Gateway: string
      Name: string
      Path: string
      Plc: PlcType Nullable
      ElemCount: int Nullable
      ElemSize: int Nullable
      AutoSyncReadMs: int Nullable
      AutoSyncWriteMs: int Nullable
      UseConnectedMessaging: bool Nullable
      AllowPacking: bool Nullable }

module TagAttributes =
    let inline create protocol gateway name =
        { Protocol = protocol
          Gateway = gateway
          Name = name
          Path = null
          Plc = Nullable()
          ElemCount = Nullable()
          ElemSize = Nullable()
          AutoSyncReadMs = Nullable()
          AutoSyncWriteMs = Nullable()
          UseConnectedMessaging = Nullable()
          AllowPacking = Nullable() }

    let inline value attributes =
        let sb = StringBuilder()

        sb.AppendFormat(
            "protocol={0}&gateway={1}&name={2}",
            (PlcProtocol.value attributes.Protocol),
            attributes.Gateway,
            attributes.Name
        )
        |> ignore

        if not <| String.IsNullOrEmpty attributes.Path then
            sb.AppendFormat("&path={0}", attributes.Path) |> ignore

        if attributes.Plc.HasValue then
            sb.AppendFormat("&plc={0}", PlcType.value attributes.Plc.Value) |> ignore

        if attributes.ElemCount.HasValue then
            sb.AppendFormat("&elem_count={0}", attributes.ElemCount.Value) |> ignore

        if attributes.ElemSize.HasValue then
            sb.AppendFormat("&elem_size={0}", attributes.ElemSize.Value) |> ignore

        if attributes.AutoSyncReadMs.HasValue then
            sb.AppendFormat("&auto_sync_read_ms={0}", attributes.AutoSyncReadMs.Value)
            |> ignore

        if attributes.AutoSyncWriteMs.HasValue then
            sb.AppendFormat("&auto_sync_write_ms={0}", attributes.AutoSyncWriteMs.Value)
            |> ignore

        if attributes.UseConnectedMessaging.HasValue then
            sb.AppendFormat("&use_connected_msg={0}", (if attributes.UseConnectedMessaging.Value then 1 else 0))
            |> ignore

        if attributes.AllowPacking.HasValue then
            sb.AppendFormat("&allow_packing={0}", (if attributes.AllowPacking.Value then 1 else 0))
            |> ignore

        sb.ToString()

[<AutoOpen>]
module TagAttributesHelper =
    let inline withPath path attributes = { attributes with Path = path }

    let inline withPlc plc attributes = { attributes with Plc = Nullable plc }

    let inline withElemCount elemCount attributes =
        { attributes with ElemCount = Nullable elemCount }

    let inline withElemSize elemSize attributes =
        { attributes with ElemSize = Nullable elemSize }

    let inline withAutoSyncReadMs autoSyncReadMs attributes =
        { attributes with AutoSyncReadMs = Nullable autoSyncReadMs }

    let inline withAutoSyncWriteMs autoSyncWriteMs attributes =
        { attributes with AutoSyncWriteMs = Nullable autoSyncWriteMs }

    let inline withConnectedMessaging useConnectedMessaging attributes =
        { attributes with UseConnectedMessaging = Nullable useConnectedMessaging }

    let inline withPacking allowPacking attributes =
        { attributes with AllowPacking = Nullable allowPacking }

// ts2fable 0.9.0-build.725
module rec Fable.Bindings.SignalR.Protocol.MsgPack

#nowarn "3390" // disable warnings for invalid XML comments

open System
open Fable.Core
open Fable.Core.JS
open Fable.Bindings.SignalR

type [<AllowNullLiteral>] BinaryMessageFormat =
    interface end

type [<AllowNullLiteral>] BinaryMessageFormatStatic =
    [<EmitConstructor>] abstract Create: unit -> BinaryMessageFormat
    abstract write: output: Uint8Array -> ArrayBuffer
    abstract parse: input: ArrayBuffer -> ResizeArray<Uint8Array>

/// Implements the MessagePack Hub Protocol
type [<AllowNullLiteral>] MessagePackHubProtocol =
    inherit IHubProtocol
    /// The name of the protocol. This is used by SignalR to resolve the protocol between the client and server.
    abstract name: string
    /// The version of the protocol.
    abstract version: float
    /// The TransferFormat of the protocol.
    abstract transferFormat: TransferFormat
    /// <summary>Creates an array of HubMessage objects from the specified serialized representation.</summary>
    /// <param name="input">An ArrayBuffer containing the serialized representation.</param>
    /// <param name="logger">A logger that will be used to log messages that occur during parsing.</param>
    abstract parseMessages: input: ArrayBuffer * logger: ILogger -> ResizeArray<HubMessage>
    /// <summary>Writes the specified HubMessage to an ArrayBuffer and returns it.</summary>
    /// <param name="message">The message to write.</param>
    /// <returns>An ArrayBuffer containing the serialized representation of the message.</returns>
    abstract writeMessage: message: HubMessage -> ArrayBuffer

/// Implements the MessagePack Hub Protocol
type [<AllowNullLiteral>] MessagePackHubProtocolStatic =
    /// <param name="messagePackOptions">MessagePack options passed to</param>
    [<EmitConstructor>] abstract Create: ?messagePackOptions: MessagePackOptions -> MessagePackHubProtocol

/// <summary>
/// MessagePack Options per:
/// <see href="https://github.com/msgpack/msgpack-javascript#api">msgpack-javascript Options</see>
/// </summary>
type [<AllowNullLiteral>] MessagePackOptions =
    abstract extensionCodec: obj with get, set
    abstract context: obj with get, set
    abstract maxDepth: float option with get, set
    abstract initialBufferSize: float option with get, set
    abstract sortKeys: bool option with get, set
    abstract forceFloat32: bool option with get, set
    abstract forceIntegerToFloat: bool option with get, set
    abstract ignoreUndefined: bool option with get, set
    abstract maxStrLength: float option with get, set
    abstract maxBinLength: float option with get, set
    abstract maxArrayLength: float option with get, set
    abstract maxMapLength: float option with get, set
    abstract maxExtLength: float option with get, set

[<Fable.Core.ImportAll("@microsoft/signalr-protocol-msgpack")>]
module Exports =
        let BinaryMessageFormat: BinaryMessageFormatStatic = jsNative

        /// The version of the SignalR Message Pack protocol library.
        let VERSION: obj = jsNative

        /// Implements the MessagePack Hub Protocol
        let MessagePackHubProtocol: MessagePackHubProtocolStatic = jsNative
        let isArrayBuffer: ``val``: obj -> bool = jsNative

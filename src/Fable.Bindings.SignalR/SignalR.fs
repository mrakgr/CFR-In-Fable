// ts2fable 0.9.0-build.725
module rec Fable.Bindings.SignalR

#nowarn "3390" // disable warnings for invalid XML comments

open System
open Fable.Core
open Fable.Core.JS

[<Erase>] type KeyOf<'T> = Key of string
type Error = System.Exception
type XMLHttpRequestResponseType = string

type [<AllowNullLiteral>] AbortController =
    inherit AbortSignal
    /// Set this to a handler that will be invoked when the request is aborted.
    abstract onabort: (unit -> unit) option with get, set
    abstract abort: unit -> unit
    abstract signal: AbortSignal
    /// Indicates if the request has been aborted.
    abstract aborted: bool

type [<AllowNullLiteral>] AbortControllerStatic =
    [<EmitConstructor>] abstract Create: unit -> AbortController

/// Represents a signal that can be monitored to determine if a request has been aborted.
type [<AllowNullLiteral>] AbortSignal =
    /// Indicates if the request has been aborted.
    abstract aborted: bool with get, set
    /// Set this to a handler that will be invoked when the request is aborted.
    abstract onabort: (unit -> unit) option with get, set

type [<AllowNullLiteral>] AccessTokenHttpClient =
    inherit HttpClient
    abstract _accessToken: string option with get, set
    abstract _accessTokenFactory: (unit -> U2<string, Promise<string>>) option with get, set
    /// <summary>Issues an HTTP request to the specified URL, returning a <see cref="Promise" /> that resolves with an <see cref="@microsoft/signalr.HttpResponse" /> representing the result.</summary>
    abstract send: request: HttpRequest -> Promise<HttpResponse>
    /// Gets all cookies that apply to the specified URL.
    abstract getCookieString: url: string -> string

type [<AllowNullLiteral>] AccessTokenHttpClientStatic =
    [<EmitConstructor>] abstract Create: innerClient: HttpClient * accessTokenFactory: (unit -> U2<string, Promise<string>>) option -> AccessTokenHttpClient

/// <summary>Default implementation of <see cref="@microsoft/signalr.HttpClient" />.</summary>
type [<AllowNullLiteral>] DefaultHttpClient =
    inherit HttpClient
    /// <summary>Issues an HTTP request to the specified URL, returning a <see cref="Promise" /> that resolves with an <see cref="@microsoft/signalr.HttpResponse" /> representing the result.</summary>
    abstract send: request: HttpRequest -> Promise<HttpResponse>
    /// Gets all cookies that apply to the specified URL.
    abstract getCookieString: url: string -> string

/// <summary>Default implementation of <see cref="@microsoft/signalr.HttpClient" />.</summary>
type [<AllowNullLiteral>] DefaultHttpClientStatic =
    /// <summary>Creates a new instance of the <see cref="@microsoft/signalr.DefaultHttpClient" />, using the provided <see cref="@microsoft/signalr.ILogger" /> to log messages.</summary>
    [<EmitConstructor>] abstract Create: logger: ILogger -> DefaultHttpClient

type [<AllowNullLiteral>] DefaultReconnectPolicy =
    inherit IRetryPolicy
    /// Called after the transport loses the connection.
    abstract nextRetryDelayInMilliseconds: retryContext: RetryContext -> float option

type [<AllowNullLiteral>] DefaultReconnectPolicyStatic =
    [<EmitConstructor>] abstract Create: ?retryDelays: ResizeArray<float> -> DefaultReconnectPolicy

/// Error thrown when an HTTP request fails.
type [<AllowNullLiteral; AbstractClass>] HttpError =
    inherit Error
    /// The HTTP status code represented by this error.
    abstract statusCode: float with get, set

/// Error thrown when an HTTP request fails.
type [<AllowNullLiteral>] HttpErrorStatic =
    /// <summary>Constructs a new instance of <see cref="@microsoft/signalr.HttpError" />.</summary>
    /// <param name="errorMessage">A descriptive error message.</param>
    /// <param name="statusCode">The HTTP status code represented by this error.</param>
    [<EmitConstructor>] abstract Create: errorMessage: string * statusCode: float -> HttpError

/// Error thrown when a timeout elapses.
type [<AllowNullLiteral; AbstractClass>] TimeoutError =
    inherit Error

/// Error thrown when a timeout elapses.
type [<AllowNullLiteral>] TimeoutErrorStatic =
    /// <summary>Constructs a new instance of <see cref="@microsoft/signalr.TimeoutError" />.</summary>
    /// <param name="errorMessage">A descriptive error message.</param>
    [<EmitConstructor>] abstract Create: ?errorMessage: string -> TimeoutError

/// Error thrown when an action is aborted.
type [<AllowNullLiteral; AbstractClass>] AbortError =
    inherit Error

/// Error thrown when an action is aborted.
type [<AllowNullLiteral>] AbortErrorStatic =
    /// <summary>Constructs a new instance of <see cref="AbortError" />.</summary>
    /// <param name="errorMessage">A descriptive error message.</param>
    [<EmitConstructor>] abstract Create: ?errorMessage: string -> AbortError

type [<AllowNullLiteral; AbstractClass>] UnsupportedTransportError =
    inherit Error
    /// <summary>The <see cref="@microsoft/signalr.HttpTransportType" /> this error occurred on.</summary>
    abstract transport: HttpTransportType with get, set
    /// The type name of this error.
    abstract errorType: string with get, set

type [<AllowNullLiteral>] UnsupportedTransportErrorStatic =
    /// <summary>Constructs a new instance of <see cref="@microsoft/signalr.UnsupportedTransportError" />.</summary>
    /// <param name="message">A descriptive error message.</param>
    /// <param name="transport">The {@link  @microsoft/signalr.HttpTransportType} this error occurred on.</param>
    [<EmitConstructor>] abstract Create: message: string * transport: HttpTransportType -> UnsupportedTransportError

type [<AllowNullLiteral; AbstractClass>] DisabledTransportError =
    inherit Error
    /// <summary>The <see cref="@microsoft/signalr.HttpTransportType" /> this error occurred on.</summary>
    abstract transport: HttpTransportType with get, set
    /// The type name of this error.
    abstract errorType: string with get, set

type [<AllowNullLiteral>] DisabledTransportErrorStatic =
    /// <summary>Constructs a new instance of <see cref="@microsoft/signalr.DisabledTransportError" />.</summary>
    /// <param name="message">A descriptive error message.</param>
    /// <param name="transport">The {@link  @microsoft/signalr.HttpTransportType} this error occurred on.</param>
    [<EmitConstructor>] abstract Create: message: string * transport: HttpTransportType -> DisabledTransportError

type [<AllowNullLiteral; AbstractClass>] FailedToStartTransportError =
    inherit Error
    /// <summary>The <see cref="@microsoft/signalr.HttpTransportType" /> this error occurred on.</summary>
    abstract transport: HttpTransportType with get, set
    /// The type name of this error.
    abstract errorType: string with get, set

type [<AllowNullLiteral>] FailedToStartTransportErrorStatic =
    /// <summary>Constructs a new instance of <see cref="@microsoft/signalr.FailedToStartTransportError" />.</summary>
    /// <param name="message">A descriptive error message.</param>
    /// <param name="transport">The {@link  @microsoft/signalr.HttpTransportType} this error occurred on.</param>
    [<EmitConstructor>] abstract Create: message: string * transport: HttpTransportType -> FailedToStartTransportError

type [<AllowNullLiteral; AbstractClass>] FailedToNegotiateWithServerError =
    inherit Error
    /// The type name of this error.
    abstract errorType: string with get, set

type [<AllowNullLiteral>] FailedToNegotiateWithServerErrorStatic =
    /// <summary>Constructs a new instance of <see cref="@microsoft/signalr.FailedToNegotiateWithServerError" />.</summary>
    /// <param name="message">A descriptive error message.</param>
    [<EmitConstructor>] abstract Create: message: string -> FailedToNegotiateWithServerError

type [<AllowNullLiteral; AbstractClass>] AggregateErrors =
    inherit Error
    /// The collection of errors this error is aggregating.
    abstract innerErrors: ResizeArray<Error> with get, set

type [<AllowNullLiteral>] AggregateErrorsStatic =
    /// <summary>Constructs a new instance of <see cref="@microsoft/signalr.AggregateErrors" />.</summary>
    /// <param name="message">A descriptive error message.</param>
    /// <param name="innerErrors">The collection of errors this error is aggregating.</param>
    [<EmitConstructor>] abstract Create: message: string * innerErrors: ResizeArray<Error> -> AggregateErrors

type [<AllowNullLiteral>] FetchHttpClient =
    inherit HttpClient
    /// <summary>Issues an HTTP request to the specified URL, returning a <see cref="Promise" /> that resolves with an <see cref="@microsoft/signalr.HttpResponse" /> representing the result.</summary>
    abstract send: request: HttpRequest -> Promise<HttpResponse>
    /// Gets all cookies that apply to the specified URL.
    abstract getCookieString: url: string -> string

type [<AllowNullLiteral>] FetchHttpClientStatic =
    [<EmitConstructor>] abstract Create: logger: ILogger -> FetchHttpClient

type [<AllowNullLiteral>] HandshakeRequestMessage =
    abstract protocol: string
    abstract version: float

type [<AllowNullLiteral>] HandshakeResponseMessage =
    abstract error: string
    abstract minorVersion: float

type [<AllowNullLiteral>] HandshakeProtocol =
    abstract writeHandshakeRequest: handshakeRequest: HandshakeRequestMessage -> string
    abstract parseHandshakeResponse: data: obj option -> obj option * HandshakeResponseMessage

type [<AllowNullLiteral>] HandshakeProtocolStatic =
    [<EmitConstructor>] abstract Create: unit -> HandshakeProtocol

type [<AllowNullLiteral>] HeaderNames =
    interface end

type [<AllowNullLiteral>] HeaderNamesStatic =
    [<EmitConstructor>] abstract Create: unit -> HeaderNames
    abstract Authorization: obj
    abstract Cookie: obj

/// Represents an HTTP request.
type [<AllowNullLiteral>] HttpRequest =
    /// The HTTP method to use for the request.
    abstract method: string option with get, set
    /// The URL for the request.
    abstract url: string option with get, set
    /// The body content for the request. May be a string or an ArrayBuffer (for binary data).
    abstract content: U2<string, ArrayBuffer> option with get, set
    /// An object describing headers to apply to the request.
    abstract headers: MessageHeaders option with get, set
    /// The XMLHttpRequestResponseType to apply to the request.
    abstract responseType: XMLHttpRequestResponseType option with get, set
    /// An AbortSignal that can be monitored for cancellation.
    abstract abortSignal: AbortSignal option with get, set
    /// The time to wait for the request to complete before throwing a TimeoutError. Measured in milliseconds.
    abstract timeout: float option with get, set
    /// This controls whether credentials such as cookies are sent in cross-site requests.
    abstract withCredentials: bool option with get, set

/// Represents an HTTP response.
type [<AllowNullLiteral>] HttpResponse =
    abstract statusCode: float
    abstract statusText: string option
    abstract content: U2<string, ArrayBuffer> option

/// Represents an HTTP response.
type [<AllowNullLiteral>] HttpResponseStatic =
    /// <summary>Constructs a new instance of <see cref="@microsoft/signalr.HttpResponse" /> with the specified status code.</summary>
    /// <param name="statusCode">The status code of the response.</param>
    [<EmitConstructor>] abstract Create: statusCode: float -> HttpResponse
    /// <summary>Constructs a new instance of <see cref="@microsoft/signalr.HttpResponse" /> with the specified status code and message.</summary>
    /// <param name="statusCode">The status code of the response.</param>
    /// <param name="statusText">The status message of the response.</param>
    [<EmitConstructor>] abstract Create: statusCode: float * statusText: string -> HttpResponse
    /// <summary>Constructs a new instance of <see cref="@microsoft/signalr.HttpResponse" /> with the specified status code, message and string content.</summary>
    /// <param name="statusCode">The status code of the response.</param>
    /// <param name="statusText">The status message of the response.</param>
    /// <param name="content">The content of the response.</param>
    [<EmitConstructor>] abstract Create: statusCode: float * statusText: string * content: string -> HttpResponse
    /// <summary>Constructs a new instance of <see cref="@microsoft/signalr.HttpResponse" /> with the specified status code, message and binary content.</summary>
    /// <param name="statusCode">The status code of the response.</param>
    /// <param name="statusText">The status message of the response.</param>
    /// <param name="content">The content of the response.</param>
    [<EmitConstructor>] abstract Create: statusCode: float * statusText: string * content: ArrayBuffer -> HttpResponse
    /// <summary>Constructs a new instance of <see cref="@microsoft/signalr.HttpResponse" /> with the specified status code, message and binary content.</summary>
    /// <param name="statusCode">The status code of the response.</param>
    /// <param name="statusText">The status message of the response.</param>
    /// <param name="content">The content of the response.</param>
    [<EmitConstructor>] abstract Create: statusCode: float * statusText: string * content: U2<string, ArrayBuffer> -> HttpResponse

/// Abstraction over an HTTP client.
///
/// This class provides an abstraction over an HTTP client so that a different implementation can be provided on different platforms.
type [<AllowNullLiteral>] HttpClient =
    /// <summary>Issues an HTTP GET request to the specified URL, returning a Promise that resolves with an <see cref="@microsoft/signalr.HttpResponse" /> representing the result.</summary>
    /// <param name="url">The URL for the request.</param>
    /// <returns>A Promise that resolves with an {@link  @microsoft/signalr.HttpResponse} describing the response, or rejects with an Error indicating a failure.</returns>
    abstract get: url: string -> Promise<HttpResponse>
    /// <summary>Issues an HTTP GET request to the specified URL, returning a Promise that resolves with an <see cref="@microsoft/signalr.HttpResponse" /> representing the result.</summary>
    /// <param name="url">The URL for the request.</param>
    /// <param name="options">Additional options to configure the request. The 'url' field in this object will be overridden by the url parameter.</param>
    /// <returns>A Promise that resolves with an {@link  @microsoft/signalr.HttpResponse} describing the response, or rejects with an Error indicating a failure.</returns>
    abstract get: url: string * options: HttpRequest -> Promise<HttpResponse>
    /// <summary>Issues an HTTP POST request to the specified URL, returning a Promise that resolves with an <see cref="@microsoft/signalr.HttpResponse" /> representing the result.</summary>
    /// <param name="url">The URL for the request.</param>
    /// <returns>A Promise that resolves with an {@link  @microsoft/signalr.HttpResponse} describing the response, or rejects with an Error indicating a failure.</returns>
    abstract post: url: string -> Promise<HttpResponse>
    /// <summary>Issues an HTTP POST request to the specified URL, returning a Promise that resolves with an <see cref="@microsoft/signalr.HttpResponse" /> representing the result.</summary>
    /// <param name="url">The URL for the request.</param>
    /// <param name="options">Additional options to configure the request. The 'url' field in this object will be overridden by the url parameter.</param>
    /// <returns>A Promise that resolves with an {@link  @microsoft/signalr.HttpResponse} describing the response, or rejects with an Error indicating a failure.</returns>
    abstract post: url: string * options: HttpRequest -> Promise<HttpResponse>
    /// <summary>Issues an HTTP DELETE request to the specified URL, returning a Promise that resolves with an <see cref="@microsoft/signalr.HttpResponse" /> representing the result.</summary>
    /// <param name="url">The URL for the request.</param>
    /// <returns>A Promise that resolves with an {@link  @microsoft/signalr.HttpResponse} describing the response, or rejects with an Error indicating a failure.</returns>
    abstract delete: url: string -> Promise<HttpResponse>
    /// <summary>Issues an HTTP DELETE request to the specified URL, returning a Promise that resolves with an <see cref="@microsoft/signalr.HttpResponse" /> representing the result.</summary>
    /// <param name="url">The URL for the request.</param>
    /// <param name="options">Additional options to configure the request. The 'url' field in this object will be overridden by the url parameter.</param>
    /// <returns>A Promise that resolves with an {@link  @microsoft/signalr.HttpResponse} describing the response, or rejects with an Error indicating a failure.</returns>
    abstract delete: url: string * options: HttpRequest -> Promise<HttpResponse>
    /// <summary>Issues an HTTP request to the specified URL, returning a <see cref="Promise" /> that resolves with an <see cref="@microsoft/signalr.HttpResponse" /> representing the result.</summary>
    /// <param name="request">An {@link  @microsoft/signalr.HttpRequest} describing the request to send.</param>
    /// <returns>A Promise that resolves with an HttpResponse describing the response, or rejects with an Error indicating a failure.</returns>
    abstract send: request: HttpRequest -> Promise<HttpResponse>
    /// <summary>Gets all cookies that apply to the specified URL.</summary>
    /// <param name="url">The URL that the cookies are valid for.</param>
    /// <returns>A string containing all the key-value cookie pairs for the specified URL.</returns>
    abstract getCookieString: url: string -> string

/// Abstraction over an HTTP client.
///
/// This class provides an abstraction over an HTTP client so that a different implementation can be provided on different platforms.
type [<AllowNullLiteral>] HttpClientStatic =
    [<EmitConstructor>] abstract Create: unit -> HttpClient

type [<AllowNullLiteral>] INegotiateResponse =
    abstract connectionId: string option with get, set
    abstract connectionToken: string option with get, set
    abstract negotiateVersion: float option with get, set
    abstract availableTransports: ResizeArray<IAvailableTransport> option with get, set
    abstract url: string option with get, set
    abstract accessToken: string option with get, set
    abstract error: string option with get, set

type [<AllowNullLiteral>] IAvailableTransport =
    abstract transport: KeyOf<obj> with get, set
    abstract transferFormats: ResizeArray<KeyOf<obj>> with get, set

type [<AllowNullLiteral>] HttpConnection =
    inherit IConnection
    abstract features: obj option
    abstract baseUrl: string with get, set
    abstract connectionId: string option with get, set
    abstract onreceive: (U2<string, ArrayBuffer> -> unit) option with get, set
    abstract onclose: ((Error) option -> unit) option with get, set
    abstract start: unit -> Promise<unit>
    abstract start: transferFormat: TransferFormat -> Promise<unit>
    abstract send: data: U2<string, ArrayBuffer> -> Promise<unit>
    abstract stop: ?error: Error -> Promise<unit>

type [<AllowNullLiteral>] HttpConnectionStatic =
    [<EmitConstructor>] abstract Create: url: string * ?options: IHttpConnectionOptions -> HttpConnection

type [<AllowNullLiteral>] TransportSendQueue =
    abstract send: data: U2<string, ArrayBuffer> -> Promise<unit>
    abstract stop: unit -> Promise<unit>

type [<AllowNullLiteral>] TransportSendQueueStatic =
    [<EmitConstructor>] abstract Create: _transport: ITransport -> TransportSendQueue

/// <summary>Describes the current state of the <see cref="HubConnection" /> to the server.</summary>
type [<StringEnum>] [<RequireQualifiedAccess>] HubConnectionState =
    /// The hub connection is disconnected.
    | [<CompiledName("Disconnected")>] Disconnected
    /// The hub connection is connecting.
    | [<CompiledName("Connecting")>] Connecting
    /// The hub connection is connected.
    | [<CompiledName("Connected")>] Connected
    /// The hub connection is disconnecting.
    | [<CompiledName("Disconnecting")>] Disconnecting
    /// The hub connection is reconnecting.
    | [<CompiledName("Reconnecting")>] Reconnecting

/// Represents a connection to a SignalR Hub.
type [<AllowNullLiteral>] HubConnection =
    /// The server timeout in milliseconds.
    ///
    /// If this timeout elapses without receiving any messages from the server, the connection will be terminated with an error.
    /// The default timeout value is 30,000 milliseconds (30 seconds).
    abstract serverTimeoutInMilliseconds: float with get, set
    /// Default interval at which to ping the server.
    ///
    /// The default value is 15,000 milliseconds (15 seconds).
    /// Allows the server to detect hard disconnects (like when a client unplugs their computer).
    /// The ping will happen at most as often as the server pings.
    /// If the server pings every 5 seconds, a value lower than 5 will ping every 5 seconds.
    abstract keepAliveIntervalInMilliseconds: float with get, set
    /// <summary>Indicates the state of the <see cref="HubConnection" /> to the server.</summary>
    abstract state: HubConnectionState
    /// <summary>
    /// Represents the connection id of the <see cref="HubConnection" /> on the server. The connection id will be null when the connection is either
    /// in the disconnected state or if the negotiation step was skipped.
    /// </summary>
    abstract connectionId: string option
    /// <summary>
    /// Indicates the url of the <see cref="HubConnection" /> to the server.
    /// Sets a new url for the HubConnection. Note that the url can only be changed when the connection is in either the Disconnected or
    /// Reconnecting states.
    /// </summary>
    abstract baseUrl: string with get, set
    /// <summary>Starts the connection.</summary>
    /// <returns>A Promise that resolves when the connection has been successfully established, or rejects with an error.</returns>
    abstract start: unit -> Promise<unit>
    /// <summary>Stops the connection.</summary>
    /// <returns>A Promise that resolves when the connection has been successfully terminated, or rejects with an error.</returns>
    abstract stop: unit -> Promise<unit>
    /// <summary>Invokes a streaming hub method on the server using the specified name and arguments.</summary>
    /// <typeparam name="T">The type of the items returned by the server.</typeparam>
    /// <param name="methodName">The name of the server method to invoke.</param>
    /// <param name="args">The arguments used to invoke the server method.</param>
    /// <returns>An object that yields results from the server as they are received.</returns>
    abstract stream: methodName: string * [<ParamArray>] args: obj option[] -> IStreamResult<'T>
    /// <summary>
    /// Invokes a hub method on the server using the specified name and arguments. Does not wait for a response from the receiver.
    ///
    /// The Promise returned by this method resolves when the client has sent the invocation to the server. The server may still
    /// be processing the invocation.
    /// </summary>
    /// <param name="methodName">The name of the server method to invoke.</param>
    /// <param name="args">The arguments used to invoke the server method.</param>
    /// <returns>A Promise that resolves when the invocation has been successfully sent, or rejects with an error.</returns>
    abstract send: methodName: string * [<ParamArray>] args: obj option[] -> Promise<unit>
    /// <summary>
    /// Invokes a hub method on the server using the specified name and arguments.
    ///
    /// The Promise returned by this method resolves when the server indicates it has finished invoking the method. When the promise
    /// resolves, the server has finished invoking the method. If the server method returns a result, it is produced as the result of
    /// resolving the Promise.
    /// </summary>
    /// <typeparam name="T">The expected return type.</typeparam>
    /// <param name="methodName">The name of the server method to invoke.</param>
    /// <param name="args">The arguments used to invoke the server method.</param>
    /// <returns>A Promise that resolves with the result of the server method (if any), or rejects with an error.</returns>
    abstract invoke: methodName: string * [<ParamArray>] args: obj option[] -> Promise<'T>
    /// <summary>Registers a handler that will be invoked when the hub method with the specified method name is invoked.</summary>
    /// <param name="methodName">The name of the hub method to define.</param>
    /// <param name="newMethod">The handler that will be raised when the hub method is invoked.</param>
    abstract on: methodName: string * newMethod: (obj [] -> unit) -> unit
    /// <summary>Removes all handlers for the specified hub method.</summary>
    /// <param name="methodName">The name of the method to remove handlers for.</param>
    abstract off: methodName: string -> unit
    /// <summary>
    /// Removes the specified handler for the specified hub method.
    ///
    /// You must pass the exact same Function instance as was previously passed to <see cref="@microsoft/signalr.HubConnection.on" />. Passing a different instance (even if the function
    /// body is the same) will not remove the handler.
    /// </summary>
    /// <param name="methodName">The name of the method to remove handlers for.</param>
    /// <param name="method">The handler to remove. This must be the same Function instance as the one passed to {@link  @microsoft/signalr.HubConnection.on}.</param>
    abstract off: methodName: string * method: (ResizeArray<obj option> -> unit) -> unit
    /// <summary>Registers a handler that will be invoked when the connection is closed.</summary>
    /// <param name="callback">The handler that will be invoked when the connection is closed. Optionally receives a single argument containing the error that caused the connection to close (if any).</param>
    abstract onclose: callback: ((Error) option -> unit) -> unit
    /// <summary>Registers a handler that will be invoked when the connection starts reconnecting.</summary>
    /// <param name="callback">The handler that will be invoked when the connection starts reconnecting. Optionally receives a single argument containing the error that caused the connection to start reconnecting (if any).</param>
    abstract onreconnecting: callback: ((Error) option -> unit) -> unit
    /// <summary>Registers a handler that will be invoked when the connection successfully reconnects.</summary>
    /// <param name="callback">The handler that will be invoked when the connection successfully reconnects.</param>
    abstract onreconnected: callback: ((string) option -> unit) -> unit

/// <summary>A builder for configuring <see cref="@microsoft/signalr.HubConnection" /> instances.</summary>
type [<AllowNullLiteral>] HubConnectionBuilder =
    /// <summary>Configures console logging for the <see cref="@microsoft/signalr.HubConnection" />.</summary>
    /// <param name="logLevel">The minimum level of messages to log. Anything at this level, or a more severe level, will be logged.</param>
    /// <returns>The {@link  @microsoft/signalr.HubConnectionBuilder} instance, for chaining.</returns>
    abstract configureLogging: logLevel: LogLevel -> HubConnectionBuilder
    /// <summary>Configures custom logging for the <see cref="@microsoft/signalr.HubConnection" />.</summary>
    /// <param name="logger">An object implementing the {@link  @microsoft/signalr.ILogger} interface, which will be used to write all log messages.</param>
    /// <returns>The {@link  @microsoft/signalr.HubConnectionBuilder} instance, for chaining.</returns>
    abstract configureLogging: logger: ILogger -> HubConnectionBuilder
    /// <summary>Configures custom logging for the <see cref="@microsoft/signalr.HubConnection" />.</summary>
    /// <param name="logLevel">
    /// A string representing a LogLevel setting a minimum level of messages to log.
    /// See <see href="https://docs.microsoft.com/aspnet/core/signalr/configuration#configure-logging">the documentation for client logging configuration</see> for more details.
    /// </param>
    abstract configureLogging: logLevel: string -> HubConnectionBuilder
    /// <summary>Configures custom logging for the <see cref="@microsoft/signalr.HubConnection" />.</summary>
    /// <param name="logging">
    /// A {@link  @microsoft/signalr.LogLevel}, a string representing a LogLevel, or an object implementing the {@link  @microsoft/signalr.ILogger} interface.
    /// See <see href="https://docs.microsoft.com/aspnet/core/signalr/configuration#configure-logging">the documentation for client logging configuration</see> for more details.
    /// </param>
    /// <returns>The {@link  @microsoft/signalr.HubConnectionBuilder} instance, for chaining.</returns>
    abstract configureLogging: logging: U3<LogLevel, string, ILogger> -> HubConnectionBuilder
    /// <summary>
    /// Configures the <see cref="@microsoft/signalr.HubConnection" /> to use HTTP-based transports to connect to the specified URL.
    ///
    /// The transport will be selected automatically based on what the server and client support.
    /// </summary>
    /// <param name="url">The URL the connection will use.</param>
    /// <returns>The {@link  @microsoft/signalr.HubConnectionBuilder} instance, for chaining.</returns>
    abstract withUrl: url: string -> HubConnectionBuilder
    /// <summary>Configures the <see cref="@microsoft/signalr.HubConnection" /> to use the specified HTTP-based transport to connect to the specified URL.</summary>
    /// <param name="url">The URL the connection will use.</param>
    /// <param name="transportType">The specific transport to use.</param>
    /// <returns>The {@link  @microsoft/signalr.HubConnectionBuilder} instance, for chaining.</returns>
    abstract withUrl: url: string * transportType: HttpTransportType -> HubConnectionBuilder
    /// <summary>Configures the <see cref="@microsoft/signalr.HubConnection" /> to use HTTP-based transports to connect to the specified URL.</summary>
    /// <param name="url">The URL the connection will use.</param>
    /// <param name="options">An options object used to configure the connection.</param>
    /// <returns>The {@link  @microsoft/signalr.HubConnectionBuilder} instance, for chaining.</returns>
    abstract withUrl: url: string * options: IHttpConnectionOptions -> HubConnectionBuilder
    /// <summary>Configures the <see cref="@microsoft/signalr.HubConnection" /> to use the specified Hub Protocol.</summary>
    /// <param name="protocol">The {@link  @microsoft/signalr.IHubProtocol} implementation to use.</param>
    abstract withHubProtocol: protocol: IHubProtocol -> HubConnectionBuilder
    /// <summary>
    /// Configures the <see cref="@microsoft/signalr.HubConnection" /> to automatically attempt to reconnect if the connection is lost.
    /// By default, the client will wait 0, 2, 10 and 30 seconds respectively before trying up to 4 reconnect attempts.
    /// </summary>
    abstract withAutomaticReconnect: unit -> HubConnectionBuilder
    /// <summary>Configures the <see cref="@microsoft/signalr.HubConnection" /> to automatically attempt to reconnect if the connection is lost.</summary>
    /// <param name="retryDelays">
    /// An array containing the delays in milliseconds before trying each reconnect attempt.
    /// The length of the array represents how many failed reconnect attempts it takes before the client will stop attempting to reconnect.
    /// </param>
    abstract withAutomaticReconnect: retryDelays: ResizeArray<float> -> HubConnectionBuilder
    /// <summary>Configures the <see cref="@microsoft/signalr.HubConnection" /> to automatically attempt to reconnect if the connection is lost.</summary>
    /// <param name="reconnectPolicy">An {@link  @microsoft/signalR.IRetryPolicy} that controls the timing and number of reconnect attempts.</param>
    abstract withAutomaticReconnect: reconnectPolicy: IRetryPolicy -> HubConnectionBuilder
    /// <summary>Creates a <see cref="@microsoft/signalr.HubConnection" /> from the configuration options specified in this builder.</summary>
    /// <returns>The configured {@link  @microsoft/signalr.HubConnection}.</returns>
    abstract build: unit -> HubConnection

/// <summary>A builder for configuring <see cref="@microsoft/signalr.HubConnection" /> instances.</summary>
type [<AllowNullLiteral>] HubConnectionBuilderStatic =
    [<EmitConstructor>] abstract Create: unit -> HubConnectionBuilder

type [<AllowNullLiteral>] IConnection =
    abstract features: obj option
    abstract connectionId: string option
    abstract baseUrl: string with get, set
    abstract start: transferFormat: TransferFormat -> Promise<unit>
    abstract send: data: U2<string, ArrayBuffer> -> Promise<unit>
    abstract stop: ?error: Error -> Promise<unit>
    abstract onreceive: (U2<string, ArrayBuffer> -> unit) option with get, set
    abstract onclose: ((Error) option -> unit) option with get, set

/// <summary>Options provided to the 'withUrl' method on <see cref="@microsoft/signalr.HubConnectionBuilder" /> to configure options for the HTTP-based transports.</summary>
type [<AllowNullLiteral>] IHttpConnectionOptions =
    /// <summary><see cref="@microsoft/signalr.MessageHeaders" /> containing custom headers to be sent with every HTTP request. Note, setting headers in the browser will not work for WebSockets or the ServerSentEvents stream.</summary>
    abstract headers: MessageHeaders option with get, set
    /// <summary>An <see cref="@microsoft/signalr.HttpClient" /> that will be used to make HTTP requests.</summary>
    abstract httpClient: HttpClient option with get, set
    /// <summary>An <see cref="@microsoft/signalr.HttpTransportType" /> value specifying the transport to use for the connection.</summary>
    abstract transport: U2<HttpTransportType, ITransport> option with get, set
    /// <summary>
    /// Configures the logger used for logging.
    ///
    /// Provide an <see cref="@microsoft/signalr.ILogger" /> instance, and log messages will be logged via that instance. Alternatively, provide a value from
    /// the <see cref="@microsoft/signalr.LogLevel" /> enumeration and a default logger which logs to the Console will be configured to log messages of the specified
    /// level (or higher).
    /// </summary>
    abstract logger: U2<ILogger, LogLevel> option with get, set
    /// <summary>A function that provides an access token required for HTTP Bearer authentication.</summary>
    /// <returns>A string containing the access token, or a Promise that resolves to a string containing the access token.</returns>
    abstract accessTokenFactory: unit -> U2<string, Promise<string>>
    /// A boolean indicating if message content should be logged.
    ///
    /// Message content can contain sensitive user data, so this is disabled by default.
    abstract logMessageContent: bool option with get, set
    /// <summary>
    /// A boolean indicating if negotiation should be skipped.
    ///
    /// Negotiation can only be skipped when the <see cref="@microsoft/signalr.IHttpConnectionOptions.transport" /> property is set to 'HttpTransportType.WebSockets'.
    /// </summary>
    abstract skipNegotiation: bool option with get, set
    /// Default value is 'true'.
    /// This controls whether credentials such as cookies are sent in cross-site requests.
    ///
    /// Cookies are used by many load-balancers for sticky sessions which is required when your app is deployed with multiple servers.
    abstract withCredentials: bool option with get, set
    /// Default value is 100,000 milliseconds.
    /// Timeout to apply to Http requests.
    ///
    /// This will not apply to Long Polling poll requests, EventSource, or WebSockets.
    abstract timeout: float option with get, set

/// Defines the type of a Hub Message.
type [<RequireQualifiedAccess>] MessageType =
    /// <summary>Indicates the message is an Invocation message and implements the <see cref="@microsoft/signalr.InvocationMessage" /> interface.</summary>
    | Invocation = 1
    /// <summary>Indicates the message is a StreamItem message and implements the <see cref="@microsoft/signalr.StreamItemMessage" /> interface.</summary>
    | StreamItem = 2
    /// <summary>Indicates the message is a Completion message and implements the <see cref="@microsoft/signalr.CompletionMessage" /> interface.</summary>
    | Completion = 3
    /// <summary>Indicates the message is a Stream Invocation message and implements the <see cref="@microsoft/signalr.StreamInvocationMessage" /> interface.</summary>
    | StreamInvocation = 4
    /// <summary>Indicates the message is a Cancel Invocation message and implements the <see cref="@microsoft/signalr.CancelInvocationMessage" /> interface.</summary>
    | CancelInvocation = 5
    /// <summary>Indicates the message is a Ping message and implements the <see cref="@microsoft/signalr.PingMessage" /> interface.</summary>
    | Ping = 6
    /// <summary>Indicates the message is a Close message and implements the <see cref="@microsoft/signalr.CloseMessage" /> interface.</summary>
    | Close = 7

/// Defines a dictionary of string keys and string values representing headers attached to a Hub message.
type [<AllowNullLiteral>] MessageHeaders =
    /// Gets or sets the header with the specified key.
    [<EmitIndexer>] abstract Item: key: string -> string with get, set

/// Union type of all known Hub messages.
type HubMessage =
    U7<InvocationMessage, StreamInvocationMessage, StreamItemMessage, CompletionMessage, CancelInvocationMessage, PingMessage, CloseMessage>

/// Defines properties common to all Hub messages.
type [<AllowNullLiteral>] HubMessageBase =
    /// <summary>A <see cref="@microsoft/signalr.MessageType" /> value indicating the type of this message.</summary>
    abstract ``type``: MessageType

/// Defines properties common to all Hub messages relating to a specific invocation.
type [<AllowNullLiteral>] HubInvocationMessage =
    inherit HubMessageBase
    /// <summary>A <see cref="@microsoft/signalr.MessageHeaders" /> dictionary containing headers attached to the message.</summary>
    abstract headers: MessageHeaders option
    /// <summary>
    /// The ID of the invocation relating to this message.
    ///
    /// This is expected to be present for <see cref="@microsoft/signalr.StreamInvocationMessage" /> and <see cref="@microsoft/signalr.CompletionMessage" />. It may
    /// be 'undefined' for an <see cref="@microsoft/signalr.InvocationMessage" /> if the sender does not expect a response.
    /// </summary>
    abstract invocationId: string option

/// A hub message representing a non-streaming invocation.
type [<AllowNullLiteral>] InvocationMessage =
    inherit HubInvocationMessage
    /// <summary>A <see cref="@microsoft/signalr.MessageType" /> value indicating the type of this message.</summary>
    abstract ``type``: MessageType
    /// The target method name.
    abstract target: string
    /// The target method arguments.
    abstract arguments: ResizeArray<obj option>
    /// The target methods stream IDs.
    abstract streamIds: ResizeArray<string> option

/// A hub message representing a streaming invocation.
type [<AllowNullLiteral>] StreamInvocationMessage =
    inherit HubInvocationMessage
    /// <summary>A <see cref="@microsoft/signalr.MessageType" /> value indicating the type of this message.</summary>
    abstract ``type``: MessageType
    /// The invocation ID.
    abstract invocationId: string
    /// The target method name.
    abstract target: string
    /// The target method arguments.
    abstract arguments: ResizeArray<obj option>
    /// The target methods stream IDs.
    abstract streamIds: ResizeArray<string> option

/// A hub message representing a single item produced as part of a result stream.
type [<AllowNullLiteral>] StreamItemMessage =
    inherit HubInvocationMessage
    /// <summary>A <see cref="@microsoft/signalr.MessageType" /> value indicating the type of this message.</summary>
    abstract ``type``: MessageType
    /// The invocation ID.
    abstract invocationId: string
    /// The item produced by the server.
    abstract item: obj option

/// A hub message representing the result of an invocation.
type [<AllowNullLiteral>] CompletionMessage =
    inherit HubInvocationMessage
    /// <summary>A <see cref="@microsoft/signalr.MessageType" /> value indicating the type of this message.</summary>
    abstract ``type``: MessageType
    /// The invocation ID.
    abstract invocationId: string
    /// <summary>
    /// The error produced by the invocation, if any.
    ///
    /// Either <see cref="@microsoft/signalr.CompletionMessage.error" /> or <see cref="@microsoft/signalr.CompletionMessage.result" /> must be defined, but not both.
    /// </summary>
    abstract error: string option
    /// <summary>
    /// The result produced by the invocation, if any.
    ///
    /// Either <see cref="@microsoft/signalr.CompletionMessage.error" /> or <see cref="@microsoft/signalr.CompletionMessage.result" /> must be defined, but not both.
    /// </summary>
    abstract result: obj option

/// A hub message indicating that the sender is still active.
type [<AllowNullLiteral>] PingMessage =
    inherit HubMessageBase
    /// <summary>A <see cref="@microsoft/signalr.MessageType" /> value indicating the type of this message.</summary>
    abstract ``type``: MessageType

/// <summary>
/// A hub message indicating that the sender is closing the connection.
///
/// If <see cref="@microsoft/signalr.CloseMessage.error" /> is defined, the sender is closing the connection due to an error.
/// </summary>
type [<AllowNullLiteral>] CloseMessage =
    inherit HubMessageBase
    /// <summary>A <see cref="@microsoft/signalr.MessageType" /> value indicating the type of this message.</summary>
    abstract ``type``: MessageType
    /// The error that triggered the close, if any.
    ///
    /// If this property is undefined, the connection was closed normally and without error.
    abstract error: string option
    /// If true, clients with automatic reconnects enabled should attempt to reconnect after receiving the CloseMessage. Otherwise, they should not.
    abstract allowReconnect: bool option

/// A hub message sent to request that a streaming invocation be canceled.
type [<AllowNullLiteral>] CancelInvocationMessage =
    inherit HubInvocationMessage
    /// <summary>A <see cref="@microsoft/signalr.MessageType" /> value indicating the type of this message.</summary>
    abstract ``type``: MessageType
    /// The invocation ID.
    abstract invocationId: string

/// A protocol abstraction for communicating with SignalR Hubs.
type [<AllowNullLiteral>] IHubProtocol =
    /// The name of the protocol. This is used by SignalR to resolve the protocol between the client and server.
    abstract name: string
    /// The version of the protocol.
    abstract version: float
    /// <summary>The <see cref="@microsoft/signalr.TransferFormat" /> of the protocol.</summary>
    abstract transferFormat: TransferFormat
    /// <summary>
    /// Creates an array of <see cref="@microsoft/signalr.HubMessage" /> objects from the specified serialized representation.
    ///
    /// If <see cref="@microsoft/signalr.IHubProtocol.transferFormat" /> is 'Text', the <c>input</c> parameter must be a string, otherwise it must be an ArrayBuffer.
    /// </summary>
    /// <param name="input">A string or ArrayBuffer containing the serialized representation.</param>
    /// <param name="logger">A logger that will be used to log messages that occur during parsing.</param>
    abstract parseMessages: input: U2<string, ArrayBuffer> * logger: ILogger -> ResizeArray<HubMessage>
    /// <summary>
    /// Writes the specified <see cref="@microsoft/signalr.HubMessage" /> to a string or ArrayBuffer and returns it.
    ///
    /// If <see cref="@microsoft/signalr.IHubProtocol.transferFormat" /> is 'Text', the result of this method will be a string, otherwise it will be an ArrayBuffer.
    /// </summary>
    /// <param name="message">The message to write.</param>
    /// <returns>A string or ArrayBuffer containing the serialized representation of the message.</returns>
    abstract writeMessage: message: HubMessage -> U2<string, ArrayBuffer>

/// <summary>
/// Indicates the severity of a log message.
///
/// Log Levels are ordered in increasing severity. So <c>Debug</c> is more severe than <c>Trace</c>, etc.
/// </summary>
type [<RequireQualifiedAccess>] LogLevel =
    /// Log level for very low severity diagnostic messages.
    | Trace = 0
    /// Log level for low severity diagnostic messages.
    | Debug = 1
    /// Log level for informational diagnostic messages.
    | Information = 2
    /// Log level for diagnostic messages that indicate a non-fatal problem.
    | Warning = 3
    /// Log level for diagnostic messages that indicate a failure in the current operation.
    | Error = 4
    /// Log level for diagnostic messages that indicate a failure that will terminate the entire application.
    | Critical = 5
    /// The highest possible log level. Used when configuring logging to indicate that no log messages should be emitted.
    | None = 6

/// An abstraction that provides a sink for diagnostic messages.
type [<AllowNullLiteral>] ILogger =
    /// <summary>Called by the framework to emit a diagnostic message.</summary>
    /// <param name="logLevel">The severity level of the message.</param>
    /// <param name="message">The message.</param>
    abstract log: logLevel: LogLevel * message: string -> unit

/// An abstraction that controls when the client attempts to reconnect and how many times it does so.
type [<AllowNullLiteral>] IRetryPolicy =
    /// <summary>Called after the transport loses the connection.</summary>
    /// <param name="retryContext">Details related to the retry event to help determine how long to wait for the next retry.</param>
    /// <returns>The amount of time in milliseconds to wait before the next retry. <c>null</c> tells the client to stop retrying.</returns>
    abstract nextRetryDelayInMilliseconds: retryContext: RetryContext -> float option

type [<AllowNullLiteral>] RetryContext =
    /// The number of consecutive failed tries so far.
    abstract previousRetryCount: float
    /// The amount of time in milliseconds spent retrying so far.
    abstract elapsedMilliseconds: float
    /// The error that forced the upcoming retry.
    abstract retryReason: Error

/// Specifies a specific HTTP transport type.
type [<RequireQualifiedAccess>] HttpTransportType =
    /// Specifies no transport preference.
    | None = 0
    /// Specifies the WebSockets transport.
    | WebSockets = 1
    /// Specifies the Server-Sent Events transport.
    | ServerSentEvents = 2
    /// Specifies the Long Polling transport.
    | LongPolling = 4

/// Specifies the transfer format for a connection.
type [<RequireQualifiedAccess>] TransferFormat =
    /// Specifies that only text data will be transmitted over the connection.
    | Text = 1
    /// Specifies that binary data will be transmitted over the connection.
    | Binary = 2

/// An abstraction over the behavior of transports. This is designed to support the framework and not intended for use by applications.
type [<AllowNullLiteral>] ITransport =
    abstract connect: url: string * transferFormat: TransferFormat -> Promise<unit>
    abstract send: data: obj option -> Promise<unit>
    abstract stop: unit -> Promise<unit>
    abstract onreceive: (U2<string, ArrayBuffer> -> unit) option with get, set
    abstract onclose: ((Error) option -> unit) option with get, set

/// Implements the JSON Hub Protocol.
type [<AllowNullLiteral>] JsonHubProtocol =
    inherit IHubProtocol
    /// <summary>The name of the protocol. This is used by SignalR to resolve the protocol between the client and server.</summary>
    abstract name: string
    /// <summary>The version of the protocol.</summary>
    abstract version: float
    /// <summary>The <see cref="@microsoft/signalr.TransferFormat" /> of the protocol.</summary>
    abstract transferFormat: TransferFormat
    /// <summary>Creates an array of <see cref="@microsoft/signalr.HubMessage" /> objects from the specified serialized representation.</summary>
    /// <param name="input">A string containing the serialized representation.</param>
    /// <param name="logger">A logger that will be used to log messages that occur during parsing.</param>
    abstract parseMessages: input: string * logger: ILogger -> ResizeArray<HubMessage>
    /// <summary>Writes the specified <see cref="@microsoft/signalr.HubMessage" /> to a string and returns it.</summary>
    /// <param name="message">The message to write.</param>
    /// <returns>A string containing the serialized representation of the message.</returns>
    abstract writeMessage: message: HubMessage -> string

/// Implements the JSON Hub Protocol.
type [<AllowNullLiteral>] JsonHubProtocolStatic =
    [<EmitConstructor>] abstract Create: unit -> JsonHubProtocol

/// A logger that does nothing when log messages are sent to it.
type [<AllowNullLiteral>] NullLogger =
    inherit ILogger
    /// <summary>Called by the framework to emit a diagnostic message.</summary>
    abstract log: _logLevel: LogLevel * _message: string -> unit

/// A logger that does nothing when log messages are sent to it.
type [<AllowNullLiteral>] NullLoggerStatic =
    /// <summary>The singleton instance of the <see cref="@microsoft/signalr.NullLogger" />.</summary>
    abstract instance: ILogger with get, set

type [<AllowNullLiteral>] LongPollingTransport =
    inherit ITransport
    abstract onreceive: (U2<string, ArrayBuffer> -> unit) option with get, set
    abstract onclose: ((Error) option -> unit) option with get, set
    abstract pollAborted: bool
    abstract connect: url: string * transferFormat: TransferFormat -> Promise<unit>
    abstract send: data: obj option -> Promise<unit>
    abstract stop: unit -> Promise<unit>

type [<AllowNullLiteral>] LongPollingTransportStatic =
    [<EmitConstructor>] abstract Create: httpClient: HttpClient * logger: ILogger * options: IHttpConnectionOptions -> LongPollingTransport

type EventSourceConstructor =
    obj

type [<AllowNullLiteral>] WebSocketConstructor =
    abstract CLOSED: float
    abstract CLOSING: float
    abstract CONNECTING: float
    abstract OPEN: float

type [<AllowNullLiteral>] WebSocketConstructorStatic =
    [<EmitConstructor>] abstract Create: url: string * ?protocols: U2<string, ResizeArray<string>> * ?options: obj -> WebSocketConstructor

type [<AllowNullLiteral>] ServerSentEventsTransport =
    inherit ITransport
    abstract onreceive: (U2<string, ArrayBuffer> -> unit) option with get, set
    abstract onclose: ((Error) option -> unit) option with get, set
    abstract connect: url: string * transferFormat: TransferFormat -> Promise<unit>
    abstract send: data: obj option -> Promise<unit>
    abstract stop: unit -> Promise<unit>

type [<AllowNullLiteral>] ServerSentEventsTransportStatic =
    [<EmitConstructor>] abstract Create: httpClient: HttpClient * accessToken: string option * logger: ILogger * options: IHttpConnectionOptions -> ServerSentEventsTransport

/// <summary>Defines the expected type for a receiver of results streamed by the server.</summary>
/// <typeparam name="T">The type of the items being sent by the server.</typeparam>
type [<AllowNullLiteral>] IStreamSubscriber<'T> =
    /// <summary>A boolean that will be set by the <see cref="@microsoft/signalr.IStreamResult" /> when the stream is closed.</summary>
    abstract closed: bool option with get, set
    /// Called by the framework when a new item is available.
    abstract next: value: 'T -> unit
    /// <summary>
    /// Called by the framework when an error has occurred.
    ///
    /// After this method is called, no additional methods on the <see cref="@microsoft/signalr.IStreamSubscriber" /> will be called.
    /// </summary>
    abstract error: err: obj option -> unit
    /// <summary>
    /// Called by the framework when the end of the stream is reached.
    ///
    /// After this method is called, no additional methods on the <see cref="@microsoft/signalr.IStreamSubscriber" /> will be called.
    /// </summary>
    abstract complete: unit -> unit

/// <summary>Defines the result of a streaming hub method.</summary>
/// <typeparam name="T">The type of the items being sent by the server.</typeparam>
type [<AllowNullLiteral>] IStreamResult<'T> =
    /// <summary>Attaches a <see cref="@microsoft/signalr.IStreamSubscriber" />, which will be invoked when new items are available from the stream.</summary>
    /// <param name="observer">The subscriber to attach.</param>
    /// <returns>A subscription that can be disposed to terminate the stream and stop calling methods on the {@link  @microsoft/signalr.IStreamSubscriber}.</returns>
    abstract subscribe: subscriber: IStreamSubscriber<'T> -> ISubscription<'T>

/// <summary>An interface that allows an <see cref="@microsoft/signalr.IStreamSubscriber" /> to be disconnected from a stream.</summary>
/// <typeparam name="T">The type of the items being sent by the server.</typeparam>
type [<AllowNullLiteral>] ISubscription<'T> =
    /// <summary>Disconnects the <see cref="@microsoft/signalr.IStreamSubscriber" /> associated with this subscription from the stream.</summary>
    abstract dispose: unit -> unit

/// Stream implementation to stream items to the server.
type [<AllowNullLiteral>] Subject<'T> =
    inherit IStreamResult<'T>
    abstract next: item: 'T -> unit
    abstract error: err: obj option -> unit
    abstract complete: unit -> unit
    /// <summary>Attaches a <see cref="@microsoft/signalr.IStreamSubscriber" />, which will be invoked when new items are available from the stream.</summary>
    abstract subscribe: observer: IStreamSubscriber<'T> -> ISubscription<'T>

/// Stream implementation to stream items to the server.
type [<AllowNullLiteral>] SubjectStatic =
    [<EmitConstructor>] abstract Create: unit -> Subject<'T>

type [<AllowNullLiteral>] TextMessageFormat =
    interface end

type [<AllowNullLiteral>] TextMessageFormatStatic =
    [<EmitConstructor>] abstract Create: unit -> TextMessageFormat
    abstract RecordSeparatorCode: float with get, set
    abstract RecordSeparator: string with get, set
    abstract write: output: string -> string
    abstract parse: input: string -> ResizeArray<string>

type [<AllowNullLiteral>] Arg =
    interface end

type [<AllowNullLiteral>] ArgStatic =
    [<EmitConstructor>] abstract Create: unit -> Arg
    abstract isRequired: ``val``: obj option * name: string -> unit
    abstract isNotEmpty: ``val``: string * name: string -> unit
    abstract isIn: ``val``: obj option * values: obj option * name: string -> unit

type [<AllowNullLiteral>] Platform =
    interface end

type [<AllowNullLiteral>] PlatformStatic =
    [<EmitConstructor>] abstract Create: unit -> Platform
    abstract isBrowser: bool
    abstract isWebWorker: bool
    abstract isReactNative: bool
    abstract isNode: bool

type [<AllowNullLiteral>] SubjectSubscription<'T> =
    inherit ISubscription<'T>
    /// <summary>Disconnects the <see cref="@microsoft/signalr.IStreamSubscriber" /> associated with this subscription from the stream.</summary>
    abstract dispose: unit -> unit

type [<AllowNullLiteral>] SubjectSubscriptionStatic =
    [<EmitConstructor>] abstract Create: subject: Subject<'T> * observer: IStreamSubscriber<'T> -> SubjectSubscription<'T>

type [<AllowNullLiteral>] ConsoleLogger =
    inherit ILogger
    abstract out: {| error: obj option -> unit; warn: obj option -> unit; info: obj option -> unit; log: obj option -> unit |} with get, set
    /// Called by the framework to emit a diagnostic message.
    abstract log: logLevel: LogLevel * message: string -> unit

type [<AllowNullLiteral>] ConsoleLoggerStatic =
    [<EmitConstructor>] abstract Create: minimumLogLevel: LogLevel -> ConsoleLogger

type [<AllowNullLiteral>] WebSocketTransport =
    inherit ITransport
    abstract onreceive: (U2<string, ArrayBuffer> -> unit) option with get, set
    abstract onclose: ((Error) option -> unit) option with get, set
    abstract connect: url: string * transferFormat: TransferFormat -> Promise<unit>
    abstract send: data: obj option -> Promise<unit>
    abstract stop: unit -> Promise<unit>

type [<AllowNullLiteral>] WebSocketTransportStatic =
    [<EmitConstructor>] abstract Create: httpClient: HttpClient * accessTokenFactory: (unit -> U2<string, Promise<string>>) option * logger: ILogger * logMessageContent: bool * webSocketConstructor: WebSocketConstructor * headers: MessageHeaders -> WebSocketTransport

type [<AllowNullLiteral>] XhrHttpClient =
    inherit HttpClient
    /// <summary>Issues an HTTP request to the specified URL, returning a <see cref="Promise" /> that resolves with an <see cref="@microsoft/signalr.HttpResponse" /> representing the result.</summary>
    abstract send: request: HttpRequest -> Promise<HttpResponse>

type [<AllowNullLiteral>] XhrHttpClientStatic =
    [<EmitConstructor>] abstract Create: logger: ILogger -> XhrHttpClient

type [<AllowNullLiteral>] IExports =
    abstract AbortController: AbortControllerStatic
    abstract AccessTokenHttpClient: AccessTokenHttpClientStatic

    /// <summary>Default implementation of <see cref="@microsoft/signalr.HttpClient" />.</summary>
    abstract DefaultHttpClient: DefaultHttpClientStatic
    abstract DefaultReconnectPolicy: DefaultReconnectPolicyStatic

    /// Error thrown when an HTTP request fails.
    abstract HttpError: HttpErrorStatic
    /// Error thrown when a timeout elapses.
    abstract TimeoutError: TimeoutErrorStatic
    /// Error thrown when an action is aborted.
    abstract AbortError: AbortErrorStatic

    abstract UnsupportedTransportError: UnsupportedTransportErrorStatic
    abstract DisabledTransportError: DisabledTransportErrorStatic
    abstract FailedToStartTransportError: FailedToStartTransportErrorStatic
    abstract FailedToNegotiateWithServerError: FailedToNegotiateWithServerErrorStatic
    abstract AggregateErrors: AggregateErrorsStatic
    abstract FetchHttpClient: FetchHttpClientStatic
    abstract HandshakeProtocol: HandshakeProtocolStatic
    abstract HeaderNames: HeaderNamesStatic

    /// Represents an HTTP response.
    abstract HttpResponse: HttpResponseStatic
    /// Abstraction over an HTTP client.
    ///
    /// This class provides an abstraction over an HTTP client so that a different implementation can be provided on different platforms.
    abstract HttpClient: HttpClientStatic
    abstract HttpConnection: HttpConnectionStatic
    abstract TransportSendQueue: TransportSendQueueStatic

    /// <summary>A builder for configuring <see cref="@microsoft/signalr.HubConnection" /> instances.</summary>
    abstract HubConnectionBuilder: HubConnectionBuilderStatic

    /// Implements the JSON Hub Protocol.
    abstract JsonHubProtocol: JsonHubProtocolStatic

    /// A logger that does nothing when log messages are sent to it.
    abstract NullLogger: NullLoggerStatic
    abstract LongPollingTransport: LongPollingTransportStatic
    abstract WebSocketConstructor: WebSocketConstructorStatic
    abstract ServerSentEventsTransport: ServerSentEventsTransportStatic

    /// Stream implementation to stream items to the server.
    abstract Subject: SubjectStatic
    abstract TextMessageFormat: TextMessageFormatStatic
    abstract WebSocketTransport: WebSocketTransportStatic
    abstract XhrHttpClient: XhrHttpClientStatic

// This is from the Utils.d.ts
// I wasn't sure what to do with it.

// type [<AllowNullLiteral>] IExports =
//     /// The version of the SignalR client.
//     abstract VERSION: string
//     abstract Arg: ArgStatic
//     abstract Platform: PlatformStatic
//     abstract getDataDetail: data: obj option * includeContent: bool -> string
//     abstract formatArrayBuffer: data: ArrayBuffer -> string
//     abstract isArrayBuffer: ``val``: obj option -> bool
//     abstract sendMessage: logger: ILogger * transportName: string * httpClient: HttpClient * url: string * content: U2<string, ArrayBuffer> * options: IHttpConnectionOptions -> Promise<unit>
//     abstract createLogger: ?logger: U2<ILogger, LogLevel> -> ILogger
//     abstract SubjectSubscription: SubjectSubscriptionStatic
//     abstract ConsoleLogger: ConsoleLoggerStatic
//     abstract getUserAgentHeader: unit -> string * string
//     abstract constructUserAgent: version: string * os: string * runtime: string * runtimeVersion: string option -> string
//     abstract getErrorString: e: obj option -> string
//     abstract getGlobalThis: unit -> obj

[<Fable.Core.ImportAll("@microsoft/signalr")>]
module Exports =
    let AbortController: AbortControllerStatic = jsNative
    let AccessTokenHttpClient: AccessTokenHttpClientStatic = jsNative

    /// <summary>Default implementation of <see cref="@microsoft/signalr.HttpClient" />.</summary>
    let DefaultHttpClient: DefaultHttpClientStatic = jsNative
    let DefaultReconnectPolicy: DefaultReconnectPolicyStatic = jsNative

    /// Error thrown when an HTTP request fails.
    let HttpError: HttpErrorStatic = jsNative
    /// Error thrown when a timeout elapses.
    let TimeoutError: TimeoutErrorStatic = jsNative
    /// Error thrown when an action is aborted.
    let AbortError: AbortErrorStatic = jsNative

    let UnsupportedTransportError: UnsupportedTransportErrorStatic = jsNative
    let DisabledTransportError: DisabledTransportErrorStatic = jsNative
    let FailedToStartTransportError: FailedToStartTransportErrorStatic = jsNative
    let FailedToNegotiateWithServerError: FailedToNegotiateWithServerErrorStatic = jsNative
    let AggregateErrors: AggregateErrorsStatic = jsNative
    let FetchHttpClient: FetchHttpClientStatic = jsNative
    let HandshakeProtocol: HandshakeProtocolStatic = jsNative
    let HeaderNames: HeaderNamesStatic = jsNative

    /// Represents an HTTP response.
    let HttpResponse: HttpResponseStatic = jsNative
    /// letion over an HTTP client.
    ///
    /// This class provides an letion over an HTTP client so that a different implementation can be provided on different platforms.
    let HttpClient: HttpClientStatic = jsNative
    let HttpConnection: HttpConnectionStatic = jsNative
    let TransportSendQueue: TransportSendQueueStatic = jsNative

    /// <summary>A builder for configuring <see cref="@microsoft/signalr.HubConnection" /> instances.</summary>
    let HubConnectionBuilder: HubConnectionBuilderStatic = jsNative

    /// Implements the JSON Hub Protocol.
    let JsonHubProtocol: JsonHubProtocolStatic = jsNative

    /// A logger that does nothing when log messages are sent to it.
    let NullLogger: NullLoggerStatic = jsNative
    let LongPollingTransport: LongPollingTransportStatic = jsNative
    let WebSocketConstructor: WebSocketConstructorStatic = jsNative
    let ServerSentEventsTransport: ServerSentEventsTransportStatic = jsNative

    /// Stream implementation to stream items to the server.
    let Subject: SubjectStatic = jsNative
    let TextMessageFormat: TextMessageFormatStatic = jsNative
    let WebSocketTransport: WebSocketTransportStatic = jsNative
    let XhrHttpClient: XhrHttpClientStatic = jsNative
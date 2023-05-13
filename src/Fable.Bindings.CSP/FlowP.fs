// ts2fable 0.9.0-build.725
module rec FlowP

#nowarn "3390" // disable warnings for invalid XML comments

open System
open Fable.Core
open Fable.Core.JS

type Error = System.Exception
type PromiseLike<'T> = Fable.Core.JS.Promise<'T>
type Symbol = obj


/// <summary>Indicates that the buffered items in queue has reached its capacity</summary>
type [<AllowNullLiteral; AbstractClass>] ClosedChannelError =
    inherit Error
    abstract message: string with get, set

/// <summary>Indicates that the buffered items in queue has reached its capacity</summary>
type [<AllowNullLiteral>] ClosedChannelErrorStatic =
    [<EmitConstructor>] abstract Create: unit -> ClosedChannelError

/// <summary>Indicates that the buffered items in queue has reached its capacity</summary>
type [<AllowNullLiteral; AbstractClass>] ChannelFullError =
    inherit Error
    abstract message: string with get, set

/// <summary>Indicates that the buffered items in queue has reached its capacity</summary>
type [<AllowNullLiteral>] ChannelFullErrorStatic =
    [<EmitConstructor>] abstract Create: unit -> ChannelFullError

/// <summary>Indicates that the buffered items in queue has reached its capacity</summary>
type [<AllowNullLiteral>] ChannelStream<'T> =
    inherit AsyncIterable<'T>
    abstract next: (unit -> Promise<'T>) with get, set

type [<AllowNullLiteral>] ChannelPipeOptions =
    /// <summary>
    /// Called when <c>target[read]</c> throws e.g. pipe a closed target channel.
    ///
    /// param will be called immediately every time the read throws an error.
    /// </summary>
    abstract onPipeError: (obj -> obj option) option with get, set

    /// <summary>
    ///
    /// Promise based multi producer single consumer channel
    ///
    ///
    /// - buffered message queue
    ///
    /// - <c>send</c> / <c>receive</c> basic message passing
    ///
    /// - <c>pipe</c> piping to other channels (or use <c>pipe.to()</c>)
    ///
    /// - <c>stream</c> ES6 async iterator api
    ///
    /// - <c>freeze</c> temporarily block all consumers, useful if your target has limited rate of consumption like Node.js net.Socket
    /// </summary>
    /// <typeparam name="T">type of messages in queue</typeparam>
    type [<AllowNullLiteral>] Channel<'T> =
        inherit PipeSource<'T>
        inherit PipeTarget<'T>
        /// <summary>
        /// send a value to channel.
        ///
        /// if the channel has reached its capacity, then call to <c>send</c> will be blocked
        /// </summary>
        /// <exception cref="" />
        abstract send: value: 'T -> Promise<unit>
        /// <summary>
        /// retrieve a value from channel.
        ///
        /// will never resolve if <see cref="Channel.pipe" /> or is enabled;
        /// will race with <see cref="Channel.stream" />
        /// </summary>
        abstract receive: unit -> Promise<'T>
        /// <summary>try to send a value synchronosly</summary>
        /// <exception cref="" />
        /// <exception cref="" />
        abstract trySend: value: 'T -> unit
        /// send a promise to channel, after the promise is resolved, send its fullfilled value
        abstract sendAsync: value: Promise<'T> -> Promise<unit>
        /// <summary>try receive one message</summary>
        /// <returns>message <c>T</c> or <c>undefined</c> if no messages in the queue</returns>
        abstract tryReceive: unit -> 'T option
        /// <summary>get a stream to read from the channel, internally uses <see cref="Channel.receive" /></summary>
        abstract stream: unit -> ChannelStream<'T>
        abstract ``[read]``: value: 'T -> unit
        /// <summary>
        /// pipe channel output to target
        ///
        /// there is only one target at the same time, use <c>ChannelHub</c> if you want to have multiple readers
        /// </summary>
        abstract pipe: target: PipeTarget<'T> * ?options: ChannelPipeOptions -> unit
        /// unlink output with target, future input will be stored in channel's internal buffer
        abstract unpipe: unit -> unit
        /// <summary>
        /// stop <see cref="Channel.stream" /> / <see cref="Channel.pipe" /> / <see cref="Channel.receive" /> new items until <see cref="Channel.resume" /> is called
        ///
        /// items sending to the channel will be queued despite pipe enabled
        /// </summary>
        abstract pause: unit -> unit
        /// <summary>resume the channel so <see cref="Channel.stream" /> / <see cref="Channel.pipe" /> / <see cref="Channel.receive" /> can continue to handle new messages</summary>
        abstract resume: unit -> unit
        /// <summary>close the channel, future <c>send</c> will throw a <see cref="ClosedChannelError" /></summary>
        abstract close: unit -> unit
        /// check if channel has been closed
        abstract closed: bool
        /// Get the number of maximum items in queue
        abstract capacity: float
        /// Get the number of current queued items
        abstract size: float

/// <summary>
///
/// Promise based multi producer single consumer channel
///
///
/// - buffered message queue
///
/// - <c>send</c> / <c>receive</c> basic message passing
///
/// - <c>pipe</c> piping to other channels (or use <c>pipe.to()</c>)
///
/// - <c>stream</c> ES6 async iterator api
///
/// - <c>freeze</c> temporarily block all consumers, useful if your target has limited rate of consumption like Node.js net.Socket
/// </summary>
/// <typeparam name="T">type of messages in queue</typeparam>
type [<AllowNullLiteral>] ChannelStatic =
    /// <summary>
    /// <code>
    /// class ClosedChannelError extends Error
    /// </code>
    /// </summary>
    abstract ClosedChannelError: obj with get, set
    /// <summary>
    /// <code>
    /// class ChannelFullError extends Error
    /// </code>
    /// </summary>
    abstract ChannelFullError: obj with get, set
    /// <summary>create a new channel with specified capacity</summary>
    /// <typeparam name="T">type of messages in queue</typeparam>
    /// <param name="capacity">channel capacity, defaults to <c>Infinity</c></param>
    /// <exception cref="">RangeError - capacity is negative or NaN</exception>
    [<EmitConstructor>] abstract Create: ?capacity: float -> Channel<'T>

/// <summary>compose multiple channels into one</summary>
type ChannelHub =
    ChannelHub<obj>

/// <summary>compose multiple channels into one</summary>
type [<AllowNullLiteral>] ChannelHub<'T> =
    inherit PipeTarget<'T>
    /// <summary>send a value to the hub, will be received by all readers</summary>
    /// <param name="value" />
    /// <exception cref=""><c>Channel.ClosedChannelError</c> - if ChannelHub is closed</exception>
    abstract broadcast: value: 'T -> unit
    /// <summary>
    /// get a reader channel that can get messages from channel hub
    ///
    /// use <see cref="ChannelHub.disconnect" /> if you don't want to receive messages from the hub
    /// </summary>
    /// <exception cref=""><c>Channel.ClosedChannelError</c> - if ChannelHub is closed</exception>
    abstract reader: unit -> Channel<'T>
    /// <summary>
    /// get a writer channel that can send messages to channel hub
    ///
    /// use <see cref="ChannelHub.disconnect" /> if you don't want to send messages to the hub
    /// </summary>
    /// <exception cref=""><c>Channel.ClosedChannelError</c> - if ChannelHub is closed</exception>
    abstract writer: unit -> Channel<'T>
    /// diconnect a channel from the hub, could be a reader or a writer
    ///
    /// disconnected channel will NOT be closed automatically,
    /// they can still be used to send and receive messages
    abstract disconnect: ch: Channel<'T> -> unit
    abstract ``[read]``: value: 'T -> unit
    /// close the hub and all readers/writers connected to it
    ///
    /// no-op if already closed
    abstract close: unit -> unit

/// <summary>compose multiple channels into one</summary>
type [<AllowNullLiteral>] ChannelHubStatic =
    /// a helper function, equivalant to ChannelHub.constructor
    abstract from: ?writers: ResizeArray<Channel<'T>> * ?readers: ResizeArray<Channel<'T>> -> ChannelHub<'T>
    [<EmitConstructor>] abstract Create: ?writers: ResizeArray<Channel<'T>> * ?readers: ResizeArray<Channel<'T>> -> ChannelHub<'T>

/// <summary>
/// a value created by <c>mutex.lock()</c>
///
/// calling the <c>guard()</c> or <c>guard.release()</c> will release the mutex and revoke <c>MutexGuard.value</c>
/// so that any subsequent access to the value will throw a TypeError
/// </summary>
type MutexGuard = obj

/// <summary>Asynchronos style mutex lock</summary>
/// <typeparam name="V">type of the object wrapped by the mutex, and a immutable T does not make sense</typeparam>
type Mutex =
    Mutex<unit>

/// <summary>Asynchronos style mutex lock</summary>
/// <typeparam name="V">type of the object wrapped by the mutex, and a immutable T does not make sense</typeparam>
type [<AllowNullLiteral>] Mutex<'V> =
    abstract _value: 'V with get, set
    /// <summary>acquire lock</summary>
    /// <returns><c>MutexGuard</c> - a function to release the lock, you can access wrapped value using <c>MutexGuard.value</c> before release</returns>
    /// <example>
    /// <code lang="typescript">
    /// const mutex = new Mutex({ a: 1 })
    /// const { release, value } = await mutex.lock()
    /// const ref = value
    /// ref.a // =&gt; 1
    /// release()
    /// ref.a // =&gt; TypeError, temporary reference destroyed
    /// </code>
    /// </example>
    abstract lock: ?timeout: float -> Promise<MutexGuard>
    /// <summary>synchronosly acquire lock</summary>
    /// <exception cref="">Error if failed to acquire lock</exception>
    /// <returns><c>MutexGuard</c> - a function to release the lock, you can access wrapped value using <c>MutexGuard.value</c> before release</returns>
    /// <example>
    /// <code lang="typescript">
    /// const mutex = new Mutex({ a: 1 })
    /// const { release, value } = mutex.tryLock()
    /// const ref = value // value is a temporary reference which does not equal the value stores in mutex
    /// ref.a // =&gt; 1
    /// release()
    /// ref.a // =&gt; TypeError, temporary reference destroyed
    /// </code>
    /// </example>
    abstract tryLock: unit -> MutexGuard
    /// check if mutex is available, returns true if it is not locked and frozen
    abstract canLock: bool
    /// Schedule a task to run when mutex is not locked.
    abstract schedule: fn: ('V -> 'T) -> Promise<Awaited<'T>>
    /// <summary>freeze the mutex lock, see <see cref="Semaphore.freeze" /></summary>
    abstract freeze: unit -> unit
    /// <summary>unfreeze the mutex lock, see <see cref="Semaphore.unfreeze" />}</summary>
    abstract unfreeze: unit -> Promise<unit>
    /// <summary>unfreeze the mutex lock, see <see cref="Semaphore.unfreeze" />}</summary>
    abstract frozen: bool

/// <summary>Asynchronos style mutex lock</summary>
/// <typeparam name="V">type of the object wrapped by the mutex, and a immutable T does not make sense</typeparam>
type [<AllowNullLiteral>] MutexStatic =
    /// <summary><see cref="Semaphore" /> with capacity of 1</summary>
    /// <param name="value">you may optionally wrap an object with mutex</param>
    [<EmitConstructor>] abstract Create: unit -> Mutex<'V>
    [<EmitConstructor>] abstract Create: value: 'V -> Mutex<'V>

/// <summary>Semaphore with async api</summary>
/// <param name="permits">number of permits</param>
/// <example>
/// const sem = new Semaphore(5)
/// const release = await sem.acquire()
/// // do something
/// release()
/// </example>
type [<AllowNullLiteral>] Semaphore =
    /// check if semaphore is frozen (non-undefined value), other uses are not guaranteed
    abstract frozen: Future<unit> option with get, set
    /// <summary>Acquire a permit, resolve when resouce is available.</summary>
    /// <returns>a function to release semaphore</returns>
    abstract acquire: ?timeout: float -> Promise<(unit -> unit)>
    /// <summary>Try to synchronosly acquire if there's remaining permits</summary>
    /// <returns>a function to release the semaphore</returns>
    /// <exception cref="">Error if semaphore is drained</exception>
    abstract tryAcquire: unit -> (unit -> unit)
    /// Schedule a task to run when a permit is available and automatically release after run.
    abstract schedule: fn: (unit -> 'T) -> Promise<Awaited<'T>>
    /// <summary>Give n permits to the semaphore, will immediately start this number of waiting tasks if not frozen</summary>
    /// <param name="permits">number of permits</param>
    /// <exception cref="">RangeError - if permits is less than 0</exception>
    abstract grant: ?permits: float -> unit
    /// <summary>
    /// Destroy n permits, effective until <c>remain</c> fills the n permits
    ///
    /// **note**: you may need to check if <c>permits &gt; semaphore.permits</c>, or it will wait until granted that many permits
    /// </summary>
    /// <param name="permits">number of permits</param>
    /// <exception cref="">RangeError - if permits is less than 0</exception>
    abstract revoke: ?permits: float -> Promise<unit>
    /// <summary>
    /// Freeze this semaphore, calling <c>acquire</c> won't resolve and <c>tryAcquire</c> will throw (release can still be called).
    ///
    /// NOTE: don't call this again if <see cref="Semaphore.frozen" />, not supported yet
    /// </summary>
    abstract freeze: unit -> unit
    /// <summary>unfreeze this semaphore, it is synchronos and the returned value should be ignored</summary>
    /// <returns>a promise that's already resolved you can add a</returns>
    abstract unfreeze: unit -> Promise<unit>
    /// Get the number of remaining permits
    abstract remain: float
    /// Get the number of total permits currently
    abstract permits: float
    /// <summary>
    /// Check if all permits are being used
    ///
    /// always return <c>true</c> if <c>permits = 0</c>
    /// </summary>
    abstract isFull: bool
    /// <summary>
    /// Check if no task is using the semaphore'
    ///
    /// always return <c>true</c> if <c>permits = 0</c>
    /// </summary>
    abstract isEmpty: bool

/// <summary>Semaphore with async api</summary>
/// <param name="permits">number of permits</param>
/// <example>
/// const sem = new Semaphore(5)
/// const release = await sem.acquire()
/// // do something
/// release()
/// </example>
type [<AllowNullLiteral>] SemaphoreStatic =
    /// <summary>constructs a new Semaphore with n permits</summary>
    /// <param name="permits">number of permits</param>
    [<EmitConstructor>] abstract Create: ?permits: float -> Semaphore

type [<AllowNullLiteral>] Delegated<'T> =
    interface end

/// <summary>Future is a resolve-later Promise, you can resolve it any time after a future is created.</summary>
/// <example>
/// <code lang="typescript">
/// const future = new Future&lt;number&gt;()
/// // somewhere
/// const count = await future
/// // elsewhere, and the future becomes `fullfilled`
/// future.resolve(count)
/// </code>
/// </example>
type Future =
    Future<obj>

/// <summary>Future is a resolve-later Promise, you can resolve it any time after a future is created.</summary>
/// <example>
/// <code lang="typescript">
/// const future = new Future&lt;number&gt;()
/// // somewhere
/// const count = await future
/// // elsewhere, and the future becomes `fullfilled`
/// future.resolve(count)
/// </code>
/// </example>
type [<AllowNullLiteral>] Future<'T> =
    inherit Promise<'T>
    abstract _resolve: (U2<'T, PromiseLike<'T>> -> unit) with get, set
    abstract _reject: (obj -> unit) with get, set
    abstract promiseState: Symbol with get, set
    abstract settledValue: U2<'T, obj> option with get, set
    /// <summary>
    /// resolve the future with given value
    ///
    /// tips: the method has already bound to <c>this</c>, so you can write <c>emitter.on('event', future.resolve)</c>
    /// </summary>
    abstract resolve: U2<'T, PromiseLike<'T>> -> unit
    /// <summary>
    /// reject the future with given value.
    ///
    /// the method has already bound to <c>this</c>, so you can write <c>emitter.on('error', future.reject)</c>
    /// </summary>
    abstract reject: (obj) option -> unit
    /// check if the promise is neither fulfilled nor rejected
    abstract pending: bool
    /// check if future has been fullfilled.
    abstract fulfilled: bool
    /// check if future has been rejected.
    abstract rejected: bool
    /// get the promise settled result, for debug purpose only.
    abstract settled: obj

/// <summary>Future is a resolve-later Promise, you can resolve it any time after a future is created.</summary>
/// <example>
/// <code lang="typescript">
/// const future = new Future&lt;number&gt;()
/// // somewhere
/// const count = await future
/// // elsewhere, and the future becomes `fullfilled`
/// future.resolve(count)
/// </code>
/// </example>
type [<AllowNullLiteral>] FutureStatic =
    abstract ``[Symbol.species]``: PromiseConstructor
    [<EmitConstructor>] abstract Create: unit -> Future<'T>

type ProgressInspectionResult<'Result, 'Progress> =
    U3<{| state: string; progress: 'Progress |}, {| state: string; value: 'Result |}, {| state: string; reason: obj |}>

/// <summary>Create a promise, but with progress reporting</summary>
/// <typeparam name="Result">type of the progress's fulfilled value</typeparam>
/// <typeparam name="CurrentProgress">type of current progress used in p.report or p.progress</typeparam>
type Progress =
    Progress<obj, obj>

/// <summary>Create a promise, but with progress reporting</summary>
/// <typeparam name="Result">type of the progress's fulfilled value</typeparam>
/// <typeparam name="CurrentProgress">type of current progress used in p.report or p.progress</typeparam>
type Progress<'Result> =
    Progress<'Result, obj>

/// <summary>Create a promise, but with progress reporting</summary>
/// <typeparam name="Result">type of the progress's fulfilled value</typeparam>
/// <typeparam name="CurrentProgress">type of current progress used in p.report or p.progress</typeparam>
type [<AllowNullLiteral>] Progress<'Result, 'CurrentProgress> =
    inherit Future<'Result>
    /// get last reported progress (despite current progress state)
    abstract progress: 'CurrentProgress
    /// inspect the current progress, for debug purpose only.
    abstract inspect: unit -> ProgressInspectionResult<'Result, 'CurrentProgress>
    /// register a listener on progress report, and use the returned function to cancel listening.
    ///
    /// listeners won't receive messages on progress rejection
    abstract onProgress: listener: (obj -> obj) -> (unit -> bool)
    /// report current progress
    ///
    /// no-op if progress has already fulfilled or rejected
    abstract report: progress: 'CurrentProgress -> unit

/// <summary>Create a promise, but with progress reporting</summary>
/// <typeparam name="Result">type of the progress's fulfilled value</typeparam>
/// <typeparam name="CurrentProgress">type of current progress used in p.report or p.progress</typeparam>
type [<AllowNullLiteral>] ProgressStatic =
    /// <summary>
    /// creates a new progress object and runs the given function with the progress as parameter,
    /// returns the created progress object.
    ///
    /// the function should report progress and call <c>progress.resolve</c> / <c>progress.reject</c> once done.
    /// </summary>
    /// <example>
    /// Progress.run((progress) =&gt; {
    ///   progress.report(100)
    ///   progress.resolve('hello')
    /// }, 0)
    /// </example>
    abstract run: fn: (Progress<'Result, 'CurrentProgress> -> obj) * initialProgress: 'CurrentProgress -> Progress<'Result, 'CurrentProgress>
    /// create a promise, but with progress reporting.
    [<EmitConstructor>] abstract Create: initialProgress: 'CurrentProgress -> Progress<'Result, 'CurrentProgress>

/// util.promisify only works on browser
type [<AllowNullLiteral>] SleepTimer =
    [<Emit("$0($1...)")>] abstract Invoke: ms: float -> Promise<unit>
    [<Emit("$0($1...)")>] abstract Invoke: ms: float * value: 'T -> Promise<'T>

/// <summary>a pipe source should be able to write its output to a pipe target</summary>
type [<AllowNullLiteral>] PipeSource<'T> =
    abstract pipe: (PipeTarget<'T> -> unit) with get, set
    abstract unpipe: (unit -> unit) with get, set

/// <summary>a pipe target can receive data from a pipe source</summary>
type PipeTarget<'T> =
    PipeTarget<'T, PipeSource<'T>>

/// <summary>a pipe target can receive data from a pipe source</summary>
type [<AllowNullLiteral>] PipeTarget<'T, 'S> =
    abstract ``[read]``: ('T -> ('S) option -> unit) with get, set

type [<AllowNullLiteral>] Pipe<'TIn, 'TOut> =
    inherit PipeTarget<'TIn>
    inherit PipeSource<'TOut>

type [<AllowNullLiteral>] Transform<'TIn, 'TOut> =
    inherit PipeTarget<'TIn, PipeSource<'TIn>>
    inherit PipeSource<'TOut>
    abstract handler: ('TIn -> (PipeSource<'TIn>) option -> 'TOut) with get, set
    abstract pipe: target: PipeTarget<'TOut> -> unit
    abstract unpipe: unit -> unit
    abstract ``[read]``: value: 'TIn * ?source: PipeSource<'TIn> -> unit

type [<AllowNullLiteral>] TransformStatic =
    /// <summary>creates a pipe that transforms data from <c>TIn</c> to <c>TOut</c></summary>
    /// <param name="handler">transform data in pipe</param>
    [<EmitConstructor>] abstract Create: handler: ('TIn -> 'TOut) -> Transform<'TIn, 'TOut>

type [<StringEnum>] [<RequireQualifiedAccess>] ConsoleLevel =
    | Debug
    | Log
    | Warn
    | Error

type Equal<'X, 'Y> =
    obj

type Not<'Cond> =
    obj

type NotEqual<'X, 'Y> =
    Not<Equal<'X, 'Y>>

type Assert<'Cond> =
    obj

type AssertEqual<'X, 'Y> =
    Assert<Equal<'X, 'Y>>

type AssertNotEqual<'X, 'Y> =
    Assert<NotEqual<'X, 'Y>>

type [<AllowNullLiteral>] F<'TArgs, 'TReturn> =
    [<Emit("$0($1...)")>] abstract Invoke: [<ParamArray>] args: 'TArgs -> 'TReturn

type Callable =
    F<ResizeArray<obj option>, obj option>


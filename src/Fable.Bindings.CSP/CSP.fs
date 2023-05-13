// ts2fable 0.9.0-build.725
module rec CSP

#nowarn "3390" // disable warnings for invalid XML comments

open System
open Fable.Core
open Fable.Core.JS

type Error = System.Exception

type [<AllowNullLiteral>] ChannelItem<'T> =
    abstract value: unit -> Promise<'T>
    abstract resolve: success: bool -> unit

type [<AllowNullLiteral>] IBuffer<'T> =
    inherit ILength
    inherit IRelease
    abstract isEmpty: unit -> bool
    abstract isFull: unit -> bool
    abstract drop: unit -> ChannelItem<'T> option
    abstract push: x: ChannelItem<'T> -> bool

type [<AllowNullLiteral>] IChannel<'T> =
    inherit IID<string>
    abstract channel: unit -> Channel<'T>
    abstract close: ?flush: bool -> Promise<unit> option

type [<AllowNullLiteral>] IReadableChannel<'T> =
    inherit IChannel<'T>
    abstract read: unit -> Promise<'T option>

type [<AllowNullLiteral>] IWriteableChannel<'T> =
    inherit IChannel<'T>
    abstract write: ``val``: obj option -> Promise<bool>

type [<AllowNullLiteral>] IReadWriteableChannel<'T> =
    inherit IReadableChannel<'T>
    inherit IWriteableChannel<'T>

type TopicFn<'T> =
    Fn<'T, string>

type [<AllowNullLiteral>] ErrorHandler =
    [<Emit("$0($1...)")>] abstract Invoke: e: Error * chan: Channel<obj option> * ?``val``: obj -> unit

type [<AllowNullLiteral>] FixedBuffer<'T> =
    inherit IBuffer<'T>
    abstract buf: DCons<ChannelItem<'T>> with get, set
    abstract limit: float with get, set
    abstract length: float
    abstract isEmpty: unit -> bool
    abstract isFull: unit -> bool
    abstract release: unit -> bool
    abstract push: x: ChannelItem<'T> -> bool
    abstract drop: unit -> ChannelItem<'T> option

type [<AllowNullLiteral>] FixedBufferStatic =
    [<EmitConstructor>] abstract Create: ?limit: float -> FixedBuffer<'T>

type [<AllowNullLiteral>] DroppingBuffer<'T> =
    inherit FixedBuffer<'T>
    abstract isFull: unit -> bool
    abstract push: x: ChannelItem<'T> -> bool

type [<AllowNullLiteral>] DroppingBufferStatic =
    [<EmitConstructor>] abstract Create: ?limit: float -> DroppingBuffer<'T>

type [<AllowNullLiteral>] SlidingBuffer<'T> =
    inherit FixedBuffer<'T>
    abstract isFull: unit -> bool
    abstract push: x: ChannelItem<'T> -> bool

type [<AllowNullLiteral>] SlidingBufferStatic =
    [<EmitConstructor>] abstract Create: ?limit: float -> SlidingBuffer<'T>


type [<RequireQualifiedAccess>] State =
    | OPEN = 0
    | CLOSED = 1
    | DONE = 2

type [<AllowNullLiteral>] Channel<'T> =
    inherit IReadWriteableChannel<'T>
    abstract id: string with get, set
    abstract onerror: ErrorHandler with get, set
    abstract state: State with get, set
    abstract buf: IBuffer<'T> with get, set
    abstract tx: Reducer<DCons<'T>, 'T> with get, set
    abstract writes: DCons<ChannelItem<'T>> with get, set
    abstract reads: DCons<('T -> unit)> with get, set
    abstract txbuf: DCons<'T> with get, set
    abstract isBusy: bool with get, set
    abstract channel: unit -> Channel<'T>
    abstract write: value: obj option -> Promise<bool>
    abstract read: unit -> Promise<'T option>
    abstract tryRead: ?timeout: float -> Promise<obj>
    abstract close: ?flush: bool -> Promise<unit> option
    abstract isClosed: unit -> bool
    abstract isReadable: unit -> bool
    abstract consume: ?fn: Fn<'T, obj option> -> Promise<unit>
    abstract produce: fn: Fn0<'T> * ?close: bool -> Promise<unit>
    abstract consumeWhileReadable: ?fn: Fn<'T, obj option> -> Promise<unit>
    abstract reduce: rfn: Reducer<'A, 'T> * ?acc: 'A -> Promise<'A>
    abstract transduce: tx: Transducer<'T, 'B> * rfn: Reducer<'A, 'B> * ?acc: 'A -> Promise<'A>
    abstract into: src: Iterable<'T> * ?close: bool -> Promise<unit>
    abstract pipe: dest: U2<Channel<'R>, Transducer<'T, 'R>> * ?close: bool -> Channel<'R>
    abstract split: pred: Predicate<'T> * ?truthy: Channel<'A> * ?falsey: Channel<'B> * ?close: bool -> ResizeArray<U2<Channel<'A>, Channel<'B>>>
    abstract concat: chans: Iterable<Channel<'T>> * ?close: bool -> Promise<unit>
    abstract release: unit -> unit
    abstract ``process``: unit -> Promise<unit>
    abstract flush: unit -> unit

type [<AllowNullLiteral>] ChannelStatic =
    abstract constantly: x: 'T * ?delay: float -> Channel<'T>
    abstract repeatedly: fn: Fn0<'T> * ?delay: float -> Channel<'T>
    abstract cycle: src: Iterable<'T> * ?delay: float -> Channel<obj>
    abstract range: unit -> Channel<float>
    abstract range: ``to``: float -> Channel<float>
    abstract range: from: float * ``to``: float -> Channel<float>
    abstract range: from: float * ``to``: float * step: float -> Channel<float>
    abstract range: from: float * ``to``: float * step: float * delay: float -> Channel<float>
    /// <summary>Constructs new channel which closes automatically after given period.</summary>
    /// <param name="delay">time in ms</param>
    abstract timeout: delay: float -> Channel<obj option>
    /// <summary>Shorthand for: <c>Channel.timeout(delay).take()</c></summary>
    /// <param name="delay">time in ms</param>
    abstract sleep: delay: float -> Promise<obj option>
    /// <summary>
    /// Creates new channel with single value from given promise, then closes
    /// automatically iff promise has been resolved.
    /// </summary>
    /// <param name="p">promise</param>
    abstract fromPromise: p: Promise<'T> -> Channel<'T>
    abstract from: src: Iterable<obj option> -> Channel<'T>
    abstract from: src: Iterable<obj option> * close: bool -> Channel<'T>
    abstract from: src: Iterable<obj option> * tx: Transducer<obj option, 'T> -> Channel<'T>
    abstract from: src: Iterable<obj option> * tx: Transducer<obj option, 'T> * close: bool -> Channel<'T>
    /// <summary>
    /// Takes an array of channels and blocks until any of them becomes
    /// readable (or has been closed). The returned promised resolves into
    /// an array of <c>[value, channel]</c>. Channel order is repeatedly
    /// shuffled for each read attempt.
    /// </summary>
    /// <param name="chans">source channels</param>
    abstract select: chans: ResizeArray<Channel<obj option>> -> Promise<obj option>
    /// <summary>
    /// Takes an array of channels to merge into new channel. Any closed
    /// channels will be automatically removed from the input selection.
    /// Once all inputs are closed, the target channel will close too (by
    /// default).
    /// </summary>
    /// <remarks>
    /// If <c>named</c> is true, the merged channel will have tuples of:
    /// <c>[src-id, val]</c> If false (default), only received values will be
    /// forwarded.
    /// </remarks>
    /// <param name="chans">source channels</param>
    /// <param name="out">result channel</param>
    /// <param name="close">true, if result closes</param>
    /// <param name="named">true, to emit labeled tuples</param>
    abstract merge: chans: ResizeArray<Channel<obj option>> * ?out: Channel<obj option> * ?close: bool * ?named: bool -> Channel<obj option>
    /// <summary>
    /// Takes an array of channels to merge into new channel of tuples.
    /// Whereas <c>Channel.merge()</c> realizes a sequential merging with no
    /// guarantees about ordering of the output.
    /// </summary>
    /// <remarks>
    /// The output channel of this function will collect values from all
    /// channels and a new tuple is emitted only once a new value has
    /// been read from ALL channels. Therefore the overall throughput is
    /// dictated by the slowest of the inputs.
    ///
    /// Once any of the inputs closes, the process is terminated and the
    /// output channel is closed too (by default).
    /// </remarks>
    /// <example>
    /// <code lang="ts">
    /// Channel.mergeTuples([
    ///   Channel.from([1, 2, 3]),
    ///   Channel.from([10, 20, 30]),
    ///   Channel.from([100, 200, 300])
    /// ]).consume();
    ///
    /// // chan-0 : [ 1, 10, 100 ]
    /// // chan-0 : [ 2, 20, 200 ]
    /// // chan-0 : [ 3, 30, 300 ]
    /// // chan-0 done
    ///
    /// Channel.mergeTuples([
    ///   Channel.from([1, 2, 3]),
    ///   Channel.from([10, 20, 30]),
    ///   Channel.from([100, 200, 300])
    /// ], null, false).consume();
    /// </code>
    /// </example>
    /// <param name="chans">source channels</param>
    /// <param name="out">result channel</param>
    /// <param name="closeOnFirst">true, if result closes when first input is done</param>
    /// <param name="closeOutput">true, if result closes when all inputs are done</param>
    abstract mergeTuples: chans: ResizeArray<Channel<obj option>> * ?out: Channel<obj option> * ?closeOnFirst: bool * ?closeOutput: bool -> Channel<obj option>
    abstract MAX_WRITES: float with get, set
    abstract NEXT_ID: float with get, set
    abstract SCHEDULE: Fn2<FnAny<unit>, float, unit> with get, set
    [<EmitConstructor>] abstract Create: unit -> Channel<'T>
    [<EmitConstructor>] abstract Create: id: string -> Channel<'T>
    [<EmitConstructor>] abstract Create: buf: U2<float, IBuffer<'T>> -> Channel<'T>
    [<EmitConstructor>] abstract Create: tx: Transducer<obj option, 'T> -> Channel<'T>
    [<EmitConstructor>] abstract Create: tx: Transducer<obj option, 'T> * err: ErrorHandler -> Channel<'T>
    [<EmitConstructor>] abstract Create: id: string * buf: U2<float, IBuffer<'T>> -> Channel<'T>
    [<EmitConstructor>] abstract Create: id: string * tx: Transducer<obj option, 'T> -> Channel<'T>
    [<EmitConstructor>] abstract Create: id: string * tx: Transducer<obj option, 'T> * err: ErrorHandler -> Channel<'T>
    [<EmitConstructor>] abstract Create: id: string * buf: U2<float, IBuffer<'T>> * tx: Transducer<obj option, 'T> -> Channel<'T>
    [<EmitConstructor>] abstract Create: id: string * buf: U2<float, IBuffer<'T>> * tx: Transducer<obj option, 'T> * err: ErrorHandler -> Channel<'T>

type [<AllowNullLiteral>] Mult<'T> =
    inherit IWriteableChannel<'T>
    abstract src: Channel<obj option> with get, set
    abstract taps: DCons<Channel<obj option>> with get, set
    abstract tapID: float with get, set
    abstract id: string with get, set
    abstract channel: unit -> Channel<obj option>
    abstract write: ``val``: obj option -> Promise<bool>
    abstract close: ?flush: bool -> Promise<unit> option
    abstract tap: ?ch: U2<Channel<'R>, Transducer<'T, 'R>> -> Channel<'R> option
    abstract untap: ch: Channel<obj option> -> bool
    abstract untapAll: ?close: bool -> bool
    abstract ``process``: unit -> Promise<unit>

type [<AllowNullLiteral>] MultStatic =
    abstract nextID: float with get, set
    [<EmitConstructor>] abstract Create: unit -> Mult<'T>
    [<EmitConstructor>] abstract Create: id: string -> Mult<'T>
    [<EmitConstructor>] abstract Create: src: Channel<'T> -> Mult<'T>
    [<EmitConstructor>] abstract Create: tx: Transducer<obj option, 'T> -> Mult<'T>
    [<EmitConstructor>] abstract Create: id: string * tx: Transducer<obj option, 'T> -> Mult<'T>

type [<AllowNullLiteral>] PubSub<'T> =
    inherit IWriteableChannel<'T>
    abstract src: Channel<'T> with get, set
    abstract fn: TopicFn<'T> with get, set
    abstract topics: IObjectOf<Mult<'T>> with get, set
    abstract id: string with get, set
    abstract channel: unit -> Channel<'T>
    abstract write: ``val``: obj option -> Promise<bool>
    abstract close: ?flush: bool -> Promise<unit> option
    /// <summary>
    /// Creates a new topic subscription channel and returns it.
    /// Each topic is managed by its own <see cref="Mult" /> and can have arbitrary
    /// number of subscribers. If the optional transducer is given, it will
    /// only be applied to the new subscription channel.
    ///
    /// The special "*" topic can be used to subscribe to all messages and
    /// acts as multiplexed pass-through of the source channel.
    /// </summary>
    /// <param name="id">topic id</param>
    /// <param name="tx">transducer for new subscription</param>
    abstract sub: id: string * ?tx: Transducer<'T, obj option> -> Channel<obj option> option
    abstract unsub: id: string * ch: Channel<'T> -> bool
    abstract unsubAll: id: string * ?close: bool -> bool
    abstract ``process``: unit -> Promise<unit>

type [<AllowNullLiteral>] PubSubStatic =
    abstract NEXT_ID: float with get, set
    [<EmitConstructor>] abstract Create: fn: TopicFn<'T> -> PubSub<'T>
    [<EmitConstructor>] abstract Create: src: Channel<'T> * fn: TopicFn<'T> -> PubSub<'T>

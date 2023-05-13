module Client.Hub

open Fable.Core
open Fable.Bindings.SignalR
open Thoth.Json

type MailBox<'a>() =
    let arg : 'a ResizeArray = ResizeArray()
    let cont = ResizeArray()

    member this.Post x =
        arg.Add(x)
        this.TryResolve()

    member this.Receive() =
        Promise.create (fun on_succ _ ->
            cont.Add on_succ
            this.TryResolve()
            )

    member this.Count = arg.Count

    member this.TryResolve() =
        let inline pop (x : ResizeArray<_>) =
            // Getting an array value from the beginning is not the most efficient way, but it won't be a problem.
            let i = 0
            let a = x[i]
            x.RemoveAt(i)
            a
        if cont.Count > 0 && arg.Count > 0 then
            let a = pop arg
            let f = pop cont

            // We don't want to just apply the function on the same thread otherwise it might lead to deadlocks.
            // Doing it like this will push it onto the system queue.
            promise { f a } |> Promise.start

type Hub<'msg_server_to_client, 'msg_client_to_server>(
        decode : string -> 'msg_server_to_client,
        encode : 'msg_client_to_server -> string,
        hub : HubConnection, is_switchhub) =
    let mutable dispatch = fun _ -> failwith $"Don't forget to set the dispatch in {nameof Hub}." : unit

    do hub.on("MailBox", fun x -> decode x |> dispatch)

    let invoke (msg : 'msg_client_to_server) = hub.invoke("MailBox",encode msg)
    let send (msg : 'msg_client_to_server) = hub.send("MailBox",encode msg)

    let mb = MailBox()
    do
        if is_switchhub then
            promise {
                while true do
                    let! msg = mb.Receive()
                    do! hub.start()
                    let rec loop msg = promise {
                        do! invoke msg
                        if mb.Count > 0 then
                            let! x = mb.Receive()
                            return! loop x
                    }
                    do! loop msg
                    do! hub.stop()
            } |> Promise.start
        else
            promise {
                do! hub.start()
                while true do
                    let! msg = mb.Receive()
                    do! send msg
            } |> Promise.start

    member _.Invoke x = mb.Post x
    member _.Send x =
        assert (is_switchhub = false)
        mb.Post x
    member _.SetDispatch x = dispatch <- x

let inline decode x =
    match Decode.Auto.fromString<_> x with
    | Ok x -> x
    | Error x -> failwith x
let inline encode x = Encode.Auto.toString<_> x

type Auto =
    static member inline SwitchHub<'msg_server_to_client, 'msg_client_to_server> hub : Hub<'msg_server_to_client, 'msg_client_to_server> = Hub(decode,encode,hub,true)
    static member inline PersistentHub<'msg_server_to_client, 'msg_client_to_server> hub : Hub<'msg_server_to_client, 'msg_client_to_server> = Hub(decode,encode,hub,false)
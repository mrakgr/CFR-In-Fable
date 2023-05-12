module Client.Hub

open Fable.Core
open Fable.Bindings.SignalR
open Thoth.Json

let create_mailbox<'t> f =
    let mb = new MailboxProcessor<'t>(f)
    mb.Start()
    mb

type Hub<'msg_server_to_client, 'msg_client_to_server>(
        decode : string -> 'msg_server_to_client,
        encode : 'msg_client_to_server -> string,
        hub : HubConnection, is_switchhub) =
    let mutable dispatch = fun _ -> failwith $"Don't forget to set the dispatch in {nameof Hub}." : unit

    do hub.on("MailBox", fun x -> decode x |> dispatch)

    let invoke (msg : 'msg_client_to_server) = hub.invoke("MailBox",encode msg)
    let send (msg : 'msg_client_to_server) = hub.send("MailBox",encode msg)

    let mb =
        if is_switchhub then
            create_mailbox <| fun mb -> async {
                while true do
                    let! msg = mb.Receive()
                    do! hub.start() |> Async.AwaitPromise
                    printfn "Started connection."
                    let rec loop msg = async {
                        do! invoke msg |> Async.AwaitPromise
                        printfn "Done invoking."
                        if mb.CurrentQueueLength > 0 then
                            let! x = mb.Receive()
                            return! loop x
                    }
                    do! loop msg
                    do! hub.stop() |> Async.AwaitPromise
                    printfn "Stopped connection."
            }
        else
            create_mailbox <| fun mb -> async {
                do! hub.start() |> Async.AwaitPromise
                while true do
                    let! msg = mb.Receive()
                    do! send msg |> Async.AwaitPromise
            }

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
module App

open System
open Elmish
open Elmish.React
open Elmish.Bridge

#if DEBUG
open Elmish.Debug
open Elmish.HMR
#endif

open Shared.Messages
open Shared.Constants
open Thoth.Json

module LearnHub =
    open Fable.Bindings.SignalR
    open Fable.Bindings.SignalR.Exports

    let hub =
        HubConnectionBuilder.Create()
            .withUrl(Browser.Url.URL.Create(Url.learn_hub, Url.learn_server).href)
            .configureLogging(LogLevel.Error)
            .build()

    let queue = ResizeArray()

    let send (msg : MsgClientToLearnServer) = hub.send("MailBox",Encode.Auto.toString msg) |> ignore

    let dispatch (msg : MsgClientToLearnServer) =
        match hub.state with
        | HubConnectionState.Connected -> send msg
        | _ -> queue.Add msg

open LearnHub

Program.mkProgram Index.init (Index.update dispatch) Index.view
|> Program.withSubscription (fun model ->
    let body (dispatch : Index.MsgClient -> unit) =
        promise {
            do! hub.start()
            printf "Started connection."
            hub.on("MailBox",fun (x : string) ->
                match Decode.Auto.fromString<_> x with
                | Ok x -> dispatch (Index.FromServer x)
                | Error x -> failwith x
                )
            for msg in queue do
                send msg
            queue.Clear()
        } |> Promise.start
        {new IDisposable with
            member _.Dispose() =
                promise {
                    do! hub.stop()
                    hub.off("MailBox")
                    printf "Closed connection."
                    dispatch Index.LearnConnectionClosed
                } |> Promise.start
            }
    let is_busy =
        model.cfr_players |> Map.exists (fun name pl ->
            pl.testing_iterations_left > 0u || pl.training_iterations_left > 0u
            )
    [
        if is_busy then
            [nameof is_busy], body
    ]
    )
|> Program.withBridgeConfig (
        Bridge.endpoint socket_endpoint
        |> Bridge.withMapping Index.FromServer
        )
|> Program.withReactSynchronous "app"
#if DEBUG
|> Program.withDebugger
#endif
|> Program.run


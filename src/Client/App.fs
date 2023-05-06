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
open Fable.SignalR

Program.mkProgram Index.init Index.update Index.view
|> Program.withSubscription (fun model ->
    let body msg msg_closed dispatch =
        let hub = SignalR.connect <| fun hub ->
            hub.withUrl($"{Url.learn_server}/{socket_endpoint}")
                .withAutomaticReconnect()
                .configureLogging(LogLevel.Debug)
                .onClose(fun ex ->
                    dispatch msg_closed
                    )
                .onMessage (Index.FromServer >> dispatch)

        hub.startNow()
        hub.sendNow msg
        {new IDisposable with
            member _.Dispose() = hub.stopNow()
            }
    [
        for KeyValue(name,pl) in model.cfr_players do
            if pl.training_iterations_left > 0 then
                let key = [ string name; "train"; string pl.training_iterations_left ]
                key, body (FromClient (MsgClientToServer.Train (pl.training_iterations_left, pl.training_model)))
                        (Index.ConnectionClosed(name,true))
            if pl.testing_iterations_left > 0 then
                let key = [ string name; "test"; string pl.testing_iterations_left ]
                key, body (FromClient (MsgClientToServer.Test (pl.testing_iterations_left, pl.testing_model)))
                        (Index.ConnectionClosed(name,false))
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
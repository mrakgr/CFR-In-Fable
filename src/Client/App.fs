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

Program.mkProgram Index.init Index.update Index.view
// |> Program.withSubscription (fun model ->
//     let body (msg : MsgServer) msg_closed dispatch =
//         let config =
//             Bridge.endpoint $"{Url.learn_server}/{socket_endpoint}"
//             |> Bridge.withName "learn" // it uses names to find the sockets under the hood
//             |> Bridge.withWhenDown msg_closed
//             |> Bridge.withMapping Index.FromServer
//
//         Bridge.asSubscription config dispatch
//         Bridge.NamedSend("learn",msg) // error
//
//         config :> IDisposable
//     [
//         for KeyValue(name,pl) in model.cfr_players do
//             if pl.training_iterations_left > 0 then
//                 let key = [ string name; "train" ]
//                 key, body (FromClient (MsgClientToServer.Train (pl.training_iterations_left, pl.training_model)))
//                         (Index.ConnectionClosed(name,true))
//             if pl.testing_iterations_left > 0 then
//                 let key = [ string name; "test" ]
//                 key, body (FromClient (MsgClientToServer.Test (pl.testing_iterations_left, pl.testing_model)))
//                         (Index.ConnectionClosed(name,false))
//     ]
//     )
|> Program.withBridgeConfig (
        Bridge.endpoint socket_endpoint
        |> Bridge.withMapping Index.FromServer
        )
|> Program.withReactSynchronous "app"
#if DEBUG
|> Program.withDebugger
#endif
|> Program.run
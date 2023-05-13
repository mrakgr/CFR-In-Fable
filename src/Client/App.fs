module App

open System
open Client.Hub
open Elmish
open Elmish.React
open Elmish.Bridge

#if DEBUG
open Elmish.Debug
open Elmish.HMR
#endif

open Shared.Messages
open Shared.Constants

[<AutoOpen>]
module LearnHub =
    open Fable.Bindings.SignalR

    let hub =
        HubConnectionBuilder.Create()
            .withUrl(Browser.Url.URL.Create(Url.learn_hub, Url.learn_server).href)
            .configureLogging(LogLevel.Error)
            .build()

    let switch_hub : Hub<MsgServerToClient,MsgClientToLearnServer> = Auto.SwitchHub hub

Program.mkProgram (Index.init switch_hub) (Index.update switch_hub) Index.view
|> Program.withBridgeConfig (
        Bridge.endpoint socket_endpoint
        |> Bridge.withMapping Index.FromServer
        )
|> Program.withReactSynchronous "app"
#if DEBUG
|> Program.withDebugger
#endif
|> Program.run
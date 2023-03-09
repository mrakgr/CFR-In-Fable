module Server

open Elmish
open Saturn
open Shared
open Elmish.Bridge
open Leduc.Types

type ServerModel = {
    action_cont : (Action -> unit) option
}

let init _ () : ServerModel * Cmd<_> = {action_cont=None}, []
let update dispact_client msg (model : ServerModel) : ServerModel * Cmd<_> =
    match msg with
    | FromLeducGame (Action(leducModel, l, allowedActions, actionFunc)) ->
        dispact_client (GameState(leducModel,l,allowedActions))
        {model with action_cont=Some actionFunc}, []
    | FromLeducGame (Terminal(a,b)) ->
        dispact_client (GameState(a,b,AllowedActions.Default))
        model, []
    | FromClient (SelectedAction action) ->
        Option.iter (fun f -> f action) model.action_cont
        {model with action_cont=None}, []
    | FromClient StartGame ->
        failwith "todo"

let server =
    Bridge.mkServer endpoint init update
    |> Bridge.run Giraffe.server

let webApp = server

let app =
    application {
        use_router webApp
        app_config Giraffe.useWebSockets
    }

[<EntryPoint>]
let main _ =
    run app
    0
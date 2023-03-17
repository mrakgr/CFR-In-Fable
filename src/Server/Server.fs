module Server

open Elmish
open Saturn
open Elmish.Bridge
open Shared.Leduc.Types
open Shared.Messages

type ServerModel = {
    action_cont : (Action -> unit) option
}

let init _ () : ServerModel * Cmd<_> = {action_cont=None}, []
let update dispact_client msg (model : ServerModel) : ServerModel * Cmd<_> =
    match msg with
    | FromLeducGame (Action(leduc_model, msgs, allowed_actions, cont)) ->
        dispact_client (GameState({leduc_model with p2_card=None},msgs,allowed_actions))
        {model with action_cont=Some cont}, []
    | FromLeducGame (Terminal(leduc_model, msgs)) ->
        dispact_client (GameState(leduc_model,msgs,AllowedActions.Default))
        model, []
    | FromClient (SelectedAction action) ->
        Option.iter (fun f -> f action) model.action_cont
        {model with action_cont=None}, []
    | FromClient (StartGame(p0,p1)) ->
        model, Cmd.ofEffect (fun dispatch -> Leduc.Play.game (FromLeducGame >> dispatch) (p0,p1))

let server =
    Bridge.mkServer Shared.Constants.endpoint init update
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
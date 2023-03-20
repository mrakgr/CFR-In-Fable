module Server

open System.Collections.Generic
open Elmish
open Leduc
open Saturn
open Elmish.Bridge
open Shared.Leduc.Types
open Shared.Messages

type ServerModel = {
    action_cont : (Action -> unit) option
    agent_cfr : Learn.LearningDictionary
}

let init _ () : ServerModel * Cmd<_> = {action_cont=None; agent_cfr=Dictionary()}, []
let update dispact_client msg (model : ServerModel) : ServerModel * Cmd<_> =
    match msg with
    | FromLeducGame (Action(leduc_model, msgs, allowed_actions, cont)) ->
        dispact_client (GameState({leduc_model with p1_card=None},msgs,allowed_actions))
        {model with action_cont=Some cont}, []
    | FromLeducGame (Terminal(leduc_model, msgs)) ->
        dispact_client (GameState(leduc_model,msgs,AllowedActions.Default))
        model, []
    | FromClient (SelectedAction action) ->
        Option.iter (fun f -> f action) model.action_cont
        {model with action_cont=None}, []
    | FromClient (Train num_iters) ->
        let d = model.agent_cfr
        try
            for i=1 to num_iters do
                Learn.game d |> TrainingResult |> dispact_client
        with e ->
            printfn $"%A{e}"
        d |> Seq.map (fun (KeyValue(k,v)) -> List.rev k, Array.zip v.actions (CFR.Learn.normalize v.unnormalized_policy_average))
        |> Map |> TrainingModel |> dispact_client
        model, []
    | FromClient (Test num_iters) ->
        let d = Dictionary(model.agent_cfr)
        try
            for i=1 to num_iters do
                Learn.test d |> TestingResult |> dispact_client
        with e ->
            printfn $"%A{e}"
        d |> Seq.map (fun (KeyValue(k,v)) -> List.rev k, Array.zip v.actions (CFR.Learn.normalize v.unnormalized_policy_average))
        |> Map |> TestingModel |> dispact_client
        model, []
    | FromClient (StartGame(p0,p1)) ->
        model, Cmd.ofEffect (fun dispatch -> Play.game model.agent_cfr (FromLeducGame >> dispatch) (p0,p1))

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
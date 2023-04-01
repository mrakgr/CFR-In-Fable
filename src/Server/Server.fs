module Server

open System.Collections.Generic
open Elmish
open Leduc
open Leduc.Play
open Saturn
open Elmish.Bridge
open Shared.Leduc.Types
open Shared.Messages

type ServerModel = {
    action_cont : (Action -> unit) option
    agent_cfr_enum : Learn.PolicyDictionary
    agent_cfr_mc : Learn.PolicyDictionary * Learn.ValueDictionary
}

let init _ () : ServerModel * Cmd<_> = {action_cont=None; agent_cfr_enum=Dictionary(); agent_cfr_mc=Dictionary(),Dictionary()}, []
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
    | FromClient (Train (num_iters, pl)) ->
        let inline train_template f =
            try
                for i=1 to num_iters do
                    f() |> TrainingResult |> dispact_client
            with e ->
                printfn $"%A{e}"
        let train_enum d = train_template (fun () -> Learn.train_enum d); d
        let train_mc (d,d') =
            train_template (fun () ->
                let inline f op (a,b) (a',b') = op a a', op b b'
                let iters = 500
                let mutable r = 0.0,0.0
                for i=1 to iters do
                    r <- f (+) r (Learn.train_mc d d')
                f (/) r (float iters, float iters)
                )
            d
        let d =
            match pl with
            | Enum -> train_enum model.agent_cfr_enum
            | MC -> train_mc model.agent_cfr_mc
        d |> Seq.map (fun (KeyValue(k,v)) -> List.rev k, Array.zip v.actions (CFR.Enumerative.normalize v.unnormalized_policy_average))
        |> Map |> TrainingModel |> dispact_client
        model, []
    | FromClient (Test (num_iters, pl)) ->
        let d =
            match pl with
            | Enum -> model.agent_cfr_enum
            | MC -> model.agent_cfr_mc |> fst
            |> Dictionary
        try
            for i=1 to num_iters do
                Learn.test d |> TestingResult |> dispact_client
        with e ->
            printfn $"%A{e}"
        d |> Seq.map (fun (KeyValue(k,v)) -> List.rev k, Array.zip v.actions (CFR.Enumerative.normalize v.unnormalized_policy_average))
        |> Map |> TestingModel |> dispact_client
        model, []
    | FromClient (StartGame(p0,p1)) ->
        model, Cmd.ofEffect (fun dispatch ->
            let dispatch = FromLeducGame >> dispatch
            let f = function
                | Human -> LeducActionHuman dispatch :> ILeducAction
                | Random -> LeducActionRandom()
                | CFR Enum -> LeducActionCFR(model.agent_cfr_enum)
                | CFR MC -> LeducActionCFR(model.agent_cfr_mc |> fst)
            game dispatch (f p0,f p1)
            )

let server =
    Bridge.mkServer Shared.Constants.endpoint init update
    |> Bridge.run Giraffe.server

let webApp = server

let app =
    application {
        use_router webApp
        use_static "public"
        app_config Giraffe.useWebSockets
        url "http://localhost:80"
    }

[<EntryPoint>]
let main _ =
    printfn "Name: %s; PID: %i" (System.Diagnostics.Process.GetCurrentProcess().ProcessName) (System.Diagnostics.Process.GetCurrentProcess().Id)
    run app
    0
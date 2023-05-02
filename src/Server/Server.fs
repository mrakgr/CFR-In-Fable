module Server

open System.Collections.Generic
open Elmish
open Leduc
open Leduc.Play
open Saturn
open Elmish.Bridge
open Shared.Leduc.Types
open Shared.Messages
open Shared.Utils

type ServerModel = {
    action_cont : (Action -> unit) option
}

let init _ () : ServerModel * Cmd<_> = {action_cont=None}, []
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
        let train_enum (d : Map<_,_>) =
            let d = Dictionary d
            train_template (fun () -> Learn.train_enum d)
            UI.Enum, UI.ModelEnum (to_map d)
        let train_mc (d : Map<_,_>,d' : Map<_,_>) =
            let d = Dictionary d
            let d' = Dictionary d'
            train_template (fun () ->
                let inline f op (a,b) (a',b') = op a a', op b b'
                let iters = 500
                let mutable r = 0.0,0.0
                for i=1 to iters do
                    r <- f (+) r (Learn.train_mc d d')
                f (/) r (float iters, float iters)
                )
            UI.MC, UI.ModelMC (to_map d, to_map d')
        match pl with
        | UI.ModelEnum x -> train_enum x
        | UI.ModelMC(a,b) -> train_mc (a, b)
        |> TrainingModel |> dispact_client
        model, []
    | FromClient (Test (num_iters, pl)) ->
        try
            let test_template d =
                for i=1 to num_iters do
                    Learn.test d |> TestingResult |> dispact_client
            let test_enum (d : Map<_,_>) =
                let d = Dictionary d
                test_template d
                UI.Enum, UI.ModelEnum (to_map d)
            let test_mc (d : Map<_,_>,d' : Map<_,_>) =
                let d = Dictionary d
                test_template d
                UI.MC, UI.ModelMC (to_map d, d')
            match pl with
            | UI.ModelEnum x -> test_enum x
            | UI.ModelMC(a,b) -> test_mc (a, b)
            |> TestingModel |> dispact_client
        with e ->
            printfn $"%A{e}"
        model, []
    | FromClient (StartGame(p0,p1)) ->
        model, Cmd.ofEffect (fun dispatch ->
            let dispatch = FromLeducGame >> dispatch
            let f = function
                | UI.PLM_Human -> LeducActionHuman dispatch :> ILeducAction
                | UI.PLM_Random -> LeducActionRandom()
                | UI.PLM_CFR (UI.ModelEnum x)
                | UI.PLM_CFR (UI.ModelMC(x,_)) -> LeducActionCFR(Dictionary x)
            game dispatch (f p0,f p1)
            )

let server =
    Bridge.mkServer Shared.Constants.endpoint init update
    |> Bridge.run Giraffe.server

let webApp = server

let app =
    application {
        use_router webApp
        app_config Giraffe.useWebSockets
        use_static "public"
        use_gzip
    }

[<EntryPoint>]
let main _ =
    printfn "Name: %s; PID: %i" (System.Diagnostics.Process.GetCurrentProcess().ProcessName) (System.Diagnostics.Process.GetCurrentProcess().Id)
    run app
    0
module Server

open System
open System.Collections.Generic
open System.Diagnostics
open System.IO
open System.Threading
open System.Threading.Tasks
open Elmish
open Leduc
open Leduc.Play
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Saturn
open Elmish.Bridge
open Shared.Leduc.Types
open Shared.Leduc.Types.UI
open Shared.Messages
open Shared.Utils
open Thoth.Json.Net

module Play =
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
        | _ -> model, []

    let webApp =
        Bridge.mkServer Shared.Constants.socket_endpoint init update
        |> Bridge.run Giraffe.server

module Learn =
    type ServerModel = unit

    let init _ () : ServerModel * Cmd<_> = (), []
    let update dispact_client msg (model : ServerModel) : ServerModel * Cmd<_> =
        match msg with
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
        | _ -> model, []

    let webApp =
        Bridge.mkServer Shared.Constants.socket_endpoint init update
        |> Bridge.run Giraffe.server

module TestSignalR =
    open Microsoft.AspNetCore.SignalR
    open Fable.Remoting

    type W = Qwe of float | Asd of bool
    type R = {x : string; y : W}

    open Thoth.Json

    type TestHub() =
        inherit Hub()

        member this.Hello(q : string) = task {
            printfn "Got: %A" (Decode.Auto.fromString<R> q)
            do! this.Clients.Caller.SendAsync("response", Encode.Auto.toString {x="I am fine."; y=Asd true})
        }

open Argu

type ServerType = Play | Learn | All | TestSignalR
type Arguments =
    | [<Unique>] Mode of ServerType

    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Mode _ -> "specify a server type."

[<EntryPoint>]
let main args =
    printfn "Name: %s; PID: %i" (Process.GetCurrentProcess().ProcessName) (System.Diagnostics.Process.GetCurrentProcess().Id)
    printfn "%A" args
    let parser = ArgumentParser.Create<Arguments>()
    let results = parser.Parse(args)
    let start_play () =
        application {
            use_router Play.webApp
            app_config Giraffe.useWebSockets
            use_static "public"
            url Shared.Constants.Url.play_server
        } |> run
    let start_learn () =
        application {
            use_router Learn.webApp
            app_config Giraffe.useWebSockets
            url Shared.Constants.Url.learn_server
        } |> run
    match results.GetResult(Mode, All) with
    | Play -> start_play()
    | Learn -> start_learn()
    | All ->
        for f in [start_play; start_learn] do
            Thread(ThreadStart(f)).Start()
    | TestSignalR -> // I've configured the dotnet run to start this mode for this video.
        let builder = WebApplication.CreateBuilder(args)
        builder.Services.AddSignalR() |> ignore
        builder.WebHost.UseUrls [|Shared.Constants.Url.play_server|] |> ignore
        let app = builder.Build()
        app.UseFileServer() |> ignore
        app.MapHub<TestSignalR.TestHub>("socket/test") |> ignore
        app.Run()


    0
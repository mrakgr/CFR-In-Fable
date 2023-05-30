namespace Lproj.UI

open System
open System.Collections.Generic
open System.Threading
open Leduc
open Microsoft.AspNetCore.Components
open Leduc.Play
open Lproj.Types

[<AbstractClass>]
type StatefulComponent<'TModel,'TAction>(initModel) =
    inherit ComponentBase()

    member val Model = initModel with get, set
    abstract member Update : msg: 'TAction -> 'TModel
    member this.Dispatch(msg) =
        this.Model <- this.Update msg
        this.StateHasChanged()

module Servers =

    type PlayServerModel =
        {
        action_cont : (Action -> unit) option
        }

        static member Init = {action_cont=None}

    type ViewComponent() as this =
        inherit StatefulComponent<ClientModel,MsgClient>(ClientModel.Default)

        let token_source = new CancellationTokenSource()
        let cancellation_token = token_source.Token

        let mb init update =
            let mb =
                new MailboxProcessor<_>(fun mb -> async {
                    let mutable model = init
                    while true do
                        let! msg = mb.Receive()
                        let! r = task { return update msg model mb.Post} |> Async.AwaitTask
                        model <- r
                    })
            mb.Start()
            mb

        let server_play =
            // TODO: Am I sure this part won't be null?
            let dispatch_client = FromServer >> this.Dispatch
            let update msg (model : PlayServerModel) dispatch : PlayServerModel =
                match msg with
                | FromLeducGame (Action(leduc_model, msgs, allowed_actions, cont)) ->
                    dispatch_client (GameState({leduc_model with p1_card=None},msgs,allowed_actions))
                    {model with action_cont=Some cont}
                | FromLeducGame (Terminal(leduc_model, msgs)) ->
                    dispatch_client (GameState(leduc_model,msgs,AllowedActions.Default))
                    model
                | FromClient (SrvSelectedAction action) ->
                    Option.iter (fun f -> f action) model.action_cont
                    {model with action_cont=None}
                | FromClient (SrvStartGame(p0,p1)) ->
                    let dispatch = FromLeducGame >> dispatch
                    let f = function
                        | PLM_Human -> LeducActionHuman dispatch :> ILeducAction
                        | PLM_Random -> LeducActionRandom()
                        | PLM_CFR (ModelEnum x)
                        | PLM_CFR (ModelMC(x,_)) -> LeducActionCFR(Dictionary x)
                    game dispatch (f p0,f p1)
                    model
                | FromClient(TestMessage s) ->
                    printfn "Got: %s" s
                    model

            mb PlayServerModel.Init update

        let server_learn =
            let dispatch_client = FromServer >> this.Dispatch
            let to_map d = d |> Seq.map (|KeyValue|) |> Map

            let update msg () dispatch : unit =
                match msg with
                | Train (num_iters, pl) ->
                    let train_template f =
                        /// This is day 4.
                        /// And hopefully the last one.
                        /// I've done research on Blazor multi threading,
                        /// and it is in prototype stage. There was some activity last year in .NET 7, but the
                        /// devs aren't working on it at, or maybe they are, but it is all under wraps.
                        ///
                        /// It is scheduled to be released in very late 2023, this year.
                        /// Right now we don't have access to it.
                        ///
                        /// This is a huge problem.
                        /// There isn't much we can do about it.
                        ///
                        /// Right now all these mailboxes are useless.
                        /// If we wait half a year once .NET 8 is out in full we'll have proper WASM multi threading,
                        /// but now things are different.
                        ///
                        /// We'll have to move to Blazor Server.
                        /// Right now the training works, but it is a really bad look to have the UI freeze while that is going on
                        /// in the background.
                        ///
                        /// ...
                        /// Sigh.
                        ///
                        /// For an application like this, I'd really have liked to push it all onto the client.
                        /// Since the training is so demanding, I am not a big fan of having the user waste my own server's
                        /// resources just to train an agent.
                        ///
                        /// This is one of the reasons, why I didn't feel like making a container app out of this.
                        /// This Leduc project feels like I am just LARPing as a webdev. I wish I could have skipped Fable and this
                        /// and gone straight into the VN compiler project.
                        ///
                        /// Let's get it over with so we can move on.
                        /// Now how do we switch projects to Blazor Server?
                        ///
                        /// Let's start by creating a template.
                        ///
                        /// <Project Sdk="Microsoft.NET.Sdk.Web">
                        ///
                        /// So the server version of Blazor is using regular web SDK.
                        ///
                        /// <Project Sdk="Microsoft.NET.Sdk.BlazorWebAssembly">
                        ///
                        /// The Blazor WASM on the other hand uses this.
                        /// <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly" Version="8.0.0-preview.3.23177.8" />
                        /// <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly.DevServer" Version="8.0.0-preview.3.23177.8" PrivateAssets="all" />
                        ///
                        /// It is also lacking any of the Blazor server specific packages that we'd expect.
                        ///
                        /// Let me pause here, I'll have to think a bit.
                        /// Isn't there Blazor Unified in .NET 8? Would it be possible to just use that instead of changing my entire project from WASM to Web?
                        /// ...
                        /// ...
                        ///
                        /// It is a prototype, forget it.
                        /// Let's just dig in and get cracking.
                        /// We have two choices right now...either to put the stuff in the Lproj.UI into EmptyBlazorApp1, or the other way around.
                        /// We'll do the former, as it will allow us to just copy the components straightforwardly.
                        ///
                        /// This is a good opportunity to learn some MVC, we hate it and never looked at it, but since we are aiming for .NET web
                        /// expertise, it is something we should get intimate with.
                        ///
                        /// Maybe in the future all this won't be necessary, but for now, we'll roll.
                        /// We'll go with vanilla.


                        let mutable num_iters = num_iters
                        while num_iters > 0u do
                            cancellation_token.ThrowIfCancellationRequested()
                            f() |> TrainingResult |> dispatch_client
                            num_iters <- num_iters-1u
                    let train_enum (d : Map<_,_>) =
                        let d = Dictionary d
                        train_template (fun () -> Enum, Learn.train_enum d)
                        Enum, ModelEnum (to_map d)
                    let train_mc (d : Map<_,_>,d' : Map<_,_>) =
                        let d = Dictionary d
                        let d' = Dictionary d'
                        train_template (fun () ->
                            let inline f op (a,b) (a',b') = op a a', op b b'
                            let iters = 500
                            let mutable r = 0.0,0.0
                            for i=1 to iters do
                                r <- f (+) r (Learn.train_mc d d')
                            MC, f (/) r (float iters, float iters)
                            )
                        MC, ModelMC (to_map d, to_map d')
                    match pl with
                    | ModelEnum x -> train_enum x
                    | ModelMC(a,b) -> train_mc (a, b)
                    |> TrainingModel |> dispatch_client
                | Test (num_iters, pl) ->
                    let test_template pl d =
                        let mutable num_iters = num_iters
                        while num_iters > 0u do
                            cancellation_token.ThrowIfCancellationRequested()
                            Learn.test d |> fun x -> TestingResult(pl,x) |> dispatch_client
                            num_iters <- num_iters-1u
                    let test_enum (d : Map<_,_>) =
                        let d = Dictionary d
                        test_template Enum d
                        Enum, ModelEnum (to_map d)
                    let test_mc (d : Map<_,_>,d' : Map<_,_>) =
                        let d = Dictionary d
                        test_template MC d
                        MC, ModelMC (to_map d, d')
                    match pl with
                    | ModelEnum x -> test_enum x
                    | ModelMC(a,b) -> test_mc (a, b)
                    |> TestingModel |> dispatch_client
            mb () update

        override this.Update(msg) =
            let model = this.Model

            let inline update' active_cfr_player f = // Inlining funs with closures often improves performance.
                let m = f model.cfr_players[active_cfr_player]
                {model with cfr_players=Map.add active_cfr_player m model.cfr_players}
            let inline update f = update' model.active_cfr_player f

            match msg with
            | ClickedOn x ->
                server_play.Post(FromClient (SrvSelectedAction x))
                {model with allowed_actions=AllowedActions.Default}
            | StartGame ->
                let get_model = function
                    | Human -> PLM_Human
                    | Random -> PLM_Random
                    | CFR x -> PLM_CFR model.cfr_players[x].training_model
                server_play.Post(FromClient (SrvStartGame(get_model model.p0, get_model model.p1)))
                model
            | PlayerChange(id, pl) ->
                let model =
                    match id with
                    | 0 -> {model with p0 = pl}
                    | 1 -> {model with p1 = pl}
                    | _ -> model
                model
            | TabClicked tab ->
                {model with active_tab=tab}
            | TrainingStartClicked ->
                update <| fun m ->
                    let iter = uint m.training_run_iterations
                    server_learn.Post (MsgClientToLearnServer.Train (iter, m.training_model))
                    {m with training_iterations_left=m.training_iterations_left + iter}
            | TestingStartClicked ->
                update <| fun m ->
                    let iter = uint m.testing_run_iterations
                    server_learn.Post (MsgClientToLearnServer.Test (iter, m.training_model))
                    {m with testing_iterations_left=m.testing_iterations_left+iter; testing_results=[]; testing_model=init_player_model model.active_cfr_player}
            | TrainingInputIterationsChanged s -> update <| fun m -> {m with training_run_iterations = s}
            | TestingInputIterationsChanged s -> update <| fun m -> {m with testing_run_iterations=s}
            | CFRPlayerSelected s -> {model with active_cfr_player=cfr_player_types[s]}
            | FromServer (GameState(leduc_model, message_list, allowed_actions)) ->
                {model with leduc_model=leduc_model; message_list=message_list; allowed_actions=allowed_actions}
            | FromServer (TrainingResult(pl,(a,b))) ->
                update' pl <| fun m ->
                    let mutable i = 150
                    let x = (m.training_iterations,(a,b)) :: m.training_results |> List.takeWhile (fun _ -> if i > 0 then i <- i-1; true else false)
                    {m with training_results=x; training_iterations_left=m.training_iterations_left-1u; training_iterations=m.training_iterations+1}
            | FromServer (TestingResult(pl,x)) ->
                update' pl <| fun m ->
                    {m with testing_results=x :: m.testing_results; testing_iterations_left=m.testing_iterations_left-1u}
            | FromServer (TrainingModel (a,x)) ->
                update' a <| fun m ->
                    {m with training_model=x}
            | FromServer (TestingModel (a,x)) ->
                update' a <| fun m ->
                    {m with testing_model=x}

        interface IDisposable with
            member _.Dispose() = token_source.Dispose(); server_learn.Dispose(); server_play.Dispose()
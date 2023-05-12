module Server

open System
open System.Collections.Generic
open System.Diagnostics
open System.Threading
open System.Threading.Tasks
open Elmish
open Leduc
open Leduc.Play
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Saturn
open Elmish.Bridge
open Shared.Leduc.Types
open Shared.Leduc.Types.UI
open Shared.Messages
open Shared.Utils
open Thoth.Json.Net

module HubUtils =
    open Microsoft.AspNetCore.SignalR

    type Dispatch<'msg> = 'msg -> unit
    let create_mailbox_loop dispatch init update =
        let mb = new MailboxProcessor<_>(fun mb -> async {
            let rec loop model = async {
                let! msg,(ch : AsyncReplyChannel<_>) = mb.Receive()
                printfn "Started Training."
                let model,cmds = update dispatch msg model
                for cmd in cmds do cmd dispatch
                ch.Reply()
                printfn "Finished Training."
                return! loop model
            }

            let init,cmds = init dispatch ()
            for cmd in cmds do cmd dispatch
            return! loop init
        })
        mb.Start()
        mb

    type ElmishHub<'model,'msg_server_to_client,'msg_client_to_server>(
            init : 'msg_server_to_client Dispatch -> unit -> 'model * ('msg_server_to_client Dispatch -> unit) list,
            update : 'msg_server_to_client Dispatch -> 'msg_client_to_server -> 'model -> 'model * ('msg_server_to_client Dispatch -> unit) list
            ) =
        inherit Hub()

        let dispatch (ctx : ISingleClientProxy) (x : 'msg_server_to_client) = ctx.SendAsync("MailBox", Encode.Auto.toString x) |> ignore
        member this.Mailbox(msg : string) = task {
            let mb = this.Context.Items["mb"] :?> MailboxProcessor<'msg_client_to_server * unit AsyncReplyChannel>
            let msg = Decode.Auto.fromString<'msg_client_to_server> msg
            do! mb.PostAndAsyncReply(fun ch -> msg.OkValue, ch)
        }

        override this.OnConnectedAsync() = task {
            printf "Started connection."
            this.Context.Items["mb"] <- create_mailbox_loop (dispatch this.Clients.Caller) init update
        }

        override this.OnDisconnectedAsync _ = task {
            printf "Closed connection."
            let mb = this.Context.Items["mb"] :?> IDisposable
            mb.Dispose()
        }

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
                    | PLM_Human -> LeducActionHuman dispatch :> ILeducAction
                    | PLM_Random -> LeducActionRandom()
                    | PLM_CFR (ModelEnum x)
                    | PLM_CFR (ModelMC(x,_)) -> LeducActionCFR(Dictionary x)
                game dispatch (f p0,f p1)
                )
    let webApp =
        Bridge.mkServer Shared.Constants.socket_endpoint init update
        |> Bridge.run Giraffe.server

module Learn =
    type ServerModel = unit

    let init _ () : ServerModel * Cmd<_> = (), []
    let update dispatch_client msg (model : ServerModel) : ServerModel * Cmd<_> =
        match msg with
        | Train (num_iters, pl) ->
            let train_template f =
                try
                    let mutable num_iters = num_iters
                    while num_iters > 0u do // TODO: Don't forget the cancellation token.
                        f() |> TrainingResult |> dispatch_client
                        num_iters <- num_iters-1u
                with e ->
                    printfn $"%A{e}"
            let train_enum (d : Map<_,_>) =
                let d = Dictionary d
                train_template (fun () -> Learn.train_enum d)
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
                    f (/) r (float iters, float iters)
                    )
                MC, ModelMC (to_map d, to_map d')
            match pl with
            | ModelEnum x -> train_enum x
            | ModelMC(a,b) -> train_mc (a, b)
            |> TrainingModel |> dispatch_client
            model, []
        | Test (num_iters, pl) ->
            try
                let test_template d =
                    let mutable num_iters = num_iters
                    while num_iters > 0u do // TODO: Don't forget the cancellation token.
                        Learn.test d |> TestingResult |> dispatch_client
                        num_iters <- num_iters-1u
                let test_enum (d : Map<_,_>) =
                    let d = Dictionary d
                    test_template d
                    Enum, ModelEnum (to_map d)
                let test_mc (d : Map<_,_>,d' : Map<_,_>) =
                    let d = Dictionary d
                    test_template d
                    MC, ModelMC (to_map d, d')
                match pl with
                | ModelEnum x -> test_enum x
                | ModelMC(a,b) -> test_mc (a, b)
                |> TestingModel |> dispatch_client
            with e ->
                printfn $"%A{e}"
            model, []

    type LearnHub() =
        inherit HubUtils.ElmishHub<ServerModel,MsgServerToClient,MsgClientToLearnServer>(init, update)



open Argu

type ServerType = Play | Learn | All
type Arguments =
    | [<Unique>] Mode of ServerType

    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Mode _ -> "specify a server type."

[<EntryPoint>]
let main args =
    printfn "Name: %s; PID: %i" (Process.GetCurrentProcess().ProcessName) (Process.GetCurrentProcess().Id)
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
        let builder = WebApplication.CreateBuilder(args)
        // builder.WebHost.ConfigureLogging(fun x ->
        //     x.AddFilter("Microsoft.AspNetCore.SignalR", LogLevel.Debug)
        //         .AddFilter("Microsoft.AspNetCore.Http.Connections", LogLevel.Debug) |> ignore
        //     ) |> ignore
        builder.Services
            .AddCors()
            .AddSignalR(fun x ->
                x.EnableDetailedErrors <- true
                x.MaximumReceiveMessageSize <- Nullable()
                ) |> ignore
        builder.WebHost.UseUrls [|Shared.Constants.Url.learn_server|] |> ignore
        let app = builder.Build()
        app.UseCors(fun x ->
            // x.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod() |> ignore
            x.WithOrigins("http://localhost:8080")
                .AllowAnyHeader()
                .WithMethods("GET", "POST")
                .AllowCredentials() |> ignore
            ) |> ignore
        app.MapHub<Learn.LearnHub>(Shared.Constants.Url.learn_hub) |> ignore
        app.Run()
    match results.GetResult(Mode, All) with
    | Play -> start_play()
    | Learn -> start_learn()
    | All ->
        for f in [start_play; start_learn] do
            Thread(ThreadStart(f)).Start()
    0
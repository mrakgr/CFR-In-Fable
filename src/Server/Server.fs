module Server

open Elmish
open Saturn
open Shared
open Elmish.Bridge
open Leduc.Types

type Model = {
    leduc_model : Server.Model
    message_list : string list
}

let hub = ServerHub()

let def_model =
    let def : Server.Model = { client = Client.def
                               cont = fun _ -> () }
    {leduc_model=def; message_list=["Starting the game."]}
let init dispact_client () : Model * Cmd<_> =
    dispact_client (MessageList def_model.message_list)
    dispact_client (LeducModel def_model.leduc_model.client)
    def_model, []
let update dispact_client msg (model : Model) : Model * Cmd<_> =
    let t = hub.AskClient dispact_client (fun x -> failwith "")
    match msg with
    | ClickedOn action ->
        model.leduc_model.cont action
        {model with message_list= $"Clicked on %A{action}" :: model.message_list}, []
    | StartGame -> {def_model with message_list=def_model.message_list @ model.message_list}, []
    | EndGame reward ->
        {model with
            message_list= $"The games is over. Player one's reward is %f{reward}" :: model.message_list
            leduc_model={model.leduc_model with cont=fun _ -> ()}
            }, []
    | GetAction x ->
        dispact_client (LeducModel x.client)
        {model with leduc_model=x}, []
    |> fun (a,b) ->
        dispact_client (MessageList a.message_list)
        a,b

let server =
    Bridge.mkServer endpoint init update
    |> Bridge.withServerHub hub
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
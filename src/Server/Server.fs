module Server

open Saturn

open Shared
open Elmish.Bridge

type Model = {
    message_list : string list
}

let init _ _ = {message_list=[]}, []
let update dispact_client msg model =
        match msg with
        | ConfirmYouGotIt x ->
            let messages = x :: model.message_list
            dispact_client (MessageList messages)
            {model with message_list=messages}, []

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
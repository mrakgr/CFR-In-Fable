module Index

open Elmish

type Model = { x : int }

type Msg =
    | TODO

let init () : Model * Cmd<Msg> =
    let model = { x = 1 }
    let cmd = []

    model, cmd

let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
    match msg with
    | TODO -> model, []

open Feliz

let view (model: Model) (dispatch: Msg -> unit) : ReactElement =
    Html.h1 "Hello, React!"
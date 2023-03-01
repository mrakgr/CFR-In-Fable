module Index

open Elmish
open Fable.Remoting.Client
open Shared

type Model = { Todos: Todo list; Input: string }

type Msg =
    | GotTodos of Todo list
    | SetInput of string
    | AddTodo
    | AddedTodo of Todo

let todosApi =
    Remoting.createApi ()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.buildProxy<ITodosApi>

let init () : Model * Cmd<Msg> =
    let model = { Todos = []; Input = "" }

    let cmd = Cmd.OfAsync.perform todosApi.getTodos () GotTodos

    model, cmd

let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
    match msg with
    | GotTodos todos -> { model with Todos = todos }, Cmd.none
    | SetInput value -> { model with Input = value }, Cmd.none
    | AddTodo ->
        let todo = Todo.create model.Input

        let cmd = Cmd.OfAsync.perform todosApi.addTodo todo AddedTodo

        { model with Input = "" }, cmd
    | AddedTodo todo -> { model with Todos = model.Todos @ [ todo ] }, Cmd.none

open Feliz

let view (model: Model) (dispatch: Msg -> unit) : ReactElement =
    let open_card (x : string) =
        Html.div [
            prop.className "open-card"
            prop.text x
        ]
    let padder (x : float) =
        Html.div [
            prop.className "action"
            prop.style [style.flexBasis (length.em x)]
        ]
    let action (x : string) =
        Html.button [
            prop.className "action"
            prop.text x
        ]
    Html.div [
        prop.className "ui"
        prop.children [
            Html.div [
                prop.className "top"
                prop.children [
                    open_card "K"
                ]
            ]
            Html.div [
                prop.className "middle"
                prop.children [
                    Html.strong "Pot: 4"
                    Html.strong "Total Pot: 6"
                    Html.strong "Pot: 2"
                ]
            ]
            Html.div [
                prop.className "bottom"
                prop.children [
                    Html.div [
                        prop.className "bottom-card"
                        prop.children [
                            open_card "J"
                        ]
                    ]
                    Html.div [
                        prop.className "bottom-actions"
                        prop.children [
                            padder 0
                            action "Fold"
                            action "Call"
                            action "Raise"
                            // padder 5
                            padder 0
                            padder 0
                        ]
                    ]
                ]
            ]
        ]
    ]
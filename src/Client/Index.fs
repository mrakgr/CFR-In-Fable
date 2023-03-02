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
    let card (x : string) =
        Html.div [
            prop.className "card"
            prop.children [
                Html.strong x
            ]
        ]
    let button (x : string) =
        Html.button [
            prop.className "action"
            prop.text x
        ]
    let padder_template (className : string)  (x : float) =
        Html.div [
            prop.className className
            prop.style [
                style.flexBasis (length.em x)
            ]
        ]

    let padder_middle = padder_template "middle-padder"
    let padder_action = padder_template "action-padder"

    Html.div [
        prop.className "game-ui"
        prop.children [
            Html.div [
                prop.className "top"
                prop.children [
                    card "K"
                ]
            ]
            Html.div [
                prop.className "middle"
                prop.children [
                    card "Q"
                    padder_middle 1
                    Html.div [
                        prop.className "pot-size"
                        prop.children [
                            Html.text "4"
                        ]
                    ]
                ]
            ]
            Html.div [
                prop.className "bottom"
                prop.children [
                    Html.div [
                        prop.className "bottom-left"
                        prop.children [
                            card "J"
                        ]
                    ]
                    Html.div [
                        prop.className "bottom-right"
                        prop.children [
                            button "Fold"
                            button "Call"
                            button "Raise"
                            // padder_action 3
                            padder_action 0
                        ]
                    ]
                ]
            ]
        ]
    ]
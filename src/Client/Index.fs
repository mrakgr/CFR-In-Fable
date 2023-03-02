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
                    Html.div [
                        prop.style [
                            style.flexBasis (length.em 1)
                        ]
                    ]
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
                    card "J"
                ]
            ]
        ]
    ]
module Index

open Elmish
open Shared
open Elmish.Bridge

type MsgClient =
    | ClickedOn of string
    | GotFromServer of MsgServerToClient

type Model = {
    message_list : string list
}

let init () = {message_list = []}, []

let update msg (model : Model)  =
    match msg with
    | ClickedOn x ->
        Bridge.Send(ConfirmYouGotIt x)
        model, []
    | GotFromServer (MessageList l) ->
        {model with message_list=l},[]

module View =
    open Feliz

    let game_ui dispatch =
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
                prop.onClick (fun _ -> dispatch (ClickedOn $"Clicked on %s{x}!"))
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
            prop.className "game-ui border"
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
                        ]
                        Html.div [
                            prop.className "bottom-middle"
                            prop.children [
                                card "J"
                            ]
                        ]
                        Html.div [
                            prop.className "bottom-right"
                            prop.children [
                                padder_action 3
                                button "Fold"
                                button "Call"
                                button "Raise"
                                // padder_action 3
                            ]
                        ]
                    ]
                ]
            ]
        ]

    let view (model: Model) dispatch : ReactElement =
        Html.div [
            prop.className "ui"
            prop.children [
                game_ui dispatch
                Html.div [
                    prop.className "message-ui border"
                    prop.children (model.message_list |> List.map Html.p)
                ]
            ]
        ]

let view = View.view
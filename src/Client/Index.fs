module Index

open Elmish
open Elmish.Bridge
open Shared.Messages
open Shared.Leduc.Types

type MsgClient =
    | ClickedOn of Action
    | StartGame
    | FromServer of MsgServerToClient

type ClientModel = {
    leduc_model : LeducModel
    message_list : string list
    allowed_actions : AllowedActions
}

let init () : ClientModel * Cmd<_> =
    { leduc_model = LeducModel.Default; message_list = []; allowed_actions = AllowedActions.Default}, []

let update msg (model : ClientModel) : ClientModel * Cmd<_>  =
    match msg with
    | ClickedOn x -> {model with allowed_actions=AllowedActions.Default}, Cmd.bridgeSend(FromClient (SelectedAction x))
    | StartGame -> model, Cmd.bridgeSend(FromClient MsgServerFromClient.StartGame)
    | FromServer (GameState(leduc_model, message_list, allowed_actions)) ->
        {model with leduc_model=leduc_model; message_list=message_list; allowed_actions=allowed_actions}, []

module View =
    open Feliz

    let game_ui dispatch (model : LeducModel, allowed_actions : AllowedActions) =
        let card (x : Card option) =
            let card =
                match x with
                | Some King -> "K"
                | Some Queen -> "Q"
                | Some Jack -> "J"
                | None -> " "
            Html.div [
                prop.className "card"
                prop.children [
                    Html.strong card
                ]
            ]
        let button (x : string) action =
            Html.button [
                prop.className "action"
                prop.text x
                prop.onClick (fun _ -> dispatch (ClickedOn action))
                prop.disabled (not (AllowedActions.IsAllowed allowed_actions action))
            ]

        let padder_template (className : string)  (x : float) =
            Html.div [
                prop.className className
                prop.style [
                    style.flexBasis (length.em x)
                ]
            ]

        let padder_middle = padder_template "middle-padder"
        // let padder_action = padder_template "action-padder"

        let pot (i : int) =
            Html.div [
                prop.className "pot-size"
                prop.text i
            ]

        let id (s : string) =
            Html.div [
                prop.className "id"
                prop.text s
            ]

        Html.div [
            prop.className "game-ui border"
            prop.children [
                Html.div [
                    prop.className "game-ui-background"
                    prop.children [
                        let el (id : string) =
                            Html.p [
                                prop.className (if id = Shared.Constants.names[0] then "bg-red" else "bg-blue")
                                prop.text id
                            ]
                        el model.p2_id
                        el model.p1_id
                    ]
                ]
                Html.div [
                    prop.className "top"
                    prop.children [
                        Html.div [
                            prop.className "top-left"
                            prop.children [
                                Html.div [
                                    prop.className "id-pot"
                                    prop.children [
                                        id model.p2_id
                                        pot model.p2_pot
                                    ]
                                ]
                            ]
                        ]
                        Html.div [
                            prop.className "top-middle"
                            prop.children [
                                card model.p2_card
                            ]
                        ]
                        Html.div [
                            prop.className "top-right"
                        ]
                    ]
                ]
                Html.div [
                    prop.className "middle"
                    prop.children [
                        card model.community_card
                        padder_middle 1
                        pot (model.p1_pot + model.p2_pot)
                    ]
                ]
                Html.div [
                    prop.className "bottom"
                    prop.children [
                        Html.div [
                            prop.className "bottom-left"
                            prop.children [
                                Html.div [
                                    prop.className "id-pot"
                                    prop.children [
                                        pot model.p1_pot
                                        id model.p1_id
                                    ]
                                ]
                            ]
                        ]
                        Html.div [
                            prop.className "bottom-middle"
                            prop.children [
                                card model.p1_card
                            ]
                        ]
                        Html.div [
                            prop.className "bottom-right"
                            prop.children [
                                button "Fold" Fold
                                button "Call" Call
                                button "Raise" Raise
                            ]
                        ]
                    ]
                ]
            ]
        ]


    let view (model: ClientModel) dispatch : ReactElement =
        Html.div [
            prop.className "ui"
            prop.children [
                Html.div [
                    prop.className "menu-game-ui"
                    prop.children [
                        Html.div [
                            prop.className "menu-ui border"
                            prop.children [
                                Html.button [
                                    prop.className "menu-button"
                                    prop.onClick (fun _ -> dispatch StartGame)
                                    prop.text "Start Game"
                                ]
                            ]
                        ]
                        game_ui dispatch (model.leduc_model, model.allowed_actions)
                    ]
                ]
                Html.div [
                    prop.className "message-ui border"
                    prop.children (model.message_list |> List.map Html.p)
                ]
            ]
        ]

let view = View.view
module Index

open Elmish
open Shared
open Elmish.Bridge
open Leduc.Types

type MsgClient =
    | ClickedOn of Action
    | GotFromServer of MsgServerToClient

type Model = {
    leduc : Client.Model
    message_list : string list
}

let init () : Model * Cmd<_> =
    { leduc = Client.def; message_list = [] }, Cmd.ofEffect (fun disp -> disp (GotFromServer (MessageList [])))

let update msg (model : Model) : Model * Cmd<unit>  =
    match msg with
    | ClickedOn x -> model, Cmd.bridgeSend(MsgServer.ClickedOn x)
    | GotFromServer (MessageList l) ->
        {model with message_list=l}, []
    | GotFromServer (LeducModel x) ->
        {model with leduc=x}, []

module View =
    open Feliz

    let game_ui dispatch (model : Client.Model) =
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
        let button (x : string) is_allowed action =
            Html.button [
                prop.className "action"
                prop.text x
                prop.onClick (fun _ -> dispatch (ClickedOn action))
                prop.disabled (not is_allowed)
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

        Html.div [
            prop.className "game-ui border"
            prop.children [
                Html.div [
                    prop.className "top"
                    prop.children [
                        Html.div [
                            prop.className "top-left"
                            prop.children [
                                Html.div [
                                    prop.className "pot-size"
                                    prop.children [
                                        Html.text model.p2_pot
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
                        Html.div [
                            prop.className "pot-size"
                            prop.children [
                                Html.text (model.p1_pot + model.p2_pot)
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
                                Html.div [
                                    prop.className "pot-size"
                                    prop.children [
                                        Html.text model.p1_pot
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
                                button "Fold" model.allow_fold Fold
                                button "Call" model.allow_call Call
                                button "Raise" model.allow_raise Raise
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
                game_ui dispatch model.leduc
                Html.div [
                    prop.className "message-ui border"
                    prop.children (model.message_list |> List.map Html.p)
                ]
            ]
        ]

let view = View.view
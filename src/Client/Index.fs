module Index

open Elmish
open Elmish.Bridge
open Shared.Messages
open Shared.Leduc.Types

type Tabs =
    | Game
    | Train

type MsgClient =
    | ClickedOn of act: Action
    | StartGame
    | PlayerChange of id: int * pl: PlayerType
    | TabClicked of tab: Tabs
    | FromServer of msg: MsgServerToClient

type ClientModel = {
    leduc_model : LeducModel
    message_list : string list
    allowed_actions : AllowedActions
    p0 : PlayerType
    p1 : PlayerType
    active_tab : Tabs
    training_data : float list
}

let init () : ClientModel * Cmd<_> =
    {
        leduc_model = LeducModel.Default
        message_list = []
        allowed_actions = AllowedActions.Default
        p0 = Human; p1 = Random
        active_tab = Game
        training_data = [
            let rng = System.Random()
            for _ = 1 to 100 do rng.NextDouble()
        ]
    }, []

let update msg (model : ClientModel) : ClientModel * Cmd<_>  =
    match msg with
    | ClickedOn x -> {model with allowed_actions=AllowedActions.Default}, Cmd.bridgeSend(FromClient (SelectedAction x))
    | StartGame -> model, Cmd.bridgeSend(FromClient (MsgServerFromClient.StartGame(model.p0, model.p1)))
    | PlayerChange(id, pl) ->
        let model =
            match id with
            | 0 -> {model with p0 = pl}
            | 1 -> {model with p1 = pl}
            | _ -> model
        model, []
    | TabClicked tab ->
        {model with active_tab=tab}, []
    | FromServer (GameState(leduc_model, message_list, allowed_actions)) ->
        {model with leduc_model=leduc_model; message_list=message_list; allowed_actions=allowed_actions}, []

module View =
    open Feliz

    let game_ui (model : LeducModel, allowed_actions : AllowedActions) dispatch =
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

        let id (s : int) =
            Html.div [
                prop.className "id"
                prop.text (Shared.Constants.names[s])
            ]

        Html.div [
            prop.className "game-ui border"
            prop.children [
                Html.div [
                    prop.className "game-ui-background"
                    prop.children [
                        let el (id : int) =
                            Html.p [
                                prop.className (if id = 0 then "bg-red" else "bg-blue")
                                prop.text (Shared.Constants.names[id])
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
                                        pot model.p1_pot
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
                        pot (model.p0_pot + model.p1_pot)
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
                                        pot model.p0_pot
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

    let menu_ui_select_children =
        let opt (x : string) =
            Html.option [
                prop.className "player-select-option"
                prop.value x
                prop.text x
            ]
        prop.children (Map.foldBack (fun k _ s -> opt k :: s) player_types [])

    let menu_ui (model: ClientModel) dispatch =
        Html.div [
            prop.className "menu-ui border"
            prop.children [
                let select id (def : PlayerType) =
                    Html.select [
                        prop.className "player-select"
                        menu_ui_select_children
                        prop.value (def.ToString())
                        prop.onChange (fun x ->  dispatch (PlayerChange(id,player_types[x])))
                    ]
                select 1 model.p1
                Html.button [
                    prop.className "menu-button"
                    prop.onClick (fun _ -> dispatch StartGame)
                    prop.text "Start Game"
                ]
                select 0 model.p0
            ]
        ]

    let menu_game_ui (model: ClientModel) (dispatch : MsgClient -> unit) : ReactElement =
        Html.div [
            prop.className "menu-game-ui"
            prop.children [
                menu_ui model dispatch
                game_ui (model.leduc_model, model.allowed_actions) dispatch
            ]
        ]

    let message_ui (model: ClientModel) : ReactElement =
        Html.div [
            prop.className "message-ui border"
            prop.children (model.message_list |> List.map Html.p)
        ]

    let tabs_ui active_tab dispatch =
        let tab x =
            Html.button [
                prop.classes ["tab"; if active_tab = x then "active-tab" else "inactive-tab"]
                prop.text (x.ToString())
                prop.onClick (fun _ -> dispatch (TabClicked x))
            ]
        Html.div [
            prop.className "tabs-ui border"
            prop.children [
                tab Game
                tab Train
            ]
        ]

    open Feliz.Recharts

    [<ReactComponent>]
    let SimpleLineChart(data : float list) =
        Recharts.responsiveContainer [
            responsiveContainer.width (length.perc 100)
            responsiveContainer.height (length.perc 100)
            Recharts.lineChart [
                lineChart.data data
                lineChart.margin(top=5, right=30)
                lineChart.children [
                    Recharts.cartesianGrid [ cartesianGrid.strokeDasharray(3, 3) ]
                    Recharts.xAxis [
                        Interop.mkXAxisAttr "label" {|value="Iteration"; position="insideBottomRight"; offset= -10|}
                    ]
                    Recharts.yAxis [ Interop.mkYAxisAttr "label" {|value="Reward"; angle= -90; position="insideLeft"; offset= 20 |} ]
                    Recharts.tooltip [ ]
                    Recharts.legend [ ]
                    Recharts.line [
                        line.monotone
                        line.dataKey (id : float -> _)
                        line.stroke "#101010"
                        line.strokeWidth 2
                    ]
                ]
            ] |> responsiveContainer.chart
        ]

    let train_ui (model: ClientModel) (dispatch : MsgClient -> unit) : ReactElement =
        Html.div [
            prop.className "train-ui border"
            prop.children [
                SimpleLineChart(model.training_data)
            ]
        ]

    let main (model: ClientModel) (dispatch : MsgClient -> unit) : ReactElement =
        Html.div [
            prop.className "ui"
            prop.children [
                tabs_ui model.active_tab dispatch
                match model.active_tab with
                | Game ->
                    menu_game_ui model dispatch
                    message_ui model
                | Train ->
                    train_ui model dispatch
            ]
        ]

let view = View.main
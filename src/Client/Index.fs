module Index

open System
open Elmish
open Elmish.Bridge
open Shared.Messages
open Shared.Leduc.Types
open Shared.Leduc.Types.UI

type Tabs =
    | Game
    | Train
    | Test

type MsgClient =
    | ClickedOn of act: Action
    | StartGame
    | PlayerChange of id: int * pl: PlayerType
    | TabClicked of tab: Tabs
    | CFRPlayerSelected of string
    | TrainingStartClicked
    | TestingStartClicked
    | TrainingInputIterationsChanged of string
    | TestingInputIterationsChanged of string
    | LearnConnectionClosed
    | FromServer of msg: MsgServerToClient

type ClientModel = {
    leduc_model : LeducModel
    message_list : string list
    allowed_actions : AllowedActions
    p0 : PlayerType
    p1 : PlayerType
    active_tab : Tabs
    active_cfr_player : CFRPlayerType
    cfr_players : Map<CFRPlayerType,UICFRPlayerModel>
}

let init_player_model = function
    | Enum -> ModelEnum Map.empty
    | MC -> ModelMC (Map.empty, Map.empty)

let init () : ClientModel * Cmd<_> =
    {
        leduc_model = LeducModel.Default
        message_list = []
        allowed_actions = AllowedActions.Default
        p0 = Human; p1 = CFR MC
        active_tab = Train
        active_cfr_player = MC
        cfr_players =
            [
                for pl_type in cfr_player_types' do
                    let model = init_player_model pl_type
                    pl_type, {
                        training_model = model
                        testing_model = model
                        training_iterations = 0
                        training_iterations_left = 0u
                        training_results = []
                        training_run_iterations = "50"
                        testing_iterations_left = 0u
                        testing_results = []
                        testing_run_iterations = "100"
                        }
            ] |> Map
    }, []

let update dispatch_learn msg (model : ClientModel) : ClientModel * Cmd<_> =
    try
        let inline update' active_cfr_player f = // Inlining funs with closures often improves performance.
            let m,cmd = f model.cfr_players[active_cfr_player]
            {model with cfr_players=Map.add model.active_cfr_player m model.cfr_players}, cmd
        let inline update f = update' model.active_cfr_player f
        match msg with
        | ClickedOn x -> {model with allowed_actions=AllowedActions.Default}, Cmd.bridgeSend(FromClient (SelectedAction x))
        | StartGame ->
            let get_model = function
                | Human -> PLM_Human
                | Random -> PLM_Random
                | CFR x -> PLM_CFR model.cfr_players[x].training_model
            model, Cmd.bridgeSend(FromClient (MsgClientToPlayServer.StartGame(get_model model.p0, get_model model.p1)))
        | PlayerChange(id, pl) ->
            let model =
                match id with
                | 0 -> {model with p0 = pl}
                | 1 -> {model with p1 = pl}
                | _ -> model
            model, []
        | TabClicked tab ->
            {model with active_tab=tab}, []
        | TrainingStartClicked ->
            update <| fun m ->
                let iter = uint m.training_run_iterations
                dispatch_learn (MsgClientToLearnServer.Train (iter, m.training_model))
                {m with training_iterations_left=m.training_iterations_left + iter}, []
        | TestingStartClicked ->
            update <| fun m ->
                let iter = uint m.testing_run_iterations
                dispatch_learn (MsgClientToLearnServer.Test (iter, m.training_model))
                {m with testing_iterations_left=m.testing_iterations_left+iter; testing_results=[]; testing_model=init_player_model model.active_cfr_player}, []
        | TrainingInputIterationsChanged s -> update <| fun m -> {m with training_run_iterations = s}, []
        | TestingInputIterationsChanged s -> update <| fun m -> {m with testing_run_iterations=s}, []
        | CFRPlayerSelected s -> {model with active_cfr_player=cfr_player_types[s]}, []
        | LearnConnectionClosed ->
            let f m = {m with testing_iterations_left=0u; training_iterations_left=0u}
            {model with cfr_players=Map.map (fun _ -> f) model.cfr_players}, []
        | FromServer (GameState(leduc_model, message_list, allowed_actions)) ->
            {model with leduc_model=leduc_model; message_list=message_list; allowed_actions=allowed_actions}, []
        | FromServer (TrainingResult(a,b)) ->
            update <| fun m ->
                let mutable i = 150
                let x = (m.training_iterations,(a,b)) :: m.training_results |> List.takeWhile (fun _ -> if i > 0 then i <- i-1; true else false)
                {m with training_results=x; training_iterations_left=m.training_iterations_left-1u; training_iterations=m.training_iterations+1}, []
        | FromServer (TrainingModel (a,x)) ->
            update' a <| fun m ->
                {m with training_model=x}, []
        | FromServer (TestingResult x) ->
            update <| fun m ->
                {m with testing_results=x :: m.testing_results; testing_iterations_left=m.testing_iterations_left-1u}, []
        | FromServer (TestingModel (a,x)) ->
            update' a <| fun m ->
                {m with testing_model=x}, []
    with e ->
        printfn $"%A{e}"
        model, []

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
                        el model.p1_id
                        el model.p0_id
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
                                        id model.p1_id
                                        pot model.p1_pot
                                    ]
                                ]
                            ]
                        ]
                        Html.div [
                            prop.className "top-middle"
                            prop.children [
                                card model.p1_card
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
                                        id model.p0_id
                                    ]
                                ]
                            ]
                        ]
                        Html.div [
                            prop.className "bottom-middle"
                            prop.children [
                                card model.p0_card
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
                tab Test
            ]
        ]

    open Feliz.Recharts

    let chart_template lines xAxisDataKey (data : _ list) =
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
                        match xAxisDataKey with
                        | Some x -> x : IXAxisProperty
                        | _ -> ()
                    ]
                    Recharts.yAxis [ Interop.mkYAxisAttr "label" {|value="Reward"; angle= -90; position="insideLeft"; offset= 20 |} ]
                    Recharts.tooltip [ ]
                    Recharts.legend [ ]
                    yield! lines
                ]
            ] |> responsiveContainer.chart
        ]

    module TrainChart =
        let lines = [
            Recharts.line [
                line.monotone
                line.dataKey (snd >> fst : _ -> float)
                line.stroke "#00ff00"
                line.strokeWidth 2
                line.name "Player 0"
            ]
            Recharts.line [
                line.monotone
                line.dataKey (snd >> snd : _ -> float)
                line.stroke "#0000ff"
                line.strokeWidth 2
                line.name "Player 1"
            ]
        ]
        let xAxisDataKey = Some (xAxis.dataKey (fst : _ -> int))

        [<ReactComponent>]
        let view (data : (int * (float * float)) list) = chart_template lines xAxisDataKey data

    module TestChart =
        let lines = [
            Recharts.line [
                line.monotone
                line.dataKey (id : float -> _)
                line.stroke "#00ff00"
                line.strokeWidth 2
                line.name "Player 0"
            ]
        ]

        [<ReactComponent>]
        let view (data : float list) = chart_template lines None data

    [<ReactMemoComponent>]
    let table is_train (model : CFRPlayerModel) =
        let model =
            match model with
            | ModelEnum x | ModelMC(x,_) -> x
        Html.div [
            prop.className "train-table border"
            prop.children [
                Html.h1 "Game Sequence"
                Html.h1 "Action(Probability)"
                for KeyValue(k,v) in model do
                    let model_action act =
                        let c =
                            match act with
                            | Fold -> "train-action-fold"
                            | Call -> "train-action-call"
                            | Raise -> "train-action-raise"
                        Html.div [
                            prop.className c
                            prop.text (act.ToString())
                        ]
                    Html.div [
                        prop.className "train-sequences"
                        prop.children [
                            for k in k do
                                match k with
                                | Choice1Of2 act ->
                                    model_action act
                                | Choice2Of2 card ->
                                    Html.div [
                                        prop.className "train-card"
                                        prop.text (card.ToString())
                                    ]
                        ]
                    ]
                    Html.div [
                        prop.className "train-action-probs"
                        prop.children (
                            let act = v.actions
                            let prob = Shared.Utils.CFR.normalize (if is_train then v.current_regrets else v.unnormalized_policy_average)
                            (act,prob) ||> Array.map2 (fun act prob ->
                                Html.div [
                                    prop.className "train-action-probs-item"
                                    prop.children [
                                        model_action act
                                        Html.div [
                                            prop.className "train-probability"
                                            let prob = (prob*100.0).ToString("0.##")
                                            prop.text $"(%s{prob})"
                                        ]
                                    ]
                                ])
                        )
                    ]
            ]
        ]

    [<ReactComponent>]
    let training_ui_menu (cfr_select_value : string, training_run_iterations : string, training_iterations_left : uint, msg) (select_on_change : string -> _) start (iters_changed : string -> _) =
        Html.div [
            prop.className "train-ui-menu"
            prop.children [
                let is_input_valid, _ = UInt32.TryParse(training_run_iterations)
                Html.button [
                    prop.className "train-start-button"
                    if 0u < training_iterations_left then
                        prop.text $"%s{msg} In Progress: %i{training_iterations_left} left..."
                    else
                        prop.text $"Click To Begin %s{msg}"
                    prop.onClick start
                    prop.disabled (not is_input_valid)
                ]
                Html.select [
                    prop.className "train-cfr-player-type"
                    prop.children (Map.keys cfr_player_types |> Seq.map Html.option)
                    prop.value cfr_select_value
                    prop.onChange select_on_change
                ]
                Html.div [
                    prop.className "train-label"
                    prop.children [
                        Html.pre "Number Of Iterations:  "
                        Html.input [
                            prop.className "train-input"
                            prop.value training_run_iterations
                            if not is_input_valid then
                                prop.style [style.color "red"]
                            prop.onChange iters_changed
                        ]
                    ]
                ]
            ]
        ]

    let train_ui (model: ClientModel) (dispatch : MsgClient -> unit) : ReactElement =
        Html.div [
            prop.className "train-ui border"
            prop.children [
                let m = model.cfr_players[model.active_cfr_player]
                training_ui_menu (string model.active_cfr_player, m.training_run_iterations, m.training_iterations_left, "Training")
                    (CFRPlayerSelected >> dispatch)
                    (fun _ -> dispatch TrainingStartClicked)
                    (TrainingInputIterationsChanged >> dispatch)
                Html.div [
                    prop.className "train-chart border"
                    prop.children [
                        TrainChart.view (List.rev m.training_results)
                    ]
                ]
                table true m.training_model
            ]
        ]

    let test_ui (model: ClientModel) (dispatch : MsgClient -> unit) : ReactElement =
        Html.div [
            prop.className "train-ui border"
            prop.children [
                let m = model.cfr_players[model.active_cfr_player]
                training_ui_menu (string model.active_cfr_player, m.testing_run_iterations, m.testing_iterations_left, "Testing")
                    (CFRPlayerSelected >> dispatch)
                    (fun _ -> dispatch TestingStartClicked)
                    (TestingInputIterationsChanged >> dispatch)
                Html.div [
                    prop.className "train-chart border"
                    prop.children [
                        TestChart.view (List.rev m.testing_results)
                    ]
                ]
                table false m.testing_model
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
                | Test ->
                    test_ui model dispatch
            ]
        ]

let view model dispatch = View.main model dispatch // Note: Since Fable 4, this function musn't be partially applied. Don't reduce the args.
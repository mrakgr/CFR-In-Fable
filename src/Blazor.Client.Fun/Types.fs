﻿namespace Blazor.Client

open System
open System.Threading

module Fun =
    open Microsoft.AspNetCore.Components

    [<AbstractClass>]
    type StatefulComponent<'TModel,'TAction>(initModel) =
        inherit ComponentBase()

        member val Model = initModel with get, set
        abstract member Update : msg: 'TAction -> 'TModel
        member this.Dispatch(msg) =
            this.Model <- this.Update msg
            this.StateHasChanged()

            // Let's give it a try.


    type Action = Fold | Call | Raise

    // Awkward, but this is the best choice.
    let css_train_action = function
        | Fold -> "train-action-fold"
        | Call -> "train-action-call"
        | Raise -> "train-action-raise"

    type Card = King | Queen | Jack
    type GameModel = Choice<Action,Card> list
    type Player = { card : Card; id : int; pot : int }
    type Mask = uint64

    type ValueArrays = (struct (float * float )) []
    type PolicyArrays<'action> = {current_regrets : float[]; unnormalized_policy_average : float[]; actions : 'action []}
    type PolicyDictionary<'game_model,'action> = System.Collections.Generic.Dictionary<'game_model, PolicyArrays<'action>>
    type ValueDictionary<'game_model> = System.Collections.Generic.Dictionary<'game_model, ValueArrays>

    let normalize (x : float []) =
        let s = Array.sum x
        if s = 0.0 then Array.replicate x.Length (1.0 / float x.Length)
        else Array.map (fun x -> x / s) x

    type Tabs = Game | Train | Test
    type CFRPlayerType = Enum | MC
    type [<ReferenceEquality>] CFRPlayerModel =
        | ModelEnum of Map<GameModel,PolicyArrays<Action>>
        | ModelMC of Map<GameModel,PolicyArrays<Action>> * Map<GameModel,ValueArrays>

        // It is much easier to extract the field on the F# side.
        member this.GetPolicyMap() =
            match this with
            | ModelEnum x | ModelMC(x,_) -> x

    type PlayerType = Human | Random | CFR of CFRPlayerType
    type PlayerModel =
        | PLM_Human
        | PLM_Random
        | PLM_CFR of CFRPlayerModel

    type LeducModel =
        {
            p0_id: int
            p0_card: Card option
            p0_pot: int
            p1_id: int
            p1_card: Card option
            p1_pot: int
            community_card : Card option
        }

        static member Default = {
            p0_id = 0
            p0_card = None
            p0_pot = 0
            p1_id = 1
            p1_card = None
            p1_pot = 0
            community_card = None
        }

    type UICFRPlayerModel = {
        training_model : CFRPlayerModel
        testing_model : CFRPlayerModel
        training_run_iterations : string
        training_iterations : int
        training_iterations_left : uint
        training_results : (int * (float * float)) list
        testing_run_iterations : string
        testing_iterations_left : uint
        testing_results : float list
    }

    // Note that the record must not have regular members otherwise it won't be serializable.
    type AllowedActions = { is_fold : bool; is_call : bool; is_raise : bool } with
        static member IsAllowed (q, a) = match a with Fold -> q.is_fold | Call -> q.is_call | Raise -> q.is_raise
        static member Mask q =
            let f b i m = if b then m else m ||| (1UL <<< i)
            0UL |> f q.is_fold 0 |> f q.is_call 1 |> f q.is_raise 2
        static member Array q = [|
            if q.is_fold then Fold
            if q.is_call then Call
            if q.is_raise then Raise
        |]
        static member Default = { is_fold = false; is_call = false; is_raise = false }
        static member FromData(p0_pot : int, p1_pot : int, raises_left : int) = {is_fold=p0_pot <> p1_pot; is_call=true; is_raise=raises_left > 0}
        static member FromDataToArray(p0_pot : int, p1_pot : int, raises_left : int) = AllowedActions.FromData (p0_pot,p1_pot,raises_left) |> AllowedActions.Array
        static member FromModel(model : LeducModel, raises_left : int) = AllowedActions.FromData(model.p0_pot,model.p1_pot,raises_left)

    let init_player_model = function
        | Enum -> ModelEnum Map.empty
        | MC -> ModelMC (Map.empty, Map.empty)

    let cfr_player_types' = [|Enum; MC|]
    let player_types' = [|yield Human; yield Random; for x in cfr_player_types' -> CFR x|]
    let player_types_template x = Array.fold (fun s x -> Map.add (x.ToString()) x s) Map.empty x
    let player_types = player_types_template player_types'
    let cfr_player_types = player_types_template cfr_player_types'

    type ClientModel =
        {
            leduc_model : LeducModel
            message_list : string list
            allowed_actions : AllowedActions
            p0 : PlayerType
            p1 : PlayerType
            active_tab : Tabs
            active_cfr_player : CFRPlayerType
            cfr_players : Map<CFRPlayerType,UICFRPlayerModel>
        }

        static member Default =
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
            }

    type MsgServerToClient =
        | GameState of LeducModel * string list * AllowedActions
        | TrainingResult of CFRPlayerType * (float * float)
        | TrainingModel of CFRPlayerType * CFRPlayerModel
        | TestingResult of CFRPlayerType * float
        | TestingModel of CFRPlayerType * CFRPlayerModel

    type MsgClientToPlayServer =
        | SelectedAction of Action
        | StartGame of p0: PlayerModel * p1: PlayerModel

    type MsgClientToLearnServer =
        | Train of num_iter: uint * pl: CFRPlayerModel
        | Test of num_iter: uint * pl: CFRPlayerModel

    type MsgLeduc =
        | Action of LeducModel * string list * AllowedActions * (Action -> unit)
        | Terminal of LeducModel * string list

    type MsgPlayServer =
        | FromLeducGame of MsgLeduc
        | FromClient of MsgClientToPlayServer

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
        | FromServer of msg: MsgServerToClient

    type ViewComponent() =
        inherit StatefulComponent<ClientModel,MsgClient>(ClientModel.Default)

        let token_source = new CancellationTokenSource()
        let server_play = new MailboxProcessor<_>(fun mb -> async {
            failwith "TODO"
        },token_source.Token)

        let server_learn = new MailboxProcessor<_>(fun mb -> async {
            failwith "TODO"
        },token_source.Token)

        override this.Update(msg) =
            let model = this.Model

            // Now what is next?
            // ...

            // Let's implement the disposable interface for the mailboxes.
            // I think this should be enough for the component to be disposed on its own.
            // Let me check.

            // Yeah, this should be fine. But we'll also be passing cancellation tokens...
            // Maybe I'll reuse this code as a minigame on some other site, so it is good to do this.

            // With that out of the way, we can just plug all the code that we need...

            let inline update' active_cfr_player f = // Inlining funs with closures often improves performance.
                let m = f model.cfr_players[active_cfr_player]
                {model with cfr_players=Map.add active_cfr_player m model.cfr_players}
            let inline update f = update' model.active_cfr_player f
            match msg with
            | ClickedOn x ->
                server_play.Post(FromClient (SelectedAction x))
                {model with allowed_actions=AllowedActions.Default}
            | StartGame ->
                let get_model = function
                    | Human -> PLM_Human
                    | Random -> PLM_Random
                    | CFR x -> PLM_CFR model.cfr_players[x].training_model
                server_play.Post(FromClient (MsgClientToPlayServer.StartGame(get_model model.p0, get_model model.p1)))
                model
            | PlayerChange(id, pl) ->
                let model =
                    match id with
                    | 0 -> {model with p0 = pl}
                    | 1 -> {model with p1 = pl}
                    | _ -> model
                model
            | TabClicked tab ->
                {model with active_tab=tab}
            | TrainingStartClicked ->
                update <| fun m ->
                    let iter = uint m.training_run_iterations
                    server_learn.Post (MsgClientToLearnServer.Train (iter, m.training_model))
                    {m with training_iterations_left=m.training_iterations_left + iter}
            | TestingStartClicked ->
                update <| fun m ->
                    let iter = uint m.testing_run_iterations
                    server_learn.Post (MsgClientToLearnServer.Test (iter, m.training_model))
                    {m with testing_iterations_left=m.testing_iterations_left+iter; testing_results=[]; testing_model=init_player_model model.active_cfr_player}
            | TrainingInputIterationsChanged s -> update <| fun m -> {m with training_run_iterations = s}
            | TestingInputIterationsChanged s -> update <| fun m -> {m with testing_run_iterations=s}
            | CFRPlayerSelected s -> {model with active_cfr_player=cfr_player_types[s]}
            | FromServer (GameState(leduc_model, message_list, allowed_actions)) ->
                {model with leduc_model=leduc_model; message_list=message_list; allowed_actions=allowed_actions}
            | FromServer (TrainingResult(pl,(a,b))) ->
                update' pl <| fun m ->
                    let mutable i = 150
                    let x = (m.training_iterations,(a,b)) :: m.training_results |> List.takeWhile (fun _ -> if i > 0 then i <- i-1; true else false)
                    {m with training_results=x; training_iterations_left=m.training_iterations_left-1u; training_iterations=m.training_iterations+1}
            | FromServer (TestingResult(pl,x)) ->
                update' pl <| fun m ->
                    {m with testing_results=x :: m.testing_results; testing_iterations_left=m.testing_iterations_left-1u}
            | FromServer (TrainingModel (a,x)) ->
                update' a <| fun m ->
                    {m with training_model=x}
            | FromServer (TestingModel (a,x)) ->
                update' a <| fun m ->
                    {m with testing_model=x}

        interface IDisposable with
            member _.Dispose() = token_source.Dispose(); server_learn.Dispose(); server_play.Dispose()

    let names = [| "Larry"; "Tom" |]

    let card = function
        | Some King -> "K"
        | Some Queen -> "Q"
        | Some Jack -> "J"
        | None -> " "
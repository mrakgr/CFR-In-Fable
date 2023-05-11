namespace Shared.Leduc.Types

type Action = Fold | Call | Raise
type Card = King | Queen | Jack
type GameModel = Choice<Action,Card> list
type Player = { card : Card; id : int; pot : int }
type Mask = uint64

module CFR =
    type ValueArrays = (struct (float * float )) []
    type PolicyArrays<'action> = {current_regrets : float[]; unnormalized_policy_average : float[]; actions : 'action []}
    type PolicyDictionary<'game_model,'action> = System.Collections.Generic.Dictionary<'game_model, PolicyArrays<'action>>
    type ValueDictionary<'game_model> = System.Collections.Generic.Dictionary<'game_model, ValueArrays>

open CFR

module UI =
    type CFRPlayerType = Enum | MC
    type [<ReferenceEquality>] CFRPlayerModel =
        | ModelEnum of Map<GameModel,PolicyArrays<Action>>
        | ModelMC of Map<GameModel,PolicyArrays<Action>> * Map<GameModel,ValueArrays>

    type PlayerType = Human | Random | CFR of CFRPlayerType
    type PlayerModel =
        | PLM_Human
        | PLM_Random
        | PLM_CFR of CFRPlayerModel
    let cfr_player_types' = [|Enum; MC|]
    let player_types' = [|yield Human; yield Random; for x in cfr_player_types' -> CFR x|]
    let player_types_template x = Array.fold (fun s x -> Map.add (x.ToString()) x s) Map.empty x
    let player_types = player_types_template player_types'
    let cfr_player_types = player_types_template cfr_player_types'

    type LeducModel = {
        p0_id: int
        p0_card: Card option
        p0_pot: int
        p1_id: int
        p1_card: Card option
        p1_pot: int
        community_card : Card option
    } with
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
    static member IsAllowed q = function Fold -> q.is_fold | Call -> q.is_call | Raise -> q.is_raise
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
    static member FromModel(model : UI.LeducModel, raises_left : int) = AllowedActions.FromData(model.p0_pot,model.p1_pot,raises_left)

type MsgLeduc =
    | Action of UI.LeducModel * string list * AllowedActions * (Action -> unit)
    | Terminal of UI.LeducModel * string list

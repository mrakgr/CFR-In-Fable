module Shared.Leduc.Types

type Action = Fold | Call | Raise
type Card = King | Queen | Jack
type Player = { card : Card; id : int; pot : int }
type Mask = uint64

type PlayerType = Human | Random

type LeducModel = {
    p1_id: int
    p1_card: Card option
    p1_pot: int
    p2_id: int
    p2_card: Card option
    p2_pot: int
    community_card : Card option
} with
    static member Default = {
        p1_id = 0
        p1_card = None
        p1_pot = 0
        p2_id = 1
        p2_card = None
        p2_pot = 0
        community_card = None
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
    static member FromModel(model : LeducModel, raises_left : int) = {is_fold=model.p1_pot <> model.p2_pot; is_call=true; is_raise=raises_left > 0}

type MsgLeduc =
    | Action of LeducModel * string list * AllowedActions * (Action -> unit)
    | Terminal of LeducModel * string list

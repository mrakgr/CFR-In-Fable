module Leduc.Types

type Action = Fold | Call | Raise
type Card = King | Queen | Jack
type Player = { card : Card; id : int; pot : int }
type Mask = uint64

type ILeducGame<'r> =
    abstract member chance_init : player_id: int * mask: Mask * cont: (Card * Mask -> 'r) -> 'r
    abstract member chance_community_card : mask: Mask * cont: (Card -> 'r) -> 'r
    abstract member action_round_one : p1: Player * p2: Player * raises_left: int * cont: (Action -> 'r) -> 'r
    abstract member action_round_two : p1: Player * p2: Player * raises_left: int * community_card: Card * cont: (Action -> 'r) -> 'r
    abstract member terminal : id: int * pot: int -> 'r

type LeducModel = {
    p1_card: Card option
    p1_pot: int
    p2_card: Card option
    p2_pot: int
    community_card : Card option
} with
    static member Default = {
        p1_card = None
        p1_pot = 0
        p2_card = None
        p2_pot = 0
        community_card = None
    }

type AllowedActions = { is_fold : bool; is_call : bool; is_raise : bool } with
    member q.IsAllowed = function Fold -> q.is_fold | Call -> q.is_call | Raise -> q.is_raise
    member q.Mask =
        let f b i m = if b then m ||| (1UL <<< i) else m
        0UL |> f q.is_fold 0 |> f q.is_call 1 |> f q.is_raise 2
    member q.Array = [|
        if q.is_fold then Fold
        if q.is_call then Call
        if q.is_raise then Raise
    |]
    static member Default = { is_fold = false; is_call = false; is_raise = false }

type MsgLeduc =
    | Action of LeducModel * string list * AllowedActions * (Action -> unit)
    | Terminal of LeducModel * string list

module Leduc.Types

type Action = Fold | Call | Raise
type Card = King | Queen | Jack
type Player = { card : Card; id : int; pot : int }
type Mask = uint64

type ILeducGame<'r> =
    abstract member chance_init : player_id: int * Mask * (Card * Mask -> 'r) -> 'r
    abstract member chance_community_card : Mask * (Card -> 'r) -> 'r
    abstract member action_round_one : p1: Player * p2: Player * raises_left: int * (Action -> 'r) -> 'r
    abstract member action_round_two : p1: Player * p2: Player * raises_left: int * community_card: Card * (Action -> 'r) -> 'r
    abstract member terminal : id: int * pot: int -> 'r

module Client =
    type Model = {
        p1_card: Card option
        p1_pot: int
        p2_card: Card option
        p2_pot: int
        community_card : Card option
        allow_fold: bool
        allow_call: bool
        allow_raise: bool
    }

    let def = {
        p1_card = None
        p1_pot = 0
        p2_card = None
        p2_pot = 0
        community_card = None
        allow_fold = false
        allow_call = false
        allow_raise = false
    }

module Server =
    type Model = {
        client : Client.Model
        cont: (Action -> unit)
    }


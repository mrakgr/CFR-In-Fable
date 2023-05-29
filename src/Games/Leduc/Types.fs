module Leduc.Types

open Lproj.Types

type ILeducGame<'r> =
    abstract member chance_init : id: int * cont: (Card -> 'r) -> 'r
    abstract member chance_community_card : cont: (Card -> 'r) -> 'r
    abstract member action_round_one : is_call_a_check: bool * p0: Player * p1: Player * raises_left: int * cont: (Action -> 'r) -> 'r
    abstract member action_round_two : is_call_a_check: bool * p0: Player * p1: Player * raises_left: int * community_card: Card * cont: (Action -> 'r) -> 'r
    abstract member terminal_fold : p0: Player * id: int * pot: int -> 'r
    abstract member terminal_call : p0: Player * p1: Player * community_card: Card * id: int * pot: int -> 'r

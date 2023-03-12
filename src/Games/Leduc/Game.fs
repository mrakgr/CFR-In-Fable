module Leduc.Game
open Shared.Leduc.Types

let compare_hands (community_card : Card) (p1 : Player, p2 : Player) =
    let tag = function King -> 2 | Queen -> 1 | Jack -> 0
    let community_card = tag community_card
    let a = tag p1.card, community_card
    let b = tag p2.card, community_card
    let order (a,b) = if a > b then a,b else b,a
    let is_pair (a,b) = a = b
    match is_pair a, is_pair b with
    | true, true -> compare (fst a) (fst b)
    | true, false -> 1
    | false, true -> -1
    | _ -> compare (order a) (order b)

let raiseBy amount (p: Player) = p.pot + amount

let game (ret : ILeducGame<'r>) : 'r =
    ret.chance_init(0, 0UL, fun (c1,mask) ->
    ret.chance_init(1, mask, fun (c2,mask) ->

    let rec round_two is_call_a_check raises_left (p1,p2) community_card =
        ret.action_round_two(is_call_a_check, p1, p2, raises_left, community_card, function
        | Fold -> ret.terminal_fold(p1, p2.id, p1.pot)
        | Call when is_call_a_check -> round_two false raises_left (p2,p1) community_card
        | Call ->
                let p1 = {p1 with pot=p2.pot}
                let id, pot =
                    match compare_hands community_card (p1,p2) with
                    | 1 -> p1.id, p2.pot
                    | 0 -> p1.id, 0
                    | -1 -> p2.id, p1.pot
                    | _ -> failwith "impossible"
                ret.terminal_call(p1,p2,community_card,id,pot)
        | Raise -> round_two false (raises_left-1) (p2,{p1 with pot=raiseBy 4 p2}) community_card
        )

    let rec round_one is_call_a_check raises_left (p1, p2) =
        ret.action_round_one(is_call_a_check, p1, p2, raises_left, function
        | Fold -> ret.terminal_fold(p1, p2.id, p1.pot)
        | Call when is_call_a_check -> round_one false raises_left (p2,p1)
        | Call ->
            let p1 = {p1 with pot=p2.pot}
            ret.chance_community_card(mask, round_two true 2 (if p1.id = 0 then p1,p2 else p2,p1))
        | Raise -> round_one false (raises_left-1) (p2, {p1 with pot=raiseBy 2 p2})
        )

    round_one true 2 ({id=0; card=c1; pot=1}, {id=1; card=c2; pot=1})
    ))
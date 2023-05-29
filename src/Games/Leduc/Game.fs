module Leduc.Game

open Blazor.Client.Fun
open Leduc.Types

let compare_hands (community_card : Card) (p0 : Player, p1 : Player) =
    let tag = function King -> 2 | Queen -> 1 | Jack -> 0
    let community_card = tag community_card
    let a = tag p0.card, community_card
    let b = tag p1.card, community_card
    let order (a,b) = if a > b then a,b else b,a
    let is_pair (a,b) = a = b
    match is_pair a, is_pair b with
    | true, true -> compare (fst a) (fst b)
    | true, false -> 1
    | false, true -> -1
    | _ -> compare (order a) (order b)

let raiseBy amount (p: Player) = p.pot + amount

let game (ret : ILeducGame<'r>) : 'r =
    ret.chance_init(0, fun c1 ->
    ret.chance_init(1, fun c2 ->

    let rec round_two is_call_a_check raises_left (p0,p1) community_card =
        ret.action_round_two(is_call_a_check, p0, p1, raises_left, community_card, function
        | Fold -> ret.terminal_fold(p0, p1.id, p0.pot)
        | Call when is_call_a_check -> round_two false raises_left (p1,p0) community_card
        | Call ->
                let p0 = {p0 with pot=p1.pot}
                let id, pot =
                    match compare_hands community_card (p0,p1) with
                    | 1 -> p0.id, p1.pot
                    | 0 -> p1.id, 0
                    | -1 -> p1.id, p0.pot
                    | _ -> failwith "impossible"
                ret.terminal_call(p0,p1,community_card,id,pot)
        | Raise -> round_two false (raises_left-1) (p1,{p0 with pot=raiseBy 4 p1}) community_card
        )

    let rec round_one is_call_a_check raises_left (p0, p1) =
        ret.action_round_one(is_call_a_check, p0, p1, raises_left, function
        | Fold -> ret.terminal_fold(p0, p1.id, p0.pot)
        | Call when is_call_a_check -> round_one false raises_left (p1,p0)
        | Call ->
            let p0 = {p0 with pot=p1.pot}
            ret.chance_community_card(round_two true 2 (if p0.id = 0 then p0,p1 else p1,p0))
        | Raise -> round_one false (raises_left-1) (p1, {p0 with pot=raiseBy 2 p1})
        )

    round_one true 2 ({id=0; card=c1; pot=1}, {id=1; card=c2; pot=1})
    ))

let deck = [|King; King; Queen; Queen; Jack; Jack|]
let actions = [|Fold; Call; Raise|]
let sample_card mask = MaskedArray.sample deck mask
let sample_action mask = MaskedArray.sample actions mask
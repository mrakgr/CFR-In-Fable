module Leduc.Implementation
open Leduc.Types
open Types

let deck = [|King; King; Queen; Queen; Jack; Jack|]
let sample_card mask = Sampler.sample deck mask

type HumanLeducPlayer(dispatch : (MsgLeduc -> unit) -> unit) =
    interface ILeducGame<LeducModel * Map<int,string list> -> unit> with
        member this.chance_init(player_id, mask, cont) =

            failwith ""
        member this.chance_community_card(mask, cont) = failwith "todo"
        member this.action_round_one(p1, p2, raises_left, cont) = failwith "todo"
        member this.action_round_two(p1, p2, raises_left, community_card, cont) = failwith "todo"
        member this.terminal(id, pot) = failwith "todo"



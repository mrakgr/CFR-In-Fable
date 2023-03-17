module Leduc.Learn

open CFR
open Shared.Leduc.Types
open Leduc.Game

type GameModel = Choice<Action,Card> list

type LeducGameLearn(p0 : IAction<GameModel, Action>, p1 : IAction<GameModel, Action>) =
    let get id x = if id = 0 then fst x else snd x
    let update id x y = if id = 0 then y, snd x else fst x, y

    interface ILeducGame<(GameModel * GameModel) * PathProb -> float> with
        member this.action_round_one(is_call_a_check, p1, p2, raises_left, cont) = fun (model, prob) ->
            let p = get p0.id (p0, p1)
            p.action()
        member this.action_round_two(is_call_a_check, p1, p2, raises_left, community_card, cont) = failwith "todo"
        member this.chance_community_card(cont) = failwith "todo"
        member this.chance_init(player_id, cont) = failwith "todo"
        member this.terminal_call(p1, p2, community_card, id, pot) = failwith "todo"
        member this.terminal_fold(p1, id, pot) = failwith "todo"
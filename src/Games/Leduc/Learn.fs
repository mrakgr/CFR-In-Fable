module Leduc.Learn

open System.Collections.Generic
open CFR
open Shared.Leduc.Types
open Leduc.Types
open Leduc.Game

type GameModel = Choice<Action,Card> list
type GameModels = GameModel * GameModel

type IChance =
    abstract member chance : mask: Mask * cont: (Card * Mask -> float) -> float

type LeducChanceEnumarate() =
    interface IChance with
        member this.chance(mask, cont) =
            let mutable reward = 0.0
            let mutable count = 0
            MaskedArray.iter (fun x ->
                reward <- reward + cont x
                count <- count + 1
                ) deck mask
            reward / float count

type LeducGameLearn(chance : IChance, p0 : IAction<GameModel,Action>, p1 : IAction<GameModel,Action>) =
    let get id x = if id = 0 then fst x else snd x
    let put id x y = if id = 0 then y, snd x else fst x, y
    let modify id f x = if id = 0 then f (fst x), snd x else fst x, f (snd x)
    let swap id (a,b) = if id = 0 then a,b else b,a
    let reward_negate id x = if id = 0 then x else -x

    let action id allowed_actions cont (model,probs,mask) =
        (get id (p0, p1)).action(get id model,AllowedActions.FromDataToArray allowed_actions,swap id probs,fun (act,probs) ->
            cont act (modify id (fun model -> Choice1Of2 act :: model) model, swap id probs, mask)
            |> reward_negate id
            )
        |> reward_negate id

    let chance update cont (model,prob,mask) =
        // Since the distribution is uniform, I skip updating the path probabilities.
        // If it wasn't uniform I'd have needed to introduce a PathProb field specifically for chance nodes.
        chance.chance(mask, fun (card,mask) ->
            let model = update card model
            cont card (model,prob,mask)
            )

    let terminal (id,pot) = if id = 0 then float pot else -(float pot)

    interface ILeducGame<GameModels * PathProbs * Mask -> float> with
        member this.action_round_one(_, p0, p1, raises_left, cont) = action p0.id (p0.pot, p1.pot, raises_left) cont
        member this.action_round_two(_, p0, p1, raises_left, _, cont) = action p0.id (p0.pot, p1.pot, raises_left) cont
        member this.chance_init(id, cont) = chance (fun card -> modify id (fun model -> Choice2Of2 card :: model)) cont
        member this.chance_community_card(cont) =
            let f card model = Choice2Of2 card :: model
            chance (fun card -> modify 0 (f card) >> modify 1 (f card)) cont
        member this.terminal_call(_, _, _, id, pot) = fun _ -> terminal (id, pot)
        member this.terminal_fold(_, id, pot) = fun _ -> terminal (id, pot)

let game () =
    let d = Dictionary()
    let p0 = Learn.AgentActive(p0)
    game(LeducGameLearn(LeducChanceEnumarate(),p0,p0)) (([],[]), (1.0,1.0), 0UL)
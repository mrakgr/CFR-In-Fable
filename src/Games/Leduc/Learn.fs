module Leduc.Learn

open System.Collections.Generic
open CFR
open Shared.Leduc.Types
open Leduc.Types
open Leduc.Game

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

module Utils =
    let get id x = if id = 0 then fst x else snd x
    let put id x y = if id = 0 then y, snd x else fst x, y
    let modify id f x = if id = 0 then f (fst x), snd x else fst x, f (snd x)
    let swap id (a,b) = if id = 0 then a,b else b,a
    let reward_negate id x = if id = 0 then x else -x

open Utils
type LeducGameLearn(chance : IChance, p0 : IAction<GameModel,Action>, p1 : IAction<GameModel,Action>) =
    let action id allowed_actions cont (model,probs,mask) =
        (get id (p0, p1)).action(get id model,AllowedActions.FromDataToArray allowed_actions,swap id probs,fun (act,probs) ->
            let f id = modify id (fun model -> Choice1Of2 act :: model)
            cont act (model |> f 0 |> f 1, swap id probs, mask)
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
            let f id card = modify id (fun model -> Choice2Of2 card :: model)
            chance (fun card -> f 0 card >> f 1 card) cont
        member this.terminal_call(_, _, _, id, pot) = fun _ -> terminal (id, pot)
        member this.terminal_fold(_, id, pot) = fun _ -> terminal (id, pot)

open Learn

type LearningDictionary = Dictionary<GameModel, PolicyArrays<Action>>

/// Tests the agent by running it against its average policy.
let test d =
    let init = (([],[]), (1.0,1.0), 0UL)
    game(LeducGameLearn(LeducChanceEnumarate(),AgentActive(d),AgentPassiveEnum(d,false))) init

/// Tests the agent by running it iteratively against itself.
let game d =
    let init = (([],[]), (1.0,1.0), 0UL)
    let r1 = game(LeducGameLearn(LeducChanceEnumarate(),AgentActive(d),AgentPassiveEnum(d,true))) init
    let r2 = game(LeducGameLearn(LeducChanceEnumarate(),AgentPassiveEnum(d,true),AgentActive(d))) init
    r1, r2


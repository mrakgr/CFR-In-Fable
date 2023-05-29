namespace CFR

open System
open System.Collections.Generic
open Lproj.Types

type PathProbs = float * float * float
type IAction<'model,'action> =
    abstract member action : model: 'model * actions: 'action [] * path_prob: PathProbs * cont: ('action * PathProbs -> float) -> float

module Operators =
    let ( *...) ((self_prob,opp_prob,beh_prob) : PathProbs) (self, opp, beh) = self_prob * self, opp_prob * opp, beh_prob * beh
    let ( *..) ((self_prob,opp_prob,beh_prob) : PathProbs) (self, beh) = self_prob * self, opp_prob, beh_prob * beh
    let ( *.) path_prob self = path_prob *.. (self, 1.0)

module Enumerative =
    open Operators

    let get_policy' (d : Dictionary<'model,PolicyArrays<'action>>) model actions =
        match d.TryGetValue(model) with
        | true, v -> v
        | _ ->
            let len = Array.length actions
            let x = {current_regrets=Array.zeroCreate len; unnormalized_policy_average=Array.zeroCreate len; actions=actions}
            d[model] <- x
            x

    let get_policy (d : Dictionary<'model,PolicyArrays<'action>>) model num_action = normalize (get_policy' d model num_action).current_regrets

    let update_policy d model actions rewards avg_reward (self_prob, opp_prob, beh_prob) =
        if opp_prob > 0.0 then
            let policy_arrays = get_policy' d model actions
            let new_regrets = Array.map (fun reward -> reward - avg_reward) rewards
            let new_current_regrets = Array.map2 (fun new_regret old_regret -> opp_prob * new_regret / beh_prob + old_regret |> max 0.0) new_regrets policy_arrays.current_regrets
            let new_unnormalized_policy_average = Array.map2 (fun a b -> (a + b) * 0.5) (normalize new_current_regrets) policy_arrays.unnormalized_policy_average
            d[model] <- {policy_arrays with current_regrets=new_current_regrets; unnormalized_policy_average=new_unnormalized_policy_average}

    type AgentActiveEnum<'model,'action when 'model: equality>(d : Dictionary<'model,PolicyArrays<'action>>) =
        interface IAction<'model,'action> with
            member this.action(model, actions, path_prob, cont) =
                let current_policy : float [] = get_policy d model actions
                let rewards = Array.map2 (fun act policy_prob -> cont (act, path_prob *. policy_prob)) actions current_policy
                let avg_reward = Array.fold2 (fun s x policy_prob -> s + x * policy_prob) 0.0 rewards current_policy
                update_policy d model actions rewards avg_reward path_prob
                avg_reward

    type AgentPassiveEnum<'model,'action when 'model: equality>(d : Dictionary<'model,PolicyArrays<'action>>, use_current_policy) =
        interface IAction<'model,'action> with
            member this.action(model, actions, path_prob, cont) =
                let policy =
                    let x = get_policy' d model actions
                    normalize <| if use_current_policy then x.current_regrets else x.unnormalized_policy_average

                let rewards = Array.map2 (fun act policy_prob -> cont (act, path_prob *. policy_prob)) actions policy
                Array.fold2 (fun s x policy_prob -> s + x * policy_prob) 0.0 rewards policy

    /// The array passed into this should be positive and sum up to 1.
    let rec sample' (rng : Random) (dist : float []) epsilon =
        let r = rng.NextDouble()
        let rec loop i prob_sum =
            if i < dist.Length then
                let prob_cur = dist[i] * (1.0 - epsilon) + (1.0 / float dist.Length) * epsilon
                let prob_sum = prob_sum + prob_cur
                if r < prob_sum then i, prob_cur
                else loop (i+1) prob_sum
            else // It might hang here if the array is not a proper prob distro.
                sample' rng dist epsilon
        loop 0 0.0

    let sample (rng : Random) (actions: 'action []) (normalized_policy_average : float []) =
        let i, prob = sample' rng normalized_policy_average 0.0
        actions[i], prob

    type AgentPassiveSample<'model,'action when 'model: equality>(d : PolicyDictionary<'model,'action>, use_current_policy) =
        let rng = System.Random()

        interface IAction<'model,'action> with
            member this.action(model, actions, path_prob, cont) =
                let policy =
                    let x = get_policy' d model actions
                    if use_current_policy then x.current_regrets else x.unnormalized_policy_average
                    |> normalize

                let act,policy_prob = sample rng actions policy
                cont (act, path_prob *.. (policy_prob, policy_prob))

module Sampling =
    open Operators
    open Enumerative

    type AgentActiveSample<'model,'action when 'model: equality>(d : PolicyDictionary<'model,'action>, d' : ValueDictionary<'model>) =
        let rng = System.Random()

        let get_values' model actions =
            match d'.TryGetValue model with
            | true, v -> v
            | _ ->
                let v = Array.zeroCreate (Array.length actions)
                d'[model] <- v
                v

        let get_values model actions =
            get_values' model actions
            |> Array.map (fun (struct (a,b)) -> if b = 0.0 then 0.0 else a / b)

        let update_values model actions i x (self_prob, opp_prob, beh_prob as path_prob) =
            if opp_prob > 0.0 then
                let ar = get_values' model actions
                let struct (a,b) = ar[i]
                let decay = 0.5
                ar[i] <- (a + opp_prob / beh_prob * x) * decay, (b + opp_prob / beh_prob) * decay

        interface IAction<'model,'action> with
            member this.action(model, actions, path_prob, cont) =
                let current_policy : float [] = get_policy d model actions
                let rewards =
                    let i',beh_prob = sample' rng current_policy 0.25
                    let value_arrays : float [] = get_values model actions
                    Array.mapi2 (fun i act policy_prob ->
                        let exp_y = value_arrays[i]
                        if i = i' then
                            let path_prob = path_prob *.. (policy_prob, beh_prob)
                            let x = cont (act, path_prob)
                            update_values model actions i x path_prob
                            let y = exp_y
                            (x - y) / beh_prob + exp_y
                        else
                            exp_y
                        ) actions current_policy
                let avg_reward = Array.fold2 (fun s x policy_prob -> s + x * policy_prob) 0.0 rewards current_policy
                update_policy d model actions rewards avg_reward path_prob
                avg_reward
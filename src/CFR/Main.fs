namespace CFR

open System
open System.Collections.Generic

type PathProbs = float * float
type IAction<'model,'action> =
    abstract member action : model: 'model * actions: 'action [] * path_prob: PathProbs * cont: ('action * PathProbs -> float) -> float

module Enumerative =
    let ( *.) (a : PathProbs) b = fst a * b, snd a

    let normalize (x : float []) =
        let s = Array.sum x
        if s = 0.0 then Array.replicate x.Length (1.0 / float x.Length)
        else Array.map (fun x -> x / s) x

    type PolicyArrays<'action> = {current_regrets : float[]; unnormalized_policy_average : float[]; actions : 'action []}

    let get_policy' (d : Dictionary<'model,PolicyArrays<'action>>) model actions =
        match d.TryGetValue(model) with
        | true, v -> v
        | _ ->
            let len = Array.length actions
            let x = {current_regrets=Array.zeroCreate len; unnormalized_policy_average=Array.zeroCreate len; actions=actions}
            d[model] <- x
            x

    let get_policy (d : Dictionary<'model,PolicyArrays<'action>>) model num_action = normalize (get_policy' d model num_action).current_regrets

    let update_policy d model actions rewards avg_reward (self_prob, opp_prob) =
        if opp_prob > 0.0 then
            let policy_arrays = get_policy' d model actions
            let new_regrets = Array.map (fun reward -> reward - avg_reward) rewards
            let new_current_regrets = Array.map2 (fun new_regret old_regret -> opp_prob * new_regret + old_regret |> max 0.0) new_regrets policy_arrays.current_regrets
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
    let rec sample' (rng : Random) (dist : float []) =
        let r = rng.NextDouble()
        let rec loop i prob_sum =
            if i < dist.Length then
                let prob_cur = dist[i]
                let prob_sum = prob_sum + prob_cur
                if r < prob_sum then i, prob_cur
                else loop (i+1) prob_sum
            else // It might hang here if the array is not a proper prob distro.
                sample' rng dist
        loop 0 0.0

    let sample (rng : Random) (actions: 'action []) (normalized_policy_average : float []) =
        let i, prob = sample' rng normalized_policy_average
        actions[i], prob

    type AgentPassiveSample<'model,'action when 'model: equality>(d : Dictionary<'model,PolicyArrays<'action>>) =
        let rng = Random()

        interface IAction<'model,'action> with
            member this.action(model, actions, path_prob, cont) =
                let policy = normalize (get_policy' d model actions).unnormalized_policy_average
                let act,policy_prob = sample rng actions policy
                cont (act, path_prob *. policy_prob)

module Sampling =
    open Enumerative

    type ValueArrays = (struct (float * float )) []

    type AgentActiveSample<'model,'action when 'model: equality>(d : Dictionary<'model,PolicyArrays<'action>>, d' : Dictionary<'model,ValueArrays>) =
        let rng = Random()

        interface IAction<'model,'action> with
            member this.action(model, actions, path_prob, cont) =
                let current_policy : float [] = get_policy d model actions
                let rewards =
                    let i = sample' rng current_policy
                    let value_arrays : float [] = get_values d model
                    Array.mapi2 (fun i act policy_prob ->
                        cont (act, path_prob *. policy_prob)
                        ) actions current_policy
                let avg_reward = Array.fold2 (fun s x policy_prob -> s + x * policy_prob) 0.0 rewards current_policy
                update_policy d model actions rewards avg_reward path_prob
                avg_reward
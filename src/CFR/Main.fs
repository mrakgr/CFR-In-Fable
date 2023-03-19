namespace CFR

open System
open System.Collections.Generic

type PathProbs = float * float
type IAction<'model,'action> =
    abstract member action : model: 'model * actions: 'action [] * path_prob: PathProbs * cont: ('action * PathProbs -> float) -> float

module Learn =
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

    type AgentActive<'model,'action when 'model: equality>(d : Dictionary<'model,PolicyArrays<'action>>) =
        let update_policy model actions rewards avg_reward (self_prob, opp_prob as path_prob) =
            let policy_arrays = get_policy' d model actions
            let new_regrets = Array.map (fun reward -> reward - avg_reward) rewards
            let new_current_regrets = Array.map2 (fun new_regret old_regret -> opp_prob * new_regret + old_regret |> max 0.0) new_regrets policy_arrays.current_regrets
            // TODO: Why multiply by the self probability?
            // https://www.reddit.com/r/reinforcementlearning/comments/11ujf28/in_the_enumerative_cfr_algorithm_why_does_the/
            let f0, f1 = self_prob, 2.0
            let new_unnormalized_policy_average =
                Array.map2 (fun a b -> (f0 * a + f1 * b) / (f0 + f1)) (normalize new_current_regrets) policy_arrays.unnormalized_policy_average
            d[model] <- {policy_arrays with current_regrets=new_current_regrets; unnormalized_policy_average=new_unnormalized_policy_average}

        interface IAction<'model,'action> with
            member this.action(model, actions, path_prob, cont) =
                let current_policy : float [] = get_policy d model actions
                let rewards =
                    Array.map2 (fun act policy_prob ->
                        if fst path_prob = 0 && snd path_prob = 0 then 0.0
                        else cont (act, path_prob *. policy_prob)
                        ) actions current_policy
                let avg_reward = Array.fold2 (fun s x policy_prob -> s + x * policy_prob) 0.0 rewards current_policy
                update_policy model actions rewards avg_reward path_prob
                avg_reward

    type AgentPassiveEnum<'model,'action when 'model: equality>(d : Dictionary<'model,PolicyArrays<'action>>, use_current_policy) =
        interface IAction<'model,'action> with
            member this.action(model, actions, path_prob, cont) =
                let policy =
                    let x = get_policy' d model actions
                    normalize <| if use_current_policy then x.current_regrets else x.unnormalized_policy_average

                let rewards = Array.map2 (fun act policy_prob -> cont (act, path_prob *. policy_prob)) actions policy
                Array.fold2 (fun s x policy_prob -> s + x * policy_prob) 0.0 rewards policy

    type AgentPassiveSample<'model,'action when 'model: equality>(d : Dictionary<'model,PolicyArrays<'action>>) =
        let rng = Random()
        let sample (actions: 'action []) (normalized_policy_average : float []) =
            let r = rng.NextDouble()
            let rec loop i prob_sum =
                if i < actions.Length then
                    let prob_cur = normalized_policy_average[i]
                    let prob_sum = prob_sum + prob_cur
                    if r < prob_sum then actions[i], prob_cur
                    else loop (i+1) prob_sum
                else // Shouldn't ever trigger, but better safe than sorry.
                    let i = Array.findIndexBack ((<) 0.0) normalized_policy_average
                    actions[i], normalized_policy_average[i]
            loop 0 0.0

        interface IAction<'model,'action> with
            member this.action(model, actions, path_prob, cont) =
                let policy = normalize (get_policy' d model actions).unnormalized_policy_average
                let act,policy_prob = sample actions policy
                cont (act, path_prob *. policy_prob)

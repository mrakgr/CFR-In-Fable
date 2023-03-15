module CFR

open System.Collections.Generic

type PathProb = float * float

type IAction<'model,'action> =
    abstract member action : model: 'model * actions: 'action [] * path_prob: PathProb * cont: ('action * PathProb -> float) -> float

let ( *.) (a : PathProb) b = fst a * b, snd a

let normalize (x : float []) =
    let s = Array.sum x
    if s = 0.0 then Array.replicate x.Length (1.0 / float x.Length)
    else Array.map (fun x -> x / s) x

type PolicyArrays = {current_regrets : float[]; unnormalized_policy_average : float[]}

type CFR<'model,'action when 'model: equality>() =
    let d : Dictionary<'model,PolicyArrays> = Dictionary()

    let get_policy' model num_action =
        match d.TryGetValue(model) with
        | true, v -> v
        | _ ->
            let x = {current_regrets=Array.zeroCreate num_action; unnormalized_policy_average=Array.zeroCreate num_action}
            d.[model] <- x
            x

    let get_policy model num_action = get_policy' model num_action |> fun x -> normalize x.current_regrets

    let update_policy model num_actions rewards avg_reward (self_prob, opp_prob as path_prob) =
        let policy_arrays = get_policy' model num_actions
        let new_regrets = Array.map (fun reward -> reward - avg_reward) rewards
        let new_current_regrets = Array.map2 (fun new_regret old_regret -> opp_prob * new_regret + old_regret |> max 0.0) new_regrets policy_arrays.current_regrets
        let new_unnormalized_policy_average = Array.map2 (+) (normalize new_current_regrets) policy_arrays.unnormalized_policy_average
        d.[model] <- {current_regrets=new_current_regrets; unnormalized_policy_average=new_unnormalized_policy_average}

    interface IAction<'model,'action> with
        member this.action(model, actions, path_prob, cont) =
            let current_policy : float [] = get_policy model actions.Length
            let rewards = Array.map2 (fun act policy_prob -> cont (act, path_prob *. policy_prob)) actions current_policy
            let avg_reward = Array.fold2 (fun s x policy_prob -> s + x * policy_prob) 0.0 rewards current_policy
            update_policy model actions.Length rewards avg_reward path_prob
            avg_reward

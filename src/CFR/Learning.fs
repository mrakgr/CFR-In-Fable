module CFR

open System.Collections.Generic

type PathProb = float * float

type IAction<'model,'action> =
    abstract member action : model: 'model * actions: 'action [] * path_prob: PathProb * cont: ('action * PathProb -> float) -> float

let ( *.) (a : PathProb) b = fst a * b, snd a

type CFR<'model,'action when 'model: equality>(update_policy) =
    let d = Dictionary()

    let get_policy model num_action =
        match d.TryGetValue(model) with
        | true, v -> v
        | _ ->
            let x = Array.zeroCreate num_action
            d.[model] <- x
            x


    interface IAction<'model,'action> with
        member this.action(model, actions, path_prob, cont) =
            let current_policy : float [] = get_policy model actions.Length
            let rewards = Array.map2 (fun act policy_prob -> cont (act, path_prob *. policy_prob)) actions current_policy
            let avg_reward = Array.fold2 (fun s x policy_prob -> s + x * policy_prob) 0.0 rewards current_policy
            update_policy model rewards avg_reward path_prob
            avg_reward

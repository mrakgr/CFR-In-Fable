module CFR

type IAction<'model,'action> =
    abstract member action : model: 'model * actions: 'action [] * cont: ('action -> float) -> float

type CFR<'model,'action when 'model: equality>(get_policy, update_policy) =
    interface IAction<'model,'action> with
        member this.action(model, actions, cont) =
            let current_policy : float [] = get_policy model
            let rewards = Array.map2 (fun act policy_prob -> cont act * policy_prob) actions current_policy
            let avg_reward = Array.sum rewards
            update_policy model rewards avg_reward
            avg_reward

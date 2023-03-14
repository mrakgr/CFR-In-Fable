module CFR

open System.Collections.Generic

type PathProb = float * float
type IAction<'model,'action> =
    abstract member action : model: 'model * actions: 'action [] * prob: PathProb * cont: ('action * PathProb -> float) -> float

let ( *.) (a : PathProb) (b : float) = fst a * b, snd a

type CFR<'model,'action when 'model: equality>() =
    let d : Dictionary<'model, float[]> = Dictionary()
    let get_policy model len = failwith "todo: get the model from the dictionary"

    interface IAction<'model,'action> with
        member this.action(model, actions, prob, cont) =
            let current_policy : float [] = get_policy model actions.Length
            let rewards = Array.map2 (fun act policy_prob -> cont (act, prob *. policy_prob)) actions current_policy
            0

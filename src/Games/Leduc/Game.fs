module Game

type Action = Fold | Call | Raise
type Card = King | Queen | Jack
type Player = { card : Card; id : uint8; pot : int32 }
type Model = { p1 : Player; p2 : Player; raises_left : int; table_card : Card ValueOption; mask_desk : uint64 }
/// The rewards for player 1.
type Reward = float32

let compare_hands (community_card : Card) (p1 : Player, p2 : Player) =
    let tag = function King -> 2 | Queen -> 1 | Jack -> 0
    let community_card = tag community_card
    let a = tag p1.card, community_card
    let b = tag p2.card, community_card
    let is_pair (a,b) = a = b
    if is_pair a && is_pair b then compare (fst a) (fst b)
    elif is_pair a then 1
    elif is_pair b then -1
    else
        let order (a,b) = if a > b then a,b else b,a
        compare (order a) (order b)

let raiseBy amount (p1 : Player, p2 : Player) = p2.pot + amount

let deck = [|King; King; Queen; Queen; Jack; Jack|]
let sample_card mask = Sampler.sample deck mask

let init () : Model * Reward =
    let mask = 0UL
    let c,mask = sample_card mask
    let p1 : Player = {id=0uy; card=c; pot=1}
    let c,mask = sample_card mask
    let p2 : Player = {id=1uy; card=c; pot=1}
    {p1=p1; p2=p2; raises_left=2; table_card=ValueNone; mask_desk=mask}, 0.0f

let update (model : Model) (msg : Action) =
    match msg with
    | Fold ->
        failwith "TODO"
    | Call ->
        failwith "TODO"
    | Raise ->
        failwith "TODO"

type ActionPicker = { is_fold : bool; is_call : bool; is_raise : bool }
let actions = [|Fold; Call; Raise|]

let actions_allowed (model : Model option) =
    match model with
    | Some model -> {is_fold=model.p1.pot <> model.p2.pot; is_call=true; is_raise=model.raises_left > 0}
    | None -> {is_fold=false; is_call=false; is_raise=false}

let actions_mask (x : ActionPicker) =
    let f b i m = if b then m ||| (1UL <<< i) else m
    0UL |> f x.is_fold 0 |> f x.is_call 1 |> f x.is_raise 2

/// Use the actions_mask instead unless you are enumerating the actions.
let actions_array (x : ActionPicker) = [|
    if x.is_fold then Fold
    if x.is_call then Call
    if x.is_raise then Raise
|]

let sample_action model = Sampler.sample actions (actions_allowed model |> actions_mask) |> fst


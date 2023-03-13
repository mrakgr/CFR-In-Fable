module Leduc.Implementation
open Shared.Constants
open Shared.Leduc.Types
open Leduc.Game

let deck = [|King; King; Queen; Queen; Jack; Jack|]
let actions = [|Fold; Call; Raise|]
let sample_card mask = Sampler.sample deck mask
let sample_action mask = Sampler.sample actions mask

type ILeducChance =
    abstract member chance : id: int option * mask: Mask * cont: (Card * Mask -> unit) -> unit
type ILeducAction =
    abstract member action : model: LeducModel * msgs: string list * allowed_actions: AllowedActions * cont: (Action -> unit) -> unit
type ILeducTerminal =
    abstract member terminal : model: LeducModel * msgs: string list -> unit

type LeducChanceSample() =
    interface ILeducChance with
        member this.chance(_, mask, cont) = sample_card mask |> cont

type LeducTerminalIgnore() =
    interface ILeducTerminal with
        member this.terminal(_, _) = ()
type LeducTerminalDispatch(dispatch) =
    interface ILeducTerminal with
        member this.terminal(model, msgs) = Terminal(model,msgs) |> dispatch

type LeducActionRandom() =
    interface ILeducAction with
        member this.action(_, _, allowed_actions, cont) = sample_action (AllowedActions.Mask allowed_actions) |> fst |> cont

type LeducActionHuman(dispatch) =
    interface ILeducAction with
        member this.action(model, msgs, allowed_actions, cont) = Action(model,msgs,allowed_actions,cont) |> dispatch

type ILeducPlayer =
    abstract member chance : id: int option * mask: Mask * cont: (Card * Mask -> unit) -> unit
    abstract member action : model: LeducModel * msgs: string list * allowed_actions: AllowedActions * cont: (Action -> unit) -> unit
    abstract member terminal : model: LeducModel * msgs: string list -> unit

type Leduc2P(chance : ILeducChance,terminal : ILeducTerminal,p0 : ILeducAction,p1 : ILeducAction) =
    interface ILeducPlayer with
        member this.chance(id, mask, cont) = chance.chance(id,mask,cont)
        member this.action(model, msgs, allowed_actions, cont) =
            if model.p1_id = 0 then p0.action(model, msgs, allowed_actions, cont)
            else p1.action(model, msgs, allowed_actions, cont)
        member this.terminal(model, msgs) = terminal.terminal(model,msgs)

type LeducGame(p : ILeducPlayer) =
    let add_to_msgs msg msgs = Map.map (fun _ msgs -> msg :: msgs) msgs
    let action (is_call_a_check, p1, p2, raises_left, community_card, cont) = fun (_, msgs, mask) ->
        let model : LeducModel = { p1_id = p1.id
                                   p1_card = Some p1.card
                                   p1_pot = p1.pot
                                   p2_id = p2.id
                                   p2_card = Some p2.card
                                   p2_pot = p2.pot
                                   community_card = community_card }
        let msg = $"It is player %s{names[p1.id]}'s turn to act..."
        p.action(model,msg :: Map.find p1.id msgs,AllowedActions.FromModel(model,raises_left),fun a ->
            let msg =
                match a with
                | Fold -> $"Player %s{names[p1.id]} folds."
                | Call when is_call_a_check -> $"Player %s{names[p1.id]} checks."
                | Call -> $"Player %s{names[p1.id]} calls."
                | Raise -> $"Player %s{names[p1.id]} raises."
            cont a (model,add_to_msgs msg msgs,mask)
            )
    let terminal(id, pot) = fun (model, msgs) ->
        let net id' = if id = id' then pot else -pot
        let msg id' =
            match net id' with
            | x when x > 1 -> $"Player %s{names[id']} wins %i{x} chips!"
            | 1 -> $"Player %s{names[id']} wins 1 chip."
            | 0 -> "The two players tie."
            | -1 -> $"Player %s{names[id']} losses 1 chip."
            | x -> $"Player %s{names[id']} losses %i{x} chips!"
        p.terminal(model,msg id :: Map.find id msgs)

    interface ILeducGame<LeducModel * Map<int,string list> * Mask -> unit> with
        member this.chance_init(player_id, cont) = fun (model, msgs, mask) ->
            p.chance(Some player_id, mask, fun (card,mask) ->
                let msg = $"Player %s{names[player_id]} draws a %A{card}"
                let msgs = Map.add player_id (msg :: msgs[player_id]) msgs
                cont card (model,msgs,mask)
                )
        member this.chance_community_card(cont) = fun (model, msgs, mask) ->
            p.chance(None, mask, fun (card,mask) ->
                let msg = $"The community card is a %A{card}"
                cont card (model,add_to_msgs msg msgs,mask)
                )
        member this.action_round_one(is_call_a_check, p1, p2, raises_left, cont) = action (is_call_a_check, p1, p2, raises_left, None, cont)
        member this.action_round_two(is_call_a_check, p1, p2, raises_left, community_card, cont) = action (is_call_a_check, p1, p2, raises_left, Some community_card, cont)
        member this.terminal_call(p1, p2, community_card, id, pot) = fun (model, msgs, _) ->
            msgs
            |> add_to_msgs $"Player %s{names[p1.id]} shows {p1.card}-{community_card}."
            |> add_to_msgs $"Player %s{names[(p1.id + 1) % 2]} shows {p2.card}-{community_card}."
            |> fun msgs -> terminal (id,pot) ({model with p1_pot=p1.pot; p2_pot=p2.pot},msgs)
        member this.terminal_fold(_, id, pot) = fun (model, msgs, _) ->
            terminal (id,pot) (model,msgs)

let game dispatch (p0,p1) =
    let msgs = Map.empty |> Map.add 0 [] |> Map.add 1 []
    let f = function Human -> LeducActionHuman dispatch :> ILeducAction | Random -> LeducActionRandom()
    game(LeducGame (Leduc2P(LeducChanceSample(),LeducTerminalDispatch dispatch,f p0,f p1))) (LeducModel.Default, msgs, 0UL)
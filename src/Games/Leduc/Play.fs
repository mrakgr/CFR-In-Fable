module Leduc.Play
open System.Collections.Generic
open CFR
open CFR.Enumerative
open Leduc.Learn
open Shared.Constants
open Shared.Leduc.Types
open Leduc.Types
open Leduc.Game

type ILeducChance =
    abstract member chance : id: int option * mask: Mask * cont: (Card * Mask -> unit) -> unit
type ILeducAction =
    abstract member action : ui_model: LeducModel * msgs: string list * allowed_actions: AllowedActions * game_model: GameModels * cont: (Action -> unit) -> unit
type ILeducTerminal =
    abstract member terminal : model: LeducModel * msgs: string list -> unit
type ILeducPlayer =
    inherit ILeducChance
    inherit ILeducAction
    inherit ILeducTerminal

type LeducChanceSample() =
    interface ILeducChance with
        member this.chance(_, mask, cont) = sample_card mask |> cont

type LeducActionRandom() =
    interface ILeducAction with
        member this.action(_, _, allowed_actions, _, cont) = sample_action (AllowedActions.Mask allowed_actions) |> fst |> cont
type LeducActionHuman(dispatch) =
    interface ILeducAction with
        member this.action(ui_model, msgs, allowed_actions, _, cont) = Action(ui_model,msgs,allowed_actions,cont) |> dispatch

type LeducActionCFR(d : Dictionary<GameModel,PolicyArrays<Action>>) =
    let agent : IAction<_,_> = AgentPassiveSample(d)

    interface ILeducAction with
        member this.action(ui_model, _, allowed_actions, game_model, cont) =
            agent.action(Utils.get ui_model.p0_id game_model,AllowedActions.Array allowed_actions,(1.0,1.0),fun (a,_) -> cont a; 0.0) |> ignore

type LeducTerminalIgnore() =
    interface ILeducTerminal with
        member this.terminal(_, _) = ()
type LeducTerminalDispatch(dispatch) =
    interface ILeducTerminal with
        member this.terminal(ui_model, msgs) = Terminal(ui_model,msgs) |> dispatch

type Leduc2P(chance : ILeducChance,terminal : ILeducTerminal,p0 : ILeducAction,p1 : ILeducAction) =
    interface ILeducPlayer with
        member this.chance(id, mask, cont) = chance.chance(id,mask,cont)
        member this.action(ui_model, msgs, allowed_actions, game_models, cont) =
            if ui_model.p0_id = 0 then p0.action(ui_model, msgs, allowed_actions, game_models, cont)
            else p1.action(ui_model, msgs, allowed_actions, game_models, cont)
        member this.terminal(ui_model, msgs) = terminal.terminal(ui_model,msgs)

open Utils
type LeducGamePlay(p : ILeducPlayer) =
    let add_to_msgs msg msgs = Map.map (fun _ msgs -> msg :: msgs) msgs

    let action (is_call_a_check, p0, p1, raises_left, community_card, cont) = fun (_, msgs, game_models, mask) ->
        let ui_model : LeducModel =  { p0_id = p0.id
                                       p0_card = Some p0.card
                                       p0_pot = p0.pot
                                       p1_id = p1.id
                                       p1_card = Some p1.card
                                       p1_pot = p1.pot
                                       community_card = community_card }
        let msgs' =
            let msg = $"It is player %s{names[p0.id]}'s turn to act..."
            msg :: Map.find p0.id msgs
        p.action(ui_model,msgs',AllowedActions.FromModel(ui_model,raises_left),game_models,fun a ->
            let msgs =
                let msg =
                    match a with
                    | Fold -> $"Player %s{names[p0.id]} folds."
                    | Call when is_call_a_check -> $"Player %s{names[p0.id]} checks."
                    | Call -> $"Player %s{names[p0.id]} calls."
                    | Raise -> $"Player %s{names[p0.id]} raises."
                add_to_msgs msg msgs
            let game_models =
                let f id = modify id (fun model -> Choice1Of2 a :: model)
                game_models |> f 0 |> f 1
            cont a (ui_model,msgs,game_models,mask)
            )

    let terminal(id, pot) = fun (model, msgs) ->
        let ui_model : LeducModel =
           { model with
               p0_id = get model.p0_id (model.p0_id, model.p1_id)
               p0_card = get model.p0_id (model.p0_card, model.p1_card)
               p0_pot = get model.p0_id (model.p0_pot, model.p1_pot)
               p1_id = get model.p1_id (model.p0_id, model.p1_id)
               p1_card = get model.p1_id (model.p0_card, model.p1_card)
               p1_pot = get model.p1_id (model.p0_pot, model.p1_pot)
           }
        let net id' = if id = id' then pot else -pot
        let msg id' =
            match net id' with
            | x when x > 1 -> $"Player %s{names[id']} wins %i{x} chips!"
            | 1 -> $"Player %s{names[id']} wins 1 chip."
            | 0 -> "The two players tie."
            | -1 -> $"Player %s{names[id']} losses 1 chip."
            | x -> $"Player %s{names[id']} losses %i{x} chips!"
        p.terminal(ui_model,msg id :: Map.find id msgs)

    interface ILeducGame<LeducModel * Map<int,string list> * GameModels * Mask -> unit> with
        member this.chance_init(id, cont) = fun (ui_model, msgs, game_models, mask) ->
            p.chance(Some id, mask, fun (card,mask) ->
                let msgs =
                    let msg = $"Player %s{names[id]} draws a %A{card}"
                    Map.add id (msg :: msgs[id]) msgs
                let game_models = modify id (fun model -> Choice2Of2 card :: model) game_models
                cont card (ui_model,msgs,game_models,mask)
                )
        member this.chance_community_card(cont) = fun (ui_model, msgs, game_models, mask) ->
            p.chance(None, mask, fun (card,mask) ->
                let msgs =
                    let msg = $"The community card is a %A{card}"
                    add_to_msgs msg msgs
                let game_models =
                    let f id = modify id (fun model -> Choice2Of2 card :: model)
                    game_models |> f 0 |> f 1
                cont card (ui_model,msgs,game_models,mask)
                )
        member this.action_round_one(is_call_a_check, p0, p1, raises_left, cont) = action (is_call_a_check, p0, p1, raises_left, None, cont)
        member this.action_round_two(is_call_a_check, p0, p1, raises_left, community_card, cont) = action (is_call_a_check, p0, p1, raises_left, Some community_card, cont)
        member this.terminal_call(p0, p1, community_card, id, pot) = fun (model, msgs, _, _) ->
            msgs
            |> add_to_msgs $"Player %s{names[p0.id]} shows {p0.card}-{community_card}."
            |> add_to_msgs $"Player %s{names[(p0.id + 1) % 2]} shows {p1.card}-{community_card}."
            |> fun msgs -> terminal (id,pot) ({model with p0_pot=p0.pot; p1_pot=p1.pot},msgs)
        member this.terminal_fold(_, id, pot) = fun (model, msgs, _, _) ->
            terminal (id,pot) (model,msgs)

let game d dispatch (p0,p1) =
    let msgs = Map.empty |> Map.add 0 [] |> Map.add 1 []
    let f = function
        | Human -> LeducActionHuman dispatch :> ILeducAction
        | Random -> LeducActionRandom()
        | CFR _ -> LeducActionCFR(d)
    game(LeducGamePlay (Leduc2P(LeducChanceSample(),LeducTerminalDispatch dispatch,f p0,f p1))) (LeducModel.Default, msgs, ([],[]), 0UL)


module Leduc.Implementation
open Leduc.Types

let deck = [|King; King; Queen; Queen; Jack; Jack|]
let sample_card mask = Sampler.sample deck mask

type HumanLeducPlayer(dispatch : MsgLeduc -> unit) =
    let add_to_msgs msg msgs = Map.map (fun _ msgs -> msg :: msgs) msgs
    let action (p1, p2, raises_left, community_card, cont) = fun (_, msgs) ->
        let model : LeducModel = { p1_card = Some p1.card
                                   p1_pot = p1.pot
                                   p2_card = Some p2.card
                                   p2_pot = p2.pot
                                   community_card = community_card }
        let msg = $"It is player %i{p1.id}'s turn to act..."
        dispatch <| Action(model,msg :: Map.find p1.id msgs,AllowedActions.FromModel(model,raises_left),fun a ->
            let msg = $"Player %i{p1.id} %A{a}s."
            cont a (model,add_to_msgs msg msgs)
            )
    let terminal(id, pot) = fun (model, msgs) ->
        let net id' = if id = id' then pot else -pot
        let msg id' =
            match net id' with
            | x when x > 1 -> $"Player %i{id'} wins %i{x} chips!"
            | 1 -> $"Player %i{id'} wins 1 chip."
            | 0 -> "The two players tie."
            | -1 -> $"Player %i{id'} losses 1 chip."
            | x -> $"Player %i{id'} losses %i{x} chips!"
        dispatch <| Terminal(model,msg id :: Map.find id msgs)

    interface ILeducGame<LeducModel * Map<int,string list> -> unit> with
        member this.chance_init(player_id, mask, cont) = fun (model, msgs) ->
            let card,mask = sample_card mask
            let msg = $"Player %i{player_id} draws a %A{card}"
            let msgs = Map.add player_id (msg :: msgs[player_id]) msgs
            cont (card,mask) (model,msgs)
        member this.chance_community_card(mask, cont) = fun (model, msgs) ->
            let card,_ = sample_card mask
            let msg = $"The community card is a %A{card}"
            cont card (model,add_to_msgs msg msgs)
        member this.action_round_one(p1, p2, raises_left, cont) = action (p1, p2, raises_left, None, cont)
        member this.action_round_two(p1, p2, raises_left, community_card, cont) = action (p1, p2, raises_left, Some community_card, cont)
        member this.terminal_call(p1, p2, community_card, id, pot) = fun (model, msgs) ->
            let msgs =
                msgs
                |> add_to_msgs $"Player %i{p1.id} calls!"
                |> add_to_msgs $"Player %i{p1.id} shows {p1.card}-{community_card}."
                |> add_to_msgs $"Player %i{(p1.id + 1) % 2} shows {p2.card}-{community_card}."

            terminal (id,pot) (model,msgs)
        member this.terminal_fold(p1, id, pot) = fun (model, msgs) ->
            let msgs = add_to_msgs $"Player %i{p1.id} folds!" msgs
            terminal (id,pot) (model,msgs)

let game_vs_self dispatch =
    let msgs = Map.empty |> Map.add 0 [] |> Map.add 1 []
    Game.game (HumanLeducPlayer dispatch) (LeducModel.Default,msgs)
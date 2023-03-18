module Shared.Messages

open Shared.Leduc.Types

type MsgServerToClient =
    | GameState of LeducModel * string list * AllowedActions
    | TrainingResult of (float * float)
    | TrainingModel of Map<string list,string []>

type MsgServerFromClient =
    | SelectedAction of Action
    | StartGame of p0: PlayerType * p1: PlayerType
    | Train of num_iter: int

type MsgServer =
    | FromLeducGame of MsgLeduc
    | FromClient of MsgServerFromClient


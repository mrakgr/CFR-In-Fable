module Shared.Messages

open Shared.Leduc.Types

type TrainingModelT = Map<GameModel,(Action * float) []>
type MsgServerToClient =
    | GameState of LeducModel * string list * AllowedActions
    | TrainingResult of float * float
    | TrainingModel of TrainingModelT
    | TestingResult of float
    | TestingModel of TrainingModelT

type MsgServerFromClient =
    | SelectedAction of Action
    | StartGame of p0: PlayerType * p1: PlayerType
    | Train of num_iter: int
    | Test of num_iter: int

type MsgServer =
    | FromLeducGame of MsgLeduc
    | FromClient of MsgServerFromClient


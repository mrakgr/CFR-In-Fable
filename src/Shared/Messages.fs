module Shared.Messages

open Shared.Leduc.Types
open Shared.Leduc.Types.CFR
open Shared.Leduc.Types.UI

type MsgServerToClient =
    | GameState of LeducModel * string list * AllowedActions
    | TrainingResult of float * float
    | TrainingModel of CFRPlayerType * CFRPlayerModel
    | TestingResult of float
    | TestingModel of CFRPlayerType * CFRPlayerModel
    | ConnectionOpen of CFRPlayerType * is_train: bool

type MsgClientToServer =
    | SelectedAction of Action
    | StartGame of p0: PlayerModel * p1: PlayerModel
    | Train of num_iter: int * pl: CFRPlayerModel
    | Test of num_iter: int * pl: CFRPlayerModel

type MsgServer =
    | FromLeducGame of MsgLeduc
    | FromClient of MsgClientToServer


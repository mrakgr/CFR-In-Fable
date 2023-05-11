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

type MsgClientToPlayServer =
    | SelectedAction of Action
    | StartGame of p0: PlayerModel * p1: PlayerModel

type MsgClientToLearnServer =
    | Train of num_iter: uint * pl: CFRPlayerModel
    | Test of num_iter: uint * pl: CFRPlayerModel

type MsgPlayServer =
    | FromLeducGame of MsgLeduc
    | FromClient of MsgClientToPlayServer

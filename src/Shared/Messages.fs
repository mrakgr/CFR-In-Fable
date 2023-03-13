module Shared.Messages

open Shared.Leduc.Types

type MsgServerToClient =
    | GameState of LeducModel * string list * AllowedActions

type MsgServerFromClient =
    | SelectedAction of Action
    | StartGame of p0: PlayerType * p1: PlayerType

type MsgServer =
    | FromLeducGame of MsgLeduc
    | FromClient of MsgServerFromClient


module Shared

open Leduc.Types

type MsgServerToClient =
    | GameState of LeducModel * string list * AllowedActions

type MsgServerFromClient =
    | SelectedAction of Action
    | StartGame

type MsgServer =
    | FromLeducGame of MsgLeduc
    | FromClient of MsgServerFromClient

let endpoint = "/socket"
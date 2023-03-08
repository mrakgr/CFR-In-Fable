module Shared

open Leduc.Types

type MsgServerToClient =
    | LeducModel of Client.Model
    | MessageList of string list
type MsgServer =
    | EndGame of reward: float
    | GetAction of Server.Model
    | ClickedOn of Action
    | StartGame

let endpoint = "/socket"
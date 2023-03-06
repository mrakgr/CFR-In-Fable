module Shared

type MsgServerToClient =
    | MessageList of string list
type MsgServer =
    | ConfirmYouGotIt of string

let endpoint = "/socket"
module App

open Fable.SignalR

let hub : HubConnection<obj,obj,obj,obj,obj> =
    SignalR.connect <| fun x -> x.onMessage ignore
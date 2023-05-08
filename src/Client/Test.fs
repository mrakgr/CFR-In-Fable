module Client.Test

open Fable.Bindings
open Fable.Bindings.SignalR.Exports

let t = 11234

let hub =
    HubConnectionBuilder.Create()
        .withUrl("test")
        .withAutomaticReconnect()
        .build()


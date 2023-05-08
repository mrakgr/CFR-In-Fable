module Client.Bindings_Test

open SignalR
open Fable.Core

let hub =
    signalr.HubConnectionBuilder.Create()
        .withUrl("test")
        .withAutomaticReconnect()
        .build()

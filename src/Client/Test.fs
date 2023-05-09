module Client.Test

open Fable.Core
open Fable.Bindings.SignalR
open Fable.Bindings.SignalR.Exports

let hub =
    HubConnectionBuilder.Create()
        .withUrl("socket/test")
        .withAutomaticReconnect()
        .build()

hub.on("response", fun x ->
    printfn "%A" x
    None
    )

promise {
    do! hub.start()
    do! hub.send("Hello", [|unbox "How are you today?"|])
} |> Promise.start


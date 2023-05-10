module Client.Test

open Fable.Bindings.SignalR
open Fable.Bindings.SignalR.Exports
open Thoth.Json

printfn "%A" (Fable.Bindings.SignalR.Exports.Utils.VERSION)

// printfn "%A" Fable.Bindings.SignalR.Protocols.

// type W = Qwe of float | Asd of bool
// type R = {x : string; y : W}
//
// let hub =
//     HubConnectionBuilder.Create()
//         .withUrl("socket/test")
//         .withAutomaticReconnect()
//         .build()
//
// hub.on("response", fun (q : string) ->
//     printfn "%A" (Decode.Auto.fromString<R> q)
//     )
//
// promise {
//     do! hub.start()
//     do! hub.send("Hello", Encode.Auto.toString {x = "How are you today?"; y = Qwe 1.23})
// } |> ignore


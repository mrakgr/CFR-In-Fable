﻿namespace Fun

open System.Threading
open Microsoft.AspNetCore.Components

type ViewComponent() as this =
    inherit ComponentBase()
    
    let token_source = new CancellationTokenSource()
    let cancellation_token = token_source.Token
    
    let mb init update =
        let mb =
            new MailboxProcessor<_>(fun mb -> async {
                let mutable model = init
                while true do
                    let! msg = mb.Receive()
                    model <- update msg model mb.Post
                }) // Removing the cancellation token makes it work.
        mb.Start()
        mb
        
    let srv = mb () (fun x () _ -> printfn "%s" x)
    
    do srv.Post("Hello from view")
    do this.Say("123456789")
    
    member _.Say(x) = srv.Post x
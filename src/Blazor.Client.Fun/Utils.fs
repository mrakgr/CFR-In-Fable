namespace Blazor.Client

module FunUtils =
    open System

    type TestData = {
        value : float32
        iteration : int
    }

    let chart_data i =
        let rng = Random()
        Array.init (i*4) (fun i -> {iteration=i/4; value=rng.NextSingle()})

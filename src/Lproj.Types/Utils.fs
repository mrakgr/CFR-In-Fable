module Lproj.FunUtils

open System

type TestData = {
    value : float32
    iteration : int
}

let chart_data i =
    let rng = Random()
    Array.init i (fun i -> {iteration=i; value=rng.NextSingle()})

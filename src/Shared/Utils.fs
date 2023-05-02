namespace Shared

module Utils =

    let to_map d = d |> Seq.map (|KeyValue|) |> Map

    module CFR =
        let normalize (x : float []) =
            let s = Array.sum x
            if s = 0.0 then Array.replicate x.Length (1.0 / float x.Length)
            else Array.map (fun x -> x / s) x
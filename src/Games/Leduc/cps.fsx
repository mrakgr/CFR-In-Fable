let f<'r>(ret : unit -> 'r): 'r =
    if true then
        ret ()
    else
        ret ()

let q = f (fun () x -> x+5)
q 10

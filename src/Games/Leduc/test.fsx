open System

// /// Weirdly, the functional tail call version is twice as fast as the imperative one.
// /// I really should be using a benchmarking library to make absolutely sure, but this is good enough rough measure.
// /// Who is going to take the time to import the library right now?
// let parallel_bit_deposit_u64 (value, mask) =
//     let rec loop s i c =
//         if i+c < 64 then
//             if (1UL <<< (i+c)) &&& mask <> 0UL then loop (s ||| (((1UL <<< i) &&& value) <<< c)) (i+1) c
//             else loop s i (c+1)
//         else s
//
//     loop 0UL 0 0

// let parallel_bit_deposit_u64' (value, mask) =
//     let mutable s = 0UL
//     let mutable i = 0
//     let mutable c = 0
//     while i+c < 64 do
//         if (1UL <<< (i+c)) &&& mask <> 0UL then
//             s <- s ||| (((1UL <<< i) &&& value) <<< c)
//             i <- i + 1
//         else
//             c <- c + 1
//     s
//
// #time "on"
// for i = 1 to 10_000_000 do
//     let f n = Random.Shared.NextInt64(0L,n) |> uint64
//     let t = f Int64.MaxValue, f Int64.MaxValue
//     let q = parallel_bit_deposit_u64' t
//     let w = Runtime.Intrinsics.X86.Bmi2.X64.ParallelBitDeposit t
//     if q <> w then failwith "q <> w"
// #time "off"
//
// let f n = Random.Shared.NextInt64(0L,n) |> uint64
// #time "on"
// for i = 1 to 10_000_000 do
//     parallel_bit_deposit_u64' (0UL, 0UL)
// #time "off"

let parallel_bit_deposit_u64 (value, mask) =
    let rec loop s i c =
        if i+c < 64 then
            if (1UL <<< (i+c)) &&& mask <> 0UL then loop (s ||| (((1UL <<< i) &&& value) <<< c)) (i+1) c
            else loop s i (c+1)
        else s

    loop 0UL 0 0

/// Samples an action from an array given a mask.
/// Returns the sampled action and the new mask.
/// Whem mask = 0, it samples a completely random action.
/// The pop count of the mask should be less than the length of the array
let sample (actions : 'action []) (mask : uint64) =
    let i =
        let popcnt = Numerics.BitOperations.PopCount mask
        let i = Random.Shared.Next(0, actions.Length - popcnt)
        if Runtime.Intrinsics.X86.Bmi2.IsSupported then
            Runtime.Intrinsics.X86.Bmi2.X64.ParallelBitDeposit(1UL <<< i, ~~~mask)
        else
            parallel_bit_deposit_u64 (1UL <<< i, ~~~mask)
        |> Numerics.BitOperations.TrailingZeroCount
    actions[i], mask ||| (1UL <<< i)


let mask_bit i = 1UL <<< i
let ar = [|1;2;3;4;5|]

let result =
    [|
        for i=0 to 999 do
            yield [|
                let mutable mask = 0UL
                for _ in ar do
                    let action, mask' = sample ar mask
                    mask <- mask'
                    yield action
            |]
    |]

result |> Array.forall (fun x -> Array.distinct x = x)


// [|
// for i=0 to 99 do
//     yield (sample [|1;2;3;4;5|] (mask_bit 1 ||| mask_bit 2))
// |]
// |> Array.groupBy fst
// |> Array.map (fun (a,b) -> a, b.Length)
// |> Array.sortBy fst

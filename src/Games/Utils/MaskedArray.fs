module MaskedArray

open System

/// Here is my own off the cuff implementation, but I spotted another more efficient implementation in here:
/// https://stackoverflow.com/questions/38938911/portable-efficient-alternative-to-pdep-without-using-bmi2
/// I do not feel like redoing it now. 3/7/2023
let private parallel_bit_deposit_u64 (value, mask) =
    let rec loop s i c =
        if i+c < 64 then
            if (1UL <<< (i+c)) &&& mask <> 0UL then loop (s ||| (((1UL <<< i) &&& value) <<< c)) (i+1) c
            else loop s i (c+1)
        else s

    loop 0UL 0 0

let len (x : 'el []) (mask : uint64) =
    let popcnt = Numerics.BitOperations.PopCount mask
    x.Length - popcnt
/// Samples an element from an array given a mask.
/// Returns the sampled action and the new mask.
/// Whem mask = 0, it samples a completely random action.
/// The pop count of the mask should be less than the length of the array
let sample (x : 'el []) (mask : uint64) =
    let len = len x mask
    let i =
        let i = Random.Shared.Next(0, len)
        if Runtime.Intrinsics.X86.Bmi2.IsSupported then
            Runtime.Intrinsics.X86.Bmi2.X64.ParallelBitDeposit(1UL <<< i, ~~~mask)
        else
            parallel_bit_deposit_u64(1UL <<< i, ~~~mask)
        |> Numerics.BitOperations.TrailingZeroCount
    x[i], mask ||| (1UL <<< i), 1.0 / float len

/// Iterates over all the elements of the array given the mask.
let inline iter f (x : 'el []) (mask : uint64) =
    let len = len x mask
    x |> Array.iteri (fun i x ->
        if mask &&& (1UL <<< i) = 0UL then f (x, mask ||| (1UL <<< i), 1.0 / float len)
        )

/// Filters out the elements based on the array.
let filter (x : 'el []) (mask : uint64) =
    let ar = ResizeArray(x.Length)
    iter (fun (a,_,_) -> ar.Add a) x mask
    ar.ToArray()

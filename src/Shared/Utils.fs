namespace Shared

open System

module Utils =

    let to_map d = d |> Seq.map (|KeyValue|) |> Map

    module CFR =
        let normalize (x : float []) =
            let s = Array.sum x
            if s = 0.0 then Array.replicate x.Length (1.0 / float x.Length)
            else Array.map (fun x -> x / s) x

    module SerializationUtils =
        open FSharp.Reflection

        let rec is_serializable (t : System.Type) =
            FSharpType.IsTuple t || FSharpType.IsRecord t || FSharpType.IsUnion t
            || (t.IsArray && is_serializable (t.GetElementType()))

        let rec serialize' (t' : System.Type) (x : obj) : obj =
            if FSharpType.IsTuple t' then
                (FSharpType.GetTupleElements t', FSharpValue.GetTupleFields x)
                ||> Array.map2 serialize'
                |> box
            elif FSharpType.IsRecord t' then
                (FSharpType.GetRecordFields t', FSharpValue.GetRecordFields x)
                ||> Array.map2 (fun t x -> serialize' t.PropertyType x)
                |> box
            elif FSharpType.IsUnion t' then
                let q,w = FSharpValue.GetUnionFields(x,t')
                let tag = box (byte q.Tag)
                let body =
                    (q.GetFields(),w)
                    ||> Array.map2 (fun q w -> serialize' q.PropertyType w)
                    |> box
                [| tag; body |] |> box
            elif t'.IsArray then
                let t = t'.GetElementType()
                if is_serializable t then
                    Array.map (serialize' t) ( x :?> obj []) |> box
                else
                    x
            else
                x

        let rec deserialize' (t' : System.Type) (x : obj) : obj =
            if FSharpType.IsTuple t' then
                let t,x = FSharpType.GetTupleElements t', x :?> obj []
                (t,x) ||> Array.map2 deserialize'
                |> fun x -> FSharpValue.MakeTuple(x,t')
            elif FSharpType.IsRecord t' then
                let t,x = FSharpType.GetRecordFields t', x :?> obj []
                (t,x) ||> Array.map2 (fun t x -> deserialize' t.PropertyType x)
                |> fun x -> FSharpValue.MakeRecord(t',x)
            elif FSharpType.IsUnion t' then
                match FSharpType.GetUnionCases(t'), x :?> obj [] with
                | q, [|tag; body|] ->
                    let tag = tag :?> byte
                    let body = body :?> obj []
                    let q = q[int tag]
                    let body = (q.GetFields(),body) ||> Array.map2 (fun q w -> deserialize' q.PropertyType w)
                    FSharpValue.MakeUnion(q,body)
                | _ -> failwith "expected a tag and body"
            elif t'.IsArray then
                let t = t'.GetElementType()
                let inline body () =
                    let x = x :?> obj []
                    let ar =
                        #if FABLE_COMPILER
                        Array.zeroCreate x.Length : obj []
                        #else
                        System.Array.CreateInstance(t,x.Length) :?> obj []
                        #endif
                    for i=0 to x.Length-1 do
                        ar[i] <- deserialize' t x[i]
                    ar
                #if FABLE_COMPILER
                if is_serializable t' then
                    body()
                #else
                body()
                #endif
            else
                #if FABLE_COMPILER
                x
                #else
                Convert.ChangeType(x,t')
                #endif

    type public Serialization = // This should inline across modules.
        static member inline deserialize<'a> (x : obj) : 'a = SerializationUtils.deserialize' typeof<'a> x :?> 'a
        static member inline serialize<'a> (x : 'a) : obj = SerializationUtils.serialize' typeof<'a> x

open FSharp.Reflection

let rec is_serializable (t : System.Type) =
    FSharpType.IsTuple t || FSharpType.IsRecord t || FSharpType.IsUnion t
    || (t.IsArray && is_serializable (t.GetElementType()))

let rec private serialize' (t' : System.Type) (x : obj) : obj =
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
        let tag = box q.Tag
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

let serialize<'a> (x : 'a) : obj = serialize' typeof<'a> x

let rec private deserialize' (t' : System.Type) (x : obj) : obj =
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
            let tag = tag :?> int
            let body = body :?> obj []
            let q = q[tag]
            let body = (q.GetFields(),body) ||> Array.map2 (fun q w -> deserialize' q.PropertyType w)
            FSharpValue.MakeUnion(q,body)
        | _ -> failwith "expected a tag and body"
    elif t'.IsArray then
        let t = t'.GetElementType()
        if is_serializable t then
            let x = x :?> obj []
            let ar = System.Array.CreateInstance(t,x.Length) :?> obj []
            for i=0 to x.Length-1 do
                ar[i] <- deserialize' t x[i]
            ar
        else
            x
    else
        x

let deserialize<'a> (x : obj) : 'a = deserialize' typeof<'a> x :?> 'a

type R = {x : int; y : string}
// serialize (1,2,3,4)
serialize {x = 1; y = "qwe"}
|> deserialize<R>

let l = ['a';'b';'c';'d']
serialize l

let l' = [{x=123; y="a"};{x=234; y="b"};{x=345; y="c"}]
l' = (serialize l' |> deserialize<R list>)

let a = [| {x=123; y="a"};{x=234; y="b"};{x=345; y="c"} |]
serialize a |> deserialize<R array>
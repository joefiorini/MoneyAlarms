// [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]

let bimap f g e =
    match e with
    | Ok x -> Ok (f x)
    | Error x -> Error (g x)
let inline map f e = bimap f id e
let inline mapError f e = bimap id f e

let sequence s =
    let folder (acc:Result<seq<'T>,seq<'E>>) (me:Result<'T,'E>) =
        match me,acc with
        | Error x, Error errors -> Error (Seq.append errors [x])
        | Error x, Ok _ -> Error (Seq.singleton x)
        | Ok _, Error errors -> Error errors
        | Ok x, Ok items -> Ok (Seq.append items [x])
    s |> Seq.fold folder (Ok Seq.empty)
let sequenceList s =
    sequence s |> bimap List.ofSeq List.ofSeq
let sequenceCollect f s =
    sequence s |> bimap (Seq.collect f) (Seq.collect id)

let unsafeGet =
    function
    | Ok a -> a
    | Error e -> invalidArg "result" (sprintf "The result value was Error '%A'" e)

let getOr errorF =
    function
    | Ok a -> a
    | Error e -> errorF e

let toOption e =
    match e with
    | Ok x -> Some x
    | Error x -> None

let tryCatch f =
    try
        Ok <| f()
    with
        | e -> Error e

let returnM = Ok
let inline bind f =
    function
    | Ok x -> f x
    | Error x -> Error x


type ResultBuilder() =
    member inline this.Return a = returnM a
    member inline this.Bind (m, f) = bind f m
    member inline this.Delay f = returnM () |> bind f
    member inline this.ReturnFrom m = m
    member inline this.Zero() = returnM ()
    member inline this.Combine(r1, r2) = r1 |> bind (fun _ -> r2)

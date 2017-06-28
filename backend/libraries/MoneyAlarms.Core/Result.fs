namespace FSharp.Extensions

type Result<'Ok, 'Error> =
    | Ok of 'Ok
    | Error of 'Error

[<AutoOpen>]
module Result =
    let map f inp = match inp with Error e -> Error e | Ok x -> Ok (f x)
    let mapError mapping result = match result with Error e -> Error (mapping e) | Ok x -> Ok x
    let bind f inp = match inp with Error e -> Error e | Ok x -> f x

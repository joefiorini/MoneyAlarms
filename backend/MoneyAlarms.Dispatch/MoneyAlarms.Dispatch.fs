module MoneyAlarms.Dispatch

open FSharp.Data
open FSharp.Data.JsonExtensions

type Dispatcher =
    | Command of (string -> Result<unit, string>)
    | UnknownCommand of string

let loadAction =
    function
        | "Blah" -> Command <| fun _ -> Error "Oops, I suck"
        | "Doo" ->
            Command <|
              fun payload ->
                do printfn "Got payload: %A" payload
                Ok ()
        | (v: string) -> UnknownCommand v

[<EntryPoint>]
let main argv =
    printfn "Args: %A" argv
    let payload = JsonValue.Parse(argv.[0])
    let actionName = payload?action_name.AsString()
    match loadAction actionName with
        | Command fn -> fn argv.[0]
        | _ -> Error <| sprintf "Action not found: %s" actionName
    |> function
        | Ok _ ->
            printfn """{ "result": "success" }"""
            0
        | Error e ->
            printfn """{ "result": "failure", "message": "%s" }""" e
            1

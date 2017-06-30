module MoneyAlarms.Dispatch.Entry

open FSharp.Data
open FSharp.Data.JsonExtensions

type Dispatcher =
    | Command of (JsonValue -> Result<unit, string>)
    | UnknownCommand of string

let loadAction =
    function
        | "ExchangeTokens" -> Command MoneyAlarms.Dispatch.Commands.exchangeTokens
        | "TestError" -> Command <| fun _ -> Error "Oops, I suck"
        | "TestSuccess" ->
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
        | Command fn -> fn payload
        | _ -> Error <| sprintf "Action not found: %s" actionName
    |> function
        | Ok _ ->
            printfn """{ "result": "success" }"""
            0
        | Error e ->
            printfn """{ "result": "failure", "message": "%s" }""" e
            1

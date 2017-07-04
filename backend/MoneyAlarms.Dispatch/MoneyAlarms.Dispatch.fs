module MoneyAlarms.Dispatch.Entry

open Chiron
open Aether
open Aether.Operators

type Dispatcher =
    | Command of (Json -> Result<unit, string>)
    | UnknownCommand of string

let loadAction =
    function
        // | "ExchangeTokens" -> Command MoneyAlarms.Dispatch.Commands.exchangeTokens
        | "PersistTransactions" -> Command MoneyAlarms.Dispatch.PersistTransactions.run
        // | "TestError" -> Command <| fun _ -> Error "Oops, I suck"
        // | "TestSuccess" ->
        //     Command <|
        //       fun payload ->
        //         do printfn "Got payload: %A" payload
        //         Ok ()
        | (v: string) -> UnknownCommand v

let actionName_ =
    Json.Object_ >?> Map.key_ "action_name" >?> Json.String_

[<EntryPoint>]
let main argv =
    printfn "Args: %A" argv
    let payload = Json.parse(argv.[0])
    let (Value actionName), payload =
      payload
      |> Json.Optic.get actionName_

    match loadAction actionName with
        | Command fn -> fn payload
        | _ -> Result.Error <| sprintf "Action not found: %s" actionName
    |> function
        | Ok _ ->
            printfn """{ "result": "success" }"""
            0
        | Result.Error e ->
            printfn """{ "result": "failure", "message": "%s" }""" e
            1

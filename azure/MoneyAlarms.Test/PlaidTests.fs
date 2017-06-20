module PlaidTests

open Expecto
open System
open System.IO
open Scotch
open DotEnvFile

open Plaid

// printfn "current dir: %s" ()
let cassettePath = AppDomain.CurrentDomain.BaseDirectory + "../MoneyAlarms.Test/cassettes"
let dotEnvPath = AppDomain.CurrentDomain.BaseDirectory + "../../.env"
let vars = DotEnvFile.LoadFile(dotEnvPath)
DotEnvFile.InjectIntoEnvironment(vars)

let shouldRecord =
  Environment.GetEnvironmentVariable "SCOTCH_MODE_RECORDING" = "true"

let scotchMode =
  if shouldRecord then
      printfn "Scotch set to Record"
      ScotchMode.Recording
  else
      printfn "Scotch set to Playback"
      ScotchMode.Replaying

[<Tests>]
let tests =
  testList "Plaid Client"
    [ testCase "returns the public token" <| fun _ ->
        let httpClient = HttpClients.NewHttpClient(Path.Combine(cassettePath, "plaid.successes.json"), scotchMode)

        let serviceConfig =
          configurePlaidService
            httpClient
            (Environment.GetEnvironmentVariable "PLAID_CLIENT_ID")
            (Environment.GetEnvironmentVariable "PLAID_SECRET")
            "https://sandbox.plaid.com"
        let publicToken = "public-sandbox-75bd6831-ce8c-47a4-92ab-b80616d54674"
        let result = plaidExchangeToken serviceConfig publicToken
        let expected = ("access-sandbox-265c9150-193c-4b8e-a6c2-189411592c7e", "LKLD5AVZLMSDkvgpdXvKF949PEa1nlcBwrmpG")
        Expect.equal result (Ok expected) "Result"

      testCase "returns error for invalid token" <| fun _ ->
        let httpClient = HttpClients.NewHttpClient(Path.Combine(cassettePath, "plaid.errors.json"), scotchMode)

        let serviceConfig =
          configurePlaidService
            httpClient
            (Environment.GetEnvironmentVariable "PLAID_CLIENT_ID")
            (Environment.GetEnvironmentVariable "PLAID_SECRET")
            "https://sandbox.plaid.com"

        let publicToken = "invalid"
        let result = plaidExchangeToken serviceConfig publicToken

        // TODO: Make PlaidError type match structure printed here and use JsonProvider
        // to parse it
        match result with
          | Error (PlaidError e) -> printfn "Got error: %s" e

        printfn "result: %O" result
        Expect.isError result "Result is error"
    ]

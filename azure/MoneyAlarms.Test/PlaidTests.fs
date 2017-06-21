module PlaidTests

open Expecto
open System
open Scotch
open Plaid
open ScotchSetup

[<Tests>]
let tests =
  testList "Plaid Client"
    [ testCase "returns the public token" <| fun _ ->
        let httpClient = httpClientForCassette "plaid.successes.json"

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
        let httpClient = httpClientForCassette "plaid.errors.json"

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
          | Error (PlaidError (InvalidPublicToken, e)) ->
            Expect.equal e.ErrorType "INVALID_INPUT" "ErrorType"
          | Error e -> Tests.failtest "Got unexpected error type %O"

        Expect.isError result "Result is error"
    ]

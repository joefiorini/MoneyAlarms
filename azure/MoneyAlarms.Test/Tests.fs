module Tests

open Expecto
open MoneyAlarms.Core
open Firebase
open Plaid

type CheckError = MoneyAlarmsError -> MoneyAlarmsError

let exampleError = DetailedError.GetSample()

let expectResult actualR expected msg =
  Expect.isOk actualR msg
  match actualR with
      | Ok actual -> Expect.equal actual expected msg
      | Error e -> Tests.failtest (e.ToString())
  ()

let expectError actualR expected msg =
  Expect.isError actualR msg
  match actualR with
      | Ok _ -> Tests.failtest "Not an error"
      | Error (MoneyAlarms.Core.FirebaseError (FirebaseError s)) ->
        Expect.equal s expected msg
      | Error (MoneyAlarms.Core.ExchangeTokenError s) ->
        Expect.equal s expected msg
  ()

[<Tests>]
let tests =
  testList "ExchangeTokens" [
    testCase "Returns account on success" <| fun _ ->
      let plaidExchangeToken publicToken =
        Ok ("AccessToken", "AccountId")

      let firebaseCreateAccount account =
        Ok account

      let dto: TokenExchangeDto =
        { PlaidPublicToken = "PublicToken"
          FirebaseUserId = "UserId"
        }
      let account =
        ExchangeTokens.createAccount
          plaidExchangeToken
          firebaseCreateAccount
          dto

      let expected =
        { ItemAccessToken = "AccessToken"
          AccountId = "AccountId"
          UserId = "UserId"
        }

      expectResult account expected "Expected account to be equal"

    testCase "Returns error when plaid fails" <| fun _ ->
      let dto =
        { PlaidPublicToken = "Public"
          FirebaseUserId = ""
        }

      let plaidExchangeToken publicToken =
        Plaid.PlaidError (UnknownError, exampleError) |> Error

      let firebaseCreateAccount account =
        Firebase.FirebaseError "Firebase Error" |> Error

      let result =
        ExchangeTokens.createAccount
          plaidExchangeToken
          firebaseCreateAccount
          dto

      match result with
        | Error (MoneyAlarms.Core.PlaidError (PlaidError (errorCode, detailedError))) ->
           Expect.equal errorCode UnknownError "UnknownError"
           Expect.equal detailedError exampleError "Detailed Error"

      Expect.isError result "isError result"

    testCase "Returns error when firebase fails" <| fun _ ->
      let dto =
        { PlaidPublicToken = "Public"
          FirebaseUserId = ""
        }

      let plaidExchangeToken publicToken =
        Ok ("AccessToken", "AccountId")

      let firebaseCreateAccount account =
        Firebase.FirebaseError "Firebase Error" |> Error

      let result =
        ExchangeTokens.createAccount
          plaidExchangeToken
          firebaseCreateAccount
          dto

      expectError result "Firebase Error" "Not a firebase error"

  ]

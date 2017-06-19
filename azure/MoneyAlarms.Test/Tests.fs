module Tests

open Expecto
open MoneyAlarms.Core
open Firebase

let plaidExchangeToken publicToken =
    Ok ("AccessToken", "AccountId")

let firebaseCreateAccount account =
    Ok account

[<Tests>]
let tests =
  testList "ExchangeTokens" [
    testCase "Returns account on success" <| fun _ ->
      let dto: TokenExchangeDto =
        { PlaidPublicToken = "PublicToken"
          FirebaseUserId = "UserId"
        }
      let account =
        ExchangeTokens.createAccount
          plaidExchangeToken
          firebaseCreateAccount
          dto
      match account with
      | Ok a ->
        Expect.equal a.ItemAccessToken "AccessToken" "ItemAccessToken"
        Expect.equal a.AccountId "AccountId" "PlaidAccountId"
        Expect.equal a.UserId "UserId" "FirebaseUserId"
      | Error e -> Tests.failtest "Error calling createAccount"
  ]

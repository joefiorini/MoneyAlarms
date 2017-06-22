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

let plaidExchangeToken publicToken =
    Ok "AccessToken"

let plaidGetAccounts accessToken =
    Ok <| Plaid.Accounts.AccountsJson.GetSample()

let plaidGetInstitutionName institutionId =
    Expect.equal institutionId
        <| Plaid.Accounts.AccountsJson.GetSample().Item.InstitutionId
        <| "Institution Ids"
    Ok "Chase"

let firebasePersistAccountsAndItem account =
    Ok <| ignore account

let addItem userId item =
    Ok item

[<Tests>]
let tests =
  testList "ExchangeTokens" [
    testCase "Returns account on success" <| fun _ ->
      let dto: TokenExchangeDto =
        { PlaidPublicToken = "PublicToken"
          FirebaseUserId = "UserId"
        }

      let accounts =
        Commands.CreateAccount.run
          plaidExchangeToken
          plaidGetAccounts
          plaidGetInstitutionName
          firebasePersistAccountsAndItem
          addItem
          dto

      let expected =
        [ { AccountId = "ekvG5RD76BCWRRw1eJ8vhdQrK7DdoLSLDbnva"
            Name = "Plaid Checking"
            OfficialName = "Plaid Gold Standard 0% Interest Checking"
            Mask = "0000"
            Type = "depository"
            SubType = "checking"
            InstitutionName = "Chase"
          }
          { AccountId = "Q4Jd5RAvzLsW44wNKva9h9R3Pvd9yzSpogAP3"
            Name = "Plaid Saving"
            OfficialName = "Plaid Silver Standard 0.1% Interest Saving"
            Mask = "1111"
            Type = "depository"
            SubType = "savings"
            InstitutionName = "Chase"
          }
        ]

      expectResult (Result.map Array.toList accounts) (expected) "Expected account to be equal"

    testCase "Returns error when plaid fails" <| fun _ ->
      let dto =
        { PlaidPublicToken = "Public"
          FirebaseUserId = ""
        }

      let plaidExchangeToken publicToken =
        Plaid.PlaidError (UnknownError, exampleError) |> Error

      let result =
        Commands.CreateAccount.run
          plaidExchangeToken
          plaidGetAccounts
          plaidGetInstitutionName
          firebasePersistAccountsAndItem
          addItem
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

      let firebaseCreateAccount account =
        Firebase.FirebaseError "Firebase Error" |> Error

      let result =
        Commands.CreateAccount.run
          plaidExchangeToken
          plaidGetAccounts
          plaidGetInstitutionName
          firebasePersistAccountsAndItem
          addItem
          dto

      expectError result "Firebase Error" "Not a firebase error"

  ]

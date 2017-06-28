module Tests

open Expecto
open MoneyAlarms.Core
open Firebase
open Plaid
open Microsoft.Azure.WebJobs.Host

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

type Log (level) =
    inherit TraceWriter(level:System.Diagnostics.TraceLevel)
    new () = Log(System.Diagnostics.TraceLevel.Verbose)
    override this.Trace(event) = printfn "%A" event

[<Tests>]
let tests =
  testList "ExchangeTokens" [
    testCase "Returns account on success" <| fun _ ->
      let dto: TokenExchangeDto =
        { PlaidPublicToken = "PublicToken"
          FirebaseUserId = "UserId"
        }

      let result =
        Commands.CreateAccount.run
          (Log())
          plaidExchangeToken
          plaidGetAccounts
          plaidGetInstitutionName
          firebasePersistAccountsAndItem
          addItem
          dto

      Expect.isOk result "Expected account to be equal"

    testCase "Returns error when plaid fails" <| fun _ ->
      let dto =
        { PlaidPublicToken = "Public"
          FirebaseUserId = ""
        }

      let plaidExchangeToken publicToken =
        Plaid.PlaidError (UnknownError, exampleError) |> Error

      let result =
        Commands.CreateAccount.run
          (Log())
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
          (Log())
          plaidExchangeToken
          plaidGetAccounts
          plaidGetInstitutionName
          firebasePersistAccountsAndItem
          addItem
          dto

      expectError result "Firebase Error" "Not a firebase error"

  ]

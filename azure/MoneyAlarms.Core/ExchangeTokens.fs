module MoneyAlarms.Core.ExchangeTokens

open Plaid
open Firebase

type MakeAccount = FirebaseUserId -> ItemAccessToken -> AccountId -> FirebaseAccount
type ExchangeToken = PlaidPublicToken -> Result<PlaidAccessToken * PlaidItemId,Error>
type DoCreateAccount = FirebaseAccount -> Result<FirebaseAccount,Error>
type CreateAccount = ExchangeToken -> DoCreateAccount -> TokenExchangeDto -> Result<FirebaseAccount, Error>

// let inline |!> res otf =
//     res |> Result.bind ||> otf

let makeAccount: MakeAccount =
    fun firebaseUserId itemAccessToken plaidAccountId ->
      { UserId = firebaseUserId
        AccountId = plaidAccountId
        ItemAccessToken = itemAccessToken
      }

// type Map<'a,'b,'c,'d> = Result<'a*'b,'c> -> ('a*'b) -> 'd
// let map: Map<'a,'b,'c,'d> =
//     fun r f ->
//         match r with
//           | Ok v -> f v
//           | Error e -> Error e

let createAccount: CreateAccount =
    fun plaidExchangeToken' firebaseCreateAccount' dto ->
      let tokenResult = plaidExchangeToken' dto.PlaidPublicToken
      match tokenResult with
          | Ok t -> t ||> makeAccount dto.FirebaseUserId |> Ok
          | Error e -> Error e
      |> Result.bind firebaseCreateAccount'

module MoneyAlarms.Core.ExchangeTokens

open Plaid
open Firebase
open Microsoft.FSharp.Reflection

type MakeAccount = FirebaseUserId -> PlaidAccessToken -> AccountId -> FirebaseAccount
type ExchangeToken = PlaidPublicToken -> Result<PlaidAccessToken * PlaidItemId, Plaid.PlaidError>
type DoCreateAccount = FirebaseAccount -> Result<FirebaseAccount,FirebaseError>
type CreateAccount = ExchangeToken -> DoCreateAccount -> TokenExchangeDto -> Result<FirebaseAccount, MoneyAlarmsError>

let makeAccount: MakeAccount =
    fun firebaseUserId itemAccessToken plaidAccountId ->
      { UserId = firebaseUserId
        AccountId = plaidAccountId
        ItemAccessToken = itemAccessToken
      }

type ResultTupleMap<'a,'b,'c,'d> = Result<'a*'b,'c> -> ('a -> 'b -> 'd) -> Result<'d,'c>
let (|*>): ResultTupleMap<'a,'b,'c,'d> =
    fun r f -> Result.map (fun v -> v ||> f) r

let toDomainType<'T> externalType: 'T =
  let found = FSharpType.GetUnionCases(typeof<'T>) |> Array.find (fun t -> t.Name = externalType.GetType().Name)
  FSharpValue.MakeUnion(found, [| externalType |]) :?> 'T

let mapDomainError externalResult: Result<_,MoneyAlarmsError> =
    Result.mapError toDomainType<MoneyAlarmsError> externalResult

type BindDomainError<'a,'e> = ('a -> Result<'a,'e>) -> Result<'a,MoneyAlarmsError> -> Result<'a,MoneyAlarmsError>
let bindDomainError: BindDomainError<'a,'e> =
    fun fn ->
      Result.bind (fun t -> mapDomainError <| fn t)

let createAccount: CreateAccount =
    fun plaidExchangeToken firebaseCreateAccount dto ->
      plaidExchangeToken dto.PlaidPublicToken
        |> mapDomainError
        |*> makeAccount dto.FirebaseUserId
        |> bindDomainError firebaseCreateAccount

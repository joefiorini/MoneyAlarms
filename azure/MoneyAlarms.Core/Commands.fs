namespace MoneyAlarms.Core.Commands

open Plaid
open Firebase
open Microsoft.FSharp.Reflection
open FSharp.Extensions
open MoneyAlarms.Core

type MakeAccount = FirebaseUserId -> PlaidAccessToken -> AccountId -> FirebaseAccount

  module DomainTypes =
    let toDomainType<'T> externalType: 'T =
      let found = FSharpType.GetUnionCases(typeof<'T>) |> Array.find (fun t -> t.Name = externalType.GetType().Name)
      FSharpValue.MakeUnion(found, [| externalType |]) :?> 'T

    let mapError externalResult: Result<_,MoneyAlarmsError> =
        Result.mapError toDomainType<MoneyAlarmsError> externalResult

    type BindDomainError<'a,'e> = ('a -> Result<'a,'e>) -> Result<'a,MoneyAlarmsError> -> Result<'a,MoneyAlarmsError>
    let bindError: BindDomainError<'a,'e> =
        fun fn ->
          Result.bind (fun t -> mapError <| fn t)

module CreateAccount =
  type ResultTupleMap<'a,'b,'c,'d> = Result<'a*'b,'c> -> ('a -> 'b -> 'd) -> Result<'d,'c>
  let (|*>): ResultTupleMap<'a,'b,'c,'d> =
      fun r f -> Result.map (fun v -> v ||> f) r

  let private makeAccount: MakeAccount =
      fun firebaseUserId itemAccessToken plaidAccountId ->
          { UserId = firebaseUserId
            AccountId = plaidAccountId
            ItemAccessToken = itemAccessToken
          }

  type Run = Plaid.ExchangeToken -> Firebase.CreateAccount -> TokenExchangeDto -> Result<FirebaseAccount, MoneyAlarmsError>
  let run: Run =
      fun plaidExchangeToken firebaseCreateAccount dto ->
        plaidExchangeToken dto.PlaidPublicToken
          |> DomainTypes.mapError
          // TODO: Get the account name, official name, type, subtype from plaid
          |*> makeAccount dto.FirebaseUserId
          |> DomainTypes.bindError firebaseCreateAccount

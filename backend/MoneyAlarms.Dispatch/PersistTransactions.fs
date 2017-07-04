module MoneyAlarms.Dispatch.PersistTransactions

open System.Net.Http
open Chiron
open Chiron.Operators
open Plaid
open MoneyAlarms.Core
open MoneyAlarms.Core.Commands
open Firebase
open MoneyAlarms.Dispatch

type Command =
    { ItemId: string
      FirebaseUserId: string
      NewTransactionCount: int
    }

    static member FromJson (_: Command) =
          fun itemId firebaseUserId newCount ->
              { ItemId = itemId
                FirebaseUserId = firebaseUserId
                NewTransactionCount = newCount
              }
        <!> Json.read "item_id"
        <*> Json.read "firebase_user_id"
        <*> Json.read "new_transactions"

type TwicePerMonth =
    | First
    | Fifteenth

type BudgetPeriod =
    | TwicePerMonth of TwicePerMonth

type ItemId = string
type TransactionCount = int

// let (<.>)<'T> (obj: IDottable) key: 'T = obj.getField(key)

// let (<!>) r fn = Result.bind fn r
// let (<*>) r fn = Result.map fn r

let run payload =
    let command = payload |> Json.deserialize
    let httpClient = new HttpClient()
    printfn "Getting config"
    let configResult = ServiceConfigHelpers.Plaid.fromPayload payload
    printfn "Maybe got config"
    let plaidConfig =
      match configResult with
          | Value a, json -> a httpClient
          | Error e, json ->
              printfn "Error getting plaid config: %A" e
              raise <| System.ArgumentException("Blah")

    // let plaidConfig = getPlaidConfig httpClient
    printfn "Got config: %A" plaidConfig

    result {
        let! accessToken = Ok "access-sandbox-554c07a0-84b6-4a5e-b6da-08a05e6ac7a6"
          // PlaidItems.getAccessTokenForItemId command.ItemId |> DomainTypes.mapError

        let! cachedValues =
          CachedValues.getForUser command.FirebaseUserId |> DomainTypes.mapError

        let startDate = "2017-06-01"
        let endDate = "2017-06-30"

        return!
          Transactions.Query.Create startDate endDate (cachedValues.LastTransactionCount + command.NewTransactionCount) accessToken
          |> Transactions.get plaidConfig
          |> DomainTypes.mapError
          |> Persistence.save "transactions"
          |> DomainTypes.mapError

        // return! Persistence.save "transactions" tr
        //   List.fold
        //       (fun r t ->
        //           match r with
        //             | Ok _ -> Persistence.save "transactions" t
        //             | Result.Error e -> Result.Error e)
        //       <| Ok ()
        //       <| transactions


    } |> Result.mapError string

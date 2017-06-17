#r "System.Net.Http"
#r "Newtonsoft.Json"
#r "MoneyAlarms.dll"

#load ".paket/load/fsharp.data.fsx"

open System.Net
open System.Net.Http
open Newtonsoft.Json
open MoneyAlarms.Types
open FSharp.Data

type Named = {
    name: string
}

let inline |!> res otf =
    res |> Result.bind ||> otf

let tokenExchangeSample = """
  {
    "client_id": String,
    "secret": String,
    "public_token": "public-sandbox-fb7cca4a-82e6-4707"
  }
"""
type PlaidTokenExchangeBody = JsonProvider<tokenExchangeSample>

let getAccessToken plaidService -> publicToken: GetAccessToken =
    let body = PlaidTokenExchangeBody
      ( plaidService.ClientId,
        plaidService.Secret,
        publicToken )

    body.JsonValue.Request "https://sandbox.plaid.com/item/public_token/exchange"

let makeAccount firebaseUserId plaidAccountId itemAccessToken: MakeAccount =
    { FirebaseUserId = firebaseUserId
      PlaidAccountId = plaidAccountId
      ItemAccessToken = itemAccessToken
    }

let createAccount firebaseService plaidService dto: CreateAccount =
    async {
      return getAccessToken plaidService dto.PublicToken
        |!> makeAccount dto.FirebaseUserId
        |> saveAccountToFirebase firebaseService
    }

let Run(req: HttpRequestMessage, log: TraceWriter) =
    async {
        let! data = req.Content.ReadAsStringAsync() |> Async.AwaitTask

        if (String.isNullOrEmpty(data)) then
          return req.CreateResponse(HttpStatusCode.BadRequest, "Need a body")
        else
          let dto = JsonConvert.DeserializeObject<TokenExchangeDto>(data)
          let! account = (createAccount dto) |> Async.AwaitTask

          match exchangeResult with
          | Ok () ->
              return req.CreateResponse(HttpStatusCode.OK, "Good");
          | Error e ->
              return req.CreateResponse(HttpStatusCode.InternalServerError, sprintf "Error: %s" e)
    } |> Async.RunSynchronously

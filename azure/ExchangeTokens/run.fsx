#r "System.Net.Http"
#r "Newtonsoft.Json"
#r "MoneyAlarms.Core.dll"

#load ".paket/load/fsharp.data.fsx"

open System
open System.Net
open System.Net.Http
open Newtonsoft.Json
open MoneyAlarms.Core
open FSharp.Data

type Result<'Ok,'Error> =
    | Ok of 'Ok
    | Error of 'Error

let Run(req: HttpRequestMessage, log: TraceWriter) =
    async {
        let! data = req.Content.ReadAsStringAsync() |> Async.AwaitTask

        // if (String.IsNullOrEmpty(data)) then
        //     return req.CreateResponse(HttpStatusCode.BadRequest, "Need a body")
        // else
        let httpClient = new HttpClient()
        let dto = JsonConvert.DeserializeObject<TokenExchangeDto>(data)

        let plaidConfig = Plaid.ServiceConfig.fromEnvironment(httpClient)
        let plaidExchangeToken = Plaid.plaidExchangeToken plaidConfig

        let firebaseToken = Firebase.AccessToken.generate()
        let firebaseConfig =
                Firebase.ServiceConfig.fromEnvironment httpClient firebaseToken

        let firebaseCreateAccount =
              Firebase.SaveAccount.firebaseCreateAccount firebaseConfig

        let exchangeResult =
            ExchangeTokens.createAccount
              plaidExchangeToken
              firebaseCreateAccount
              dto

        return
          match exchangeResult with
          | Ok _ ->
              req.CreateResponse(HttpStatusCode.OK, "Good");
          | Error e ->
              req.CreateResponse(HttpStatusCode.InternalServerError, sprintf "Error: %s" e)
    } |> Async.StartAsTask

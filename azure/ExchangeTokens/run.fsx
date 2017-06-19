#r "System.Net.Http"
#r "Newtonsoft.Json"
#r "MoneyAlarms.dll"

#load ".paket/load/fsharp.data.fsx"

open System.Net
open System.Net.Http
open Newtonsoft.Json
open MoneyAlarms
open FSharp.Data

let Run(req: HttpRequestMessage, log: TraceWriter) =
    async {
        let! data = req.Content.ReadAsStringAsync() |> Async.AwaitTask

        if (String.isNullOrEmpty(data)) then
          return req.CreateResponse(HttpStatusCode.BadRequest, "Need a body")
        else
          let dto = JsonConvert.DeserializeObject<TokenExchangeDto>(data)
          let plaidExchangeToken' = Plaid.configurePlaidService "" "" "" |> Plaid.plaidExchangeToken
          let! account = (createAccount plaidExchangeToken' dto) |> Async.AwaitTask

          match exchangeResult with
          | Ok () ->
              return req.CreateResponse(HttpStatusCode.OK, "Good");
          | Error e ->
              return req.CreateResponse(HttpStatusCode.InternalServerError, sprintf "Error: %s" e)
    } |> Async.RunSynchronously

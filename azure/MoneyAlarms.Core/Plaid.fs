module Plaid

open System
open System.Net.Http
open FSharp.Data

type PlaidClientId = string
type PlaidSecret = string
type PlaidAccountId = string
type PlaidHost = string
type PlaidPublicToken = string
type PlaidItemId = string
type PlaidAccessToken = string

type PlaidServiceConfig =
    { HttpClient: HttpClient
      ClientId: PlaidClientId
      Secret: PlaidSecret
      Host: PlaidHost
    }

type PlaidError =
    | PlaidError of string

// Configuration
type PlaidServiceEndpoint<'t> = PlaidServiceConfig -> 't
type ConfigurePlaidService = HttpClient -> PlaidClientId -> PlaidSecret -> PlaidHost -> PlaidServiceConfig

// Endpoints
type ExchangeToken = PlaidPublicToken -> Result<PlaidAccessToken * PlaidItemId, PlaidError>

[<Literal>]
let tokenExchangeSample = """
    {
        "client_id": "client_id",
        "secret": "secret",
        "public_token": "public-sandbox-fb7cca4a-82e6-4707"
    }
"""
type PlaidTokenExchangeRequestBody = JsonProvider<tokenExchangeSample>

[<Literal>]
let tokenExchangeResponseSample = """
    {
        "access_token": "access_token",
        "item_id": "item_id",
        "request_id": "request_id"
    }
"""
type PlaidTokenExchangeResponseBody = JsonProvider<tokenExchangeResponseSample>

let configurePlaidService: ConfigurePlaidService =
    fun httpClient clientId secret host ->
      { HttpClient = httpClient
        ClientId = clientId
        Secret = secret
        Host = host
      }

let plaidExchangeToken: PlaidServiceEndpoint<ExchangeToken> =
    fun plaidServiceConfig publicToken ->
        async {
            let url = plaidServiceConfig.Host + "/item/public_token/exchange"
            let requestBody =
              PlaidTokenExchangeRequestBody.Root
                ( plaidServiceConfig.ClientId,
                  plaidServiceConfig.Secret,
                  publicToken
                )

            let request = new HttpRequestMessage()
            request.RequestUri <- new Uri(url)
            request.Method <- System.Net.Http.HttpMethod.Post

            let reqContent = new StringContent(requestBody.JsonValue.ToString())
            reqContent.Headers.ContentType <- new Headers.MediaTypeHeaderValue("application/json")
            request.Content <- reqContent

            let! response = plaidServiceConfig.HttpClient.SendAsync(request) |> Async.AwaitTask

            let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask

            if not response.IsSuccessStatusCode then
              return Error (PlaidError content)
            else
              let parsedContent = PlaidTokenExchangeResponseBody.Parse content
              return Ok (parsedContent.AccessToken, parsedContent.ItemId)
        } |> Async.RunSynchronously

    // let body = PlaidTokenExchangeBody
    //     ( plaidServiceConfig.ClientId,
    //       plaidServiceConfig.Secret,
    //       publicToken )

    // body.JsonValue.Request "https://sandbox.plaid.com/item/public_token/exchange"

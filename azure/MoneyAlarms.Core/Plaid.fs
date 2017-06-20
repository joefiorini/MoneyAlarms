module Plaid

open System.Net.Http

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

// let tokenExchangeSample = """
//     {
//         "client_id": String,
//         "secret": String,
//         "public_token": "public-sandbox-fb7cca4a-82e6-4707"
//     }
// """
// type PlaidTokenExchangeBody = JsonProvider<tokenExchangeSample>

let configurePlaidService: ConfigurePlaidService =
    fun httpClient clientId secret host ->
      { HttpClient = httpClient
        ClientId = clientId
        Secret = secret
        Host = host
      }

let plaidExchangeToken: PlaidServiceEndpoint<ExchangeToken> =
    fun plaidServiceConfig publicToken -> Ok ("test", "test")
    // let body = PlaidTokenExchangeBody
    //     ( plaidServiceConfig.ClientId,
    //       plaidServiceConfig.Secret,
    //       publicToken )

    // body.JsonValue.Request "https://sandbox.plaid.com/item/public_token/exchange"

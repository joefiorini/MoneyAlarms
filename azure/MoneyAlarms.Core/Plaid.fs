module Plaid

type PlaidClientId = string
type PlaidSecret = string
type PlaidAccountId = string
type PlaidHost = string
type PlaidPublicToken = string
type PlaidItemId = string
type PlaidAccessToken = string

type PlaidServiceConfig =
    { ClientId: PlaidClientId
      Secret: PlaidSecret
      Host: PlaidHost
    }

type PlaidError = string

type ConfigurePlaidService = PlaidClientId -> PlaidSecret -> PlaidHost -> PlaidServiceConfig
type PlaidExchangeToken = PlaidServiceConfig -> PlaidPublicToken -> Result<PlaidAccessToken * PlaidItemId, PlaidError>

// let tokenExchangeSample = """
//     {
//         "client_id": String,
//         "secret": String,
//         "public_token": "public-sandbox-fb7cca4a-82e6-4707"
//     }
// """
// type PlaidTokenExchangeBody = JsonProvider<tokenExchangeSample>

let configurePlaidService: ConfigurePlaidService =
    fun clientId secret host ->
      { ClientId = clientId
        Secret = secret
        Host = host
      }

let plaidExchangeToken: PlaidExchangeToken =
    fun plaidServiceConfig publicToken -> Ok ("test", "test")
    // let body = PlaidTokenExchangeBody
    //     ( plaidServiceConfig.ClientId,
    //       plaidServiceConfig.Secret,
    //       publicToken )

    // body.JsonValue.Request "https://sandbox.plaid.com/item/public_token/exchange"

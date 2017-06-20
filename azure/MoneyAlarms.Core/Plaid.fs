module Plaid

open System
open FSharp.Data
open System.Net.Http
open HttpUtils

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

// Configuration
type PlaidServiceEndpoint<'t> = PlaidServiceConfig -> 't
type ConfigurePlaidService = HttpClient -> PlaidClientId -> PlaidSecret -> PlaidHost -> PlaidServiceConfig

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

[<Literal>]
let detailedErrorSample = """
{
  "display_message": "something",
  "error_code": "INVALID_PUBLIC_TOKEN",
  "error_message": "provided public token is expired. Public tokens expire 30 minutes after creation at which point they can no longer be exchanged",
  "error_type": "INVALID_INPUT",
  "request_id": "qPqXA"
}
"""
type DetailedError = JsonProvider<detailedErrorSample>

type ErrorCode =
    | InvalidPublicToken
    | InvalidAccessToken
    | InvalidApiKeys
    | InvalidProduct
    | InvalidAccountId
    | InvalidInstitution
    | MissingFields
    | UnknownFields
    | InvalidField
    | InvalidBody
    | InvalidHeaders
    | NotFound
    | SandboxOnly
    | AdditionLimit
    | AuthLimit
    | TransactionsLimit
    | IdentityLimit
    | IncomeLimit
    | RateLimit
    | ItemGetLimit
    | InternalServerError
    | PlannedMaintenance
    | InvalidCredentials
    | InvalidMfa
    | ItemLocked
    | ItemLoginRequired
    | ItemNotSupported
    | UserSetupRequired
    | MfaNotSupported
    | NoAccounts
    | NoAuthAccounts
    | ProductNotReady
    | InstitutionDown
    | InstitutionNotResponding
    | InstitutionNotAvailable
    | InstitutionNoLongerSupported
    | UnknownError

type PlaidError =
    | PlaidError of (ErrorCode * DetailedError.Root)

type ErrorCodeFromString = string -> ErrorCode
let errorCodeFromString =
    function
        | "INVALID_CREDENTIALS" -> InvalidCredentials
        | "MISSING_FIELDS" -> MissingFields
        | "UNKNOWN_FIELDS" -> UnknownFields
        | "INVALID_FIELD" -> InvalidField
        | "INVALID_BODY" -> InvalidBody
        | "INVALID_HEADERS" -> InvalidHeaders
        | "NOT_FOUND" -> NotFound
        | "SANDBOX_ONLY" -> SandboxOnly
        | "INVALID_API_KEYS" -> InvalidApiKeys
        | "INVALID_ACCESS_TOKEN" -> InvalidAccessToken
        | "INVALID_PUBLIC_TOKEN" -> InvalidPublicToken
        | "INVALID_PRODUCT" -> InvalidProduct
        | "INVALID_ACCOUNT_ID" -> InvalidAccountId
        | "INVALID_INSTITUTION" -> InvalidInstitution
        | "ADDITION_LIMIT" -> AdditionLimit
        | "AUTH_LIMIT" -> AuthLimit
        | "TRANSACTIONS_LIMIT" -> TransactionsLimit
        | "IDENTITY_LIMIT" -> IdentityLimit
        | "INCOME_LIMIT" -> IncomeLimit
        | "RATE_LIMIT" -> RateLimit
        | "ITEM_GET_LIMIT" -> ItemGetLimit
        | "INTERNAL_SERVER_ERROR" -> InternalServerError
        | "PLANNED_MAINTENANCE" -> PlannedMaintenance
        | "INVALID_MFA" -> InvalidMfa
        | "ITEM_LOCKED" -> ItemLocked
        | "ITEM_LOGIN_REQUIRED" -> ItemLoginRequired
        | "ITEM_NOT_SUPPORTED" -> ItemNotSupported
        | "USER_SETUP_REQUIRED" -> UserSetupRequired
        | "MFA_NOT_SUPPORTED" -> MfaNotSupported
        | "NO_ACCOUNTS" -> NoAccounts
        | "NO_AUTH_ACCOUNTS" -> NoAuthAccounts
        | "PRODUCT_NOT_READY" -> ProductNotReady
        | "INSTITUTION_DOWN" -> InstitutionDown
        | "INSTITUTION_NOT_RESPONDING" -> InstitutionNotResponding
        | "INSTITUTION_NOT_AVAILABLE" -> InstitutionNotAvailable
        | "INSTITUTION_NO_LONGER_SUPPORTED" -> InstitutionNoLongerSupported
        | _ -> UnknownError

// Endpoints
type ExchangeToken = PlaidPublicToken -> Result<PlaidAccessToken * PlaidItemId, PlaidError>

let configurePlaidService: ConfigurePlaidService =
    fun httpClient clientId secret host ->
      { HttpClient = httpClient
        ClientId = clientId
        Secret = secret
        Host = host
      }

let plaidExchangeToken: PlaidServiceEndpoint<ExchangeToken> =
    fun plaidServiceConfig publicToken ->
            let url = plaidServiceConfig.Host + "/item/public_token/exchange"
            let requestBody =
              PlaidTokenExchangeRequestBody.Root
                ( plaidServiceConfig.ClientId,
                  plaidServiceConfig.Secret,
                  publicToken
                )

            let (response: HttpResponseMessage, content) =
              PostJson.sync plaidServiceConfig.HttpClient url (requestBody.JsonValue.ToString())

            if not response.IsSuccessStatusCode then
              let parsedError = DetailedError.Parse content
              let errorCode = errorCodeFromString parsedError.ErrorCode
              Error (PlaidError (errorCode, parsedError))
            else
              let parsedContent = PlaidTokenExchangeResponseBody.Parse content
              Ok (parsedContent.AccessToken, parsedContent.ItemId)

    // let body = PlaidTokenExchangeBody
    //     ( plaidServiceConfig.ClientId,
    //       plaidServiceConfig.Secret,
    //       publicToken )

    // body.JsonValue.Request "https://sandbox.plaid.com/item/public_token/exchange"

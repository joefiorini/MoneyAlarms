module Plaid

open System
open FSharp.Data
open System.Net.Http
open HttpUtils
open FSharp.Extensions

type PlaidClientId = string
type PlaidSecret = string
type PlaidAccountId = string
type PlaidHost = string
type PlaidPublicToken = string
type PlaidItemId = string
type PlaidAccessToken = string
type PlaidPublicKey = string

type PlaidServiceConfig =
    { HttpClient: HttpClient
      ClientId: PlaidClientId
      Secret: PlaidSecret
      PublicKey: PlaidPublicKey
      Host: PlaidHost
    }

(*
Exchange Tokens
  input: publicToken
  output: accessToken, itemId
Get item:
  input: accessToken
  output: itemDto list
Get institution:
  input: InstitutionId
  output: InstitutionDto

AccountsDto:
  - AccountId
  - AccessToken
  - Name
  - OfficialName
  - Mask
  - Type
  - SubType
  - InstitutionId

InstitutionDto:
  - name
  -
*)
// Configuration
type PlaidServiceEndpoint<'t> = PlaidServiceConfig -> 't
type ConfigurePlaidService = PlaidClientId -> PlaidSecret -> PlaidHost -> PlaidPublicKey ->  HttpClient -> PlaidServiceConfig

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

type PlaidResult<'T> = Result<'T, PlaidError>

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
type ExchangeToken = PlaidPublicToken -> PlaidResult<PlaidAccessToken>

module ServiceConfig =
    let private (<~) fn s =
        Environment.GetEnvironmentVariable s |> fn

    let create: ConfigurePlaidService =
        fun clientId secret host publicKey httpClient ->
            { HttpClient = httpClient
              ClientId = clientId
              Secret = secret
              PublicKey= publicKey
              Host = host
            }

    let fromEnvironment httpClient =
      create
          <~ "PLAID_CLIENT_ID"
          <~ "PLAID_SECRET"
          <~ "PLAID_HOST"
          <~ "PLAID_PUBLIC_KEY"
          <| httpClient

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
        Ok parsedContent.AccessToken

module Institutions =
    [<Literal>]
    let institutionDtoSample = """
      {
        "institution": {
          "credentials": [
            {
              "label": "User ID",
              "name": "username",
              "type": "text"
            },
            {
              "label": "Password",
              "name": "password",
              "type": "password"
            }
          ],
          "has_mfa": true,
          "institution_id": "ins_3",
          "mfa": [
            "code",
            "list"
          ],
          "name": "Chase",
          "products": [
            "auth",
            "balance",
            "credit_details",
            "identity",
            "income",
            "transactions"
          ]
        },
        "request_id": "Ht7Nj"
      }
    """
    type InstitutionDto = JsonProvider<institutionDtoSample, RootName="institution">

    [<Literal>]
    let institutionRequestSample = """
    {
      "institution_id": "ins_3",
      "public_key": "PUBLIC_KEY"
    }
    """
    type InstitutionRequestDto = JsonProvider<institutionRequestSample>

    type InstitutionId = string
    type Get = InstitutionId -> PlaidResult<InstitutionDto.Institution>
    type GetName = InstitutionId -> PlaidResult<string>

    let get: PlaidServiceEndpoint<Get> =
      fun serviceConfig institutionId ->
        let url = serviceConfig.Host + "/institutions/get_by_id"
        let requestBody =
            InstitutionRequestDto.Root
                ( institutionId,
                  serviceConfig.PublicKey
                )

        let (response: HttpResponseMessage, content) =
            PostJson.sync serviceConfig.HttpClient url (requestBody.JsonValue.ToString())

        if not response.IsSuccessStatusCode then
            let parsedError = DetailedError.Parse content
            let errorCode = errorCodeFromString parsedError.ErrorCode
            Error (PlaidError (errorCode, parsedError))
        else
            Ok <| InstitutionDto.Parse content


    let getName: PlaidServiceEndpoint<GetName> =
        fun serviceConfig institutionId ->
            Result.map (fun (dto: InstitutionDto.Institution) -> dto.Institution.Name) (get serviceConfig institutionId)

module Accounts =
    [<Literal>]
    let accountsRequestSample = """
    {
      "client_id": "$PLAID_CLIENT_ID",
      "secret": "$PLAID_SECRET",
      "access_token": "access-sandbox-7de224ab-7359-4876-9360-4bf286e80a39"
    }
    """
    type GetAccountsDto = JsonProvider<accountsRequestSample>

    [<Literal>]
    let accountsDtoSample = """
        {
          "accounts": [
            {
              "account_id": "ekvG5RD76BCWRRw1eJ8vhdQrK7DdoLSLDbnva",
              "balances": {
                "available": 100,
                "current": 110,
                "limit": null
              },
              "mask": "aaaa",
              "name": "Plaid Checking",
              "official_name": "Plaid Gold Standard 0% Interest Checking",
              "subtype": "checking",
              "type": "depository"
            },
            {
              "account_id": "Q4Jd5RAvzLsW44wNKva9h9R3Pvd9yzSpogAP3",
              "balances": {
                "available": 200,
                "current": 210,
                "limit": null
              },
              "mask": "aaaa",
              "name": "Plaid Saving",
              "official_name": "Plaid Silver Standard 0.1% Interest Saving",
              "subtype": "savings",
              "type": "depository"
            }
          ],
          "item": {
            "available_products": [
              "balance",
              "credit_details",
              "identity",
              "income"
            ],
            "billed_products": [
              "auth",
              "transactions"
            ],
            "error": null,
            "institution_id": "ins_3",
            "item_id": "KznK5LWGR7UMBBJPXzRQUyRxylgQlPHeLxnLzM",
            "webhook": "https://requestb.in/s6e29ss6"
          },
          "request_id": "pfk6E"
        }
    """
    type AccountsJson  = JsonProvider<accountsDtoSample>
    type AccountsDto = AccountsJson.Root
    type AccountDto = AccountsJson.Account
    type ItemDto = AccountsJson.Item
    type Get = PlaidAccessToken -> PlaidResult<AccountsDto>
    let get: PlaidServiceEndpoint<Get> =
        fun serviceConfig accessToken ->
                let url = serviceConfig.Host + "/accounts/get"
                let requestBody =
                    GetAccountsDto.Root
                        ( serviceConfig.ClientId,
                          serviceConfig.Secret,
                          accessToken
                        )

                let (response: HttpResponseMessage, content) =
                    PostJson.sync serviceConfig.HttpClient url (requestBody.JsonValue.ToString())

                if not response.IsSuccessStatusCode then
                    let parsedError = DetailedError.Parse content
                    let errorCode = errorCodeFromString parsedError.ErrorCode
                    Error (PlaidError (errorCode, parsedError))
                else
                    Ok <| AccountsJson.Parse content

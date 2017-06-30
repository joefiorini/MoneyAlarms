namespace Firebase

open FSharp.Data
open FSharp.Extensions
open HttpUtils
open System
open System.Net.Http

type FirebaseUserId = string
type ItemAccessToken = string
type AccountId = string

type FirebaseServiceConfig =
    { ApiKey: string
      AuthDomain: string
      DatabaseUrl: string
      ProjectId: string
      StorageBucket: string
      MessageSenderId: string
      UserToken: string
      HttpClient: HttpClient
    }

type FirebaseAccount =
    { UserId: FirebaseUserId
      ItemAccessToken: ItemAccessToken
      AccountId: AccountId
    }

type FirebaseError =
    | FirebaseError of string

type FirebaseServiceEndpoint<'t> = FirebaseServiceConfig -> 't
type CreateAccount =  FirebaseAccount -> Result<FirebaseAccount,FirebaseError>

module AccessToken =

  open System
  open System.Threading
  open System.IO
  open Google.Apis.Auth.OAuth2

  open HttpUtils

  let generate (serviceAccountEmail: string) (adminPrivateKey: string) =
      async {
        let privateKey =
          adminPrivateKey.Replace("\\n", Environment.NewLine)
        let initializer = new ServiceAccountCredential.Initializer(serviceAccountEmail)
        initializer.Scopes <- ["https://www.googleapis.com/auth/userinfo.email"; "https://www.googleapis.com/auth/firebase.database"]
        let credential = new ServiceAccountCredential(initializer.FromPrivateKey(privateKey))
        let! status = credential.RequestAccessTokenAsync(CancellationToken.None) |> Async.AwaitTask
        if status then
            return credential.Token.AccessToken
        else
          failwith "Did not get a token"
          return ""
      } |> Async.RunSynchronously

module ServiceConfig =

  open System
  open System.Net.Http

  type Create = string -> string -> string -> string -> string -> string -> string -> HttpClient -> FirebaseServiceConfig

  let createConfig: Create =
      fun apiKey authDomain databaseUrl projectId storageBucket messageSenderId userToken httpClient ->
      { ApiKey = apiKey
        AuthDomain = authDomain
        DatabaseUrl = databaseUrl
        ProjectId = projectId
        StorageBucket = storageBucket
        MessageSenderId = messageSenderId
        UserToken = userToken
        HttpClient = httpClient
      }

  let private (<~) fn s =
      Environment.GetEnvironmentVariable s |> fn

  type FromEnvironment = HttpClient -> string -> FirebaseServiceConfig
  let fromEnvironment: FromEnvironment =
      fun httpClient accessToken ->
        createConfig
          <~ "FIREBASE_API_KEY"
          <~ "FIREBASE_AUTH_DOMAIN"
          <~ "FIREBASE_DATABASE_URL"
          <~ "FIREBASE_PROJECT_ID"
          <~ "FIREBASE_STORAGE_BUCKET"
          <~ "FIREBASE_MESSAGE_SENDER_ID"
          <| accessToken
          <| httpClient

  let internal makeDatabaseUrl config s = config.DatabaseUrl + s + "?access_token=" + config.UserToken

module SaveAccount =
(*
   AccountDto:
    - FirebaseUserId
    - PlaidAccountId
    - PlaidAccessToken
    - Name
    - OfficialName
    - Mask
    - Type
    - SubType
    - InstitutionName
*)

  [<Literal>]
  let accountDtoSample = """
      {
          "firebase_user_id": "5Hu5khcyxmgmOTMMm1AzlcyDClC2",
          "plaid_account_id": "y4W8N4pk4ET5nNbAqZM6HaEPoDGRVLioJW1Jw",
          "name": "Plaid Checking",
          "official_name": "Plaid Gold Checking",
          "type": "Depository",
          "subtype": "checking",
          "mask": "aaaa",
          "institution_name": "Houndstooth Bank"
      }
  """
  type AccountDto = JsonProvider<accountDtoSample>

  [<Literal>]
  let createAccountBodySample = """
      {
          "user_id": "client_id",
          "account_id": "y4W8N4pk4ET5nNbAqZM6HaEPoDGRVLioJW1Jw",
          "access_token": "access-sandbox-265c9150-193c-4b8e-a6c2-189411592c7e"
      }
  """
  type CreateAccountRequestBody = JsonProvider<createAccountBodySample>

  type GetAccountByAccountId = FirebaseUserId -> string -> Result<Option<string>, FirebaseError>
  let getAccountByAccountId: FirebaseServiceEndpoint<GetAccountByAccountId> =
      // TODO: This is broken because I need to query by the PlaidAccountId, but this
      // is looking for a Firebase ID
      fun serviceConfig userId accountId ->
          let url =
              [ "/users"
                userId
                "accounts"
                (accountId + ".json")
              ] |> String.concat "/"

          let (response: HttpResponseMessage, content) =
              ServiceConfig.makeDatabaseUrl serviceConfig url
                  |> GetJson.sync serviceConfig.HttpClient
          if not response.IsSuccessStatusCode then
              Error (FirebaseError content)
          else
              printfn "%A" content
              if content = "null" then
                Ok <| None
              else
                Ok <| Some(content)
  // map : ('T -> 'U) -> Result<'T, 'TError> -> Result<'U, 'TError>
  // map : ('T -> 'U) -> Option<'T> -> Option<'U>

  type SaveAccount = AccountDto.Root -> string option -> Result<unit, FirebaseError>
  let saveAccount: FirebaseServiceEndpoint<SaveAccount> =
      fun serviceConfig account accountO ->
          if Option.isNone accountO then
              let (response: HttpResponseMessage, content) =
                  ( ServiceConfig.makeDatabaseUrl serviceConfig ("/users/" + account.FirebaseUserId  +  "/accounts.json"),
                      account.JsonValue.ToString()
                  )
                      ||> PostJson.sync serviceConfig.HttpClient
              if not response.IsSuccessStatusCode then
                  Error (FirebaseError content)
              else
                  Ok ()
          else
              Ok ()

  type Run = AccountDto.Root -> Result<unit,FirebaseError>
  let run: FirebaseServiceEndpoint<Run> =
    fun serviceConfig account ->
        // Get accounts for current user by AccountId
        // If exists, return the account
        // If doesn't exist, create the account and return it
        getAccountByAccountId serviceConfig account.FirebaseUserId account.PlaidAccountId
          |> Result.bind (saveAccount serviceConfig account)
        // let (response: HttpResponseMessage, content) =
        //     ( ServiceConfig.makeDatabaseUrl serviceConfig ("/users/" + account.FirebaseUserId  +  "/accounts.json"),
        //         account.JsonValue.ToString()
        //     )
        //         ||> PostJson.sync serviceConfig.HttpClient
        // if not response.IsSuccessStatusCode then
        //     Error (FirebaseError content)
        // else
        //     Ok account

  let firebaseCreateAccount: FirebaseServiceEndpoint<CreateAccount> =
      fun serviceConfig account ->
        let requestBody =
          CreateAccountRequestBody.Root
            ( account.UserId,
              account.AccountId,
              account.ItemAccessToken
            )

        let (response: HttpResponseMessage, content) =
          ( ServiceConfig.makeDatabaseUrl serviceConfig ("/users/" + account.UserId  +  "/accounts.json"),
            requestBody.JsonValue.ToString()
          )
            ||> PostJson.sync serviceConfig.HttpClient
        if not response.IsSuccessStatusCode then
          Error (FirebaseError content)
        else
          Ok account

module AddItem =
    [<Literal>]
    let plaidItemDtoSample = """
        {
            "institution_id": "ins_3",
            "item_id": "KznK5LWGR7UMBBJPXzRQUyRxylgQlPHeLxnLzM",
            "plaid_access_token": "access-sandbox-265c9150-193c-4b8e-a6c2-189411592c7e",
            "webhook": "https://requestb.in/s6e29ss6"
        }
    """
    type PlaidItemDto = JsonProvider<plaidItemDtoSample>

    type Run = FirebaseUserId -> PlaidItemDto.Root -> Result<PlaidItemDto.Root, FirebaseError>
    let run: FirebaseServiceEndpoint<Run> =
        fun serviceConfig userId itemDto ->
            let (response: HttpResponseMessage, content) =
                ( ServiceConfig.makeDatabaseUrl serviceConfig ("/users/" + userId  +  "/plaidItems.json"),
                    itemDto.JsonValue.ToString()
                )
                    ||> PostJson.sync serviceConfig.HttpClient
            if not response.IsSuccessStatusCode then
                Error (FirebaseError content)
            else

                Ok itemDto

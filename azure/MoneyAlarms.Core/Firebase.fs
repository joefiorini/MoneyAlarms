namespace Firebase

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

  let serviceAccountEmail = Environment.GetEnvironmentVariable("FIREBASE_ADMIN_CLIENT_EMAIL")
  let private tokenUri = Environment.GetEnvironmentVariable("FIREBASE_ADMIN_TOKEN_URI")

  let generate () =
      async {
        let privateKey =
          Environment.GetEnvironmentVariable("FIREBASE_ADMIN_PRIVATE_KEY").Replace("\\n", Environment.NewLine)
        printfn "Private Key: %s" privateKey
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

  let fromEnvironment httpClient accessToken =
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

module CreateAccount =

  open FSharp.Data
  open HttpUtils
  open System

  [<Literal>]
  let createAccountBodySample = """
      {
          "user_id": "client_id",
          "account_id": "y4W8N4pk4ET5nNbAqZM6HaEPoDGRVLioJW1Jw",
          "access_token": "access-sandbox-265c9150-193c-4b8e-a6c2-189411592c7e"
      }
  """
  type CreateAccountRequestBody = JsonProvider<createAccountBodySample>

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

module MoneyAlarms.Dispatch.Commands

  open System
  open System.Net
  open System.Net.Http
  open FSharp.Data
  open FSharp.Data.JsonExtensions
  open MoneyAlarms.Core

  let private plaidConfigFromPayload httpClient (payload: JsonValue) =
      Plaid.ServiceConfig.create
        (payload?PLAID_CLIENT_ID.AsString())
        (payload?PLAID_SECRET.AsString())
        (payload?PLAID_HOST.AsString())
        (payload?PLAID_PUBLIC_KEY.AsString())
        httpClient

  let private firebaseConfigFromPayload httpClient accessToken payload =
      Firebase.ServiceConfig.createConfig
        (payload?FIREBASE_API_KEY.AsString())
        (payload?FIREBASE_AUTH_DOMAIN.AsString())
        (payload?FIREBASE_DATABASE_URL.AsString())
        (payload?FIREBASE_PROJECT_ID.AsString())
        (payload?FIREBASE_STORAGE_BUCKET.AsString())
        (payload?FIREBASE_MESSAGE_SENDER_ID.AsString())
        accessToken
        httpClient


  let exchangeTokens payload =
      async {
          printfn "Got data: %s" <| payload.ToString()
          let httpClient = new HttpClient()

          let dto = TokenExchangeDto.Parse (payload?data.ToString())

          ServicePointManager.SecurityProtocol <- SecurityProtocolType.Tls12

          printfn "Made DTO: %A" dto
          let plaidConfig = plaidConfigFromPayload httpClient payload
          printfn "Got plaid config: %A" plaidConfig
          let plaidExchangeToken = Plaid.plaidExchangeToken plaidConfig
          let plaidGetAccounts = Plaid.Accounts.get plaidConfig
          let plaidGetInstitutionName = Plaid.Institutions.getName plaidConfig

          let firebaseToken =
            try
              Firebase.AccessToken.generate
                <| payload?FIREBASE_ADMIN_CLIENT_EMAIL.AsString()
                <| payload?FIREBASE_ADMIN_PRIVATE_KEY.AsString()
            with
              | ex ->
                printfn """{ "result": "failure", "message": "%s"}""" <| ex.ToString()
                raise ex

          printfn "Got firebaseToken: %A" firebaseToken

          let firebaseConfig =
                  firebaseConfigFromPayload httpClient firebaseToken payload

          printfn "Got firebase config: %A" firebaseConfig
          let firebaseCreateAccount =
                Firebase.SaveAccount.run firebaseConfig
          let firebaseAddItem = Firebase.AddItem.run firebaseConfig

          let result =
            Commands.CreateAccount.run
              plaidExchangeToken
              plaidGetAccounts
              plaidGetInstitutionName
              firebaseCreateAccount
              firebaseAddItem
              dto
            |> function
                | Error e -> Error (e.ToString())
                | Ok v -> Ok v

          return result
      } |> Async.RunSynchronously

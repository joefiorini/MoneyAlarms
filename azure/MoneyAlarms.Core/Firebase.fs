module Firebase

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

let firebaseCreateAccount: FirebaseServiceEndpoint<CreateAccount> =
    fun serviceConfig account ->
        Ok account

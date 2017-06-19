module Firebase

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
    }

type FirebaseAccount =
    { UserId: FirebaseUserId
      ItemAccessToken: ItemAccessToken
      AccountId: AccountId
    }

type FirebaseError = string

type FirebaseCreateAccount = FirebaseServiceConfig -> FirebaseAccount -> Result<FirebaseAccount,FirebaseError>

module MoneyAlarms.Types

type PlaidClientId = string
type PlaidSecret = string
type PlaidAccountId = string
type PublicToken = string
type ItemAccessToken = string
type FirebaseUserId = string
type PlaidHost = string

type Account =
    { FirebaseUserId: FirebaseUserId
      ItemAccessToken: ItemAccessToken
      PlaidAccountId: PlaidAccountId
    }

type TokenExchangeDto =
    { PlaidPublicToken: PublicToken
      FirebaseUserId: FirebaseUserId
    }

type PlaidServiceConfig =
    { ClientId: PlaidClientId
      Secret: PlaidSecret
      Host: PlaidHost
    }

type FirebaseServiceConfig =
    { Firebase}
type ExchangeResultDto =
    { AccessToken: string
      ItemId: string
    }

type Error = TokenExchangeError of string

type CreateAccount = FirebaseService -> PlaidServiceConfig -> TokenExchangeDto -> Result<Account, Error>

type GetAccessToken = PlaidServiceConfig -> PublicToken -> Result<PlaidAccountId * ItemAccessToken, Error>
type MakeAccount = FirebaseUserId -> PlaidAccountId -> ItemAccessToken -> Account
type SaveAccountToFirebase = FirebaseService -> Account -> Result<Account, Error>

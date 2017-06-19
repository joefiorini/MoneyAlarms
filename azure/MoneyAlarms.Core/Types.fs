namespace MoneyAlarms.Core

open Firebase
open Plaid

type PublicToken = string
type ItemAccessToken = string
type AccountId = string

type TokenExchangeDto =
    { PlaidPublicToken: PublicToken
      FirebaseUserId: FirebaseUserId
    }

type ExchangeResultDto =
    { AccessToken: string
      ItemId: string
    }

type Error =
    | PlaidError
    | FirebaseError
    | ExchangeTokenError of string


type GetAccessToken = Plaid.PlaidExchangeToken -> Plaid.PlaidPublicToken -> Result<AccountId * ItemAccessToken, Error>

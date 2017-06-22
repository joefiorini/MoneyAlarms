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

type MoneyAlarmsError =
    | PlaidError of Plaid.PlaidError
    | FirebaseError of Firebase.FirebaseError
    | ExchangeTokenError of string

type Account =
    { AccountId: string
      Name: string
      OfficialName: string
      Mask: string
      Type: string
      SubType: string
      InstitutionName: string
    }

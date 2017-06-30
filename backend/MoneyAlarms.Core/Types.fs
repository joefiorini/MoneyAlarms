namespace MoneyAlarms.Core

open Firebase
open Plaid
open FSharp.Data

type PublicToken = string
type ItemAccessToken = string
type AccountId = string

module TokenExchangeSample =
  [<Literal>]
  let tokenExchangeSample = """
    {
      "plaid_public_token": "A1B2C3",
      "firebase_user_id": "a1234"
    }
  """

type TokenExchangeDto = JsonProvider<TokenExchangeSample.tokenExchangeSample>

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

type PlaidItem =
    { ItemId: string
      Webhook: string
      InstitutionId: string
    }

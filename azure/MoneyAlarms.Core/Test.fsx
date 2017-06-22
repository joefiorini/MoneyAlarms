#r "../packages/FSharp.Data/lib/net40/FSharp.Data.dll"
#r "../packages/FSharp.Data/lib/net40/FSharp.Data.DesignTime.dll"

open FSharp.Data

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
      "mask": "0000",
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
      "mask": "1111",
      "name": "Plaid Saving",
      "official_name": "Plaid Silver Standard 0.1% Interest Saving",
      "subtype": "savings",
      "type": "depository"
    },
    {
      "account_id": "Z1qZ5kArbnFbDDj38vR9hRqdKrbRZoTgLEkab",
      "balances": {
        "available": null,
        "current": 1000,
        "limit": null
      },
      "mask": "2222",
      "name": "Plaid CD",
      "official_name": "Plaid Bronze Standard 0.2% Interest CD",
      "subtype": "cd",
      "type": "depository"
    },
    {
      "account_id": "M37z5nVblQtLvvePE3bMUwnyqKNwEkI9xWZXz",
      "balances": {
        "available": null,
        "current": 410,
        "limit": 2000
      },
      "mask": "3333",
      "name": "Plaid Credit Card",
      "official_name": "Plaid Diamond 12.5% APR Interest Credit Card",
      "subtype": "credit",
      "type": "credit"
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
type AccountDto = JsonProvider<accountsDtoSample, RootName="accounts">

let actualResponse = """
{
 "accounts": [{
   "account_id": "vzeNDwK7KQIm4yEog683uElbp9GRLEFXGK98D",
   "balances": {
     "available": 100,
     "current": 110,
     "limit": null
   },
   "mask": "0000",
   "name": "Plaid Checking",
   "official_name": "Plaid Gold Checking",
   "subtype": "checking",
   "type": "depository"
 }, {
   "account_id": "6Myq63K1KDSe3lBwp7K1fnEbNGLV4nSxalVdW",
   "balances": {
     "available": null,
     "current": 410,
     "limit": 2000
   },
   "mask": "3333",
   "name": "Plaid Credit Card",
   "official_name": "Plaid Diamond Credit Card",
   "subtype": "credit card",
   "type": "credit"
 }],
 "item": {Object},
 "request_id": "45QSn"
}
"""

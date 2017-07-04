## Backend Plan
### Login to Account
#### Exhange Tokens

Input:
  - Public Token
  - Firebase User ID
Output:
  - Access Token

records:
Account: {
  FirebaseUserId,
  AccessToken,
  PlaidAccountId
}

TokenExchangeDto: {
  PlaidPublicToken,
  FirebaseUserId
}

TransactionsDto: {
  FirebaseUserId,
  Transaction List
}

functions:
- async exchangeTokens: (PlaidService, TokenExchangeDto) -> ()
- async getAccessToken: (PlaidService, publicToken) -> AccessToken
- makeAccount: FirebaseUserId -> PlaidAccountId -> AccessToken -> Account
- async saveAccount: saveFirebaseData -> Account -> Result<Unit>

### Initial Update
Webhook request:

{
 "webhook_type": "TRANSACTIONS",
 "webhook_code": "INITIAL_UPDATE",
 "item_id": "wz666MBjYWTp2PDzzggYhM6oWWmBb",
 "error": null,
 "new_transactions": 19
}

#### persistTransactions (on transactionsUpdateReady)
0. Fire "updateStarting" trigger
- fireTrigger string
1. Make sure we send webhook url with ?firebase\_user\_id=... so we can capture the id when the hook gets posted
2. Webhook gives us item\_id, so we need to look up access token the item id in Firebase (user's plaidItems collection)
getAccessTokenForItem
3. Using that item\_id, we can request transactions, but they are paginated therefore we:
  A. Use new_transactions count from webhook as "options.count" parameter
  B. Limit to only transactions for current month using start_date and end_date params
4. Request transactions; payload:
{
"accounts": [{object}],
"transactions": [{
   "account_id": "vokyE5Rn6vHKqDLRXEn5fne7LwbKPLIXGK98d",
   "amount": 2307.21,
   "category": [
     "Shops",
     "Computers and Electronics"
   ],
   "category_id": "19013000",
   "date": "2017-01-29",
   "location": {
    "address": "300 Post St",
    "city": "San Francisco",
    "state": "CA",
    "zip": "94108",
    "coordinates": {
       "lat": null,
       "lon": null,
    }
   },
   "name": "Apple Store",
   "payment_meta": Object,
   "pending": false,
   "pending_transaction_id": null,
   "account_owner": null,
   "transaction_id": "lPNjeW1nR6CDn5okmGQ6hEpMo4lLNoSrzqDje",
   "transaction_type": "place"
  }, {
   "account_id": "XA96y1wW3xS7wKyEdbRzFkpZov6x1ohxMXwep",
   "amount": 78.5,
   "category": [
     "Food and Drink",
     "Restaurants"
   ],
   "category_id": "13005000",
   "date": "2017-01-29",
   "location": {
     "address": "262 W 15th St",
     "city": "New York",
     "state": "NY",
     "zip": "10011",
     "coordinates": {
       "lat": 40.740352,
       "lon": -74.001761
     }
   },
   "name": "Golden Crepes",
   "payment_meta": Object,
   "pending": false,
   "pending_transaction_id": null,
   "account_owner": null,
   "transaction_id": "4WPD9vV5A1cogJwyQ5kVFB3vPEmpXPS3qvjXQ",
   "transaction_type": "place"
 }],
 "item": {Object},
 "request_id": "45QSn"
}
5. Extract categories from transactions
6. Fold over each category (creating a new map),
   A. Get all transactions containing category in its list
   B. Map over those transactions to extract transaction\_id, name, date, amount
   C. Sum transaction amounts to get total for category
   "dotted\_category": {
     "total": "353.20",
     "transactions": [{
       transaction_id,
       name,
       date,
       amount,
       location,
       pending
     }]
   }
   C. Add those to map with category hierarchy list as key
7. Sum transactions for each entry in map
8. Persist to user's "categorizedTotals" collection in firebase (overwrite)
9. Ping the "categoriesTotaled" trigger with given request id or new one


#### checkForViolations

1. Get rules from firebase
2. Get categorizedTotals from firebase
3. Fold categories to new list:
   A. If category name starts with category.total > rule
   B. Save into new list
4. Persist to ruleViolations in firebase

#### cleanUpUpdates (on overagesCalculated)

0. Exit if no request id given
1. Get active update from user's firebase using given request id
2. Get latest
initialTransactionUpdate: Transaction List -> ()
requestNewTransactions: ItemId -> Transaction List

- Filter transactions back to beginning of current month
filterTransactions: Transaction List -> Date -> Transaction List
- Store all transactions
prepareTransactions: Transaction List -> TransactionsDto
persistTransactions: TransactionsDto -> Result (Unit, Error)
- Call endpoint to group & sum by category
- Check for conditions

### Default Update
- Add new transactions to existing list (and update any that are already there?)
- Call endpoint to group & sum by category
- Check for conditions
### Group & Sum by Category
- Group and sum by category, store categories with total
groupTransactions: Transaction List -> CategoryWithTransactions List
sumCategories: CategoryWithTransactions List -> CategoryWithTotal List
### Check for Overages
OverageCondition {
  Categories
  ConditionTotal
  ActualTotal
}

checkForOverages: Condition List -> OverageCondition List
- If total for categories for given condition is greater than total specified in condition, then call push notification service
createNotifications: OverageCondition List -> OverageNotification List
prepareConditionOverages: OverageCondition List -> OverageConditionsDto
notifyForConditionOverage:
### Save condition
Condition: {
  Amount,
  Categories
}

### Notes:

- DO NOT NEED TO WORRY ABOUT A TRANSACTION BEING IN MULTIPLE CATEGORIES
- UI should prevent user from combining children & parent categories in a rule
  - Rules can be combined from a parent and a child of a diffrent parent though, not a big deal
- If user chooses "Travel", then all transactions in Airports (eg) should be included for alerts
- Overwriting _should_ be done by indexing collections by request id; then after a process is complete we could remove all collections indexed under a particular request id; there would be an "active_request_id" at the top level that we'd update to the new id prior to removing the current active one
- Should allow configuring the budget period (ie. weekly, every two weeks, once a month, 2/3 times per month, etc.)
  - Not going to do it yet though, starting with 1st & 15th

/categories
/users/mYfbsFHD9FQNuijiIrGHioUeIgT2
- activeUpdateId
- accounts/
- plaidItems/
- transactions/
| - {updateId}/
- categorizedTransactions
| - {updateId}/
- categoriesTotaled
| - {updateId}/

### Use Case

Rule:
  - Category: 'Travel -> Airport'
  - Category: 'Travel -> Car and Truck Rentals'
  - Category: 'Food -> Restaurants'
    Location: # Not Ohio
      State:
        Not: Ohio
  - Category: 'Shops'
    Location: Not Ohio
  - Amount:
      GreaterThan: 2000

Lookup:
 - Transactions with category starting with 'Travel.Airport'
 - Transactions with category starting with 'Travel.Car and Truck Rentals'
 - Transactions with location.state != 'Ohio'
   AND: With category starting with 'Food.Restaurants'
   OR: With category starting with 'Shops'

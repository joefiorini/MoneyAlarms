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
- async exchangeTokens: (TokenExchangeDto) -> ()
- async getAccessToken: publicToken -> AccessToken
- makeAccount: FirebaseUserId -> PlaidAccountId -> AccessToken -> Account
- async saveAccount: AccessToken -> ()

### Initial Update
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

- Make sure to account for transactions that were in more than one of the specified categories (otherwise our alert could be wrong)
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

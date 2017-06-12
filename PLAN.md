## Backend Plan
### Login to Account
#### Exhange Tokens
### Initial Update
- Retrieve all transactions back to beginning of current month
- Store all transactions
- Call endpoint to group & sum by category
- Check for conditions
### Default Update
- Add new transactions to existing list (and update any that are already there?)
- Call endpoint to group & sum by category
- Check for conditions
### Group & Sum by Category
- Group and sum by category, store categories with total
### Check for Conditions
- Make sure to account for transactions that were in more than one of the specified categories (otherwise our alert could be wrong)
- If total for categories for given condition is greater than total specified in condition, then call push notification service

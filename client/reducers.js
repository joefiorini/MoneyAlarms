import { combineReducers } from 'redux';
import userReducer from './User/reducer';
import accountsReducer from './Accounts/reducer';

export default combineReducers({
  user: userReducer,
  accounts: accountsReducer,
});

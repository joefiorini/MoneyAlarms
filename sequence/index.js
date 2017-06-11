import { effects } from 'redux-saga';

import * as constants from '../constants';
import * as actions from '../actions';

export default {
  *run() {
    console.log('running');
    yield effects.take(constants.ADD_ACCOUNT_REQUESTED);
    // TODO: Start getting account options from plaid
    // TODO: Dispatch actions.accountOptionsLoaded(accountOptions);
    yield effects.take(constants.CREATE_ACCOUNT_REQUESTED);
    console.log('create requested');
    // TODO: Use plaid & firebase to save the account in the user's record
    yield effects.put(actions.accountCreated());
  },
};

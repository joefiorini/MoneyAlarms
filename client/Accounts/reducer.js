// @flow

import { Action } from '../actions';
import * as constants from '../constants';

type State = {
  isAddingAccount: boolean,
};

const initial: State = {
  isAddingAccount: false,
};

export default function(state: State = initial, action: Action) {
  switch (action.type) {
    case constants.ADD_ACCOUNT_REQUESTED: {
      return {
        ...state,
        isAddingAccount: true,
      };
    }
    case constants.ACCOUNT_CREATED: {
      console.log('ACCOUNT_CREATED');
      return {
        ...state,
        isAddingAccount: false,
      };
    }
    default:
      return state;
  }
}

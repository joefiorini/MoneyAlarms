// @flow

import * as constants from '../constants';

type AddAccountRequested = {
  type: constants.ADD_ACCOUNT_REQUESTED,
};

export default function(): AddAccountRequested {
  return {
    type: 'ADD_ACCOUNT_REQUESTED',
  };
}

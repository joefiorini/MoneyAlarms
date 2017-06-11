// @flow
import * as constants from '../constants';

type AccountCreated = {
  type: constants.ACCOUNT_CREATED,
};

export default function accountCreated() {
  console.log('accountCreated');
  return {
    type: constants.ACCOUNT_CREATED,
  };
}

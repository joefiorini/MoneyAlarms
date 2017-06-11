import * as constants from '../constants';

type CreateAccountRequested = {
  type: constants.CREATE_ACCOUNT_REQUESTED,
};

export default function(): CreateAccountRequested {
  return {
    type: 'CREATE_ACCOUNT_REQUESTED',
  };
}

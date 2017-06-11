import { effects } from 'redux-saga';

export default {
  *run() {
    yield effects.getContext();
  },
};

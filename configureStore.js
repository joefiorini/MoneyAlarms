import { createStore, applyMiddleware } from 'redux';
import createSagaMiddleware from 'redux-saga';
import { createLogger } from 'redux-logger';

import sequence from './sequence';
import reducer from './reducers';

const loggerMiddleware = createLogger({});

const sagaMiddleware = createSagaMiddleware();

const createStoreWithMiddleware = applyMiddleware(
  sagaMiddleware,
  loggerMiddleware
)(createStore);

export default initialState => {
  const store = createStoreWithMiddleware(reducer, initialState);
  sagaMiddleware.run(sequence.run);
  return store;
};

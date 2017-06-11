// @flow

import React from 'react';
import PropTypes from 'prop-types';
import Container from './Container';
import { NavigatorIOS } from 'react-native';
import * as firebase from 'firebase';
import Config from './config';

const config = {
  apiKey: Config.FIREBASE_API_KEY,
  authDomain: Config.FIREBASE_AUTH_DOMAIN,
  databaseURL: Config.FIREBASE_DATABASE_URL,
  storageBucket: Config.FIREBASE_STORAGE_BUCKET,
};
firebase.initializeApp(config);

export default class App extends React.Component {
  render() {
    return (
      <NavigatorIOS
        style={{ flex: 1 }}
        initialRoute={{
          component: Container,
          title: 'Main',
          passProps: {
            currentUser: firebase.User,
          },
        }}
      />
    );
  }
}

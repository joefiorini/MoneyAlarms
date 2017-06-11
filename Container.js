// @flow

import React from 'react';
import PropTypes from 'prop-types';
import * as firebase from 'firebase';
import { StyleSheet, Text, View, TextInput } from 'react-native';
import Button from './Button';
import SignUp from './SignUp';
import SignIn from './SignIn';
import Accounts from './Accounts';

const { object } = PropTypes;

export default class Container extends React.Component {
  static propTypes = {
    navigator: object.isRequired,
  };

  navigateToAccounts() {
    this.props.navigator.push({
      title: 'Accounts',
      component: Accounts,
      rightButtonSystemIcon: 'add',
    });
  }

  createAccountRequested() {
    this.props.navigator.push({
      title: 'Sign Up',
      component: SignUp,
      passProps: {
        onUserAuthenticated: this.onUserAuthenticated.bind(this),
      },
    });
  }

  signInRequested() {
    this.props.navigator.push({
      title: 'Sign In',
      component: SignIn,
      passProps: {
        onUserAuthenticated: this.onUserAuthenticated.bind(this),
      },
    });
  }

  onUserAuthenticated() {
    if (firebase.User) {
      this.navigateToAccounts();
    }
  }

  render() {
    if (firebase.User) {
      setTimeout(this.navigateToAccounts.bind(this));
    }

    return (
      <View style={styles.container}>
        <Button
          title="Create Account"
          onPress={() => this.createAccountRequested()}
        />
        <Button title="Sign In" onPress={() => this.signInRequested()} />
      </View>
    );
  }
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#fff',
    alignItems: 'center',
    justifyContent: 'center',
  },
});

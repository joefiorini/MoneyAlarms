// @flow

import React from 'react';
import PropTypes from 'prop-types';
import * as firebase from 'firebase';
import { StyleSheet, Text, View, TextInput } from 'react-native';
import { connect } from 'react-redux';
import Button from './Button';
import SignUp from './SignUp';
import SignIn from './SignIn';
import Accounts from './Accounts';
import * as actions from './actions';

const { object } = PropTypes;

class Container extends React.Component {
  static propTypes = {
    navigator: object.isRequired,
  };

  navigateToAccounts() {
    this.props.navigator.push({
      title: 'Accounts',
      component: Accounts,
      rightButtonSystemIcon: 'add',
      onRightButtonPress: () => this.props.requestAddAccount(),
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

const mapDispatchToProps = {
  requestAddAccount: actions.addAccountRequested,
};
export default connect(null, mapDispatchToProps)(Container);

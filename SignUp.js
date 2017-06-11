// @flow

import React from 'react';
import PropTypes from 'prop-types';
import { StyleSheet, Text, View } from 'react-native';
import { auth } from 'firebase';

import TextInput from './TextInput';
import Button from './Button';
import Spinner from './Spinner';

const { object, func } = PropTypes;

export default class SignUp extends React.Component {
  static propTypes = {
    navigator: object.isRequired,
    onUserAuthenticated: func.isRequired,
  };

  state = {
    email: '',
    password: '',
    isCreatingAccount: false,
  };

  async createAccountRequested() {
    // do stuff
    this.setState({ isCreatingAccount: true });
    try {
      await auth().createUserWithEmailAndPassword(
        this.state.email,
        this.state.password
      );
    } catch (error) {
      this.setState({ isCreatingAccount: false, error });
      return;
    }

    this.setState({ isCreatingAccount: false, isAccountCreated: true });
    setTimeout(
      () => {
        this.props.onUserAuthenticated();
        this.props.navigator.pop();
      },
      3000
    );
  }

  focusPassword() {
    this.passwordInput.focus();
  }

  render() {
    if (this.state.isCreatingAccount) {
      return <Spinner />;
    }

    if (this.state.isAccountCreated) {
      return (
        <View style={styles.container}>
          <Text>Successfully created account!</Text>
        </View>
      );
    }

    return (
      <View style={styles.container}>
        {this.state.error
          ? <Text>Error Code: {this.state.error.code}</Text>
          : null}
        <TextInput
          editable={true}
          keyboardType="email-address"
          returnKeyType="next"
          placeholder="Email"
          autoCorrect={false}
          onSubmitEditing={() => this.focusPassword()}
          onChangeText={email => this.setState({ email })}
          autoCapitalize="none"
        />
        <TextInput
          secureTextEntry={true}
          editable={true}
          placeholder="Password"
          autoCorrect={false}
          onChangeText={password => this.setState({ password })}
          ref={component => {
            this.passwordInput = component;
          }}
          autoCapitalize="none"
        />
        <Button
          title="Create Account"
          onPress={() => this.createAccountRequested()}
        />
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

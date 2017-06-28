// @flow

import React from 'react';
import PropTypes from 'prop-types';
import { StyleSheet, Text, View } from 'react-native';
import { auth } from 'firebase';

import TextInput from './TextInput';
import Button from './Button';
import Spinner from './Spinner';

const { object, func } = PropTypes;

export default class SignIn extends React.Component {
  static propTypes = {
    navigator: object.isRequired,
    onUserAuthenticated: func.isRequired,
  };

  state = {
    email: '',
    password: '',
    isSigningIn: false,
  };

  async createAccountRequested() {
    // do stuff
    this.setState({ isSigningIn: true });
    try {
      await auth().signInWithEmailAndPassword(
        this.state.email,
        this.state.password
      );
    } catch (error) {
      this.setState({ isSigningIn: false, error });
      return;
    }

    this.setState({ isSigningIn: false, isSignedIn: true });
    this.props.onUserAuthenticated();
    this.props.navigator.pop();
  }

  focusPassword() {
    this.passwordInput.focus();
  }

  render() {
    if (this.state.isSigningIn) {
      return <Spinner />;
    }

    if (this.state.isSignedIn) {
      return (
        <View style={styles.container}>
          <Text>Signed in!</Text>
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
        <Button title="Sign In" onPress={() => this.createAccountRequested()} />
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

// @flow

import React from 'react';
import { connect } from 'react-redux';
import { View, Text, StyleSheet, Modal } from 'react-native';
import Button from './Button';
import TextInput from './TextInput';
import * as actions from '../actions';
import Config from '../config';
import MessageWebView from './MessageWebView';
import * as firebase from 'firebase';

const logValue = e => {
  console.log(e);
  return e;
};
class Accounts extends React.Component {
  async onMessage(e) {
    console.log('got message', e);
    if (e.action.match(/::connected/)) {
      // TODO: Get currentUser into store
      const response = await fetch(
        'https://moneyalarms.azurewebsites.net/api/ExchangeTokens?code=Wykh0ruO61aXcfwGPZTxzatuhxpmTJU3SdzDtspwxw2zN7MkALA/rQ==',
        {
          method: 'POST',
          headers: {
            'content-type': 'application/json',
          },
          body: logValue(
            JSON.stringify({
              plaid_public_token: e.metadata.public_token,
              firebase_user_id: firebase.auth().currentUser.uid,
            })
          ),
        }
      );

      // if (response.status >= 400) {
      //   console.log(response);
      //   console.error(`${response.status} trying to exchange plaid token`);
      // }

      // let accessToken;

      // try {
      //   ({ access_token: accessToken } = await response.json());
      // } catch (e) {
      //   console.log(response);
      //   console.error(
      //     'Error parsing json body from plaid exchange token endpoint'
      //   );
      // }

      // if (!accessToken) {
      //   console.log(response);
      //   console.error('Plaid token exchange failed for unknown reason');
      // }

      // const currentUser = firebase.auth().currentUser;
      // const plaidItemsRef = firebase
      //   .database()
      //   .ref(`users/${currentUser.uid}/plaidItems`)
      //   .push();

      // const account = {
      //   accountId: e.metadata.account_id,
      //   accessToken: accessToken,
      // };

      // plaidItemsRef.set(account);
    }
  }
  render() {
    const plaidURL = `https://cdn.plaid.com/link/v2/stable/link.html?key=${Config.PLAID_PUBLIC_KEY}&env=sandbox&product=transactions,auth&selectAccount=true&clientName=Money%20Alarms&isWebView=true&apiVersion=v2&webhook=https://requestb.in/s6e29ss6`;
    return (
      <View style={styles.container}>
        <Text>Accounts</Text>
        <Modal
          animationType="slide"
          transparent={false}
          visible={this.props.isAddingAccount}
        >
          <MessageWebView
            source={{ uri: plaidURL }}
            // onNavigationStateChange={(...args) =>
            //   console.log(`state change`, args)}
            // onShouldStartLoadWithRequest={(...args) => {
            //   console.log('should start load', args);
            //   return true;
            // }}
            onMessage={e => this.onMessage(e)}
            // renderError={error => <Text>{error}</Text>}
            // onError={error => console.error(error)}
            // onLoadEnd={() => console.log('onLoadEnd')}
            // onLoadStart={() => console.log('onLoadStart')}
          />
        </Modal>
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

const mapStateToProps = state => ({
  isAddingAccount: state.accounts.isAddingAccount,
});

const mapDispatchToProps = {
  createAccountRequested: actions.createAccountRequested,
  itemAccessTokenRequested: actions.itemAccessTokenRequested,
};

export default connect(mapStateToProps, mapDispatchToProps)(Accounts);

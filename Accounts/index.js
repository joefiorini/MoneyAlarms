// @flow

import React from 'react';
import { connect } from 'react-redux';
import { View, Text, StyleSheet, Modal } from 'react-native';
import Button from '../Button';
import * as actions from '../actions';

class Accounts extends React.Component {
  render() {
    return (
      <View style={styles.container}>
        <Text>Accounts</Text>
        <Modal
          animationType="slide"
          transparent={false}
          visible={this.props.isAddingAccount}
        >
          <Text>Add Account</Text>
          <Button
            title="Create Account"
            onPress={this.props.createAccountRequested}
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
};

export default connect(mapStateToProps, mapDispatchToProps)(Accounts);

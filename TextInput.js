import React from 'react';
import { TextInput as RNTextInput, StyleSheet, View } from 'react-native';
import NativeMethodsMixin from 'NativeMethodsMixin';

export default class TextInput extends React.Component {
  focus() {
    console.log('setting focus');
    NativeMethodsMixin.focus.call(this.input);
  }
  render() {
    const { borderColor, textColor, ...props } = this.props;

    const styles = StyleSheet.create({
      container: {
        borderBottomWidth: 4,
        paddingBottom: 10,
        borderColor,
      },
      input: {
        height: 36,
        fontSize: 36,
        width: 300,
        fontWeight: 'bold',
        color: textColor,
      },
    });

    return (
      <View style={styles.container}>
        <RNTextInput
          {...props}
          style={styles.input}
          ref={component => {
            this.input = component;
          }}
        />
      </View>
    );
  }
}

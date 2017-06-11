import React from 'react';
import { TouchableOpacity, StyleSheet, View, Text } from 'react-native';

const styles = StyleSheet.create({
  button: {
    borderWidth: 1,
    borderStyle: 'solid',
    borderRadius: 3,
    padding: 5,
  },
  text: {
    textAlign: 'center',
  },
});

export default function Button(
  {
    onPress,
    style,
    textStyle,
    title,
    disabled = false,
  },
) {
  const button = (
    <View style={[styles.button, style]}>
      <Text style={[styles.text, textStyle]}>{title}</Text>
    </View>
  );

  if (disabled) {
    return button;
  }

  return <TouchableOpacity onPress={onPress}>{button}</TouchableOpacity>;
}

import React from 'react';
import { ActivityIndicator, StyleSheet } from 'react-native';

const styles = StyleSheet.create({
  spinner: {
    alignItems: 'center',
    justifyContent: 'center',
    height: 250,
  },
});
export default function Spinner() {
  return (
    <ActivityIndicator animating={true} size="large" style={styles.spinner} />
  );
}

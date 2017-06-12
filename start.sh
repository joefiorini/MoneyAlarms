#!/usr/bin/env bash

env $(cat .env | xargs) react-native-scripts start

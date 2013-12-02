#!/bin/sh

MONO=/usr/bin/mono
PROJECT=Xamarin.WebTests
CONFIG=Debug

TEST_EXE=$PROJECT/bin/$CONFIG/$PROJECT.exe

$MONO --debug $TEST_EXE $*


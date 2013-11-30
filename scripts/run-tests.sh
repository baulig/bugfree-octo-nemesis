#!/bin/sh

MONO=/usr/bin/mono
NUNIT_DIR=`cd nunit && pwd`
PROJECT=Xamarin.WebTests
CONFIG=Debug

TEST_EXE=$PROJECT/bin/$CONFIG/$PROJECT.exe

$MONO --debug $TEST_EXE $*


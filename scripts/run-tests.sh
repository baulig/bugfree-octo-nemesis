#!/bin/sh

MONO=/usr/bin/mono
NUNIT_DIR=`cd nunit && pwd`
PROJECT=Xamarin.WebTests
CONFIG=Debug

TEST_DLL=$PROJECT/bin/$CONFIG/$PROJECT.dll

$MONO --debug $NUNIT_DIR/bin/nunit-console.exe $TEST_DLL $*

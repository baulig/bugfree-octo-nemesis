#!/bin/bash

if test -d nunit; then
	echo "Already have NUnit!"
	exit 0
fi

NUNIT_URL=http://launchpad.net/nunitv2/trunk/2.6.3/+download/NUnit-2.6.3.zip
TEMPFILE=/tmp/$$.tmp

mkdir nunit
TARGET_DIR=`cd nunit && pwd`

wget $NUNIT_URL -O $TEMPFILE
unzip -d $TARGET_DIR $TEMPFILE
rm $TEMPFILE

mv nunit/NUnit-2.6.3/* nunit
rmdir nunit/NUnit-2.6.3

echo "Successfully extracted NUnit 2.6.3 into $TARGET_DIR."

#! /bin/bash

if [ "$(id -u)" != "0" ]; then
	echo "WARNING: Not running as root, default installation directory WILL FAIL!"
fi

prefix="/usr/lib/iodine"

if [ "$#" -ge 1 ]; then
	prefix=$1
fi

echo "Using prefix $prefix"

mkdir -p /usr/lib/iodine
cp ./bin/iodine.exe $prefix/iodine.exe
cp -r ./bin/modules $prefix
cp -r ./bin/extensions $prefix
cat ./iodine_run.sh > /usr/bin/iodine
chmod a+x /usr/bin/iodine


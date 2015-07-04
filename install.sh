#! /bin/bash

if [ "$(id -u)" != "0" ]; then
	echo "WARNING: Not running as root, default installation directory WILL FAIL!"
fi

prefix="/usr/lib"

if [ "$#" -ge 1 ]; then
	prefix=$1
fi

echo "Using prefix $prefix"

if [ -d "$prefix" ]; then
	cp ./bin/iodine.exe $prefix/iodine.exe
	cp -r ./bin/modules $prefix/modules
	cp -r ./bin/extensions $prefix/extensions
fi

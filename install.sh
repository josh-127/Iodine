#! /bin/bash

if [ "$(id -u)" != "0" ]; then
	echo "WARNING: Not running as root, default installation directory WILL FAIL!"
fi

prefix="/usr/lib/iodine"

if [ "$#" -ge 1 ]; then
	prefix=$1
fi

echo "Using prefix $prefix"

if [ -d "$prefix" ]; then
	cp ./bin/iodine.exe $prefix/iodine.exe
	cp -r ./bin/modules $prefix
	cp -r ./bin/extensions $prefix
	cat ./iodine_run.sh > /usr/bin/iodine
	chmod a+x /usr/bin/iodine
	cp ./iodine_env.sh /etc/profile.d/iodine.sh
        echo "$prefix" >> /etc/profile.d/iodine.sh
fi

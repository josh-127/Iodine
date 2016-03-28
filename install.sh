#! /bin/bash

if [ "$(id -u)" != "0" ]; then
	echo "WARNING: Not running as root, default installation directory WILL FAIL!"
fi

prefix="/usr/lib"

if [ "$#" -ge 1 ]; then
	prefix=$1
fi

echo "Using prefix $prefix"

mkdir -p $prefix/iodine
mkdir -p $prefix/iodine/bin
cp ./bin/iodine.exe $prefix/iodine/bin/iodine.exe
cp ./bin/iodine.dll $prefix/iodine/bin/iodine.dll
cp -r ./modules $prefix/iodine/bin
cp -r ./bin/extensions $prefix/iodine/bin
cat ./iodine_run.sh > /usr/bin/iodine
echo -n "export IODINE_HOME=$prefix/iodine" | sudo tee /etc/profile.d/iodine.sh
chmod a+x /usr/bin/iodine



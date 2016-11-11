#! /bin/bash

if [ "$(id -u)" != "0" ]; then
	echo "WARNING: Not running as root, default installation directory WILL FAIL!"
fi

prefix="/usr/local/lib"

if [ "$#" -ge 1 ]; then
	prefix=$1
fi

echo "Using prefix $prefix"
rm -rf ./modules/.iodine_cache
mkdir -p $prefix/iodine

cp ./bin/iodine.exe $prefix/iodine/iodine.exe
cp ./bin/LibIodine.dll $prefix/iodine/LibIodine.dll
cp -r ./modules $prefix/iodine/
cp -r ./bin/extensions $prefix/iodine

echo "#! /bin/bash" > /usr/bin/iodine
echo "/usr/bin/mono $prefix/iodine/iodine.exe \"\$@\"" >> /usr/bin/iodine

cp ./tools/iosh.id /usr/bin/iosh

echo -n "export IODINE_HOME=$prefix/iodine" | tee /etc/profile.d/iodine.sh

chmod a+x /usr/bin/iodine



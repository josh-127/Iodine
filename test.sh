mono -V
make all
make install-ion
cd tests
module_path=$(readlink -f ../modules)
echo $module_path
export IODINE_PATH=$module_path
mono ../bin/iodine.exe test.id

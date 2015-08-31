git clone https://github.com/IodineLang/Ion
mono -V
cd tests
module_path=$(readlink -f ../modules)
export IODINE_PATH="$IODINE_PATH:$module_path"
mono ../bin/iodine.exe ../Ion/ion_frontend.id install-deps
mono ../bin/iodine.exe test.id

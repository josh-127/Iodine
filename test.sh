git clone https://github.com/IodineLang/Ion
mono -V
cd tests
mono ../bin/iodine.exe ../Ion/ion_frontend.id install-deps
../bin/iodine.exe test.id

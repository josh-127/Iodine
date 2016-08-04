.PHONY: all clean iodine

all: iodine

iodine:
	cd ./src/Iodine
	nuget restore
	xbuild ./src/Iodine/Iodine.sln /p:Configuration=Release /p:DefineConstants="COMPILE_EXTRAS" /t:Build "/p:Mono=true;BaseConfiguration=Release"
install:
	./install.sh

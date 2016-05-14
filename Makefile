.PHONY: all clean iodine

all: iodine

iodine:
	xbuild ./src/Iodine/Iodine.sln /p:Configuration=Release /p:DefineConstants="COMPILE_EXTRAS" /t:Build "/p:Mono=true;BaseConfiguration=Release"


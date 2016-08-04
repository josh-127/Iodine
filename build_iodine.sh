#!/usr/bin/env bash
cd src/Iodine
nuget restore
xbuild /p:Configuration=Release /p:DefineConstants="COMPILE_EXTRAS"
xbuild /p:Configure=Nuget /p:DefineConstants="COMPILE_EXTRAS"

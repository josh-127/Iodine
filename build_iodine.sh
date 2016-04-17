#!/usr/bin/env bash
cd src/Iodine
xbuild /p:Configuration=Release /p:DefineConstants="COMPILE_EXTRAS"
xbuild /p:Configure=Nuget /p:DefineConstants="COMPILE_EXTRAS"

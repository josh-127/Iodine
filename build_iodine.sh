#!/usr/bin/env bash
cd src/Iodine
xbuild /p:Configuration=Release
xbuild /p:Configure=Nuget 

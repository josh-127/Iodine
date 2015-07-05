@echo off
set msbuildpath="C:\Windows\Microsoft.NET\Framework64\v4.0.30319\"
cd src\Iodine
%msbuildpath%msbuild.exe /p:Configuration=Release /p:outdir="..\..\bin\"
pause >nul
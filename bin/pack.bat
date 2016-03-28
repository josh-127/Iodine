@echo off
xcopy libiodine.nuspec nuget\
xcopy lib\libiodine.dll nuget\lib\net45\
nuget pack nuget\libiodine.nuspec
rmdir /s /q nuget
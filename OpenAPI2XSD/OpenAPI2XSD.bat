@echo off
setlocal EnableDelayedExpansion

set PARAM=
set COUNTER=0

:NextParameter
if "%~1" == "" goto Done
set PARAM=%PARAM% "--input:%COUNTER%" "%~1"
set /A COUNTER+=1
shift
goto NextParameter

:Done

echo "%~dp0OpenAPI2XSD.exe" %PARAM%
pause
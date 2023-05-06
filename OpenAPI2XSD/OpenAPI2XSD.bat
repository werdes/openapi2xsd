@echo off
setlocal EnableDelayedExpansion

set PARAM=
set COUNTER=0
set EXECUTABLE=%~dp0OpenAPI2XSD.exe

:NextParameter
if "%~1" == "" goto Done
set PARAM=%PARAM% "--input:%COUNTER%" "%~1"
set /A COUNTER+=1
shift
goto NextParameter

:Done

"%EXECUTABLE%" %PARAM%
pause
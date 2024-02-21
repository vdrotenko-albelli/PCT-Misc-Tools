@echo off
SET INPUT_ARGS_PATH=%~1
REM Var(s)
SET LOG_PATH=%INPUT_ARGS_PATH%.PCT10481ReplayCPDs.log
SET ERR_LOG_PATH=%INPUT_ARGS_PATH%.PCT10481ReplayCPDs.err.log
ECHO Started: %DATE%T%TIME% > "%LOG_PATH%"
..\bin\Debug\net6.0\Albelli.MiscUtils.CLI.exe PCT10481ReplayCPDs "%INPUT_ARGS_PATH%" %2 %3 %4 %5 %6 %7 %8 %9 1>> "%LOG_PATH%" 2> "%ERR_LOG_PATH%"
ECHO Finished: %DATE%T%TIME% >> "%LOG_PATH%"
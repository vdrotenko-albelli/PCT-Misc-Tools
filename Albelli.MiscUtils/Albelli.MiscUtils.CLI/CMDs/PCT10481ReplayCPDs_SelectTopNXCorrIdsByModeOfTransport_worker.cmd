@echo off
SET INPUT_MODES=%~1
SET CENTRAL_LOG_FPATH=%~2
SET TOP_N=%~3

REM Var(s)
SET LOG_PATH=%CENTRAL_LOG_FPATH%.PCT10481ReplayCPDs_SelectTopNXCorrIdsByModeOfTransport.log
SET ERR_LOG_PATH=%CENTRAL_LOG_FPATH%.PCT10481ReplayCPDs_SelectTopNXCorrIdsByModeOfTransport.err.log
ECHO Started: %DATE%T%TIME% > "%LOG_PATH%"
..\bin\Debug\net6.0\Albelli.MiscUtils.CLI.exe PCT10481ReplayCPDs_SelectTopNXCorrIdsByModeOfTransport "%INPUT_MODES%" "%CENTRAL_LOG_FPATH%" "%TOP_N%" %4 %5 %6 %7 %8 %9 1>> "%LOG_PATH%" 2> "%ERR_LOG_PATH%"
ECHO Finished: %DATE%T%TIME% >> "%LOG_PATH%"
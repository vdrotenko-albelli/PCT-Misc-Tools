@echo off
SET ENV_NAME=%~1
SET AUTH_TOKEN=%~2
SET SAVE_AS_PATH=%~3

REM Var(s)
SET LOG_PATH=PCT10679ExportLeadtimes.%ENV_NAME%.log
..\bin\Debug\net6.0\Albelli.MiscUtils.CLI.exe PCT10679ExportLeadtimes "%ENV_NAME%" "%AUTH_TOKEN%" "%SAVE_AS_PATH%" %4 %5 %6 %7 %8 %9 1> "%LOG_PATH%" 2>&1
EXIT /B %ERRORLEVEL%
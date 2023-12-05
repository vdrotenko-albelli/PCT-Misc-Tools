@echo off
SET IN_FIL_PATH=%~1
SET AUTH_TOKEN=%~2





REM Var(s)
SET LOG_PATH=%IN_FIL_PATH%.ApiGetJsons.log
..\bin\Debug\net6.0\Albelli.MiscUtils.CLI.exe ApiGetJsons "%IN_FIL_PATH%" "%AUTH_TOKEN%" %3 %4 %5 %6 %7 %8 %9 1> "%LOG_PATH%" 2>&1
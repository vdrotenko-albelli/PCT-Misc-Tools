@echo off
SET ENV_NM=%~1
SET IN_FIL_PATH=%~2
SET XLS_SHEET=%~3
SET BASE_URL=%~4
SET TOKEN=%~5

REM Var(s)
SET LOG_PATH=%IN_FIL_PATH%.ApiTestGets.%ENV_NM%.log
..\bin\Debug\net6.0\Albelli.MiscUtils.CLI.exe ApiTestGets "%ENV_NM%" "%IN_FIL_PATH%" "%XLS_SHEET%" "%BASE_URL%" "%TOKEN%" %6 %7 %8 %9 1> "%LOG_PATH%" 2>&1
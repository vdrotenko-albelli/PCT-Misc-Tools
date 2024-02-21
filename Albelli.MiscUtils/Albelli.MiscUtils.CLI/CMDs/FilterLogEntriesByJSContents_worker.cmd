@echo off
SET SRC_LOG_CSV=%~1
SET MODEL_PATH=%~2
SET MODEL_TYPE_NAME=%~3

REM Var(s)
SET LOG_PATH=%SRC_LOG_CSV%.FilterLogEntriesByJSContents.log
SET ERR_LOG_PATH=%SRC_LOG_CSV%.FilterLogEntriesByJSContents.err.log
..\bin\Debug\net6.0\Albelli.MiscUtils.CLI.exe FilterLogEntriesByJSContents "%SRC_LOG_CSV%" "%MODEL_PATH%" "%MODEL_TYPE_NAME%" %4 %5 %6 %7 %8 %9 1> "%LOG_PATH%" 2> "%ERR_LOG_PATH%"
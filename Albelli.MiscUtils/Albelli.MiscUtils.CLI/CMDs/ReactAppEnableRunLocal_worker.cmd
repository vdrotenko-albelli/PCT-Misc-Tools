@echo off
SET PACKAGE_JSON_PATH=%~1
SET CONFIG_JSON_PATH=%~2
SET BE_API_URL=%~3

REM Var(s)
SET LOG_PATH=%PACKAGE_JSON_PATH%.ReactAppEnableRunLocal.log
..\bin\Debug\net6.0\Albelli.MiscUtils.CLI.exe ReactAppEnableRunLocal "%PACKAGE_JSON_PATH%" "%CONFIG_JSON_PATH%" "%BE_API_URL%" %4 %5 %6 %7 %8 %9 1> "%LOG_PATH%" 2>&1
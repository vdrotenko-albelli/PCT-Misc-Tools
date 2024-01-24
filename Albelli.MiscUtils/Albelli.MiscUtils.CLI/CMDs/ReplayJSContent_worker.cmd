@echo off
SET IN_FIL_PATH=%~1
SET API_URL=%~2
SET API_TOKEN=%~3

REM Var(s)
SET LOG_PATH=%IN_FIL_PATH%.ReplayJSContent.log
..\bin\Debug\net6.0\Albelli.MiscUtils.CLI.exe ReplayJSContent "%IN_FIL_PATH%" "%API_URL%" "%API_TOKEN%" %4 %5 %6 %7 %8 %9 1> "%LOG_PATH%" 2>&1
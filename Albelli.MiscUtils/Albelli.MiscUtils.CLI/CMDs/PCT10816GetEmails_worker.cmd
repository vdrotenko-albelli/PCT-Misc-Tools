@echo off
SET IN_FIL_PATH=%~1
SET API_URL=%~2
SET AUTH_TOKEN=%~3

REM Var(s)
SET LOG_PATH=%IN_FIL_PATH%.PCT10816GetEmails.log
SET ERR_LOG_PATH=%IN_FIL_PATH%.PCT10816GetEmails.err.log
..\bin\Debug\net6.0\Albelli.MiscUtils.CLI.exe PCT10816GetEmails "%IN_FIL_PATH%" "%API_URL%" "%AUTH_TOKEN%" %4 %5 %6 %7 %8 %9 1> "%LOG_PATH%" 2> "%ERR_LOG_PATH%"
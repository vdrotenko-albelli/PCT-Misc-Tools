@echo off
SET QLQ_URL=%~1

REM Var(s)
SET LOG_PATH=GetAwsQueuMessagesCount.log
..\bin\Debug\net6.0\Albelli.MiscUtils.CLI.exe GetAwsQueuMessagesCount "%QLQ_URL%" %2 %3 %4 %5 %6 %7 %8 %9 1> "%LOG_PATH%" 2>&1
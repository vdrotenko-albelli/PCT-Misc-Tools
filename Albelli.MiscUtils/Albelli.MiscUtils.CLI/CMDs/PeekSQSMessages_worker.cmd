@echo off

REM Var(s)
SET LOG_PATH=PeekSQSMessages.log
..\bin\Debug\net6.0\Albelli.MiscUtils.CLI.exe PeekSQSMessages %~1 %2 %3 %4 %5 %6 %7 %8 %9 1> "%LOG_PATH%" 2>&1
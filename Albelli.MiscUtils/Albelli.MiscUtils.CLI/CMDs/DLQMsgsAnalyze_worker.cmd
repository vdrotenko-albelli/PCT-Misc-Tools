@echo off
SET IN_PATH=%~1
SET KEY_PROPS=%~2
REM Var(s)
SET LOG_PATH=%IN_PATH%.DLQMsgsAnalyze.log
..\bin\Debug\net6.0\Albelli.MiscUtils.CLI.exe DLQMsgsAnalyze "%IN_PATH%" "%KEY_PROPS%" %3 %4 %5 %6 %7 %8 %9 1> "%LOG_PATH%" 2>&1
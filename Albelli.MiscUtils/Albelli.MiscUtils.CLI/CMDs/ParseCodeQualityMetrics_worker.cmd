@echo off
SET IN_FILS_DIR=%~1
SET IN_FILS_MASK=%~2
SET UT_FIL_PATH=%~3

REM Var(s)
SET LOG_PATH=%IN_FILS_DIR%.ParseCodeQualityMetrics.log
..\bin\Debug\net6.0\Albelli.MiscUtils.CLI.exe ParseCodeQualityMetrics "%IN_FILS_DIR%" "%IN_FILS_MASK%" "%UT_FIL_PATH%" %4 %5 %6 %7 %8 %9 1> "%LOG_PATH%" 2>&1
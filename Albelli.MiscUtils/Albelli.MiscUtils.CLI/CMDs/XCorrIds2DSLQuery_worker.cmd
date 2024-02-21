@echo off
SET IN_PATH=%~1
REM Var(s)
SET LOG_PATH=%IN_PATH%.XCorrIds2DSLQuery.log
..\bin\Debug\net6.0\Albelli.MiscUtils.CLI.exe XCorrIds2DSLQuery "%IN_PATH%" %2 %3 %4 %5 %6 %7 %8 %9 1> "%LOG_PATH%" 2>&1
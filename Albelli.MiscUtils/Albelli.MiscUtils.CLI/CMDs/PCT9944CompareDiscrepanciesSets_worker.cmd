@echo off
SET SRC_CSV1=%~1
SET SRC_CSV2=%~2

REM Var(s)
SET LOG_PATH=%SRC_CSV2%.PCT9944CompareDiscrepanciesSets.log
..\bin\Debug\net6.0\Albelli.MiscUtils.CLI.exe PCT9944CompareDiscrepanciesSets "%SRC_CSV1%" "%SRC_CSV2%" %3 %4 %5 %6 %7 %8 %9 1> "%LOG_PATH%" 2>&1
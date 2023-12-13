@echo off
SET INPUT_JSON_PATH=%~1

REM Var(s)

SET LOG_PATH=%INPUT_JSON_PATH%.AnalyzePCT9944NoDiscrepancies.log

:Exec
..\bin\Debug\net6.0\Albelli.MiscUtils.CLI.exe AnalyzePCT9944NoDiscrepancies "%INPUT_JSON_PATH%" %2 %3 %4 %5 %6 %7 %8 %9 1> "%LOG_PATH%" 2>&1
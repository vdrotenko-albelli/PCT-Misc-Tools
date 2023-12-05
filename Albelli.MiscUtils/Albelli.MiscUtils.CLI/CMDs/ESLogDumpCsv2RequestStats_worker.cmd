@echo off
SET SRC_CSV=%~1

REM Var(s)
SET LOG_PATH=%SRC_CSV%.xtract.stats.log
..\bin\Debug\net6.0\Albelli.MiscUtils.CLI.exe ESLogDumpCsv2RequestStats "%SRC_CSV%" %2 %3 %4 %5 %6 %7 %8 %9 1> "%LOG_PATH%" 2>&1
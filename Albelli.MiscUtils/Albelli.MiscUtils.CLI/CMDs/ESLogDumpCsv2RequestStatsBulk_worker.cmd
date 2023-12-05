@echo off
SET SRC_CSV_LST_PATH=%~1

REM Var(s)
SET LOG_PATH=%SRC_CSV_LST_PATH%.xtract.stats.bulk.log
..\bin\Debug\net6.0\Albelli.MiscUtils.CLI.exe ESLogDumpCsv2RequestStatsBulk "%SRC_CSV_LST_PATH%" %2 %3 %4 %5 %6 %7 %8 %9 1> "%LOG_PATH%" 2>&1
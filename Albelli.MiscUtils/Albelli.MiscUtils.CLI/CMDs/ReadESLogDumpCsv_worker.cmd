@echo off
SET SRC_CSV=%~1
SET PRINT_ROWS=%~2
SET PRINT_COL_NM=%~3

REM Var(s)
SET LOG_PATH=%SRC_CSV%.xtract.log
..\bin\Debug\net6.0\Albelli.MiscUtils.CLI.exe ReadESLogDumpCsv "%SRC_CSV%" "%PRINT_ROWS%" "%PRINT_COL_NM%" 1> "%LOG_PATH%" 2>&1
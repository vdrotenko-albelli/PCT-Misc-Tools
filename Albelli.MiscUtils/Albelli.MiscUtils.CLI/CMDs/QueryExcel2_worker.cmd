@echo off
SET IN_FIL_PATH=%~1
SET QUERY=%~2
REM Var(s)
SET LOG_PATH=%IN_FIL_PATH%.QueryExcel2.log
..\bin\Debug\net6.0\Albelli.MiscUtils.CLI.exe QueryExcel2 "%IN_FIL_PATH%" "%QUERY%" %3 %4 %5 %6 %7 %8 %9 1> "%LOG_PATH%" 2>&1
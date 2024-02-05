@echo off
SET FOLDER_PATH=%~1
SET CSV_MASK=%~2

REM Var(s)
SET LOG_PATH=%FOLDER_PATH%\PCT9247Extract500s.log
..\bin\Debug\net6.0\Albelli.MiscUtils.CLI.exe PCT9247Extract500s "%FOLDER_PATH%" "%CSV_MASK%" %3 %4 %5 %6 %7 %8 %9 1> "%LOG_PATH%" 2>&1
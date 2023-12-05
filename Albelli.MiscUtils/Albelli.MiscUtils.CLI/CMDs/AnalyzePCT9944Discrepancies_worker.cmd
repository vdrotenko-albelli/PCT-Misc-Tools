@echo off
SET SRC_CSV=%~1
SET PLANT_CODE=%~2

REM Var(s)
IF ""=="%PLANT_CODE%" (GOTO SetDefaultLogFile) ELSE (GOTO SetLogFileWPlant)
:SetDefaultLogFile
SET LOG_PATH=%SRC_CSV%.AnalyzePCT9944Discrepancies.log
GOTO Exec

:SetLogFileWPlant
SET LOG_PATH=%SRC_CSV%.AnalyzePCT9944Discrepancies.%PLANT_CODE%.log
GOTO Exec

:Exec
..\bin\Debug\net6.0\Albelli.MiscUtils.CLI.exe AnalyzePCT9944Discrepancies "%SRC_CSV%" "%PLANT_CODE%" %3 %4 %5 %6 %7 %8 %9 1> "%LOG_PATH%" 2>&1
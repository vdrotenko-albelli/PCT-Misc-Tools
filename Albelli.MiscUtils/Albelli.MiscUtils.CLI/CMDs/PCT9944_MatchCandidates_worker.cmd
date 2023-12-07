@echo off
SET MATRIX_PATH=%~1
SET ZONES_PATH=%~2
SET REQ_JSON_PATH=%~3
REM Var(s)
SET LOG_PATH=%REQ_JSON_PATH%.PCT9944_MatchCandidates.log
..\bin\Debug\net6.0\Albelli.MiscUtils.CLI.exe PCT9944_MatchCandidates "%MATRIX_PATH%" "%ZONES_PATH%" "%REQ_JSON_PATH%" %4 %5 %6 %7 %8 %9 1> "%LOG_PATH%" 2>&1
@echo off
SET IN_FIL_PATH_V1=%~1
SET IN_FIL_PATH_V2=%~2
SET API_URL_V1=%~3
SET API_TOKEN_V1=%~4
SET API_URL_V2=%~5
SET API_TOKEN_V2=%~6
SET UT_PATH=%~7

REM Var(s)
SET LOG_PATH=%IN_FIL_PATH_V1%.PCT9247ReplayCompareV1vsV2.log
..\bin\Debug\net6.0\Albelli.MiscUtils.CLI.exe PCT9247ReplayCompareV1vsV2 "%IN_FIL_PATH_V1%" "%IN_FIL_PATH_V2%" "%API_URL_V1%" "%API_TOKEN_V1%" "%API_URL_V2%" "%API_TOKEN_V2%" "%UT_PATH%" %8 %9 1> "%LOG_PATH%" 2>&1
@echo off
SET SRC_XLS_PATH=%~1
SET SRC_SHEET_NM=%~2
SET API_URL=%~3
SET API_TOKEN=%~4
SET API_URL_T=%~5
SET API_TOKEN_T=%~6
SET INPUT_JSON_PATH=%~7
SET DBG=%~8

REM Var(s)
SET LOG_PATH=%SRC_XLS_PATH%.PCT9944BulkRevalidate2.log
..\bin\Debug\net6.0\Albelli.MiscUtils.CLI.exe PCT9944BulkRevalidate2 "%SRC_XLS_PATH%" "%SRC_SHEET_NM%" "%API_URL%"  "%API_TOKEN%" "%API_URL_T%"  "%API_TOKEN_T%" "%INPUT_JSON_PATH%" "%DBG%" %9  1> "%LOG_PATH%" 2>&1 
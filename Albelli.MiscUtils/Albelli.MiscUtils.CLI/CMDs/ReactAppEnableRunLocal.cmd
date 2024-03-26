@echo off
REM SET APP_ROOT=D:\git\PTBX-NDC\4merge\Virtual-Plant-MyFactory-UI\src\App
SET APP_ROOT=D:\git\PTBX-NDC\srcs\Virtual-Plant-MyFactory-UI\src\App

REM CALL ReactAppEnableRunLocal_worker.cmd "%APP_ROOT%\package.json" "%APP_ROOT%\public\config\config.json" "https://localhost:44344"
CALL ReactAppEnableRunLocal_worker.cmd "%APP_ROOT%\package.json"
@echo off
SET SUBJ_ROOT_DIR=%~1

powershell Clean_build.ps1 -rootDir "%SUBJ_ROOT_DIR%"
@echo off


SET INPUTS_DIR=F:\home\vmdrot\EPAM\Projs\PTBX-NDC\JIRA\PCT-9944\EsLogs
REM CALL AnalyzePCT9944Discrepancies_worker.cmd F:\home\vmdrot\EPAM\Projs\PTBX-NDC\JIRA\PCT-9944\EsLogs\On_demand_report_2023-12-04T16_57_48.397Z_40d561d0-92c6-11ee-9c82-130304045cef.csv

REM CALL AnalyzePCT9944Discrepancies_worker.cmd "%INPUTS_DIR%\Last14h_12061109.csv" "" "%INPUTS_DIR%\Last14h_12061109.csv.json" -Debug
CALL AnalyzePCT9944Discrepancies_worker.cmd "%INPUTS_DIR%\Last14h_12061109.csv" "" "%INPUTS_DIR%\Last14h_12061109.csv.tab" -Debug
REM CALL AnalyzePCT9944Discrepancies_worker.cmd "%INPUTS_DIR%\Last24h_12061109.csv" "" "%INPUTS_DIR%\Last24h_12061109.csv.json"
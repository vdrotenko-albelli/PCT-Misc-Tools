@echo off
REM SET INPUTS_DIR=F:\home\vmdrot\EPAM\Projs\PTBX-NDC\JIRA\PCT-9944\EsLogs\1211\NoDiscr
REM SET CURR_INPUT_CSV_FN=On_demand_report_2023-12-11T11_59_25.058Z_ba80e620-981c-11ee-849f-d3e39ba8a1dd.csv
REM CALL AnalyzePCT9944NoDiscrepancies_worker.cmd "%INPUTS_DIR%\%CURR_INPUT_CSV_FN%" "" "%INPUTS_DIR%\%CURR_INPUT_CSV_FN%.tab" -Debug
CALL AnalyzePCT9944NoDiscrepancies_worker.cmd F:\home\vmdrot\EPAM\Projs\PTBX-NDC\JIRA\PCT-9944\EsLogs\1211\NoDiscr\Nodiscrepancies20231211Cfg.v2.json -SkipLogo

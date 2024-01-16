@echo off


REM SET INPUTS_DIR=F:\home\vmdrot\EPAM\Projs\PTBX-NDC\JIRA\PCT-9944\EsLogs
REM CALL AnalyzePCT9944Discrepancies_worker.cmd F:\home\vmdrot\EPAM\Projs\PTBX-NDC\JIRA\PCT-9944\EsLogs\On_demand_report_2023-12-04T16_57_48.397Z_40d561d0-92c6-11ee-9c82-130304045cef.csv

REM CALL AnalyzePCT9944Discrepancies_worker.cmd "%INPUTS_DIR%\Last14h_12061109.csv" "" "%INPUTS_DIR%\Last14h_12061109.csv.json" -Debug
REM CALL AnalyzePCT9944Discrepancies_worker.cmd "%INPUTS_DIR%\Last14h_12061109.csv" "" "%INPUTS_DIR%\Last14h_12061109.csv.tab" -Debug
REM CALL AnalyzePCT9944Discrepancies_worker.cmd "%INPUTS_DIR%\Last24h_12061109.csv" "" "%INPUTS_DIR%\Last24h_12061109.csv.json"
REM SET INPUTS_DIR=F:\home\vmdrot\EPAM\Projs\PTBX-NDC\JIRA\PCT-9944\EsLogs\1211\Discr
REM CALL AnalyzePCT9944Discrepancies_worker.cmd "%INPUTS_DIR%\On_demand_report_2023-12-11T12_40_52.333Z_8508f1d0-9822-11ee-a7d9-c5efb645eba0.csv"  "" "%INPUTS_DIR%\Last56h_12110924.csv.tab"
REM SET INPUTS_DIR=F:\home\vmdrot\EPAM\Projs\PTBX-NDC\JIRA\PCT-9944\EsLogs\1212\Discr
REM CALL AnalyzePCT9944Discrepancies_worker.cmd "%INPUTS_DIR%\On_demand_report_2023-12-12T11_21_06.419Z_8ad22c30-98e0-11ee-9c82-130304045cef.csv"  "" "%INPUTS_DIR%\Last72h_12121336.csv.tab" -Debug
REM CALL AnalyzePCT9944Discrepancies_worker.cmd "%INPUTS_DIR%\Last72h_12121336_reconfirmed.csv"  "" "%INPUTS_DIR%\Last72h_12121336_reconfirmed.csv.tab" -Debug
REM SET INPUTS_DIR=F:\home\vmdrot\EPAM\Projs\PTBX-NDC\JIRA\PCT-9944\EsLogs\1213\Discr
REM CALL AnalyzePCT9944Discrepancies_worker.cmd "%INPUTS_DIR%\On_demand_report_2023-12-12T11_21_06.419Z_8ad22c30-98e0-11ee-9c82-130304045cef.csv"  "" "%INPUTS_DIR%\Last72h_12121336.csv.tab" -Debug
REM CALL AnalyzePCT9944Discrepancies_worker.cmd "%INPUTS_DIR%\On_demand_report_2023-12-13T08_34_48.421Z_79e24150-9992-11ee-849f-d3e39ba8a1dd.csv"  "" "%INPUTS_DIR%\New_discr_1213.tab"
REM CALL AnalyzePCT9944Discrepancies_worker.cmd "%INPUTS_DIR%\On_demand_report_2023-12-13T10_58_09.541Z_808d0350-99a6-11ee-9c82-130304045cef.csv"  "" "%INPUTS_DIR%\New_discr_1213_e01.tab"
REM SET INPUTS_DIR=F:\home\vmdrot\EPAM\Projs\PTBX-NDC\JIRA\PCT-9944\EsLogs\1215\Discr
REM CALL AnalyzePCT9944Discrepancies_worker.cmd "%INPUTS_DIR%\Test_Acc_Discr_e01.csv"  "" "%INPUTS_DIR%\Test_Acc_Discr_e01.tab"
REM SET INPUTS_DIR=F:\home\vmdrot\EPAM\Projs\PTBX-NDC\JIRA\PCT-9944\EsLogs\1218\Discr
REM CALL AnalyzePCT9944Discrepancies_worker.cmd "%INPUTS_DIR%\Discr_PROD_1216-18.csv"  "" "%INPUTS_DIR%\Discr_PROD_1216-18_anlz.tab"

REM SET INPUTS_DIR=F:\home\vmdrot\EPAM\Projs\PTBX-NDC\JIRA\PCT-9944\EsLogs\1219\Discr
REM CALL AnalyzePCT9944Discrepancies_worker.cmd "%INPUTS_DIR%\Discr_TEST_ACC_1219.csv"  "" "%INPUTS_DIR%\Discr_TEST_ACC_1219_anlz.tab"
REM CALL AnalyzePCT9944Discrepancies_worker.cmd "%INPUTS_DIR%\Discr_PROD_1219.csv"  "" "%INPUTS_DIR%\Discr_PROD_1219_anlz.tab"

SET INPUTS_DIR=F:\home\vmdrot\EPAM\Projs\PTBX-NDC\JIRA\PCT-9944\EsLogs\0115\Discr
CALL AnalyzePCT9944Discrepancies_worker.cmd "%INPUTS_DIR%\Discr_PROD_0115.csv"  "" "%INPUTS_DIR%\Discr_PROD_0115_anlz.tab"

@echo off
REM CALL QueryExcel_worker.cmd F:\home\vmdrot\EPAM\Projs\PTBX-NDC\JIRA\PCT-9944\ACSS\latest\Matrix_nl@albelli.com_v7.xlsx "[Carrier Code] = 'std.postnlws.com'"
REM CALL QueryExcel_worker.cmd F:\home\vmdrot\EPAM\Projs\PTBX-NDC\JIRA\PCT-9944\ACSS\latest\Matrix_nl@albelli.com_v7.xlsx "[Is Active] = 'true' AND [Zone Code] IN ('NO','Non-EU') AND [Package Type] IN ('Box', '*')" "Priority ASC"
REM below is unsupported
REM CALL QueryExcel_worker.cmd F:\home\vmdrot\EPAM\Projs\PTBX-NDC\JIRA\PCT-9944\ACSS\latest\Matrix_nl@albelli.com_v7.xlsx "[Is Active] = 'true' AND [Zone Code] IN ('NO','Non-EU') AND [Package Type] IN ('Box', '*') AND [Max Weight] BETWEEN 1000 AND 20000" "Priority ASC"
REM CALL QueryExcel_worker.cmd F:\home\vmdrot\EPAM\Projs\PTBX-NDC\JIRA\PCT-9944\ACSS\latest\Matrix_nl@albelli.com_v7.xlsx "[Is Active] = 'true' AND [Zone Code] IN ('NO','Non-EU') AND [Package Type] IN ('Box', '*') AND ([Max Weight] = 0 OR [Max Weight] = 2000)" "Priority ASC"
CALL QueryExcel_worker.cmd F:\home\vmdrot\EPAM\Projs\PTBX-NDC\JIRA\PCT-9944\ACSS\latest\ZoneList_nl@albelli.com_v3.xlsx "Country = 'NO'" "Priority ASC"
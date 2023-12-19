@echo off
REM CALL PCT9944CompareDiscrepanciesSets_worker.cmd F:\home\vmdrot\EPAM\Projs\PTBX-NDC\JIRA\PCT-9944\EsLogs\1212\Discr\On_demand_report_2023-12-12T13_17_53.924Z_db9e9440-98f0-11ee-b6ee-13a0ea5357e0.csv F:\home\vmdrot\EPAM\Projs\PTBX-NDC\JIRA\PCT-9944\EsLogs\1213\Discr\On_demand_report_2023-12-13T10_58_09.541Z_808d0350-99a6-11ee-9c82-130304045cef.csv
SET FLDR=F:\home\vmdrot\EPAM\Projs\PTBX-NDC\JIRA\PCT-9944\EsLogs\1213\Discr
SET ONE=On_demand_report_2023-12-13T10_58_09.541Z_808d0350-99a6-11ee-9c82-130304045cef.csv
SET TWO=On_demand_report_2023-12-13T12_32_26.320Z_ac40fd00-99b3-11ee-9c82-130304045cef.csv
CALL PCT9944CompareDiscrepanciesSets_worker.cmd "%FLDR%\%ONE%" "%FLDR%\%TWO%"
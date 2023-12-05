@echo off
CALL ExtractAllSALsFromESLogs_worker.cmd "F:\home\vmdrot\EPAM\Projs\PTBX-NDC\JIRA\PCT-10418\ESLogs\On_demand_report_2023-11-20T16_00_04.289Z_de481a20-87bd-11ee-9fc5-350285b2a09e.csv" -SkipLogo
CALL ExtractAllSALsFromESLogs_worker.cmd F:\home\vmdrot\EPAM\Projs\PTBX-NDC\JIRA\PCT-10480\eslogs\SALs_Last2d.csv -SkipLogo
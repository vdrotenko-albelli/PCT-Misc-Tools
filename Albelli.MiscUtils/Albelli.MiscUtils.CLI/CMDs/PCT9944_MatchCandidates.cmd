@echo off
REM SET MTRXS=F:\home\vmdrot\EPAM\Projs\PTBX-NDC\JIRA\PCT-9944\ACSS\latest\Matrix_nl@albelli.com_v7.xlsx
SET MTRXS=F:\home\vmdrot\EPAM\Projs\PTBX-NDC\JIRA\PCT-9944\ACSS\latest\Matrix_nl@albelli.com_v11.xlsx
SET ZNS=F:\home\vmdrot\EPAM\Projs\PTBX-NDC\JIRA\PCT-9944\ACSS\latest\ZoneList_nl@albelli.com_v3.xlsx
CALL PCT9944_MatchCandidates_worker  "%MTRXS%" "%ZNS%" F:\home\vmdrot\EPAM\Projs\PTBX-NDC\JIRA\PCT-9944\EsLogs\test_req00.json
CALL PCT9944_MatchCandidates_worker  "%MTRXS%" "%ZNS%" F:\home\vmdrot\EPAM\Projs\PTBX-NDC\JIRA\PCT-9944\EsLogs\test_req01.json
CALL PCT9944_MatchCandidates_worker  "%MTRXS%" "%ZNS%" F:\home\vmdrot\EPAM\Projs\PTBX-NDC\JIRA\PCT-9944\EsLogs\test_req02.json
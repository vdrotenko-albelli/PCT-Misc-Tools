@echo off
REM SET __URL2__=https://localhost:44344/v2/carriers/available
REM SET __TKN2__=eyJhbGciOiJSUzI1NiIsImtpZCI6Ijk0RUJBODcwNzA5MEJCN0NBRTA0NDRBRDlENUM1QzI3IiwieDV0IjoiTFU0YnpMdUhsSkhWUVJ1M2ZCUFFqMlZmeUZFIiwidHlwIjoiSldUIn0.eyJpc3MiOiJodHRwczovL3QtZHRhcC5sb2dpbi5hbGJlbGxpLmNvbS92Mi8iLCJuYmYiOjE3MDYyOTI0NDYsImlhdCI6MTcwNjI5MjQ0NiwiZXhwIjoxNzA2Mjk2MDQ2LCJhdWQiOiJodHRwczovL3QtZHRhcC5sb2dpbi5hbGJlbGxpLmNvbS92Mi9yZXNvdXJjZXMiLCJzY29wZSI6WyJjYXJyaWVyLnJlYWQiLCJjYXJyaWVyLndyaXRlIiwiY2Fycmllci5jYWxjdWxhdGUiLCJjYXJyaWVyb3ZlcnJpZGVzLnJlYWQiLCJjYXJyaWVyb3ZlcnJpZGVzLndyaXRlIiwic2hpcG1lbnRzLnJlYWQiLCJzaGlwbWVudHMud3JpdGUiLCJkZWxpdmVyeW9wdGlvbnMucmVhZCIsImNhcnJpZXJjb25maWd1cmF0aW9ucy53cml0ZSIsImNhcnJpZXJjb25maWd1cmF0aW9ucy5yZWFkIl0sImFtciI6WyJleHRlcm5hbCJdLCJjbGllbnRfaWQiOiI1OGJlMWJlNWUyYjA0Y2Q0YmY1OGJmMDM2MDgzNmEyMyIsInN1YiI6ImVhMTA3YWEyOTg4YjQ1ZDg4NjI2YzczZDY0YWU1NTViIiwiYXV0aF90aW1lIjoxNzA2Mjg1NDQ3LCJpZHAiOiJhbGJlbGxpZG9tYWluIiwiZW1haWwiOiJ2YWxlcml5LmRyb3RlbmtvQGFsYmVsbGkuY29tIiwidXJuOmNsaWVudDo1OGJlMWJlNWUyYjA0Y2Q0YmY1OGJmMDM2MDgzNmEyMzpjbGllbnROYW1lIjoiNThiZTFiZTVlMmIwNGNkNGJmNThiZjAzNjA4MzZhMjMiLCJpZHBfc3ViIjoiMzIzMjM3ODhiMTc4NDU2M2IzZTI0MWU1M2QxZDRhNjAiLCJpZHBfdHlwZSI6ImFsYmVsbGlkb21haW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL2F1dGhlbnRpY2F0aW9ubWV0aG9kIjoiZG9tYWluIiwic2lkIjoiRDc5QkM0MEJDODAwNzJDNUM2NEVEOEM4N0MzMzJCQ0IiLCJqdGkiOiI4MTA2RUMwRjBDRDJGQzY3Qzg5NzZCNTdGQTQ2NUVDRCJ9.qzR7VgfHcVVHW7PJqBto36b4aw3gqcLV8mnbq_zUhSlgTs9I2cjD5jG2uqdiDlq6cX813ZTqL3yhzGJgExPX0KbvsKcpCAInnmSgcbUUkfSHQAcXi-DBZLgD8oE8aWUIkLJsVde5YY9E3o840zxulJ13p7yvHtKfMbh7ZI7fWToK0j2Yy_hsilrc-taExB2edes3znx9aCeTpIr_JDJxM6WPHlQrlWYWmEV6mN8CsA6l8Do1CAyDaIxNrDSGnfUDRZvxYhKhhj5xfT1RCd-r0X8cpkDl261J-OZRACsKwcgbPZD_F_G05s9HdrBpkA8SJeyg2hf5YU20kM6DP5sxkQ

SET __URL2__=https://carrier.tst.shipping.infra.photos/v2/carriers/available
SET __TKN2__=eyJhbGciOiJSUzI1NiIsImtpZCI6Ijk0RUJBODcwNzA5MEJCN0NBRTA0NDRBRDlENUM1QzI3IiwieDV0IjoiTFU0YnpMdUhsSkhWUVJ1M2ZCUFFqMlZmeUZFIiwidHlwIjoiSldUIn0.eyJpc3MiOiJodHRwczovL3QtZHRhcC5sb2dpbi5hbGJlbGxpLmNvbS92Mi8iLCJuYmYiOjE3MDY1Mzk0MTksImlhdCI6MTcwNjUzOTQxOSwiZXhwIjoxNzA2NTQzMDE5LCJhdWQiOiJodHRwczovL3QtZHRhcC5sb2dpbi5hbGJlbGxpLmNvbS92Mi9yZXNvdXJjZXMiLCJzY29wZSI6WyJjYXJyaWVyLnJlYWQiLCJjYXJyaWVyLndyaXRlIiwiY2Fycmllci5jYWxjdWxhdGUiLCJjYXJyaWVyb3ZlcnJpZGVzLnJlYWQiLCJjYXJyaWVyb3ZlcnJpZGVzLndyaXRlIiwic2hpcG1lbnRzLnJlYWQiLCJzaGlwbWVudHMud3JpdGUiLCJkZWxpdmVyeW9wdGlvbnMucmVhZCIsImNhcnJpZXJjb25maWd1cmF0aW9ucy53cml0ZSIsImNhcnJpZXJjb25maWd1cmF0aW9ucy5yZWFkIl0sImFtciI6WyJleHRlcm5hbCJdLCJjbGllbnRfaWQiOiI1OGJlMWJlNWUyYjA0Y2Q0YmY1OGJmMDM2MDgzNmEyMyIsInN1YiI6ImVhMTA3YWEyOTg4YjQ1ZDg4NjI2YzczZDY0YWU1NTViIiwiYXV0aF90aW1lIjoxNzA2NTMyMDg0LCJpZHAiOiJhbGJlbGxpZG9tYWluIiwiZW1haWwiOiJ2YWxlcml5LmRyb3RlbmtvQGFsYmVsbGkuY29tIiwidXJuOmNsaWVudDo1OGJlMWJlNWUyYjA0Y2Q0YmY1OGJmMDM2MDgzNmEyMzpjbGllbnROYW1lIjoiNThiZTFiZTVlMmIwNGNkNGJmNThiZjAzNjA4MzZhMjMiLCJpZHBfc3ViIjoiMzIzMjM3ODhiMTc4NDU2M2IzZTI0MWU1M2QxZDRhNjAiLCJpZHBfdHlwZSI6ImFsYmVsbGlkb21haW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL2F1dGhlbnRpY2F0aW9ubWV0aG9kIjoiZG9tYWluIiwic2lkIjoiNENDODYxNDhGOUQ2NjAyNDk0MUU5RjlBMzlCNzc0REEiLCJqdGkiOiI5RTM4QzNEQTBGNzc5MzE2RjZEQThFQkZBNkMwODhERSJ9.gH4rto863QQ3ISyUCYKDSMtE_T_ydKeV0D16TVO67NUXkvRO8ndSjWx3zox2Az71_gG2MJi_yli4Td_6n6NaxM-_oqZOoJmzjzzEiCHl6Grsps-MxLoH0Dex0yOLkvD35j0xEJtFiug54TrIBz6w8NHgBlhk6EwBv_QZGPZdwIg7mwkfyw90Qu07xwiTyrJV6yyKmR-n3CljglP-HQ4hyuae9WBFpKdjhMI37Vhiq7AA85TKZrUBuXJcyO5n1Y5cgg3-E-E2GMp4aal6Obea8TFeAH7p6-feTZq_ci8fcIkys38D1hlQcKWS9nMkG7jo5vSNNkwHLEOA_lVPkbrgOQ
CALL PCT9247Replay500s_worker.cmd "F:\home\vmdrot\EPAM\Projs\PTBX-NDC\JIRA\PCT-9247\eslogs" "On_demand_report_*.csv" "%__URL2__%" "%__TKN2__%" false


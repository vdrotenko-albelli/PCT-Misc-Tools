@echo off
SET TKN=eyJhbGciOiJSUzI1NiIsImtpZCI6IjQwQzFDNDM0OTUyOThDM0JDQzQ1NkY4OEI2M0YwRjQ2IiwieDV0Ijoib3N6a0lKaE1wM20yd3JvNmtfd0liVmEyTlNjIiwidHlwIjoiSldUIn0.eyJpc3MiOiJodHRwczovL3QtZHRhcC5sb2dpbi5hbGJlbGxpLmNvbS92Mi8iLCJuYmYiOjE3MDMwMTc0ODgsImlhdCI6MTcwMzAxNzQ4OCwiZXhwIjoxNzAzMDIxMDg4LCJhdWQiOiJodHRwczovL3QtZHRhcC5sb2dpbi5hbGJlbGxpLmNvbS92Mi9yZXNvdXJjZXMiLCJzY29wZSI6WyJwcm9kdWN0aW9uc2NoZWR1bGluZy5sZWFkdGltZS5yZWFkIiwicHJvZHVjdGlvbnNjaGVkdWxpbmcubGVhZHRpbWUud3JpdGUiXSwiYW1yIjpbImV4dGVybmFsIl0sImNsaWVudF9pZCI6IjQyNmUwYzFjY2E5OTQxODM5YTc0ZGI5Nzk0NDNjOTlhIiwic3ViIjoiZWExMDdhYTI5ODhiNDVkODg2MjZjNzNkNjRhZTU1NWIiLCJhdXRoX3RpbWUiOjE3MDMwMTc0ODcsImlkcCI6ImFsYmVsbGlkb21haW4iLCJlbWFpbCI6InZhbGVyaXkuZHJvdGVua29AYWxiZWxsaS5jb20iLCJ1cm46Y2xpZW50OjQyNmUwYzFjY2E5OTQxODM5YTc0ZGI5Nzk0NDNjOTlhOmNsaWVudE5hbWUiOiI0MjZlMGMxY2NhOTk0MTgzOWE3NGRiOTc5NDQzYzk5YSIsImlkcF9zdWIiOiIzMjMyMzc4OGIxNzg0NTYzYjNlMjQxZTUzZDFkNGE2MCIsImlkcF90eXBlIjoiYWxiZWxsaWRvbWFpbiIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvYXV0aGVudGljYXRpb25tZXRob2QiOiJkb21haW4iLCJzaWQiOiI1QjM5RDI3RTRFRjAwRjczMkM2QTk3RDM0NjFDNEQ0QiIsImp0aSI6Ijg4QTY1OUI5MDdBMjRBRDE1QjczM0Y1RDREOTlBMkQ5In0.EjDJlovlemKvFifL5OV0Ve8ie6_WWb0LvoR7bukFXLfxGLr0uOssDyhp51gLXLKu7oWXmxz08g5KsRXDkyl_OnvImLzqjgzSx8WXZ9F_y4ZmUfI_xFGcXiYztN6GwFONHYvtiVZk1QVtZhAe5FMwIvIbeuZ-ndq8Gb-Wzbkfzzretepw5xwOuKNwxvzdheYNRqKKtwJrCjInsowOIb1V-X4mCcyb6TnzB4AiwlJLwuNvmkTuysIH74SDgcaQMHg5EIfQQWfaqL2M723c9Js3WthC1DS9R7xdPUnsIGbvfvcuImRF1yFb2xAkH8fqgvhnFdFYyPW7xCZeyl2eEU772Q
CALL PCT10679ExportLeadtimes_worker Test "%TKN%" F:\home\vmdrot\EPAM\Projs\PTBX-NDC\JIRA\PCT-10679\all-leadtimes.Test.csv

SET TKN=eyJhbGciOiJSUzI1NiIsImtpZCI6IjFGNTYwN0ZERTlCQzUzNDc5QzBGM0ExQjBDMUMxQTYzIiwieDV0IjoiOHVHWU5jczZuSUJyWmVINWsyM1VrTVJUNW04IiwidHlwIjoiSldUIn0.eyJpc3MiOiJodHRwczovL2xvZ2luLmFsYmVsbGkuY29tL3YyLyIsIm5iZiI6MTcwMzAxODc1MywiaWF0IjoxNzAzMDE4NzUzLCJleHAiOjE3MDMwMjIzNTMsImF1ZCI6Imh0dHBzOi8vbG9naW4uYWxiZWxsaS5jb20vdjIvcmVzb3VyY2VzIiwic2NvcGUiOlsicHJvZHVjdGlvbnNjaGVkdWxpbmcubGVhZHRpbWUucmVhZCIsInByb2R1Y3Rpb25zY2hlZHVsaW5nLmxlYWR0aW1lLndyaXRlIl0sImFtciI6WyJleHRlcm5hbCJdLCJjbGllbnRfaWQiOiI3NTQzZGQ5ZDk4ZTY0NjJhOTFlYzlhNWVlZWFmOGVjNiIsInN1YiI6IjlhNzY4ZTEwMWMxOTQ4NDlhNjY1YmIzZTQzY2ExYTJmIiwiYXV0aF90aW1lIjoxNzAzMDAwNDA3LCJpZHAiOiJhbGJlbGxpZG9tYWluIiwiZW1haWwiOiJ2ZHJvdGVua29AYWxiZWxsaS5uZXQiLCJ1cm46Y2xpZW50Ojc1NDNkZDlkOThlNjQ2MmE5MWVjOWE1ZWVlYWY4ZWM2OmNsaWVudE5hbWUiOiI3NTQzZGQ5ZDk4ZTY0NjJhOTFlYzlhNWVlZWFmOGVjNiIsImlkcF9zdWIiOiI5YzBlMmNlN2NiMWY0MzU5ODYwMzBhYjM0ODI2ZDczOSIsImlkcF90eXBlIjoiYWxiZWxsaWRvbWFpbiIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvYXV0aGVudGljYXRpb25tZXRob2QiOiJkb21haW4iLCJzaWQiOiI3Qjg1MTFCODJFM0Q1NTI1MkMzREU3MkNDNzc0MkE4NSIsImp0aSI6IjgxNjlCMzM0RjUwOEI4RTdGMUM0RDJGRTQ5ODUxQUEwIn0.CEShJbF5Nk9WUnLjONMsUQuGl_RtKpy8bFl3bJfCmAtRZ_ocRhgwSGGBMBYLmQ1ToqjdjsCMvSt0BtPnOWoyr76hXwaadrGDbB5JkdXsajPUed0h9i3ypS4m_50yBV-BeCtZ5FOxw92YC46TF7TVT6XwxEL6g6yHWnz6IMVH_eGqlSRuKxXSySsIPuLRAVc-Z4feLm_QAXHLGywqokSveGdE5bHvRmEgAtThI7EetpjCtU_c3o3QB2PWvQlFEN32E27XuA5RoXPf-jWMxlv9YtCMhRfOtsXfDNbmTPAm5eo1Ib64lXqjr08NAOzAMYsZZ29Irukhmy7uFRZIH276uQ
CALL PCT10679ExportLeadtimes_worker PROD "%TKN%" F:\home\vmdrot\EPAM\Projs\PTBX-NDC\JIRA\PCT-10679\all-leadtimes.PROD.csv

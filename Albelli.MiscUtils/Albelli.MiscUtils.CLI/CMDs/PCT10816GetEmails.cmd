@echo off
SET CBO_API_URL=https://api.backoffice.cct.albelli.com/orders/
SET CBO_API_TOKEN=eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6ImtXYmthYTZxczh3c1RuQndpaU5ZT2hIYm5BdyIsImtpZCI6ImtXYmthYTZxczh3c1RuQndpaU5ZT2hIYm5BdyJ9.eyJhdWQiOiJjYmEzOGFkYy02N2M2LTQ0MGYtODdlMi1iYTcwOGE3MjNjMGIiLCJpc3MiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC81YWM1YTBjMC0zZWU3LTQ4NDgtYmU1ZS0yMjZjODA4NjBmNDQvIiwiaWF0IjoxNzA3MTU2NjQ0LCJuYmYiOjE3MDcxNTY2NDQsImV4cCI6MTcwNzE2MDU0NCwiYW1yIjpbInB3ZCIsIm1mYSJdLCJmYW1pbHlfbmFtZSI6IkRyb3RlbmtvIiwiZ2l2ZW5fbmFtZSI6IlZhbGVyaXkiLCJoYXNncm91cHMiOiJ0cnVlIiwiaXBhZGRyIjoiOTEuMjEwLjI1MC44MiIsIm5hbWUiOiJWYWxlcml5IERyb3RlbmtvIiwibm9uY2UiOiJkNzFkZTI3ZC01MGUyLTQxMDItOTU0Yy0xZmEyNDgyYTM1NzAiLCJvaWQiOiI1ODAwNjRlOC0zZTI1LTRjM2EtYTg0Ny1jNTAwYzU3ZTliZjQiLCJvbnByZW1fc2lkIjoiUy0xLTUtMjEtMzQzMTg1OTE1NC02OTcwOTE1NjMtMjQzMDY0OTc2MS0xNjc4MCIsInJoIjoiMC5BU0VBd0tERld1Yy1TRWktWGlKc2dJWVBSTnlLbzh2R1p3OUVoLUs2Y0lweVBBdUdBR2MuIiwicm9sZXMiOlsicmVndWxhci1hZ2VudCIsImV4cGVydCIsInN1cHBvcnQtZW5naW5lZXIiLCJhbGJlbGxpLXZpZXdlciJdLCJzdWIiOiJTcVU2VnUwanVLci1qdVBmRnhOQjdzZkNOdUdxX1p6eV93Z0ZVVEY5eml3IiwidGlkIjoiNWFjNWEwYzAtM2VlNy00ODQ4LWJlNWUtMjI2YzgwODYwZjQ0IiwidW5pcXVlX25hbWUiOiJ2ZHJvdGVua29AYWxiZWxsaS5uZXQiLCJ1cG4iOiJ2ZHJvdGVua29AYWxiZWxsaS5uZXQiLCJ1dGkiOiJGZlpNbzkzLVUwV2pSR1laT04xUEFBIiwidmVyIjoiMS4wIn0.dHxBsrJ7REL2KofIK4LtA4yNQTh2z65dsW-VZ8adv30q5UyIVmrK3kYnS96Pv60BCke3l04InmPkXRzzZ-iVwDjBsxZ1UuG3bgq7Dzm8ZHN6xfelW1p79eTuUieAZFiVvg8EEUjdkDcP4naCSh7j8UKVV4gr_EGEUGpvt5JG86tBo82MuGnu3jlExxEv9FHryp_sCO2B9bBy_dSOEza3a-lVRWS4tlyfib25evPiIQXKA_ICgxnzlNeMvEPwail74ZZNDmTShRs4_PnhvRL11fC8IRMrlXdlqkTn9D9drwkWqUhMDgl5a5w6Mne1WbykdqNYjyUcD7Bs5GFgxbjfOA

CALL PCT10816GetEmails_worker.cmd F:\home\vmdrot\EPAM\Projs\PTBX-NDC\JIRA\PCT-10816\salIds4Emails.txt "%CBO_API_URL%" "%CBO_API_TOKEN%"
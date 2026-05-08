@echo off

cd /d "%~dp0backend"

start "" cmd /k "GymAdmin.Api.exe"

timeout /t 5 >nul

start http://localhost:5000

exit
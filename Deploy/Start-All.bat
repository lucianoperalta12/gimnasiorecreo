@echo off
SETLOCAL
echo Iniciando backend...
start "GymAdmin API" cmd /k "cd /d "%~dp0backend" && set "ASPNETCORE_URLS=http://localhost:5000" && dotnet GymAdmin.Api.dll"
timeout /t 2 >nul
echo Iniciando frontend simple server (servirá carpeta frontend en http://localhost:5173)...
start "GymFrontend" cmd /k "cd /d "%~dp0frontend" && npx serve -s . -l 5173"
echo Hecho. Backend en http://localhost:5000 y Frontend en http://localhost:5173
PAUSE

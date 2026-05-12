@echo off

echo =========================
echo BUILD FRONTEND
echo =========================

cd /d "%~dp0gym-frontend"
call npm run build

echo =========================
echo COPIANDO DIST A WWWROOT
echo =========================

:: Limpiar wwwroot viejo para evitar archivos basura
if exist "%~dp0GymAdmin\src\GymAdmin.Api\wwwroot" rd /s /q "%~dp0GymAdmin\src\GymAdmin.Api\wwwroot"
mkdir "%~dp0GymAdmin\src\GymAdmin.Api\wwwroot"

xcopy "dist\*" "%~dp0GymAdmin\src\GymAdmin.Api\wwwroot\" /E /Y

echo =========================
echo PUBLICANDO BACKEND
echo =========================

cd /d "%~dp0GymAdmin"
dotnet publish src\GymAdmin.Api\GymAdmin.Api.csproj -c Release -r win-x64 --self-contained true -o "%~dp0deploy-demo\backend"

echo =========================
echo DEPLOY COMPLETADO
echo =========================

pause

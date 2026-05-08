@echo off

echo =========================
echo BUILD FRONTEND
echo =========================

cd /d "C:\RepositoriosFront\Prueba Gemini\gym-frontend"

call npm run build

echo =========================
echo COPIANDO DIST A WWWROOT
echo =========================

xcopy "dist\*" "C:\RepositoriosFront\Prueba Gemini\GymAdmin\wwwroot\" /E /Y

echo =========================
echo PUBLICANDO BACKEND
echo =========================

cd /d "C:\RepositoriosFront\Prueba Gemini\GymAdmin"

dotnet publish -c Release -r win-x64 --self-contained true -o "C:\RepositoriosFront\Prueba Gemini\deploy-demo\backend"

echo =========================
echo DEPLOY COMPLETADO
echo =========================

pause

@echo off
setlocal

echo =========================
echo DETENIENDO PROCESOS
echo =========================
taskkill /F /IM GymAdmin.Api.exe /T >nul 2>&1
timeout /t 2 >nul

echo =========================
echo BUILD FRONTEND
echo =========================

cd /d "%~dp0gym-frontend"

call npm install
if errorlevel 1 (
    echo ERROR: npm install fallo
    pause
    exit /b 1
)

call npm run build
if errorlevel 1 (
    echo ERROR: build frontend fallo
    pause
    exit /b 1
)

echo =========================
echo LIMPIANDO WWWROOT
echo =========================

set WWWROOT=%~dp0GymAdmin\src\GymAdmin.Api\wwwroot

if exist "%WWWROOT%" (
    rd /s /q "%WWWROOT%"
)

mkdir "%WWWROOT%"

echo =========================
echo COPIANDO DIST
echo =========================

robocopy "%~dp0gym-frontend\dist" "%WWWROOT%" /E

if errorlevel 8 (
    echo ERROR: fallo copia de archivos
    pause
    exit /b 1
)

echo =========================
echo CONFIGURACION DB
echo =========================

set OUTPUT=%~dp0deploy-demo\backend
set DBNAME=gymadmin.db
set BACKUP_DIR=%~dp0.db_backup
set BACKUP_FILE=%BACKUP_DIR%\%DBNAME%

if not exist "%BACKUP_DIR%" mkdir "%BACKUP_DIR%"

:ask_db
set /p KEEPDB="¿Desea MANTENER la base de datos actual? (Y=Mantener / N=Pizar-Reiniciar): "

if /I "%KEEPDB%"=="Y" (
    if exist "%OUTPUT%\%DBNAME%" (
        echo Guardando respaldo de DB actual en %BACKUP_DIR%...
        copy "%OUTPUT%\%DBNAME%" "%BACKUP_FILE%" /Y >nul
        if errorlevel 1 (
            echo ERROR: No se pudo respaldar la base de datos.
            pause
            exit /b 1
        )
    ) else (
        echo No existe base de datos previa en el output para mantener.
    )
) else if /I "%KEEPDB%"=="N" (
    echo Se procedera con una base de datos limpia.
    if exist "%BACKUP_FILE%" del "%BACKUP_FILE%"
) else (
    echo Opcion invalida. Por favor use Y o N.
    goto ask_db
)

echo =========================
echo LIMPIANDO OUTPUT
echo =========================

if exist "%OUTPUT%" (
    rd /s /q "%OUTPUT%"
)

mkdir "%OUTPUT%"

echo =========================
echo PUBLICANDO BACKEND
echo =========================

cd /d "%~dp0GymAdmin"

dotnet publish src\GymAdmin.Api\GymAdmin.Api.csproj ^
-c Release ^
-r win-x64 ^
--self-contained true ^
-p:PublishSingleFile=true ^
-p:IncludeNativeLibrariesForSelfExtract=true ^
-o "%OUTPUT%"

if errorlevel 1 (
    echo ERROR: El publish del backend fallo. Revise los errores arriba.
    pause
    exit /b 1
)

echo =========================
echo RESTAURANDO DB (SI APLICA)
echo =========================

if /I "%KEEPDB%"=="Y" (
    if exist "%BACKUP_FILE%" (
        echo Restaurando base de datos desde el respaldo...
        copy "%BACKUP_FILE%" "%OUTPUT%\%DBNAME%" /Y >nul
        if errorlevel 1 (
            echo ERROR: No se pudo restaurar la base de datos.
            pause
        ) else (
            echo DB mantenida correctamente.
        )
    )
)

echo =========================
echo DEPLOY COMPLETADO
echo =========================

echo Ubicacion del deploy:
echo %OUTPUT%
echo.
pause
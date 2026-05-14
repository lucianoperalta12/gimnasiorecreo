@echo off
setlocal EnableDelayedExpansion

echo =========================
echo DETENIENDO PROCESOS
echo =========================
taskkill /F /IM GymAdmin.Api.exe /T >nul 2>&1
taskkill /F /IM Lanzador.exe /T >nul 2>&1
timeout /t 2 /nobreak >nul

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
set BACKUP_DIR=%~dp0db_backup
set BACKUP_FILE=%BACKUP_DIR%\%DBNAME%
set SOURCE_DB=%~dp0GymAdmin\src\GymAdmin.Api\%DBNAME%

if not exist "%BACKUP_DIR%" mkdir "%BACKUP_DIR%"

:ask_db
set KEEPDB=
set /p KEEPDB="¿Desea MANTENER la base de datos actual? (Y=Mantener / N=Reiniciar limpia): "

if /I "%KEEPDB%"=="Y" (

    if exist "%OUTPUT%\%DBNAME%" (

        echo Guardando respaldo de DB y archivos WAL/SHM...

        copy "%OUTPUT%\%DBNAME%*" "%BACKUP_DIR%" /Y >nul

        if errorlevel 1 (
            echo ERROR: No se pudo respaldar la base de datos.
            echo Posiblemente algun proceso la esta usando.
            pause
            exit /b 1
        )

        echo Respaldo realizado correctamente:
        dir /b "%BACKUP_DIR%\%DBNAME%*"

    ) else (

        if exist "%SOURCE_DB%" (

            echo No existe DB en deploy.
            echo Se encontro una DB en el codigo fuente.

            set USESOURCE=
            set /p USESOURCE="¿Desea usar esa DB? (S/N): "

            if /I "!USESOURCE!"=="S" (

                copy "%SOURCE_DB%*" "%BACKUP_DIR%" /Y >nul

                if errorlevel 1 (
                    echo ERROR: No se pudo copiar la DB del codigo fuente.
                    pause
                    exit /b 1
                )

                echo DB del codigo fuente copiada al respaldo.
            )

        ) else (

            echo ATENCION: No existe ninguna base de datos previa.

        )
    )

) else (

    if /I "%KEEPDB%"=="N" (

        echo Se utilizara una base de datos limpia.

        if exist "%BACKUP_DIR%\%DBNAME%*" (
            del "%BACKUP_DIR%\%DBNAME%*" /Q
        )

    ) else (

        echo.
        echo Opcion invalida.
        echo Ingrese solamente Y o N.
        echo.
        goto ask_db

    )
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
    echo ERROR: El publish del backend fallo.
    pause
    exit /b 1
)

echo =========================
echo RESTAURANDO DB
echo =========================

if /I "%KEEPDB%"=="Y" (

    if exist "%BACKUP_DIR%\%DBNAME%" (

        echo Restaurando archivos de base de datos...

        copy "%BACKUP_DIR%\%DBNAME%*" "%OUTPUT%" /Y >nul

        if errorlevel 1 (

            echo ERROR CRITICO: No se pudieron restaurar los archivos.
            pause
            exit /b 1

        ) else (

            echo Base de datos restaurada correctamente:
            dir /b "%OUTPUT%\%DBNAME%*"

        )
    )
)

echo =========================
echo DEPLOY COMPLETADO
echo =========================

echo.
echo Ubicacion final:
echo %OUTPUT%
echo.

pause
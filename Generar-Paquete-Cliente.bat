@echo off
setlocal EnableExtensions

rem ============================================================
rem  Gym Manager - Generador de entrega para pruebas del cliente
rem  - Publica backend (.NET 8) a GymAdmin\build-check
rem  - Compila frontend (Vue) a gym-frontend\dist
rem  - Empaqueta todo en client-package (ver publish-client.bat)
rem ============================================================

set "ROOT=%~dp0"
set "FRONTEND=%ROOT%gym-frontend"
set "BACKEND=%ROOT%GymAdmin"
set "API_CSPROJ=%BACKEND%\src\GymAdmin.Api\GymAdmin.Api.csproj"
set "BACKEND_OUT=%BACKEND%\build-check"

echo.
echo == Verificando herramientas ==
where dotnet >nul 2>nul
if errorlevel 1 (
  echo ERROR: No se encontro dotnet en el PATH.
  echo Instalar .NET 8 SDK: https://dotnet.microsoft.com/download
  exit /b 1
)
where npm >nul 2>nul
if errorlevel 1 (
  echo ERROR: No se encontro npm en el PATH.
  echo Instalar Node.js LTS: https://nodejs.org/
  exit /b 1
)

if not exist "%API_CSPROJ%" (
  echo ERROR: No se encontro el proyecto backend en:
  echo   %API_CSPROJ%
  exit /b 1
)
if not exist "%FRONTEND%\package.json" (
  echo ERROR: No se encontro el frontend en:
  echo   %FRONTEND%
  exit /b 1
)

echo.
echo == Publicando backend (Release) ==
if exist "%BACKEND_OUT%" (
  rmdir /s /q "%BACKEND_OUT%"
)
mkdir "%BACKEND_OUT%" >nul 2>nul
pushd "%BACKEND%" || exit /b 1
dotnet publish "%API_CSPROJ%" -c Release -o "%BACKEND_OUT%"
if errorlevel 1 (
  popd
  echo ERROR: Fallo dotnet publish.
  exit /b 1
)
popd

echo.
echo == Compilando frontend ==
pushd "%FRONTEND%" || exit /b 1
if not exist "node_modules" (
  echo Instalando dependencias (npm ci)...
  call npm ci
  if errorlevel 1 (
    echo ERROR: Fallo npm ci.
    popd
    exit /b 1
  )
)
call npm run build
if errorlevel 1 (
  echo ERROR: Fallo npm run build.
  popd
  exit /b 1
)
popd

echo.
echo == Empaquetando entrega (client-package) ==
call "%ROOT%publish-client.bat"
if errorlevel 1 (
  echo ERROR: Fallo el empaquetado (publish-client.bat).
  exit /b 1
)

echo.
echo OK: Entrega generada en:
echo   %ROOT%client-package
echo.
echo Para probar como cliente: ejecutar client-package\Start-GymAdmin.bat
echo.
endlocal

@echo off
setlocal

set "ROOT=%~dp0"
set "FRONTEND=%ROOT%gym-frontend"
set "BACKEND=%ROOT%GymAdmin"
set "BUILD_DIR=%BACKEND%\build-check"
set "PUBLISH_DIR=%ROOT%client-package"

echo Building frontend...
pushd "%FRONTEND%" || exit /b 1
call npm run build
if errorlevel 1 exit /b 1
popd

echo Preparing wwwroot...
if not exist "%BUILD_DIR%" (
  echo Backend build output not found at %BUILD_DIR%.
  exit /b 1
)
if exist "%PUBLISH_DIR%" rmdir /s /q "%PUBLISH_DIR%"
mkdir "%PUBLISH_DIR%" >nul 2>nul
xcopy "%BUILD_DIR%\*" "%PUBLISH_DIR%\" /e /i /y >nul
if exist "%PUBLISH_DIR%\wwwroot" rmdir /s /q "%PUBLISH_DIR%\wwwroot"
mkdir "%PUBLISH_DIR%\wwwroot" >nul 2>nul
xcopy "%FRONTEND%\dist\*" "%PUBLISH_DIR%\wwwroot\" /e /i /y >nul

echo Creating client launcher...
(
  echo @echo off
  echo setlocal
  echo cd /d "%%~dp0"
  echo dotnet GymAdmin.Api.dll
) > "%PUBLISH_DIR%\Start-GymAdmin.bat"

echo Done.
echo Package created at: %PUBLISH_DIR%
endlocal

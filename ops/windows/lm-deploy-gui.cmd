@echo off
setlocal
if exist "%~dp0LmDeployConsole\LmDeployConsole.exe" (
  start "" "%~dp0LmDeployConsole\LmDeployConsole.exe"
  exit /b 0
)
set "SCRIPT=%~dp0lm-deploy-gui.ps1"
powershell -NoProfile -ExecutionPolicy Bypass -Command "Start-Process -FilePath 'powershell.exe' -WorkingDirectory '%~dp0' -Verb RunAs -ArgumentList @('-NoProfile','-ExecutionPolicy','Bypass','-File','%SCRIPT%')"
if errorlevel 1 pause

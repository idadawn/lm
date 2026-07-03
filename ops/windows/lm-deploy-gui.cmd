@echo off
setlocal
set "SCRIPT=%~dp0lm-deploy-gui.ps1"
powershell -NoProfile -ExecutionPolicy Bypass -Command "Start-Process -FilePath 'powershell.exe' -WorkingDirectory '%~dp0' -Verb RunAs -ArgumentList @('-NoProfile','-ExecutionPolicy','Bypass','-File','%SCRIPT%')"
if errorlevel 1 pause

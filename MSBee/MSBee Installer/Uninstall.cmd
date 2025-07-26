@REM Copyright (C) Microsoft Corporation. All rights reserved.

@if not "%_echo%"=="1" (echo off)
setlocal enabledelayedexpansion

@REM This uninstall script is used by runtests.cmd
msiexec /quiet /x "%~dp0MSBeeSetup.msi"
if "!ERRORLEVEL!"=="0" (
  echo "MSBee uninstalled successfully"
) else (
  echo "MSBee uninstall failed" 
)

exit /b !ERRORLEVEL!
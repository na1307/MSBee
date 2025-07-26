@REM Copyright (C) Microsoft Corporation. All rights reserved.

setlocal enabledelayedexpansion

@REM The path to the WiX directory
set WiXPath=%~1

@REM The Product_Guid is stored in the MSBee.wxs file and the MSBee Installer Design Document
set PRODUCT_GUID={5DD465DE-5024-4716-ACE9-F912385CD19F}

@REM Setup MSI File
set SETUP_MSI="%~dp0MSBeeSetup.msi"

@REM WixObj File
set WIX_OBJ="%~dp0MSBeeSetup.wixobj"


@REM Delete the previous MSI, if present.
del /F %SETUP_MSI%

@REM Attempt to uninstall MSBee - will fail if MSBee isn't already installed.
msiexec /quiet /x %PRODUCT_GUID%
if "!ERRORLEVEL!"=="0" (
  echo "MSBee uninstalled successfully using the Product GUID"
) else (
  echo "MSBee uninstall failed using the Product GUID" 
)

@REM Compile the wxs file
"!WixPath!candle.exe" -out %WIX_OBJ% "%~dp0MSBeeSetup.wxs"
if "!ERRORLEVEL!"=="0" (
  echo "Candle generated %WIX_OBJ%"
) else (
  echo "Candle returned !ERRORLEVEL!"
  exit /b !ERRORLEVEL!
)

@REM Build the installer
"!WixPath!light.exe" -out %SETUP_MSI% %WIX_OBJ% "!WixPath!lib\wixui_featuretree.wixlib" -loc "%~dp0MSBeeWixUI_en-us.wxl"

if "!ERRORLEVEL!"=="0" (
  echo "Light generated %SETUP_MSI%"
) else (
  echo "Light returned !ERRORLEVEL!"
  exit /b !ERRORLEVEL!
)

@REM Install MSBee
msiexec /quiet /i %SETUP_MSI%
if "!ERRORLEVEL!"=="0" (
  echo "MSBee installed successfully"
) else (
  echo "MSBee failed to install" 
  exit /b !ERRORLEVEL!
)

@REM Check if MSBee.dll is actually installed.
if not exist "%ProgramFiles%\MSBuild\MSBee\MSBee.dll" (
  echo "MSBee installer claimed success but the MSBee.dll is not present where expected."
  exit /b 1
)

exit /b !ERRORLEVEL!

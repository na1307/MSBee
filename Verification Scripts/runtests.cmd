@REM Copyright (C) Microsoft Corporation. All rights reserved.

setlocal enabledelayedexpansion

@REM Usage: runtests.cmd [Release|Debug]

@REM Add .NET 2.0 Framework directory to the path to find msbuild.exe.
set PATH=%windir%\Microsoft.NET\Framework\v2.0.50727;%PATH%

@REM Identify a file to store the output from the scenario tests. See runtests.targets for an explanation.
set OUTPUT_FILE=%~dp0scenarioOutput.txt

@REM If an output file already exists, delete it.
del "%OUTPUT_FILE%"

@REM Call msbuild.exe to invoke the runtests target, which runs the unit and scenario tests.
call msbuild.exe /t:RunTests /p:Configuration=%~1 "/p:OutputFile=%OUTPUT_FILE%" "%~dp0runtests.targets"

@REM Check if nunit-console.exe returned success or failure. If any tests failed, 
@REM write the test output to the screen and return the error code.
if not "!ERRORLEVEL!"=="0" (
  type "%OUTPUT_FILE%"
  exit /b !ERRORLEVEL!
)

@REM Write the test output to the screen.
type "%OUTPUT_FILE%"

@REM Call Uninstall.cmd to try uninstalling MSBee after the tests finish.
call "%~dp0..\MSBee\MSBee Installer\Uninstall.cmd"
exit /b !ERRORLEVEL!
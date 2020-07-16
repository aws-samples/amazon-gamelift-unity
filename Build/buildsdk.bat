:: Copyright 2018 Amazon
::
:: Licensed under the Apache License, Version 2.0 (the "License");
:: you may not use this file except in compliance with the License.
:: You may obtain a copy of the License at
::
::     http://www.apache.org/licenses/LICENSE-2.0
::
:: Unless required by applicable law or agreed to in writing, software
:: distributed under the License is distributed on an "AS IS" BASIS,
:: WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
:: See the License for the specific language governing permissions and
:: limitations under the License.

@ECHO OFF
SETLOCAL ENABLEDELAYEDEXPANSION

REM ------- FIND MY ABSOLUTE ROOT -------
SET REL_ROOT=..\
SET ABS_ROOT=
PUSHD %REL_ROOT%
SET ABS_ROOT=%CD%
POPD

REM ------- VALIDATE ARGUMENTS -------
:: SET DEFAULTS
SET SDK_VER=4.0.0
SET DOTNET_VER=45

REM ------- PARSE ARGUMENTS -------
IF /I "%1"=="--HELP" (
   GOTO :HELPTEXT
)
IF /I "%1"=="--TEST" (
   GOTO :TEST
)
IF /I "%1"=="--FIX" (
   GOTO :FIXSDKBUGS
)
:PARSEOPTIONS
IF "%1"=="" (
   IF "%SDK_VER%"=="4.0.0" IF "!DOTNET_VER!" NEQ "45" GOTO :PARSEERROR
   GOTO :PARSECOMPLETE
)
IF /I "%1"=="--SDK-VERSION" (
   IF "%2"=="" GOTO :PARSEERROR
   SET SDK_VER=%2
   SHIFT
   SHIFT
   IF "!SDK_VER!" NEQ "3.3.0" IF "!SDK_VER!" NEQ "3.4.0" IF "!SDK_VER!" NEQ "4.0.0" GOTO :PARSEERROR
   GOTO :PARSEOPTIONS
)
IF /I "%1"=="--DOTNET-VERSION" (
   IF "%2"=="" GOTO :PARSEERROR
   SET DOTNET_VER=%2
   SHIFT
   SHIFT
   IF "!DOTNET_VER!" NEQ "35" IF "!DOTNET_VER!" NEQ "45" GOTO :PARSEERROR
   GOTO :PARSEOPTIONS
)
GOTO :PARSEERROR

:HELPTEXT
ECHO HELP:
:PARSEERROR
ECHO Usage (one of):
ECHO buildsdk --help                    This message
ECHO buildsdk --test                    Run the tests
ECHO buildsdk                           Build and use 4.0.0 SDK (latest)
ECHO buildsdk --sdk-version 3.3.0       Build and use 3.3.0 SDK
ECHO buildsdk --sdk-version 3.4.0       Build and use 3.4.0 SDK
ECHO buildsdk --sdk-version 4.0.0       Build and use 4.0.0 SDK
ECHO buildsdk --dotnet-version 35       Build SDK for .NET 3.5
ECHO buildsdk --dotnet-version 45       Build SDK for .NET 4.5
ECHO                                    NB SDK 4.0.0 does not support .NET 3.5
EXIT /B 1
REM --- ABEND ---

:PARSECOMPLETE
REM Figure out the SDK date
IF "%SDK_VER%"=="4.0.0" SET SDK_DATE=04_16_2020
IF "%SDK_VER%"=="3.4.0" SET SDK_DATE=09_03_2019
IF "%SDK_VER%"=="3.3.0" SET SDK_DATE=12_14_2018

:SETUPENV
REM ------- CREATE ENVIRONMENT DIRECTORY -------
IF NOT EXIST %ABS_ROOT%\Environment\NUL mkdir %ABS_ROOT%\Environment
PUSHD %ABS_ROOT%\Environment
REM ------- GET NUGET.EXE -------
::TODO REMOVE POWERSHELL DEPENDENCY :(
IF NOT EXIST NUGET.EXE POWERSHELL -ex unrestricted -Command "(New-Object System.Net.WebClient).DownloadFile(""""https://dist.nuget.org/win-x86-commandline/latest/nuget.exe"""", """".\NUGET.EXE"""")"
REM ------- UPDATE NUGET -------
CALL NUGET.EXE update -Self

REM ------- INSTALL AWS .NET SDK FILES IF NEEDED -------
IF NOT EXIST "AWSSDK.GameLift.3.3.106.52\lib\net%DOTNET_VER%\AWSSDK.GameLift.dll" CALL "nuget.exe" install AWSSDK.Gamelift -Version 3.3.106.52

REM ------- GENERATE INSTALL.BAT FILE IF NEEDED -------
IF NOT EXIST install.bat ECHO vcredist_x64.exe /q > install.bat
IF NOT EXIST prerequisites.md ECHO Tested against Unity 2017.4.6 (LTS) and some later versions up to Unity 2019.2 > prerequisites.md
POPD

REM ------- COPY AWS SDK .NET DLLS FROM AWS .NET SDK -------
:SDKREADY
IF NOT EXIST %ABS_ROOT%\Assets\Plugins\NUL mkdir %ABS_ROOT%\Assets\Plugins
IF NOT EXIST "%ABS_ROOT%\Assets\Plugins\AWSSDK.Core.dll" COPY "%ABS_ROOT%\Environment\AWSSDK.Core.3.3.107.24\lib\net%DOTNET_VER%\AWSSDK.Core.dll" "%ABS_ROOT%\Assets\Plugins\AWSSDK.Core.dll"
IF NOT EXIST "%ABS_ROOT%\Assets\Plugins\AWSSDK.GameLift.dll" COPY "%ABS_ROOT%\Environment\AWSSDK.GameLift.3.3.106.52\lib\net%DOTNET_VER%\AWSSDK.GameLift.dll" "%ABS_ROOT%\Assets\Plugins\AWSSDK.GameLift.dll"

REM ------- TEST TO SEE IF WE NEED TO BUILD C# GAMELIFT SERVER SDK PROJECT -------
:: BUILD SHOULD ONLY TAKE PLACE IF THE REQUESTED DLLS ARE NOT PRESENT IN UNITY PLUGINS FOLDER ALREADY
IF /I "%DOTNET_VER%"=="35" IF NOT EXIST "%ABS_ROOT%\Assets\Plugins\System.Threading.Tasks.NET35.dll" GOTO :BUILDSERVERSDK
IF NOT EXIST "%ABS_ROOT%\Assets\Plugins\GameLiftServerSDKNet%DOTNET_VER%.dll" GOTO :BUILDSERVERSDK
IF NOT EXIST "%ABS_ROOT%\Assets\Plugins\EngineIoClientDotNet.dll" GOTO :BUILDSERVERSDK
IF NOT EXIST "%ABS_ROOT%\Assets\Plugins\log4net.dll" GOTO :BUILDSERVERSDK
IF NOT EXIST "%ABS_ROOT%\Assets\Plugins\Newtonsoft.Json.dll" GOTO :BUILDSERVERSDK
IF NOT EXIST "%ABS_ROOT%\Assets\Plugins\protobuf-net.dll" GOTO :BUILDSERVERSDK
IF NOT EXIST "%ABS_ROOT%\Assets\Plugins\SocketIoClientDotNet.dll" GOTO :BUILDSERVERSDK
IF NOT EXIST "%ABS_ROOT%\Assets\Plugins\WebSocket4Net.dll" GOTO :BUILDSERVERSDK
ECHO SERVER SDK BUILD NOT NEEDED; SKIPPED
EXIT /B 0
REM --- END ---

:BUILDSERVERSDK
ECHO Running VCVARS32
REM PROBLEMS HERE? If you get 'The input line is too long.' and 'The syntax of the command is incorrect.' messages, then vcvars32.bat has been run too many times. Close the Command Window, open a new one and it will work.

REM SET VISUAL STUDIO ENVIRONMENT - USE OLDEST TO AVOID UNNECESSARILY UPGRADING PROJECT FILES
IF /I "%DOTNET_VER%"=="35" (
    IF EXIST "%ProgramFiles(x86)%\Microsoft Visual Studio 12.0\VC\bin\vcvars32.bat" GOTO :VS2013
)
IF EXIST "%ProgramFiles(x86)%\Microsoft Visual Studio 14.0\VC\bin\vcvars32.bat" GOTO :VS2015
IF EXIST "%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Community\VC\Auxiliary\Build\vcvars32.bat" GOTO :VS2017C
IF EXIST "%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Professional\VC\Auxiliary\Build\vcvars32.bat" GOTO :VS2017P
IF EXIST "%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Enterprise\VC\Auxiliary\Build\vcvars32.bat" GOTO :VS2017E
IF EXIST "%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Community\Common7\Tools\VsDevCmd.bat" GOTO :VS2019C
IF EXIST "%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Professional\Common7\Tools\VsDevCmd.bat" GOTO :VS2019P
IF EXIST "%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Enterprise\Common7\Tools\VsDevCmd.bat" GOTO :VS2019E
GOTO :VSMISSING

:VS2013
CALL "%ProgramFiles(x86)%\Microsoft Visual Studio 12.0\VC\bin\vcvars32.bat"
GOTO :EXTRACTBUILD

:VS2015
CALL "%ProgramFiles(x86)%\Microsoft Visual Studio 14.0\VC\bin\vcvars32.bat"
GOTO :EXTRACTBUILD

:VS2017C
CALL "%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Community\VC\Auxiliary\Build\vcvars32.bat"
GOTO :EXTRACTBUILD

:VS2017P
CALL "%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Professional\VC\Auxiliary\Build\vcvars32.bat"
GOTO :EXTRACTBUILD

:VS2017E
CALL "%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Enterprise\VC\Auxiliary\Build\vcvars32.bat"
GOTO :EXTRACTBUILD

:VS2019C
SET VSCMD_DEBUG=0
CALL "%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Community\Common7\Tools\VsDevCmd.bat"
GOTO :EXTRACTBUILD

:VS2019P
SET VSCMD_DEBUG=0
CALL "%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Professional\Common7\Tools\VsDevCmd.bat"
GOTO :EXTRACTBUILD

:VS2019E
SET VSCMD_DEBUG=0
CALL "%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Enterprise\Common7\Tools\VsDevCmd.bat"
GOTO :EXTRACTBUILD

:EXTRACTBUILD
::TODO GET AND EXTRACT GAMELIFT SERVER SDK INTO SDK DIR SO I DON'T HAVE TO DISTRIBUTE IT
IF NOT EXIST %ABS_ROOT%\SDK\GameLift-CSharp-ServerSDK-%SDK_VER%\README.md GOTO :SDKMISSING

:FIX
IF "%SDK_VER%" NEQ "4.0.0" CALL :FIXSDKBUGS

:STARTBUILD
%ABS_ROOT%\Environment\Nuget.exe restore "%ABS_ROOT%\SDK\GameLift_%SDK_DATE%\GameLift-SDK-Release-%SDK_VER%\GameLift-CSharp-ServerSDK-%SDK_VER%\Net%DOTNET_VER%\packages.config" -OutputDirectory "%ABS_ROOT%\SDK\GameLift_%SDK_DATE%\GameLift-SDK-Release-%SDK_VER%\GameLift-CSharp-ServerSDK-%SDK_VER%\packages"
MSBUILD "%ABS_ROOT%\SDK\GameLift_%SDK_DATE%\GameLift-SDK-Release-%SDK_VER%\GameLift-CSharp-ServerSDK-%SDK_VER%\GameLiftServerSDKNet%DOTNET_VER%.sln" /p:Configuration=Release /p:Platform="Any CPU"
IF %ERRORLEVEL% NEQ 0 GOTO :BUILDFAILED

IF /I "%DOTNET_VER%"=="35" (
    :: DELETE FILES FROM OTHER .NET VERSION(S)
    IF EXIST "%ABS_ROOT%\Assets\Plugins\GameLiftServerSDKNet45.dll" DEL "%ABS_ROOT%\Assets\Plugins\GameLiftServerSDKNet45.dll"
)
IF /I "%DOTNET_VER%"=="45" (
    :: DELETE FILES FROM OTHER .NET VERSION(S)
    IF EXIST "%ABS_ROOT%\Assets\Plugins\System.Threading.Tasks.NET35.dll" DEL "%ABS_ROOT%\Assets\Plugins\System.Threading.Tasks.NET35.dll"
    IF EXIST "%ABS_ROOT%\Assets\Plugins\GameLiftServerSDKNet35.dll" DEL "%ABS_ROOT%\Assets\Plugins\GameLiftServerSDKNet35.dll"
)
COPY "%ABS_ROOT%\SDK\GameLift_%SDK_DATE%\GameLift-SDK-Release-%SDK_VER%\GameLift-CSharp-ServerSDK-%SDK_VER%\Net%DOTNET_VER%\bin\Release\*.dll" "%ABS_ROOT%\Assets\Plugins\"

REM ------- BUILD FINISHED MSG -------
ECHO SDK BUILD FINISHED SUCCESSFULLY
EXIT /B 0
REM --- END ---

:BUILDFAILED
ECHO ERROR: THE GAMELIFT SDK BUILD FAILED
EXIT /B 1
REM --- ABEND ---

:VSMISSING
ECHO ERROR: VISUAL STUDIO MISSING. SEE BUILDSDK.BAT
ECHO INSTALL VISUAL STUDIO 2013 OR 2017 OR 2019
IF EXIST "%ProgramFiles(x86)%\Microsoft Visual Studio 12.0\VC\bin\vcvars32.bat" ECHO NOTE THAT VISUAL STUDIO 2013 CAN'T BUILD THE .NET 4.5 VERSION OF THE SDK
EXIT /B 1
REM --- ABEND ---

:SDKMISSING
ECHO DOWNLOADING GAMELIFT SERVER SDK %SDK_VER%
IF "%SDK_VER%"=="3.3.0" GOTO :V330
IF "%SDK_VER%"=="3.4.0" GOTO :V340

:V400
PUSHD %ABS_ROOT%\Environment
POWERSHELL -ex unrestricted -Command "(New-Object System.Net.WebClient).DownloadFile(""""https://gamelift-release.s3-us-west-2.amazonaws.com/GameLift_04_16_2020.zip"""", """".\GameLift_04_16_2020.zip"""")"
IF NOT EXIST ".\System.IO.Compression.ZipFile.4.3.0\lib\netstandard1.3\System.IO.Compression.ZipFile.dll" CALL "nuget.exe" install System.IO.Compression.Zipfile -Version 4.3.0
POWERSHELL -ex unrestricted -Command "Add-Type -Path '.\System.IO.Compression.ZipFile.4.3.0\lib\netstandard1.3\System.IO.Compression.ZipFile.dll' ; [io.compression.zipfile]::ExtractToDirectory(""""GameLift_04_16_2020.zip"""", """"..\SDK"""")"
POPD
GOTO :FIX

:V340
PUSHD %ABS_ROOT%\Environment
POWERSHELL -ex unrestricted -Command "(New-Object System.Net.WebClient).DownloadFile(""""https://s3-us-west-2.amazonaws.com/gamelift-release/GameLift_09_03_2019.zip"""", """".\GameLift_09_03_2019.zip"""")"
IF NOT EXIST ".\System.IO.Compression.ZipFile.4.3.0\lib\netstandard1.3\System.IO.Compression.ZipFile.dll" CALL "nuget.exe" install System.IO.Compression.Zipfile -Version 4.3.0
POWERSHELL -ex unrestricted -Command "Add-Type -Path '.\System.IO.Compression.ZipFile.4.3.0\lib\netstandard1.3\System.IO.Compression.ZipFile.dll' ; [io.compression.zipfile]::ExtractToDirectory(""""GameLift_09_03_2019.zip"""", """"..\SDK"""")"
POPD
GOTO :FIX

:V330
PUSHD %ABS_ROOT%\Environment
POWERSHELL -ex unrestricted -Command "(New-Object System.Net.WebClient).DownloadFile(""""https://s3-us-west-2.amazonaws.com/gamelift-release/GameLift_12_14_2018.zip"""", """".\GameLift_12_14_2018.zip"""")"
IF NOT EXIST ".\System.IO.Compression.ZipFile.4.3.0\lib\netstandard1.3\System.IO.Compression.ZipFile.dll" CALL "nuget.exe" install System.IO.Compression.Zipfile -Version 4.3.0
POWERSHELL -ex unrestricted -Command "Add-Type -Path '.\System.IO.Compression.ZipFile.4.3.0\lib\netstandard1.3\System.IO.Compression.ZipFile.dll' ; [io.compression.zipfile]::ExtractToDirectory(""""GameLift_12_14_2018.zip"""", """"..\SDK"""")"
POPD
GOTO :FIX


:FIXSDKBUGS
ECHO FIXING SDK BUGS
PUSHD FixSdk\
CD
CALL dotnet build --output bin\Debug\netcoreapp3.1\
CALL dotnet bin\Debug\netcoreapp3.1\FixSdk.dll %ABS_ROOT%\SDK
POPD
EXIT /B 0
REM --- END ---

:TEST
CALL clean.bat sdk
CALL buildsdk --sdk-version 4.0.0 --dotnet-version 45
IF %ERRORLEVEL% NEQ 0 (
    ECHO *** TEST 001 FAILED ***
    ECHO buildsdk --sdk-version 4.00.0 --dotnet-version 45
)
CALL clean.bat sdk
CALL buildsdk --sdk-version 3.4.0 --dotnet-version 45
IF %ERRORLEVEL% NEQ 0 (
    ECHO *** TEST 002 FAILED ***
    ECHO buildsdk --sdk-version 3.4.0 --dotnet-version 45
)
CALL clean.bat sdk
CALL buildsdk --sdk-version 3.4.0 --dotnet-version 35
IF %ERRORLEVEL% NEQ 0 (
    ECHO *** TEST 003 FAILED ***
    ECHO buildsdk --sdk-version 3.4.0 --dotnet-version 35
)
CALL clean.bat sdk
CALL buildsdk --sdk-version 3.3.0 --dotnet-version 45
IF %ERRORLEVEL% NEQ 0 (
    ECHO *** TEST 004 FAILED ***
    ECHO buildsdk --sdk-version 3.3.0 --dotnet-version 45
)
CALL clean.bat sdk
CALL buildsdk --sdk-version 3.3.0 --dotnet-version 35
IF %ERRORLEVEL% NEQ 0 (
    ECHO *** TEST 005 FAILED ***
    ECHO buildsdk --sdk-version 3.3.0 --dotnet-version 35
)
CALL clean.bat sdk
EXIT /B 0
REM --- END ---



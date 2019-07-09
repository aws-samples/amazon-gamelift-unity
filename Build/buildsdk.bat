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

REM ------- FIND MY ABSOLUTE ROOT -------
SET REL_ROOT=..\
SET ABS_ROOT=
PUSHD %REL_ROOT%
SET ABS_ROOT=%CD%
POPD

REM ------- VALIDATE ARGUMENTS -------
:: SET DEFAULTS
SET SDKVER=3.3.0

REM ------- PARSE ARGUMENTS -------
IF "%1"=="--HELP" (
   GOTO :PARSEERROR
)
IF "%1"=="" (
   GOTO :PARSECOMPLETE
)
IF /I "%1"=="--SDK-VERSION" (
   IF "%2"=="" GOTO :PARSEERROR
   SET SDKVER=%2
   IF "%SDKVER%" NEQ "3.2.1" IF "%SDKVER%" NEQ "3.3.0" GOTO :PARSEERROR
   GOTO :PARSECOMPLETE
)
GOTO :PARSEERROR

:PARSEERROR
ECHO Usage (one of):
ECHO buildsdk --help                    This message
ECHO buildsdk                           Build and use 3.3.0 SDK
ECHO buildsdk --sdk-version 3.2.1       Build and use 3.2.1 SDK
ECHO buildsdk --sdk-version 3.3.0       Build and use 3.3.0 SDK
GOTO END

:PARSECOMPLETE
REM ------- CREATE ENVIRONMENT DIRECTORY -------
IF NOT EXIST %ABS_ROOT%\Environment\NUL mkdir %ABS_ROOT%\Environment
PUSHD %ABS_ROOT%\Environment
REM ------- GET NUGET.EXE -------
::TODO REMOVE POWERSHELL DEPENDENCY :(
IF NOT EXIST NUGET.EXE POWERSHELL -ex unrestricted -Command "(New-Object System.Net.WebClient).DownloadFile(""""http://nuget.org/nuget.exe"""", """".\NUGET.EXE"""")"
REM ------- UPDATE NUGET -------
CALL NUGET.EXE update -Self

REM ------- INSTALL AWS .NET SDK FILES IF NEEDED -------
IF NOT EXIST "AWSSDK.GameLift.3.3.9.2\lib\net35\AWSSDK.GameLift.dll" CALL "nuget.exe" install AWSSDK.Gamelift -Version 3.3.9.2

REM ------- GENERATE INSTALL.BAT FILE IF NEEDED -------
IF NOT EXIST install.bat ECHO vcredist_x64.exe /q > install.bat
IF NOT EXIST prerequisites.md ECHO Tested against Unity 2017.4.6 (LTS) > prerequisites.md
CD %~dp0

REM ------- COPY AWS SDK .NET35 DLLS FROM AWS .NET SDK -------
:SDKREADY
IF NOT EXIST %ABS_ROOT%\Assets\Plugins\NUL mkdir %ABS_ROOT%\Assets\Plugins
IF NOT EXIST "%ABS_ROOT%\Assets\Plugins\AWSSDK.Core.dll" COPY "%ABS_ROOT%\Environment\AWSSDK.Core.3.3.18.2\lib\net35\AWSSDK.Core.dll" "%ABS_ROOT%\Assets\Plugins\AWSSDK.Core.dll"
IF NOT EXIST "%ABS_ROOT%\Assets\Plugins\AWSSDK.GameLift.dll" COPY "%ABS_ROOT%\Environment\AWSSDK.GameLift.3.3.9.2\lib\net35\AWSSDK.GameLift.dll" "%ABS_ROOT%\Assets\Plugins\AWSSDK.GameLift.dll"

REM ------- BUILD C# GAMELIFT SERVER SDK PROJECT -------
IF NOT EXIST "%ABS_ROOT%\Assets\Plugins\EngineIoClientDotNet.dll" GOTO BUILDSERVERSDK
IF NOT EXIST "%ABS_ROOT%\Assets\Plugins\GameLiftServerSDKNet35.dll" GOTO BUILDSERVERSDK
IF NOT EXIST "%ABS_ROOT%\Assets\Plugins\log4net.dll" GOTO BUILDSERVERSDK
IF NOT EXIST "%ABS_ROOT%\Assets\Plugins\Newtonsoft.Json.dll" GOTO BUILDSERVERSDK
IF NOT EXIST "%ABS_ROOT%\Assets\Plugins\protobuf-net.dll" GOTO BUILDSERVERSDK
IF NOT EXIST "%ABS_ROOT%\Assets\Plugins\SocketIoClientDotNet.dll" GOTO BUILDSERVERSDK
IF NOT EXIST "%ABS_ROOT%\Assets\Plugins\System.Threading.Tasks.NET35.dll" GOTO BUILDSERVERSDK
IF NOT EXIST "%ABS_ROOT%\Assets\Plugins\WebSocket4Net.dll" GOTO BUILDSERVERSDK
ECHO SERVER SDK BUILD NOT NEEDED; SKIPPED
EXIT /B 0
REM --- END ---

:BUILDSERVERSDK
REM SET VISUAL STUDIO ENVIRONMENT
IF EXIST "%ProgramFiles(x86)%\Microsoft Visual Studio 12.0\VC\bin\vcvars32.bat" GOTO VS2013
IF EXIST "%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Community\VC\Auxiliary\Build\vcvars32.bat" GOTO VS2017C
IF EXIST "%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Professional\VC\Auxiliary\Build\vcvars32.bat" GOTO VS2017
IF EXIST "%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Community\Common7\Tools\VsDevCmd.bat" GOTO VS2019C
GOTO VSMISSING

:VS2013
CALL "%ProgramFiles(x86)%\Microsoft Visual Studio 12.0\VC\bin\vcvars32.bat"
GOTO EXTRACTBUILD

:VS2017C
CALL "%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Community\VC\Auxiliary\Build\vcvars32.bat"
GOTO EXTRACTBUILD

:VS2017
CALL "%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Professional\VC\Auxiliary\Build\vcvars32.bat"
GOTO EXTRACTBUILD

:VS2019C
SET VSCMD_DEBUG=0
CALL "%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Community\Common7\Tools\VsDevCmd.bat"
GOTO EXTRACTBUILD

:EXTRACTBUILD
::TODO GET AND EXTRACT GAMELIFT SERVER SDK INTO SDK DIR SO I DON'T HAVE TO DISTRIBUTE IT
IF NOT EXIST %ABS_ROOT%\SDK\GameLift-CSharp-ServerSDK-%SDKVER%\README.md GOTO SDKMISSING

:STARTBUILD
%ABS_ROOT%\Environment\Nuget.exe restore "%ABS_ROOT%\SDK\GameLift-CSharp-ServerSDK-%SDKVER%\Net35\packages.config" -OutputDirectory "%ABS_ROOT%\SDK\GameLift-CSharp-ServerSDK-%SDKVER%\packages"
MSBUILD "%ABS_ROOT%\SDK\GameLift-CSharp-ServerSDK-%SDKVER%\GameLiftServerSDKNet35.sln" /p:Configuration=Release /p:Platform="Any CPU"
COPY "%ABS_ROOT%\SDK\GameLift-CSharp-ServerSDK-%SDKVER%\Net35\bin\Release\*.dll" "%ABS_ROOT%\Assets\Plugins\"

REM ------- BUILD FINISHED MSG -------
ECHO SDK BUILD FINISHED SUCCESSFULLY
EXIT /B 0
REM --- END ---

:VSMISSING
ECHO ERROR: VISUAL STUDIO MISSING. SEE BUILDSDK.BAT
ECHO INSTALL VISUAL STUDIO 2013 OR 2017
EXIT /B 1
REM --- ABEND ---

:SDKMISSING
ECHO ERROR: SDK MISSING
ECHO GET THE GAMELIFT SERVER SDK FROM
IF "%SDKVER%"=="3.2.1" GOTO :V321

ECHO https://s3-us-west-2.amazonaws.com/gamelift-release/GameLift_12_14_2018.zip
ECHO UNZIP THE FOLDER \GameLift_12_14_2018\GameLift-SDK-Release-3.3.0\GameLift-CSharp-ServerSDK-3.3.0
ECHO INTO THE GAMELIFTUNITY\SDK\ DIRECTORY SO THIS FILE EXISTS
ECHO GAMELIFTUNITY\SDK\GameLift-CSharp-ServerSDK-3.3.0\README.md
EXIT /B 1
REM --- ABEND ---

:V321
ECHO https://s3-us-west-2.amazonaws.com/gamelift-release/GameLift_02_15_2018.zip
ECHO UNZIP THE FOLDER \GameLift_02_15_2018\GameLift-SDK-Release-3.2.1\GameLift-CSharp-ServerSDK-3.2.1
ECHO INTO THE GAMELIFTUNITY\SDK\ DIRECTORY SO THIS FILE EXISTS
ECHO GAMELIFTUNITY\SDK\GameLift-CSharp-ServerSDK-3.2.1\README.md
EXIT /B 1
REM --- ABEND ---

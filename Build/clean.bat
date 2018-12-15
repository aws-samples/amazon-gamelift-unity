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

REM TASKKILL KILLS THE UNITY.EXE PROCESS IF IT IS RUNNING. IF UNITY IS NOT RUNNING THEN IT WILL THROW AN ERROR MSG TO STDERR.
REM BUT WE DON'T CARE ABOUT THAT SO CAPTURE THE STDERR OUTPUT AND IGNORE IT.
TASKKILL /IM unity.exe 2> NUL

REM <TODO> CLOSE MSVS TOO?

REM ------- FIND MY ABSOLUTE ROOT -------
SET REL_ROOT=..\
SET ABS_ROOT=
PUSHD %REL_ROOT%
SET ABS_ROOT=%CD%
POPD
CD %~DP0

REM GET RID OF THE TEMPORARY STUFF THAT WE DON'T NEED IN THE DISTRO
:CLEAN
ECHO CLEAN STARTED
IF EXIST "%ABS_ROOT%\Assets\Plugins" RMDIR /S /Q "%ABS_ROOT%\Assets\Plugins"
IF EXIST "%ABS_ROOT%\Library" RMDIR /S /Q "%ABS_ROOT%\Library"
IF EXIST "%ABS_ROOT%\Environment" RMDIR /S /Q "%ABS_ROOT%\Environment"
IF EXIST "%ABS_ROOT%\Output" RMDIR /S /Q "%ABS_ROOT%\Output"
IF EXIST "%ABS_ROOT%\ProjectSettings" RMDIR /S /Q "%ABS_ROOT%\ProjectSettings"
IF EXIST "%ABS_ROOT%\Temp" RMDIR /S /Q "%ABS_ROOT%\Temp"
IF EXIST "%ABS_ROOT%\.VS" RMDIR /S /Q "%ABS_ROOT%\.VS"

REM CLEAN DEPLOYTOOL
IF EXIST "%ABS_ROOT%\DeployTool\packages" RMDIR /S /Q "%ABS_ROOT%\DeployTool\packages"
IF EXIST "%ABS_ROOT%\DeployTool\bin" RMDIR /S /Q "%ABS_ROOT%\DeployTool\bin"
IF EXIST "%ABS_ROOT%\DeployTool\obj" RMDIR /S /Q "%ABS_ROOT%\DeployTool\obj"
IF EXIST "%ABS_ROOT%\DeployTool\.vs" RMDIR /S /Q "%ABS_ROOT%\DeployTool\.vs"
IF EXIST "%ABS_ROOT%\DeployTool\Properties" RMDIR /S /Q "%ABS_ROOT%\DeployTool\Properties"
DEL /Q %ABS_ROOT%\DeployTool\DeployTool.csproj.user 2> NUL
ATTRIB -H %ABS_ROOT%\DeployTool\*.suo > NUL
DEL /Q %ABS_ROOT%\DeployTool\*.suo 2> NUL

REM TIDY UP SDK OUTPUTS
IF EXIST "%ABS_ROOT%\SDK\GameLift-CSharp-ServerSDK-3.2.1\packages" RMDIR /S /Q "%ABS_ROOT%\SDK\GameLift-CSharp-ServerSDK-3.2.1\packages"
IF EXIST "%ABS_ROOT%\SDK\GameLift-CSharp-ServerSDK-3.2.1\Net35\bin" RMDIR /S /Q "%ABS_ROOT%\SDK\GameLift-CSharp-ServerSDK-3.2.1\Net35\bin"
IF EXIST "%ABS_ROOT%\SDK\GameLift-CSharp-ServerSDK-3.2.1\Net35\obj" RMDIR /S /Q "%ABS_ROOT%\SDK\GameLift-CSharp-ServerSDK-3.2.1\Net35\obj"

IF /I "%1" == "SDK" DEL /S /Q "%ABS_ROOT%\Assets\Plugins\*.dll" >nul 2>&1

REM SDF EXTENSION FILES
DEL /S /Q %ABS_ROOT%\*.sdf 2> NUL
REM INDIVIDUAL FILES
DEL /Q %ABS_ROOT%\GameLiftUnity.sln 2> NUL
DEL /Q %ABS_ROOT%\Assembly-CSharp.csproj 2> NUL
ECHO CLEAN SUCCEEDED

:FINISHED
CD %~DP0

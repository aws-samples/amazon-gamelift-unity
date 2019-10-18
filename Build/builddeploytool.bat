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

REM ------- BUILD DEPLOYTOOL -------
REM SET VISUAL STUDIO ENVIRONMENT
IF EXIST "%ProgramFiles(x86)%\Microsoft Visual Studio 12.0\VC\bin\vcvars32.bat" GOTO VS2013
IF EXIST "%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Community\VC\Auxiliary\Build\vcvars32.bat" GOTO VS2017C
IF EXIST "%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Professional\VC\Auxiliary\Build\vcvars32.bat" GOTO VS2017P
IF EXIST "%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Enterprise\VC\Auxiliary\Build\vcvars32.bat" GOTO VS2017E
IF EXIST "%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Community\Common7\Tools\VsDevCmd.bat" GOTO VS2019C
IF EXIST "%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Professional\Common7\Tools\VsDevCmd.bat" GOTO VS2019P
IF EXIST "%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Enterprise\Common7\Tools\VsDevCmd.bat" GOTO VS2019E
GOTO VSMISSING

:VS2013
ECHO Running VCVARS32
REM PROBLEMS HERE? If you get 'The input line is too long.' and 'The syntax of the command is incorrect.' messages, then vcvars32.bat has been run too many times. Close the Command Window, open a new one and it will work.
CALL "%ProgramFiles(x86)%\Microsoft Visual Studio 12.0\VC\bin\vcvars32.bat"
GOTO STARTBUILD

:VS2017C
CALL "%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Community\VC\Auxiliary\Build\vcvars32.bat"
GOTO STARTBUILD

:VS2017P
CALL "%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Professional\VC\Auxiliary\Build\vcvars32.bat"
GOTO STARTBUILD

:VS2017E
CALL "%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Enterprise\VC\Auxiliary\Build\vcvars32.bat"
GOTO STARTBUILD

:VS2019C
SET VSCMD_DEBUG=0
CALL "%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Community\Common7\Tools\VsDevCmd.bat"
GOTO STARTBUILD

:VS2019P
SET VSCMD_DEBUG=0
CALL "%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Professional\Common7\Tools\VsDevCmd.bat"
GOTO STARTBUILD

:VS2019E
SET VSCMD_DEBUG=0
CALL "%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Enterprise\Common7\Tools\VsDevCmd.bat"
GOTO STARTBUILD

:STARTBUILD
%ABS_ROOT%\Environment\Nuget.exe restore "%ABS_ROOT%\DeployTool\packages.config" -OutputDirectory "%ABS_ROOT%\DeployTool\packages"
MSBUILD "%ABS_ROOT%\DeployTool\DeployTool.sln" /p:Configuration=Release /p:Platform="Any CPU"
DEL "%ABS_ROOT%\DeployTool\bin\Release\*.pdb"
DEL "%ABS_ROOT%\DeployTool\bin\Release\*.xml"

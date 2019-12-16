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
CD %~dp0

REM ------- VALIDATE ARGUMENTS -------
IF "%1" == "" GOTO NOVER
SET DEPLOYNAME=GameLiftUnity
IF NOT "%2" == "" SET DEPLOYNAME=%2

REM ------- CHECK BUILD OUTPUT IS PRESENT -------
IF NOT EXIST %ABS_ROOT%\Output\Server\Image\GameLiftUnity.exe GOTO NOBUILD

REM ------- DEPLOY BUILD, CREATE FLEET AND CREATE ALIAS DIRECTED TO IT (NEW COMBINED C# SCRIPT) -------
ECHO PLEASE WAIT. DEPLOYMENT PROCESS TAKES A FEW MINUTES.
CALL %ABS_ROOT%\DeployTool\bin\Release\DeployTool.exe --name %DEPLOYNAME% --version %1 --root-path %ABS_ROOT%\Output\Server\Image --alias
GOTO END

:NOBUILD
ECHO BUILD OUTPUT (%ABS_ROOT%\Output\Server\Image\GameLiftUnity.exe) MUST BE PRESENT TO DEPLOY
ECHO EXECUTE BUILD.BAT AND VERIFY 'BUILD COMPLETED SUCCESSFULLY' MESSAGE FOR SERVER
GOTO END

:NOVER
ECHO NEED A VERSION NUMBER AS THE FIRST COMMAND LINE PARAMETER
ECHO OPTIONAL SECOND PARAMETER IS NAME OF BUILD/FLEET (DEFAULT GameLiftUnity)

:END

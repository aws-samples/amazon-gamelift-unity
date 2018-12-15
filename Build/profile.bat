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

:: Set name of your special profile here if you are using one, or change to
:: the word "default" (lower case, no quotes, no leading or trailing spaces)
SET PROFILENAME=lumberyard-fieldtech

REM ------- VALIDATE ARGUMENTS -------
IF "%1" EQU "configure" GOTO CONFIGURE
IF NOT "%1" EQU "" SET PROFILENAME=%1

REM ------- SET THE PROFILE -------
set AWS_DEFAULT_PROFILE=%PROFILENAME%
GOTO END

REM ------- CONFIGURE THE PROFILE
:CONFIGURE
IF NOT "%2" EQU "" SET PROFILENAME=%2
aws configure --profile %PROFILENAME%

:END
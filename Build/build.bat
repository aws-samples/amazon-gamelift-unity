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
ECHO Root directory: %ABS_ROOT%

REM ------- BUILD SDKS -------
ECHO BUILDING SDK
CALL BUILDSDK.BAT
IF ERRORLEVEL 1 EXIT /B %ERRORLEVEL%

REM ------- BUILD DEPLOYTOOL -------
ECHO BUILDING DEPLOYTOOL
CALL BUILDDEPLOYTOOL.BAT
IF ERRORLEVEL 1 EXIT /B %ERRORLEVEL%

REM ------- BUILD GAME -------
CALL BUILDCONFIG.BAT Client
IF ERRORLEVEL 1 EXIT /B %ERRORLEVEL%
CALL BUILDCONFIG.BAT Server
IF ERRORLEVEL 1 EXIT /B %ERRORLEVEL%

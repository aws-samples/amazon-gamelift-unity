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

REM ------- RUN STANDALONG CLIENT PLAYER -------
ECHO LOGGING TO %UserProfile%\AppData\LocalLow\DefaultCompany\GameLiftUnity\output_log.txt

::Modify start command to always use your custom alias as follows:
::START %ABS_ROOT%\Output\Client\Image\GameLiftUnity.exe --alias alias-6822cfcc-d773-40dc-9a04-5bb1e07d5c6b

START %ABS_ROOT%\Output\Client\Image\GameLiftUnity.exe %*
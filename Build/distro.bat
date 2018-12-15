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

REM ------- FIND MY ABSOLUTE ROOT -------
SET REL_ROOT=..\
SET ABS_ROOT=
PUSHD %REL_ROOT%
SET ABS_ROOT=%CD%
POPD
CD %~dp0

REM ------- WAS A DISTRO NUM SPECIFIED -------
IF "" NEQ "%1" (
    ECHO DISTRO %1 REQUESTED
    SET DISTRO_NUM=%1
    IF EXIST %ABS_ROOT%%DISTRO_NUM%\NUL GOTO ERRORALREADYPRESENT
    GOTO BACKUP
) 

REM IDENTIFY THE BACKUP NUMBER
SET DISTRO_NUM=200
:DECREMENT_DISTRO_NUM
SET /A DISTRO_NUM=DISTRO_NUM-1
IF "%DISTRO_NUM%" EQU "1" GOTO BACKUP
IF NOT EXIST %ABS_ROOT%%DISTRO_NUM%\NUL GOTO DECREMENT_DISTRO_NUM
SET /A DISTRO_NUM=DISTRO_NUM+1

REM BACKUP THE FOLDER BEFORE CLEANING
:BACKUP
ECHO BACKING UP PROJECT TO %ABS_ROOT%%DISTRO_NUM%
MKDIR %ABS_ROOT%%DISTRO_NUM%
XCOPY %ABS_ROOT% %ABS_ROOT%%DISTRO_NUM%\ /E /Q /Y /H /R

REM GET RID OF THE TEMPORARY STUFF THAT WE DON'T NEED IN THE DISTRO
:CLEAN
call clean.bat

:DISTRO
DEL %ABS_ROOT%\..\GameLiftUnity%DISTRO_NUM%.7z
CD /D %ABS_ROOT%\..\
"C:\Program Files\7-Zip\7z.exe" a C:\dev\GameLiftUnity%DISTRO_NUM%.7z GameLiftUnity\ "-xr!.git\" "-xr!.gitignore" "-xr!.gitattributes" "-xr!SDK\"

:FINISHED
CD %~dp0
GOTO END

:ERRORALREADYPRESENT
ECHO ERROR. %ABS_ROOT%%DISTRO_NUM%\ EXISTS, CAN'T MAKE DISTRO.

:END
<!---
   Copyright 2018 Amazon

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
-->

# What is GameLiftUnity #

This sample code shows how to set up a basic GameLift server and client for games using the Unity Game Engine. It consists of a Unity project that has been configured to be built in two ways, as a SERVER or a CLIENT. In CLIENT configuration, the preprocessor symbol CLIENT is defined. In SERVER configuration, the preprocessor SERVER is defined.

The code can be viewed in the three C# scripts in the `Assets\Scripts` sub-folder. GameLogic.cs is the game, GameLift.cs is the GameLift integration and Credentials.cs manages access to the server on the client. That's basically it. Everything else supports Unity. Or deployment.

The server can be deployed to GameLift with the deploy tool, and a fleet created. One or more clients can be run on your local machine and they can connect to the fleet to join a game. When some players have arrived the game can be started. Here you see a single player running a client connected to the server and playing the game.

![Client Screenshot](Docs/res/overview.png "Client Screenshot")

The idea of the game is to use the numeric keypad keys to match up pairs, triplets, quadruplets etc. matching colored dots on a three by three board. Accordingly the middle dot is 5, the left dot on the middle row is 4, both of the blue dots. Pressing the numeric keypad keys 4 and 5 simultaneously makes a move and matches the two blue dots for a score of two. Alternatively match the purple dots by pressing 1, 3 and 8 for a score of three. Matching four and upward dots in one move is possible, however note that due to the way that keyboards are wired, certain combinations may not be possible on some hardware.

How to build and run a server and a client from the Github repository is covered in the sections below.

You can get help by raising a GitHub issue for the sample. The current maintainer will respond in one to two business days unless extenuating circumstances prevail.

# One really important note #

In the client, you will see calls to the Gamelift service from the client. This is not considered best practice as it means that the client caller is never authenticated. Always call GameLift from a backend server which authenticates the player first. Very important. It should look like this.

![The right way to call GameLift](Docs/res/goodarchitecture.png "Good Architecture")

Actually we don't care what compute is used so you can use EC2 or containers instead of the Lambda, and you can authenticate however you want, with Cognito federated identity, a Dynamo DB table of registered users, or you can authenticate with another service like Steam, Gamesparks, PSN or whatever.

Also don't forget to look into Flexmatch and Game Session Placement Queues. All these items are on the to-do list for the project in `Docs\tasks.md` and will be added when bandwidth or cloning technology allows.

That aside, let's get started.

# Obtaining assistance #

The best way to obtain assistance is to open an issue on the Github page for this project.

Code contributions are encouraged. To contribute code, you should make a pull request with your proposed changes. Code contributions will be subject to review.

# Getting the Codebase #

Almost everyone will be downloading and building the sample from GitHub. If you have been supplied with a credentials file, then see the section at the end entitled Demo Version. Setup for you will be a bit different.

Download a copy of the sample or clone it to your local machine. I like to use GitHub Desktop to help, making the process pretty foolproof. Any folder should do, but the folder I use, `C:\dev\`, has no spaces in the path and is not very long.

# Building the project #

1. Several versions of Unity have been tested to work throughout development of the project, but I only test against the most recent due to process and time constraints, and when Unity is updated something frequently breaks. The following versions have been tested at one point or another:

    Unity 2017.4.6 LTS installer available here:
    <https://download.unity3d.com/download_unity/c24f30193bac/UnityDownloadAssistant-2017.4.6f1.exe>

    Unity 2019.1.8f1 Windows installer available here:
    <https://unity3d.com/get-unity/download?thank-you=update&download_nid=62662&os=Win>

    Unity 2019.2.5f1 Windows installer available here:
    <https://unity3d.com/get-unity/download?thank-you=update&download_nid=62983&os=Win>

    If you are using another version of Unity, that's fine too, but the project may need minor modifications. It should be safe to use the most recent version of Unity. I will try to keep the project working on the most recent version of Unity but as Unity changes frequently this effort might not always succeed. In the event of difficulty using the latest released Unity version, please open an issue on the Github page.

1. Install Unity by running the downloaded installer.

1. You will also need Visual Studio 2013 or 2017. There should be no other environment required.

1. Open a command window and change directory to the build directory:

    `cd /d C:\dev\GameLiftUnity\Build`

    ought to do it.

1. Make sure no instances of the executable are running and run `build.bat` to download dependencies, build the project, the SDKs, the plugins, client, a local server, and everything else.

Note that after building the project for the first time, the client or server can be rebuilt more quickly with:
`buildconfig Client`
or
`buildconfig Server`
or
`buildconfig`
rebuilds only the most recently built configuration

Note that a built project can be cleaned, removing all known output files and intermediate files, with `clean.bat`. If you have built the project before and are getting build failures, it may be worth trying this to prepare for a complete rebuild.

# How to deploy a server to GameLift #

First you will need to have the permissions for the local user to deploy the server to GameLift and create the fleet and alias for it. If you have sufficient permissions in an ```aws configure``` profile, you may jump to step 5.

1. If you do not already have it, install the AWS Commmand Line Interface (CLI) tools, available at: <https://aws.amazon.com/cli/>

1. Create an IAM user in your AWS account for the local machine. Start off going to the following link: <https://console.aws.amazon.com/iam/home?region=us-east-1#/users$new?step=details>

1. Give your user a name, and set up the user to have programmatic access:

    ![Create an IAM User Step 1](Docs/res/iam1.png "Create an IAM User Step 1")

1. Create a policy for attaching to your user. This gives the user the permissions needed for deploying the demo server to GameLift as a fleet.

    ![Create an IAM User Step 2](Docs/res/iam2.png "Create an IAM User Step 2")

1. The policy used should contain at least the following permissions.

    ```json
    {
        "Version": "2012-10-17",
        "Statement": [
            {
                "Effect": "Allow",
                "Action": [
                    "iam:*",
                    "s3:*",
                    "gamelift:*"
                ],
                "Resource": [
                    "*"
                ]
            }
        ]
    }
    ```

    ![Create an IAM User Step 3](Docs/res/iam3.png "Create an IAM User Step 3")

1. Finish creating the user with the policy attached. Download the credentials file, which is in .CSV format, and store it somewhere safe.

    ![Create an IAM User Step 4](Docs/res/iam4.png "Create an IAM User Step 4")

1. Open it in a spreadsheet application, or text editor, and use the credentials to generate a CLI credentials profile as follows

1. Open a command windows and use the `aws configure` command to create a credentials profile on your machine for deploying, and paste in the access key and secret key from the credentials file. You may skip the last little piece about the named deploy profile if you want, or see here <https://docs.aws.amazon.com/cli/latest/userguide/cli-multiple-profiles.html> for the details of this neat feature.

    ```bat
        aws configure --profile=deploy
    ```

1. If you are using a named profile, switch to it with

    ```bat
        set AWS_PROFILE=deploy
    ```

1. The server and client need to be built. If they aren't, then build the sample with `C:\dev\GameLiftUnity\Build\build.bat` to build everything needed.

1. Deploy the sample to AWS with `C:\dev\GameLiftUnity\Build\deploy.bat 25.0`.  
    The number parameter will show up as the version of the build and fleet, so make it just a bit higher than any existing build and fleet currently deployed

1. You should go into the GameLift console and look for your new fleet. It will have been deployed to us-east-1. <https://console.aws.amazon.com/gamelift/home?region=us-east-1#/r/fleets>

    The fleet has to be in the active state before you can connect to it.

1. When the fleet is deployed, its alias ID is printed, copy it out of the window and paste it as a parameter in `rc.bat` so that your local copy will attach to the new fleet (in GAMELIFT mode). Anyone playing against you on a different machine should use the same alias to join your server, so you could distribute the client and the rc.bat, if you wanted that. NOT using an alias will try to connect to my default alias, don't do that. The command is (obviously use your own alias ID):

    ```bat
        rc --alias alias-0c67a845-bc6e-4885-a3f6-40f1d2268234
    ```

1. The deployment named credentials profile has sufficient AWS permissions to run the client. I suggest running in windowed mode at a modest resolution, 1024x768. This way you can repeat the command to have multiple clients playing the game.

1. When you have finished you can switch the profile back to the default as follows:

    ```bat
        set AWS_PROFILE=
    ```

    I actually had a little problem here and had to go into the credentials file `%UserProfile%\.aws\credentials` and separate the deploy profile from the default profile by a blank line in order to get my default credentials back. You could also take the opportunity to delete the deploy profile from that file if you are of a mind to do that.

The complete successful process looks like this:

```none
C:\dev\GameLiftUnity\Build>aws configure --profile=deploy
AWS Access Key ID [None]: AKIAJF246OGMDZD25QKA
AWS Secret Access Key [None]: b0/KQBexampleexampleexampleexampleWlsSjV
Default region name [us-east-1]: us-east-1
Default output format [None]:

C:\dev\GameLiftUnity\Build>set AWS_PROFILE=deploy

C:\dev\GameLiftUnity\Build>build
Root directory: C:\dev\GameLiftUnity
BUILDING SDK
Checking for updates from https://www.nuget.org/api/v2/.
Currently running NuGet.exe 4.7.1.
NuGet.exe is up to date.
SERVER SDK BUILD NOT NEEDED; SKIPPED
BUILDING Client
SUCCESS: Sent termination signal to the process "Unity.exe" with PID 20640.
BUILD COMPLETE. SEE C:\Users\alanmur\AppData\Local\Unity\Editor\Editor.log
BUILDING Server
BUILD COMPLETE. SEE C:\Users\alanmur\AppData\Local\Unity\Editor\Editor.log

C:\dev\GameLiftUnity\Build>deploy 25.0
PLEASE WAIT. DEPLOYMENT PROCESS TAKES A FEW MINUTES.
alias-e4a6e219-39b4-4720-ac16-252bb29483b5

C:\dev\GameLiftUnity\Build>rc --alias alias-e4a6e219-39b4-4720-ac16-252bb29483b5
LOGGING TO C:\Users\alanmur\AppData\LocalLow\DefaultCompany\GameLiftUnity\output_log.txt

C:\dev\GameLiftUnity\Build>set AWS_PROFILE=

C:\dev\GameLiftUnity\Build>
```

# Setting up the game client #

For running the application in GAMELIFT mode (only), the client will need credentials. If you created the deploy profile and used aws configure to install it, then your profile already contains the required credentials.

If you are using a predeployed server for a demo, you will be given a credentials file instead. If so, install as follows.

## Creating a Credentials file ##

If you want to use the principle of least privilege, as you should, then create a new IAM policy for clients. Note that calls to GameLift should usually go through a trusted server, see the architecture diagram above. The 'backend' part of our application needs the following access policy.

```json
{
    "Version": "2012-10-17",
    "Statement": [
        {
            "Effect": "Allow",
            "Action": [
                "gamelift:CreateGameSession",
                "gamelift:CreatePlayerSession",
                "gamelift:DescribeAlias",
                "gamelift:SearchGameSessions"
            ],
            "Resource": [
                "*"
            ]
        }
    ]
}
```

1. Create an IAM user and attach this policy to the user. Give the user programmatic access, and download the `credentials.csv` credentials file.

## Installing a credentials file ##

1. The client can ingest the credentials file directly. Place the file in the build directory, e.g. `C:\dev\GameliftUnity\Build\`

1. Open a command window and change directory to the build directory:

    `cd /d C:\dev\GameLiftUnity\Build`

    ought to do it.

1. Run the client through the 'run client' batch file, with the credentials file as follows:

    `rc --credentials credentials.csv`

    This will store the access key and secret key securely in the .NET SDK Store. This only needs doing once, after which there is no need to keep the credentials file.

## Alternative method of installing credentials file with Powershell ##

Powershell and the AWS Tools for Visual Studio can also be used to manipulate the profiles in the SDK store, although frankly the above is siimpler. See instructions here: <http://docs.aws.amazon.com/powershell/latest/userguide/specifying-your-aws-credentials.html> and here: <http://docs.aws.amazon.com/toolkit-for-visual-studio/latest/user-guide/getting-set-up.html#creds>

Store the credentials manually with the profile name demo-gamelift-unity using Powershell as follows:

`PS C:\> Set-AwsCredential -AccessKey AKIAIOSFODNN7EXAMPLE -SecretKey wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY -StoreAs demo-gamelift-unity`

To check that the credentials are installed, use the command:

`PS C:\> Get-AwsCredential -ListProfileDetail`

The client log also outputs information about what credentials are loaded.

# Understandng and using the Client #

The client also generates the usual Unity log file. The client operates in three modes depending on what you are doing for a server.

## Log Output ##

Some (old) versions of Unity generate their log files in the data directory of the build output. If this is somehow happening to you, look in the respective server or client output locations.

`C:\dev\GameLiftUnity\Output\Client\Image\GameLiftUnity_Data\output_log.txt`
`C:\dev\GameLiftUnity\Output\Server\Image\GameLiftUnity_Data\output_log.txt`

Everyone else should find your logs in the folder:

`%UserProfile%\AppData\LocalLow\DefaultCompany\GameLiftUnity\output_log.txt`

where `%UserProfile%` is usually `C:\Users\`\<your username\>

If you have multiple clients and servers running locally by the way, they are all writing to the same file making the logs messy and illegible. Not quite sure the best way to fix it but I added that to the to do list.

## Client LOCAL, GAMELIFT and DISCONNECTED modes ##

Local mode uses a server running on the local machine. Clients started on the local machine will detect the local server and try to connect to it. Local mode does not require the setup or use of the credentials file. To use Local mode, run a server on the local machine using the rs.bat file. Four player multiplayer is possible, and the server and all four clients must be run locally.

GameLift Local is not required for local mode. We will talk about that later.

GameLift mode uses an internet connection, a GameLift fleet running in an AWS account to provide server functionality, and no server must be run locally. To use GameLift mode, ensure that the connection is available, and that the credentials file is installed, and then run only the client on the local machine. Four player multiplayer is possible, across the Internet.

If no server is detected, either locally or in the designated GameLift fleet, then the client will run standalone, locally, in Disconnected mode. A single player game is possible, locally. If you think you should be attached to the Gamelift fleet but you see you are disconnected, then look in the client log to see what's up.

### How to use in LOCAL mode ###

Local mode uses a server running on the local machine, and then the client will not try to use GameLift, and will detect the server at `127.0.0.1` and connect to it instead of trying to use GameLift to find the server. (This is a feature of the game and does not use GameLift Local.)

1. Run the server with `C:\dev\GameLiftUnity\Build\rs.bat`.
    Server will report `SERVER | LOCAL | 0 CONNECTED`

1. Run one to four clients with `C:\dev\GameLiftUnity\Build\rc.bat`.
     Clients will report `CLIENT | LOCAL | CONNECTED 1UP`,  `CLIENT | LOCAL | CONNECTED 2UP`, etc.

     "Hello World" appears in each client, and the clients enter a synchronized 'attract' mode, driven by the server.

1. Press RETURN inside each client window to start. When all clients have pressed return, the game begins. (All clients need to have pressed RETURN to start the game, and you will need to alt-tab between them if they are all on one machine. The server does not need you to press RETURN.) GO! appears in each client.

1. You should be able to press numeric keypad keys in one client or another to make matches and increase your score. All clients should see the matches being made, and everyone’s scores. If you have a laptop, some function key combination may be required to activate numeric keypad keys.

    In rare cases, no numeric keypad maybe available. Keys Y U I, H J K, N M comma may be used instead.

### How to use in GAMELIFT mode ###

1. To use the GameLift server in GameLift mode, simply run the clients with `C:\dev\GameLiftUnity\Build\rc.bat` without a local server running, and the game will try to connect. Clients can be on different machines and play together. NOTE, the corporate network (or VPN connection to the corporate network) can disrupt the client’s ability to reach the GameLift fleet, due to blocked ports. Port permissions must be set on the fleet to allow access on port 1935. (NB Outbound traffic on this port might be blocked by the Amazon corporate network in some places now, other ports can be specified to the server with -port 1935 or other values if needed; See the sectioni for setting up your own GameLift server below.)

To specify an alias to connect to, run the client with the alias as a parameter. The syntax is:

`C:\dev\GameLiftUnity\Build\rc.bat [--alias <alias-id>]`

e.g.

`C:\dev\GameLiftUnity\Build\rc.bat --alias alias-0c67a845-bc6e-4885-a3f6-40f1d2268234`

This also works with the GameLiftUnity executable directly.

`C:\dev\GameLiftUnity\Output\Client\Image\GameLiftUnity.exe [--alias <alias-id>]`

### How to use in DISCONNECTED mode ###

In disconnected mode you don't need a server of any kind.

1. The game client will try to locate a local server, and if not found, attempt to find a GameLift server (with the default alias or one supplied on the command line. If a server cannot be reached, the client will go into a serverless standalone mode.

1. The game will start when RETURN is pressed, and score matches until the ESC key is pressed.

1. It will never again try to look for a server. This is by design. Just kill the client and start another if you want another mode.

### How to use the server with GameLift Local ###

1. Install the Java Runtime

1. Install GameLift Local from inside the server SDK package: Unzip GameLift_04_11_2017.zip into a directory on the development PC.

1. In the GameLiftLocal-Release-1.0.0 folder, run a command prompt

1. Start the GameLiftLocal daemon by entering GameLiftLocal.jar at the command prompt.

```none
    C:\dev\GameLiftUnity\Output\Server\Image>"C:\dev\GameLift_06_29_2017\GameLiftLocal-Release-1.0.1\GameLiftLocal.jar"

    C:\dev\GameLiftUnity\Output\Server\Image>
```

1. Start the server by running the server executable (rs.bat)

    Server will report `SERVER | GAMELIFT | 0 CONNECTED`

```none
    C:\dev\GameLiftUnity\Output\Server\Image>aws gamelift create-game-session --endpoint-url http://localhost:8080 --maximum-player-
    session-count 2 --fleet-id fleet-123 --game-session-id gsess-abc
    {
        "GameSession": {
            "Status": "ACTIVATING",
            "MaximumPlayerSessionCount": 2,
            "FleetId": "fleet-123",
            "GameSessionId": "gsess-abc",
            "IpAddress": "127.0.0.1",
            "Port": 3333
        }
    }

    C:\dev\GameLiftUnity\Output\Server\Image>aws gamelift describe-game-sessions --endpoint-url http://localhost:8080 --game-session
    -id gsess-abc
    {
        "GameSessions": [
            {
                "Status": "ACTIVE",
                "MaximumPlayerSessionCount": 2,
                "FleetId": "fleet-123",
                "GameSessionId": "gsess-abc",
                "IpAddress": "127.0.0.1",
                "Port": 3333
            }
        ]
    }
```

To do: figure out how to allow the client to connect to a GameLift Local run server.

# Other commands in the build folder #

You probably will use a few of these for manually building, pushing new builds for fleets, and so forth.

## build.bat ##

**Parameters:** none  
Builds the SDK, then both existing configs, these being `Client` and `Server`. Use this as your top level build command if in any doubt.

## buildconfig.bat ##

**Parameters:** `[config]`  
Builds the specified Unity Project config, currently selectable from the two existing configs, `Client` or `Server`. If the config parameter is missing, the last config that was built is built again. This is very useful for a minimal rebuild when editing code in either the client or the server config.

Additional configs can be made if needed, and configs are documented in `saveconfig.bat`

## buildsdk.bat ##

**Parameters:** none  
Builds only the SDK  

## clean.bat ##

**Parameters:** none  
Kills any Unity processes, then deletes intermediate and temporary files not required for the distro  

## delconfig.bat ##

**Parameters:** `config`  
For deleting unwanted configs. Don't delete the `Client` or `Server` configs!  

## deploy.bat ##

**Parameters:** `version [name]`  
deploy the server to a GameLift Build, then start a fully configured Fleet, including the alias to point to it. Seems to be confused by the credentials profiles, so delete them all except the default admin credentials for your AWS account.

## distro.bat ##

**Parameters:** `[distronumber]`  
Create a distribution of the project with the specified `distronumber`. If a `distronumber` is not supplied, then the backup folders are searched to find the highest used distronumber, and a number one higher is used. So if GameLiftUnity18\ is present a distronumber of 19 will be used.
First a backup of the GameLiftUnity folder is created in the GameLiftUnity19 folder, then the GameLiftUnity folder is cleaned and zipped into GameLiftUnity19.7z

## loadconfig.bat ##

**Parameters:** `configname`  
Prepares the Unity file system to build either the Server or Client configuration. Could be used to load other configurations. Intended for use only in development.

## profile.bat ##

**Parameters:** `profilename | configure profilename`
Makes the specified profile the `AWS_DEFAULT_PROFILE` using environment variables. Use a profilename of `default` to return to the default. Use the configure command to initiate configuration of the named profile.

## rc.bat ##

**Parameters:** `[-alias aliasid] [-credentials credentialsfilespec]`  
Runs the client on the local machine. -alias overrides the default alias of alias-4781066a-9a39-47c4-ae8c-527107c09059. -credentials sets the permissions of the application. At least the following policy is needed.
{
    "Version": "2012-10-17",
    "Statement": [
        {
            "Sid": "Stmt1510611438000",
            "Effect": "Allow",
            "Action": [
                "gamelift:CreateGameSession",
                "gamelift:CreatePlayerSession",
                "gamelift:DescribeAlias",
                "gamelift:SearchGameSessions"
            ],
            "Resource": [
                "*"
            ]
        }
    ]
}

## rs.bat ##

**Parameters:** none  
Runs the server locally

## saveconfig.bat ##

**Parameters:** `configname`  
Stores data for the Unity file system to build either the Server or Client configuration. Could be used to create other configurations. Intended for use only in development.

# Demo Version #

If you have been supplied with a credentials file (credentials.csv) then you are going to use a prebuilt server and the built-in alias. This is normally done for Amazon staff only for showing the sample in a demo. If you are trying to set up the client and server from GitHub, go back to the Getting the Codebase section near the beginning of the file.

To download this application requires read access to the S3 bucket containing the distro file for downloading. A presigned URL will be supplied for this purpose. You will also be given a credentials file required to access the prebuilt server.

Download the zip file and unzip it into a good location on your drive. To run the client, you will go to the GameLiftUnity\Build directory. Place the credentials file in this directory (should be called credentials.csv)

Run the following command:

```bat
    rc --credentials credentials.csv
```

This installs the credentials and runs the client. Once installed, you can forget about the credentials for that machine, and start the client with simply

```bat
    rc
```

which stands for 'run client', by the way.

Clients will report `CLIENT | GAMELIFT | CONNECTED 1UP`,  `CLIENT | GAMELIFT | CONNECTED 2UP`, etc.

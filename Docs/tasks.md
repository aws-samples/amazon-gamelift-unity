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

# Tasks #

1. Automate the download and unzip of the GameLift Server SDK if it is not present on the user's machine.
    1. Remove all parts of the GameLift Server SDK from the distribution
    1. Need to test for presence of SDK
    1. Need to download SDK maybe from <https://s3-us-west-2.amazonaws.com/gamelift-release/GameLift_02_15_2018.zip>
    1. Need to unzip SDK maybe powershell.exe -nologo -noprofile -command "& { Add-Type -A 'System.IO.Compression.FileSystem'; [IO.Compression.ZipFile]::ExtractToDirectory('foo.zip', 'bar'); }"
1. Copy Deploy tool sources to GameLiftUnity
1. Rename DeployBuild to DeployTool
1. Verify building works in the new location
    1. Binary placed in correct directory
1. Integrate DeployTool build into project build
1. Make nuget download the required AWS SDK dll assemblies for the tool
1. Add tool functionality for creating a Lambda
1. Add tool functionality for listing lambdas and validating their existence
1. Add tool functionality for making API gateway endpoint etc.

# Enhancements #

1. Middleman service so that client is not calling GameLift
1. Add the ability for the client to use queues
1. Matchmaking
1. Integrate Cloudwatch logs
1. Multiple processes per instance
1. Fix the server so that TcpListener doesn't break (sockets fail or time out after a period of a few hours to a day or so and clients can't connect)
1. Use default region, not always us-east-1
1. Make different instances of the client or server not log on top of each other
1. Server build should be on LINUX, not Windows Server

# Add an Installer #

1. Self extracting .7z file
1. Downloads dependencies if needed
1. Builds the SDKS
1. Builds the client
1. Builds the server
1. Can run the server locally in GameLift Local and connect from the client
1. Can upload the server to an AWS account and start a fleet
1. Can run the client and verify the port being used to connect is open
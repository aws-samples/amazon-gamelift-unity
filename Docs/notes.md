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

# Unity integration layer for Amazon GameLift #
These development notes are probably inaccurate, outdated, apocryphal, misleading, deceptive, erroneous, based on mistaken comprehension, incomplete, obsolete or just plain wrong. Any use of the information contained herein is at the user's own judgment. Instructions for use are in the other file in this folder, instructions.md, which are much more likely to be helpful, current and intended for users.

Unity project setup
----------------------------------------
1. Downloaded Unity from the Unity web site
2. Installed 32-bit Unity installation
3. Started Unity
4. Created a Unity ID and password
5. Create a new Unity project GameLiftUnity in ```C:\dev\GameLiftUnity```
6. Create Scripts folder in Assets
7. Create new script in the scripts folder call it GameLift. This will be the C# interface to the native GameLift code.
8. Create empty game object call it GameLiftStatic alongside directional light and camera.
9. Add Component to the object and select scripts GameLift
10. Put a Debug.Log in the script during startup and update to check the script is getting invoked.
11. Run the project verify debug appears
12. Save the untitled scene as Scene.unity in the Assets\Scenes directory
   Exit and backup.

Backup process
----------------------------------------
1. Make a copy of the ```C:\dev\GameLiftUnity``` folder for safety
2. Delete files except for ```C:\dev\GameLiftUnity\Assets``` and ```C:\dev\GameLiftUnity\ProjectSettings```
3. Zip or in P4 Reconcile Offline Work
4. Open the project from the original location and the deleted files will be rebuilt.

Tests
----------------------------------------
Coming soon? :P

Install AWS C++ SDK
----------------------------------------
1. Get the AWS C++ SDK (https://github.com/aws/aws-sdk-cpp/archive/master.zip)
2. Unzip to ```C:\dev\GameLiftUnity\Assets\Sdk\Aws```
3. In the ```C:\dev\GameLiftUnity\Assets\Sdk\Aws\aws-sdk-cpp``` directory, we can reduce size by removing everything except
    ```
    aws-cpp-sdk-core\
    aws-cpp-sdk-core-tests\
    aws-cpp-sdk-gamelift\
    testing-resources\
    CMakeLists.txt
    ```

Install GameLiftServer SDK
----------------------------------------
1. Get the GameLift SDK (currently https://s3-us-west-2.amazonaws.com/gamelift-server-sdk/3.0.7/GameLiftServerSDK_3_0_7.zip)
2. Unzip to ```C:\dev\GameLiftUnity\Assets\Sdk\GameLiftServer```

Build project from command line
----------------------------------------
We want to make the process of building the sample a single action. I like to use a build.bat file to trigger these. I want to
make the batch file build the server and the client version. This seems possible but I need to change the #define in the player
build settings for each build. This #define string is embedded into ProjectSettings.asset. I can either search and replace in 
the file to change the string, but possibly tidier is to maintain two entire sets of settings (ClientProjectSettings folder 
and ServerProjectSettings folder) and simply swap each folder out as needed. That way I can have completely different 
everything for each build.
1. Create ```C:\dev\GameLiftUnity\Assets\Build``` folder and create ```clean.bat``` and ```build.bat```
2. ```build.bat``` will call the SDK ```build.bat``` so create that too.
3. Set up unity command line to build the project to output directory
   ```"%ProgramFiles(x86)%\Unity\Editor\Unity.exe" -nographics -batchmode -quit -projectPath "C:\dev\GameLiftUnity" -buildWindowsPlayer "C:\dev\GameLiftUnity\Output\Server\GameLiftUnity.exe"```
4. Test this by running the game. (It worked, but I don't see my code running).

##Add some text to tell me whether I am running client or server.

1. Add a UI>Text to the scene at the root level, alongside Directional Light, Camera and GameLiftStatic objects. A Canvas and EventSystem is created for you. Described here: https://youtu.be/QvZ6Q3TmoRI?t=37m25s
2. I set the text to hello world and tested it in the standalone.
3. Then I added a script component to the canvas and called it UserInterface, and added a function to set the text on it. Took a while to get the GameLift object to talk to it, but in the end made it static. Good enough for now.
  Exit and back up.

Build the server SDK on Windows
----------------------------------------
UPDATED: ONLY x64 IS SUPPORTED. The dependencies are only supplied as x64 architecture. SO DON"T BUILD x86 AS PER THESE INSTRUCTIONS.
1. Install cmake (https://cmake.org/install/) choosing cmake-3.6.1-win32-x86.msi and adding cmake to the system path for all users.
2. ```C:\dev\GameLiftUnity\Assets\Build\build.bat``` does:
    ```
    "C:\Program Files (x86)\CMake\bin\cmake.exe" -G "Visual Studio 12" C:\dev\GameLiftUnity\Assets\Sdk\GameLiftServer\GameLiftServerSDK -DBUILD_SHARED_LIBS=0
    "C:\Program Files (x86)\MSBuild\12.0\Bin\MSBuild.exe" C:\dev\GameLiftUnity\Output\Intermediate\Sdk\GameLiftServer\aws-cpp-sdk-gamelift-server.vcxproj /p:Configuration=Release
    ```

Build the client SDK package
----------------------------------------
1. Install ```git``` if it is desired to pull down the latest
2. ```"C:\Program Files\Git\cmd\git.exe" clone https://github.com/aws/aws-sdk-cpp.git```
3. ```C:\dev\GameLiftUnity\Assets\Build\build.bat``` does:
    ```
    "C:\Program Files (x86)\CMake\bin\cmake.exe" -G "Visual Studio 12" C:\dev\GameLiftUnity\Assets\Sdk\Aws\aws-sdk-cpp -DBUILD_SHARED_LIBS=0 -DBUILD_ONLY="gamelift"
    "C:\Program Files (x86)\MSBuild\12.0\Bin\MSBuild.exe" C:\dev\GameLiftUnity\Output\Intermediate\Sdk\GameLiftClient\ALL_BUILD.vcxproj /p:Configuration=Release

    "C:\Program Files (x86)\CMake\bin\cmake.exe" -G "Visual Studio 12" C:\dev\GameLiftLogger\Cpp\aws-sdk-cpp-master -DBUILD_SHARED_LIBS=0 -DBUILD_ONLY="logs"
    "C:\Program Files (x86)\MSBuild\12.0\Bin\MSBuild.exe" C:\dev\GameLiftLogger\Cpp\aws-sdk-cpp-master\ALL_BUILD.vcxproj /p:Configuration=Release
    ```

Create a native plugin project for the client SDK
----------------------------------------
1. Create and empty vcxproj in Visual Studio 2013. For all build configurations set the output extension to .dll and the output type to dynamic linked library. Add a C++ file.
2. Link the client SDK.
    1. Add the 'additional include directories' (```aws-sdk-cpp\aws-cpp-sdk-core\include;aws-sdk-cpp\aws-cpp-sdk-gamelift\include```) and include the gamelift header.
    1. Add the 'Additional Library Directories' with ```..\..\Output\Intermediate\Sdk\GameLiftClient\aws-cpp-sdk-gamelift\Release;..\..\Output\Intermediate\Sdk\GameLiftClient\aws-cpp-sdk-core\Release```
    1. Add the following 'additional dependencies' in the linker section: ```aws-cpp-sdk-core.lib;aws-cpp-sdk-gamelift.lib;Bcrypt.lib;Winhttp.lib;Wininet.lib;Userenv.lib```
3. Build and test the header doesn't cause errors.
4. ```build.bat``` builds the plugin dll and copies it to the standalone directories. The plugin needs to be in ```C:\dev\GameLiftUnity\Output\Client\GameLiftUnity_Data\Plugins``` for the client standalone and ```C:\dev\GameLiftUnity\Output\Server\GameLiftUnity_Data\Plugins```. When running the editor, ```C:\dev\GameLiftUnity\Output\GameLiftClientSDKPlugin\Release``` contains the dll.

```[error LNK2038: mismatch detected for '_ITERATOR_DEBUG_LEVEL': value '0' doesn't match value '2' in GameLiftClientSDK.obj]``` occurs if you link the debug versions of the plugin with the release version of the sdk lib(s) or vice versa, but whatever, it's fatal so make sure if you want to build both then you have debug and release versions of everything.

Call the native plugin from the Unity project
----------------------------------------
1. Add this to the plugin:
    ```
    extern "C" { __declspec(dllexport) float FooPluginFunction()
        { return 5.0F; }
    }
    ```
2. Add the declaration:
    ```
    #if UNITY_EDITOR
        [DllImport(@"C:\dev\GameLiftUnity\Output\GameLiftClientSDKPlugin\Release\GameLiftClientSDKPlugin.dll")]
    #else
        [DllImport(@"GameLiftUnity_Data\Plugins\GameLiftClientSDKPlugin.dll")]
    #endif
        public static extern float FooPluginFunction();
    ```
3. Add this to an Awake call on a game object then print out the result so it can be seen (I added a canvas with some UI):
    ```
    float x = FooPluginFunction();
    ```
4. Call it from the Unity project by running it in the Unity player so we can see that our DLL/Unity boundary works.

Call the AWS Client library from the plugin
----------------------------------------
1. Add the extern "C" function InitGameLiftClientSDK() to the plugin. Make it call something in the client library to verify the linkage is set up. We will trigger this function in unity to test it.
    ```
    #include <aws/gamelift/GameLiftClient.h>
    #include <aws/core/Aws.h>
    Aws::SDKOptions options;
    extern "C" {
        __declspec(dllexport) float FooPluginFunction()
        {
            return 5.0F;
        }
        
        __declspec(dllexport) void InitGameLiftClientSDK()
        {
            Aws::InitAPI(options);
            Aws::ShutdownAPI(options);
        }
        
        __declspec(dllexport) void UninitGameLiftClientSDK()
        {
        }
    }
    ```
2. Add the declaration:
    ```
    #if UNITY_EDITOR
        [DllImport(@"C:\dev\GameLiftUnity\Output\GameLiftClientSDKPlugin\Release\GameLiftClientSDKPlugin.dll")]
    #else
        [DllImport(@"GameLiftUnity_Data\Plugins\GameLiftClientSDKPlugin.dll")]
    #endif
        public static extern void InitGameLiftClientSDK();
    ```
3. Call the InitGameLiftClientSDK() function from awake on a Unity object to test it, test the link to the plugin and to test the link to the native client library.

Start testing the native plugin
----------------------------------------
I am going to build out the functionality of the native plugin as I need it in the game. However, once I figure out what the interface in the game looks like where it calls the plugin, I'm going to use unit tests in the plugin so that I don't need Unity to iterate, way faster. See https://msdn.microsoft.com/en-us/library/hh598953.aspx
1. Create new Visual Studio native C++ Unit Test project. (Add it to the solution. Change the Test project Output directory to put the test dll in the same directory as the plugin dll. Later I came back and also moved the intermediate directory to a part of the tree that gets cleaned).
2. Declare the test function: ```extern "C" { float FooPluginFunction(); }```
3. Add your test code: ```Assert::AreEqual(5.0f, FooPluginFunction(), L"message", LINE_INFO());```
  When the test passes, call the ```InitGameLiftClientSDK();``` instead.

(Updated the clean.bat and created backup 5)

Build out the full build for debug and release configurations
----------------------------------------
I only was building the release client SDK, so linking that with my debug build was giving the "mismatch detected for '_ITERATOR_DEBUG_LEVEL'" error. I went through and made a separate debug and release build of everything. I also fixed all absolute directories in the build to work anywhere (regardless of where the project is installed).

Distro 6

Next create the game in Unity
----------------------------------------
Concept: There is an 3 x 3 board. Each square has a color, and corresponds to a numeric keypad key in the range 1 to 9. The colors are generated randomly, but the simulation always guarantees that at least one match is available. A match is two or more squares of the same color. One or more matches may be present on the board at any time. The player is required to simultaneously press all of the numeric key pad keys responding to the squares in a match. Releasing all keys causes the event to be evaluated. If the match is correct, the colors on the board change at those squares change and the player's score is increased by the number of square in the match. All players are acting on the same server-authoritative board to cause collisions due to player input and prevent the client from knowing for certain the state of the board. The server resolves these collisions and overwrites all clients boards with its own every time the server board changes.
Architecture: The game will have these basic parts on the client:
	'input' to get the keypresses and when an action occurs generate an event. (client only)
	'simulation' takes the existing game state and applies the input to create a new game state. Game state will include the scores, the color of each square on the board, the random number generator seed and... anything else?
	'render' to render the state to the board whenever state is updated. (client only)
The local client will do the simulation and render to the local client. The server will take inputs from all players, do the simulation and send the output to the clients every time. If a client receives a server packet, it immediately discards its own state and renders the simulation from the server. So we also need to send data between the client and the server as follows.
	'clock' maintains a network time with which input packets are timestamped. The server time is calculated by the client when receiving state from the server. The server sends a) the client's local timestamp from the last packet it received from the client, b) the server's time stamp at the moment that packet was received, c) the server's time stamp from the moment the state was sent from the server and the client knows d) the time the state was received by the client. Client computes the latency of the packet as lat=((d-a)-(c-b))/2 and then the clock discrepancy as dis=d-(c+lat)=d-c+((d-a)-(c-b))/2=1.5d-1.5c+0.5b-0.5a. Average discrepancy is the rolling mean of the most recent 100 samples (or as many as are available). Therefore network time is local time - average discrepancy
	'client sender' will push the input to the server every time there is an action event. (local and network timestamp is sent)
	'server receiver' will apply the input to the server's authoritative state any time input is received from any player in the game.
	'server sender' will push the servers new state to all of the clients
	'client receiver' will receive the new state and update (overwrite) the client state.
Also we will need a lobby! Preferably one with minimal UI!
	A player can create a game or join a game.
	If a player creates a game, a code is displayed and the game goes into ready state.
	If games are available (in the ready state with player slots open, a list of up to 9 codes are displayed and the player can choose one. The player joins the game, which remains in ready state. "PLAYER JOINED" message and score is added.
	If a player is in a game and presses escape, the player leaves the game. "PLAYER LEFT" message and score is removed.
	Any games with zero players are terminated.
	Play in a game in the ready state starts when one player (or all players?) presses enter. START is displayed and the game enters running mode.
	Running mode continues until one player presses ESC, or reaches 100 points. GAME OVER is displayed. Players remain in the game and the game returns to ready mode.


Enough of the theory: let's get cracking. 'render' first.
1. Change the Unity camera to orthographic (2D) projection.
2. Add a background.
3. Create a highlight sphere (flattened in z) and a button sphere (flattened less).
4. Repeat in a 3x3 array
5. Create 8 different color (albedo) materials, apply one to the button spheres
6. In the canvas, add a text box to display score for each player and another for status text e.g. START and GAME OVER etc.
7. Test.
8. Add code for loading materials as resources and swapping the material on one of the buttons.
9. Test.

Distro 7

10. Create local input class
11. Add logic to capture keys and record chords (multi-keypress combos for the game's matching moves)
12. Logic to show and hide the highlights
13. Create simulation class
14. Add logic to initialize the state, receive the chords, determine matches, record scores, populate the board before a match, send the board for rendering, and send or receive the state of the class from the network code.
15. Add the playing logic to say when we are in a game or not! The server may eventually set that on the clients to let them know the game is starting.
16. Add a GO and GAME OVER message
17. Make the board flash random colors when not in a playing state.

Distro 8

Separate client and server build
1. Make the build figure out if it is the server or not. I was going to do a separate build for each (which is the proper way to do it for a large project, because you aren't carrying a load of client-only code around on the server.) Okay, I talked myself into it. Unity doesn't support separate build configurations properly, but I believe that replacing the Project Settings directory can enable different settings to be injected into the build, since the project #defines are stored in ProjectSettings.asset.
  a. Make a batch file in build to save current default configuration as (name) then copy it to Assets/Configurations/(name). If already present, it prompts to overwrite.
  b. Then another batch file to build a named configuration.
  c. And another one to delete a named configuration.
  d. And another one to make a named configuration the default.

2. Create the client (release) build and saved it as a named build. It will build to C:\dev\GameLiftUnity\Output\Client\Image

3. Create the server (release) build the same to C:\dev\GameLiftUnity\Output\Server\Image

Distro 9

Background operation:
----------------------------------------
Add Application.runInBackground = true; to the awake of a root game object to stop the client or server from suspending when the window goes out of focus

Networking:
----------------------------------------
Turns out that this is quite a lot of work, much bigger than the writing of the game itself.

SERVER ACCEPT CONNECTIONS
1. Create in the server build only, a class called NetworkServer. Instantiate in GameLogic class and initialize.
2. In the NetworkServer class, start a System.Net.Sockets.TcpListener(IPAddress.Any, 3333) at construction.
3. Write in NetworkServer::Update() a test to determine if any connections are pending (i.e. someone tried to connect, but we didn't get the TcpClient for the connection). Call this every frame from GameLogic::Update().
4. Create an array of four connections (a connection is a TcpClient) and initialize them to null
5. If a connection is pending and there is a null client slot, then TcpListener.AcceptTcpClient() and fill the first null slot with the client. If there wasn't a client slot, ignore the connection.
6. Repeat until all pending connections have been serviced.

CLIENT MAKE CONNECTION AND SEND STREAMING DATA
1. Create in the client build only, a class called NetworkClient. Instantiate in GameLogic class and initialize.
2. In the constructor, call Connect(), a function that creates a new TcpClient hard coded to connect to IP address 127.0.0.1 and the same port for both (3333 worked).
3. Get the stream for the TcpClient, then create a message string (I used "CONNECT:"), and send it to the server after first converting it to a byte[];

SERVER RECEIVE STREAMING DATA
1. In NetworkServer::Update() add a section that reads data out of the stream for each of the connections in turn and prints it.
2. Run the server then the client, test the client connection is received and the slot filled on the server side, and the test data is printed correctly.

At this point it is important to realize that the data being streamed is not being interpreted as messages, so the code doesn't know how long each message is.

MESSAGE PROTOCOL TO SEND MESSAGE LENGTHS
1. In the message send code, send four bytes to the network stream with the length of each message and then send the message immediately thereafter in network order as a byte array (instead of a string).
2. In the message receive code, read the four bytes from the network stream, then create a receive buffer of the right size and only read the number of bytes to get one message. Convert the bytes to machine order and back into a string. 
3. (production code) make sure that the buffer length can't be tampered with in the network data, or if it is that the game handles the maximum buffer size to prevent overruns.
4. Test that this works

CREATE THE MESSAGE TYPES
Concept: The client will send different messages to the server based on what it is doing. I implemented the five below:
CONNECT: The client has connected. The server will zero the score for this client, then transmit the current server state (this can be received by each client and overwrites the client's state).
READY: The client is ready to start playing.
INPUT: The client has made an input event, i.e. a chord. 
END: The client has ended the game. Only one player has to send the message for the game to end and we go to game over on the server at this point. The NetworkServer.ready array is cleared. The simulation state is set to not playing, and the state is transmitted to each player.
DISCONNECT: The client has exited (cleanly)

1. For each type, create a sender function in NetworkClient, named for the message it will send, and in that function send the message (including its length). We will add the payload data to the input message in an upcoming step. 

2. In NetworkServer::Update() add code that determines what the header data is for the received message (i.e. which of the five types) and calls a handler for that type of message. Add a parameter that pulls out any payload from the message and passes it to the handler.

3. For each type, create the handler function in NetworkServer. First I tested with only a print of the message type

WRITE CODE TO HANDLE A CONNECT
1. When the server receives a connect message, the TcpClient is already configured on the server (see Server Accept Connections section above). However, connecting also sets the score for the player that connected to 0 (which prints a score of 000 indicating connected). Secondly the handler calls the stub that sends the server state to the client.

WRITE CODE TO HANDLE A DISCONNECT
1. When the server receives a disconnect message, the client has exited (cleanly). The server changes the score for this player to -1 (which prints a score of --- indicating not connected), closes the TcpClient stream and connection for that client, and removes the client from the connections list.

WRITE CODE TO HANDLE A READY REQUEST
1. When the server receives a READY message, the player has pressed RETURN and is ready to play. The server records the readiness of the clients in the NetworkServer.ready array (four bools initialized to false) and when all connected clients are ready then the server starts the game (gl.StartGame()) setting the simulation state to playing and transmitting the new server state.

WRITE THE CODE TO SERIALIZE (AND DESERIALIZE) THE INPUT EVENTS
1. Separate out the state of an input (basically an array of nine bools that represents the keys that are pressed, index 0 is the KeyCode.Keypad1 key. (It looks like it adds a lot of code for no real functional benefit, so I might collapse Chord back into Input again, not sure.)
2. Write accessor functions for the class including Set(index), Reset(), and IsChanged().
3. Use Unity's JsonUtility.ToJson() function to serialize the [public members] of this instance to a json string.
4. Use the reverse functionality to set the state of this instance from a json string.
5. Write a static member CreateFromSerial() to instantiate a new instance and set its state from a json string. We will use this to receive input message when it is received by the server.

WRITE CODE TO SEND AN INPUT EVENT TO THE SERVER
1. In the SendInput() sender function in NetworkClient, add a parameter containing the player index and the chord that was pressed.
2. GameLogic.SendInput() is the point that the input class tells the simulation class that there is an input event, and causes a simulation. If we are the client, then call the NetworkClient::SendInput() function there. I had to have the simulation class say what playerIdx we are to make this work. Note that this isn't set up just yet, but it will default to 0 and be overwritten when we get server state. It will work when we get there. 

WRITE CODE TO HANDLE AN INPUT EVENT
1. In the Input message handler NetworkServer::SvrRcvInput(), we get the json for the input from the input message. Deserialize the message into a new chord instance with CreateFromSerial(). This is passed to GameLogic.SendInput() where the input class tells the simulation class to run. Now the server simulates the move for the correct client (it knows which player because it knows what connection the message was received from).

The current server state is sent to each player, which is the bit we will write next.

WRITE THE CODE TO SERIALIZE (AND DESERIALIZE) THE SERVER GAME STATE FOR EACH CLIENT
Concept: The server game state, common to all clients, is stored in the Simulation class and includes the following data:
a. the color of each square on the board
b. the score for each client (or -1 if not connected, meaning "---")
c. the state of the random number generator
d. whether the game is being played (or not, in which case we are in a joining/waiting pattern)
The server also gives each client a player index which is not the same for each client. This tells the client which score it will increment when the player scores.

1. Create the Simulation::Serialize() function. In it, we receive the player index of the player we are serializing for, so store that in the class instance so it is right when we create the json. We also copy the global 'playing' flag to the Simulation class for inclusion in the state. (see Todo DD.) Then the random number generator state is stored in the Simulation class. Then the class is converted to Json using the Unity JsonUtility.ToJson() function.
2. Create the matching Simulation::Deserialize() function. If we have valid json, overwrite the local client Simulation state with the server's authoritative state. Restore the random number generator state from the updated simulation client. Set the client's player index; the server is authoritative here too. (I also render but this is obsolete)
3. Create GameLogic::GetState(playerIdx) to make it visible to the network code. This simply serializes the Simulation with Simulation::Serialize().
4. Create GameLogic::SetState() for the same reason. When setting state we call Simulation::Deserialize() but we may also need to handle changes on the client, like what happened if the game started or stopped, and render the new state.


WRITE THE CODE TO SEND THE SERVER STATE TO THE CLIENT
If we are the server then whenever our state changes we should send this to the clients as we have mentioned before. These include:
a. after zeroing the score for the client when a new client connects. This is the first time for the new client, but updates the others too.
b. after every simulation step on the server
c. when not in a game, the server will randomly recolor the board every second, after which we transmit the new state
d. when the game starts
e. when the game ends
f. after initialization
**** SEE TODO BB. && CC. BELOW

1. Create the NetworkServer::SvrTransmitState() function. When called from the above places, the function loops through all connected clients and if valid, gets the state for the client with GameLogic::GetState(playerIdx), then sends it in a message (without a header for now since the server only ever sends this sort of message to the client) with the serialized data.
2. If there is a send error, assume the client disconnected and simulate receiving a Disconnect message by calling its handler.

WRITE THE CODE TO RECEIVE THE SERVER STATE ON THE CLIENT AND UPDATE THE CLIENT
1. Create NetworkClient::Update() and call every frame from GameLogic::Update().
2. Whenever the client is connected to a server and as long as data is available in the stream from the server, get a message. The message is in the same format as client messages, so handle in the same way, reading four bytes for size, then the number of bytes specified for the message, convert to Host/Machine order and back into a string. Set the state of the client from the string with GameLogic::SetState()

ON THE CLIENT HANDLE START AND END LOGIC FROM STATE UPDATES
1. If we just got the server state and it says we are playing, but the old client state was not playing, then start the game with GameLogic::StartGame()
2. If we just got the server state and it says we are not playing, but the old client state was playing, then end the game with GameLogic::EndGame();

HANDLE CLIENT OR SERVER DISCONNECTS
1. Create a connect and disconnect event for connecting to the server and disconnecting. The server assigns a random player number.
2. Create code that uploads the input from the client to the server, whenever the client receives an input event. 

Distro 10

Updated a lot of documentation (this file). 
Refactoring and debugging as explained below:
1. Moved the frame number from GameLogic to Simulation. If this is not synched, the random color changing when not playing the game gets out of sync between the client and the server.
2. Prevented the client from random color changing when connected to the server
3. Take the playing flag out of the game logic class and put it in the simulation class. Refer to the one and only copy of the flag. Simulation::Deserialize() should return the old playing flag so gl.SetState() can correctly identify a start and a stop.
4. If there is no server when the client starts it goes into offline mode and does single player games.
5. Fix bug with Connect message not sending correct length, causing crash
6. Pull out the message send and receive protocols separate from the message type. Make the protocol identical for server and client with common send and receive functions.
7. Fix the scores so that when a player joins, the server sets the score to 0, when a player leaves the server resets the score (to -1), and if players are connected when restarting a game, the server accounts for which ones are connected to determine the initial value (0 if connected or -1 otherwise)
8. The point where the GameLogic::SendInput is called. We should probably send the input message to the server first if we are the client, then simulate. Instead of the simulation calling the render, this can probably be brought out into the SendInput() function, which actually now represents all of the steps required for an InputEvent, and might be renamed.
9. Make GameLogic::TransmitState() private and call it instead of the SvrTransmitState() function, or just remove it entirely.
10. Make the score and messages and stuff fit on the screen for lower resolutions where they get cut off now.

Distro 11
Create the Server SDK Unity plugin (based on the client one).
1. If you do it right, everything should be built as x64 from the outset, however I was running x86 and had to change everything (sdks, plugins, unity project, command line build for unity project) to x64
2. Add the wrapper code to the native plugin; it must use an interface that is marshallable in C# (not all APIs are).
3. Build C# unit test harness for verifying functionality of marshaling over the managed/unmanaged boundary
4. Move native plugins outside of the Assets folder so that Unity doesn't try to include and build the unit tests (meh)
5. Build some simple type marshalling tests

Fixed bug: QQ. When the Game ends with more than one player, the scores are all reset right away (first randomization) instead of being displayed until the start of the next game.

INTEGRATE THE CLIENT SDK
When I had got this far, I was told that the C# GameLift SDK was in progress, so I switched to trying to get the client side to use the AWS Client SDK. One of our customers had managed to get the AWS SDK for .NET working with Unity 5.x using the .net35 binaries. I decided to try to do the same, and it seems to build now. Here are the steps.
1. Install the AWS SDK for .NET
2. Copy the following files from the AWS SDK for .NET\bin\Net35 directory (.Net45 binaries are incompatible) into the Assets/Plugins
    AWSSDK.Core.dll
    AWSSDK.GameLift.dll
3. Set the Unity .NET API Compatibility from .NET 2.0 Subset to .NET 2.0. This setting can be found in the PC/Mac/Linux player build options.

NB The customer had to copy the following files from the ...\Unity\Editor\Data\Mono\lib\mono\2.0\ folder to Assets/Plugins
    System.Core.dll
    Mono.Posix.dll
    System.Configuration.dll
    System.Security.dll
I did not and I think this is because I used Unity's .NET 2.0 API Compatibility.

MOVE SCRIPT CONSTRUCTOR CODE TO AWAKE CALL
This is now in the Awake function, along with all the other stuff that used to be in the GameLogic class constructor, as for 
some reason, Unity has started calling my constructor multiple times. Google said that advanced users don't use constructors 
for most of the initialization work, and so I copied all the stuff I was doing in the constructor to the Awake() function.

SSL CERTIFICATE VALIDATION FOR AWS CALLS:
Documented here: http://stackoverflow.com/questions/4926676
Add usings for SSL checking to GameLogic.cs:
    using System.Security.Cryptography.X509Certificates;
    using System.Net.Security;

Add a function to check the SSL Certificate chain
    public bool MyRemoteCertificateValidationCallback(System.Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) {
        bool isOk = true;
        // If there are errors in the certificate chain, look at each error to determine the cause.
        if (sslPolicyErrors != SslPolicyErrors.None) {
            for (int i=0; i<chain.ChainStatus.Length; i++) {
                if (chain.ChainStatus [i].Status != X509ChainStatusFlags.RevocationStatusUnknown) {
                    chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                    chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                    chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan (0, 1, 0);
                    chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
                    bool chainIsValid = chain.Build ((X509Certificate2)certificate);
                    if (!chainIsValid) {
                        isOk = false;
                    }
                }
            }
        }
        return isOk;
    }

Set the validation callback as soon as the game starts, near the beginning of the Awake function.
    ServicePointManager.ServerCertificateValidationCallback = MyRemoteCertificateValidationCallback;

CREATE AN ALIAS FOR YOU TO READ THE NAME OF IN YOUR AWS ACCOUNT
1. Create AWS account
2. Add an alias in the GameLift dashboard; note the alias ID for use in the API call
3. Add an IAM user with permissions to make any calls to GameLift
4. Use the AWS Access key and Secret key pair for your AWS user to list the name of the alias using the CLI, to test it works.
5. Use the AWS Access key and secret key pair in the test code below to test your program can access the SDK using the client API.

CALL THE CLIENT SDK
Add usings for AWSSDK dlls to GameLogic.cs:
    using Amazon;
    using Amazon.GameLift;

Add some test code that reads the name of the given alias. For simplicity I put it in the Start() function.
    // AWS TEST
    var config = new AmazonGameLiftConfig();
    config.RegionEndpoint = Amazon.RegionEndpoint.USEast1;
    using (aglc = new AmazonGameLiftClient("AKIAJFECT3YP4XV7F7DA","DtAF95inkvf4cC1X3EuoPJ8HfMzrpWAdzq8+G2yI",config))
    {
        var dareq = new Amazon.GameLift.Model.DescribeAliasRequest ();
        dareq.AliasId = "alias-b0535156-12c7-4c2b-aadf-38a1043afdd0";
        Amazon.GameLift.Model.DescribeAliasResponse dares = aglc.DescribeAlias (dareq);
        Amazon.GameLift.Model.Alias alias = dares.Alias; 
        log.WriteLine ("ALIAS NAME: " + alias.Name);
    }

Distro 15

INSTALL AND BUILD THE C# SERVER SDK
1. Unzip the C# SDK into a directory
2. Using VS2013, build GameLiftServerSDKNet35.sln for Release (not Debug)
3. Copy all the DLLs from GameLift-CSharpSDK-3.1.3\Net35\bin\Release to Assets/Plugins

CALL THE SERVER SDK
1. Add usings for the server SDK calls
    using Aws.GameLift.Server;
2. Use a local call to the Server DLL to test that the server SDK is integrated correctly. It does not call the GameLift service.
    string sdkVersion = GameLiftServerAPI.GetSdkVersion().Result;
3. This requires a fleet to test.

MANUALLY CREATE A FLEET
1. Build the server with buildconfig
2. Deploy the server to GameLift. The batch file will use the AWS CLI and thus by default the C:\Users\alanmur\.aws\credentials file to determine what account the build appears in, and the default region for that account.
3. Go to the account eg. https://console.aws.amazon.com/gamelift/home?region=us-east-1#/r/builds
4. Create a fleet from the build. Details:
    Name:               Unity
    Launch path:        GameLiftUnity.exe
    Launch parameters:  -batchmode -nographics
    Port settings:      3333    TCP     0.0.0.0/0           general access
                        3389    TCP     123.45.67.89/32     rdp access to local public ip 123.45.67.89 only; use only during development
    Safe scale policy:  off (unchecked, at least for now)

DEBUG THE FAILED INSTANCE
You must be using the most recent version of AWS CLI, and know the fleet id and your local public IPv4 address (123.45.67.89 form).
I prefer to disconnect from any VPN I am on. The local public IP addess can be obtained by asking Google 'what is my ip'.

WINDOWS
Give yourself permissions to access the instances of the fleet using RDP protocol (TCP port 3389) from your local public IP address (or 0.0.0.0/0 if that is not known):
    aws gamelift update-fleet-port-settings --fleet-id  "fleet-80942b2c-74a6-499e-acce-630568523fe0" --inbound-permission-authorizations "FromPort=3389,ToPort=3389,IpRange=0.0.0.0/0,Protocol=TCP"
LINUX
Give yourself permissions to access the instances of the fleet using SSH protocol (TCP port 22) from your local public IP address (or 0.0.0.0/0 if that is not known):
    aws gamelift update-fleet-port-settings --fleet-id  "fleet-735f4005-5a63-4f50-ac3f-3be60305c17a" --inbound-permission-authorizations "FromPort=22,ToPort=22,IpRange=0.0.0.0/0,Protocol=TCP"

BOTH
Get the list of instances that are in your fleet:
    aws gamelift describe-instances --fleet-id "fleet-80942b2c-74a6-499e-acce-630568523fe0" --limit 3 

Get the login details for one of the instances:
    aws gamelift get-instance-access --fleet-id "fleet-80942b2c-74a6-499e-acce-630568523fe0" --instance-id "i-09e8b43a35483b5fe"

WINDOWS
Open the Remote Desktop Connection application in Windows. In the Computer box, type or paste the IP address of the instance. Click Connect.
Enter your credentials: First, choose 'Use another account' to avoid using your local windows user. Use the username:
    \gl-user-remote
Note the backslash at the start of the username, which means don't use the local domain or workgroup. The username is always the same and is returned by the get-instance-access command above. For the password, use the Secret string.

Open C:\game\GameLiftUnity_Data\output_log.txt. This turned out to be tricky with the server running; I had to use an Administrator command prompt to run this batch file (a couple of times):
    TASKKILL /F /IM C:\Game\GameLiftUnity.exe
    copy C:\Game\GameLiftUnity_Data\output_log.txt out.log
and was able to open the copy in Notepad. You may have to kill the server process and copy the file before it respawns, about 10 seconds later.

LINUX
At the command prompt:

C:\dev>aws gamelift describe-instances --fleet-id "fleet-a38a8d0a-b161-4bb4-9abe-262a3672b2eb" --limit 3
{
    "Instances": [
        {
            "Status": "ACTIVE",
            "InstanceId": "i-0d2311e551c585f47",
            "Type": "c4.large",
            "CreationTime": 1504809694.72,
            "FleetId": "fleet-a38a8d0a-b161-4bb4-9abe-262a3672b2eb",
            "IpAddress": "204.236.192.93",
            "OperatingSystem": "AMAZON_LINUX"
        }
    ]
}

C:\dev>aws gamelift get-instance-access --fleet-id "fleet-a38a8d0a-b161-4bb4-9abe-262a3672b2eb" --instance-id "i-0d2311e551c585f47"
{
    "InstanceAccess": {
        "InstanceId": "i-0d2311e551c585f47",
        "IpAddress": "204.236.192.93",
        "FleetId": "fleet-a38a8d0a-b161-4bb4-9abe-262a3672b2eb",
        "OperatingSystem": "AMAZON_LINUX",
        "Credentials": {
            "UserName": "gl-user-remote",
            "Secret": "-----BEGIN RSA PRIVATE KEY-----\nMIIEpAIBAAKCAQEAt78EtOyfU+AHhXXzC6E54YBIrY94tCBnanVmSF/W58p+ptOM\nT5F0ON
1fp3GMphR3vYMbMJaQJPfJQjM5jNncLpLXj4NQ9/6dQpyj+cxdjn10zT97\nUAhLqPU3vcXt7dp7ZYyjFjEpfrz0i3a2ANxWExh4OLHqUmqormZ+FeB2y/jhnnEU\nrz
wpm57Bzj+s9YeaiN/C4rs9LdEW2Kx1BLv+YoP5bFbSjUit7HBABKtKY8uz928V\n0i8uGZI9kkjosWcB9xr0/EGMbJHMFeUkYa4brO/7DOIg0fu5s9inKlyxkOb0OPl2
\nfpsp+2f9J8Tp3Jiw/lbXhmhPShmcC6YuM2rHRQIDAQABAoIBAGvv1/H39fgtZ/2s\nNeOlB/1BgHAEEaGuT1GoOTdWpUVwHEofhxLOnPkygZg5Cagd6eD3fSdjqrUs
oZsz\nwCDPcZoiRGJXf17OwM56eZCpzmN/qvdOzT9MQDW2JtJhzMymRUp3/O1yX6/Fi9nJ\nGE0VIbMB8s1LJhzX7tLT/kkXnHFgeSrRIXAOfMMkHeSqRdXE/U7PQO6l
tqHHvpm8\nNfx8P7N5RWUBf4hLGGOrjQ8ksjpGlC2jhuRLr2spFJUSgrGnRGo/zH8Y3jd0s/qz\nr/yI+yD0whQazn2/Pve6JNyGS3xXcB/C6SUiEXcnNfwdsQsa6E4I
AwY2O7gOzB5d\n8TK9VpkCgYEA35xSty0vTLDkarLIea3677O1j7ayng+3IronsebmLyNaM93f31Ey\nv+YaRJGCeQeVb9DlPXZoTzYwsxvNpJxQ/mLATM9LWkmD23uM
/qsEYQS9gHiVaQCf\nRIusFo5+nTcbFfVaDzrMFQHb+yY5ENxoAppxKh80Gy+aQQiTD1RWip8CgYEA0lx8\narg+6pq9T/ymW0wNcCue1lfIzXS+kijLK8czjkSK/28B
ckkCLPc3AMqk/DDqxp26\nxSReI8zPAodZ5jo7QHejqmdqsJXcq1tfuTBiytspRM3agx83sYDLOQO9ydtMuRIx\n+7ZgByzZ+X+4QooXhc66pZTka8JwU3aoyBM1h5sC
gYEAtBWFWBh/u1fK3VNWuQgw\ny9MDKdaNS1aEuucJCPFX+CaUgqjxnzwZjwqVpRs65JYC96ZYuIMfxotx9Q0zNJrf\nb7+/9xwLJ4+FLcH27zJzuF7E7y4txf7GRcHm
udPQjHTQz4JlIVbM/S4eJ8nEs5uN\n+GrBMJcos6xGopFxPitB9ykCgYEAh1NX9o9wahwBjWNK4ZCbVH27QMhYNVPVBNGZ\nYiBT2kHd6VTP6WVuMN8YUzoJyPLvFbaC
YDB1HzKyOT45ZxIu69oLP6QnzlGaE+JE\nQi4OX9F9SvXijFeYzGe+VH9DqIebY7OA1B0OyY7g6tBvCN8tIrdK+xo9l9UNzaOY\nEJ3K4eECgYAEYqpilAZDVowLw6qB
ZF/CUp+voG9CD2MRapViDOFPhTMdlKYlaDbq\nWCsj0yu/CdbW/nAet5kP0GMQZuuoIiPBoSwC0DRjExwr41LAiB20+bcBy53rgnLL\nVYYcR3g3OYT5aw+Ay0mGAjTf
a/GPSQCeaGZ+PHChMkHmQ530ly7NCQ==\n-----END RSA PRIVATE KEY-----\n"
        }
    }
}

Take the Secret string from the quotes, remove all the actual newlines out of it so it lies on one line. In notepad++ regex replace \r\n with (blank)
Now replace all the \n with 0x10 new line characters. In notepad++ regex replace \\n with \n
The block should have all the same line lengths:

-----BEGIN RSA PRIVATE KEY-----
MIIEpAIBAAKCAQEAt78EtOyfU+AHhXXzC6E54YBIrY94tCBnanVmSF/W58p+ptOM
T5F0ON1fp3GMphR3vYMbMJaQJPfJQjM5jNncLpLXj4NQ9/6dQpyj+cxdjn10zT97
UAhLqPU3vcXt7dp7ZYyjFjEpfrz0i3a2ANxWExh4OLHqUmqormZ+FeB2y/jhnnEU
rzwpm57Bzj+s9YeaiN/C4rs9LdEW2Kx1BLv+YoP5bFbSjUit7HBABKtKY8uz928V
0i8uGZI9kkjosWcB9xr0/EGMbJHMFeUkYa4brO/7DOIg0fu5s9inKlyxkOb0OPl2
fpsp+2f9J8Tp3Jiw/lbXhmhPShmcC6YuM2rHRQIDAQABAoIBAGvv1/H39fgtZ/2s
NeOlB/1BgHAEEaGuT1GoOTdWpUVwHEofhxLOnPkygZg5Cagd6eD3fSdjqrUsoZsz
wCDPcZoiRGJXf17OwM56eZCpzmN/qvdOzT9MQDW2JtJhzMymRUp3/O1yX6/Fi9nJ
GE0VIbMB8s1LJhzX7tLT/kkXnHFgeSrRIXAOfMMkHeSqRdXE/U7PQO6ltqHHvpm8
Nfx8P7N5RWUBf4hLGGOrjQ8ksjpGlC2jhuRLr2spFJUSgrGnRGo/zH8Y3jd0s/qz
r/yI+yD0whQazn2/Pve6JNyGS3xXcB/C6SUiEXcnNfwdsQsa6E4IAwY2O7gOzB5d
8TK9VpkCgYEA35xSty0vTLDkarLIea3677O1j7ayng+3IronsebmLyNaM93f31Ey
v+YaRJGCeQeVb9DlPXZoTzYwsxvNpJxQ/mLATM9LWkmD23uM/qsEYQS9gHiVaQCf
RIusFo5+nTcbFfVaDzrMFQHb+yY5ENxoAppxKh80Gy+aQQiTD1RWip8CgYEA0lx8
arg+6pq9T/ymW0wNcCue1lfIzXS+kijLK8czjkSK/28BckkCLPc3AMqk/DDqxp26
xSReI8zPAodZ5jo7QHejqmdqsJXcq1tfuTBiytspRM3agx83sYDLOQO9ydtMuRIx
+7ZgByzZ+X+4QooXhc66pZTka8JwU3aoyBM1h5sCgYEAtBWFWBh/u1fK3VNWuQgw
y9MDKdaNS1aEuucJCPFX+CaUgqjxnzwZjwqVpRs65JYC96ZYuIMfxotx9Q0zNJrf
b7+/9xwLJ4+FLcH27zJzuF7E7y4txf7GRcHmudPQjHTQz4JlIVbM/S4eJ8nEs5uN
+GrBMJcos6xGopFxPitB9ykCgYEAh1NX9o9wahwBjWNK4ZCbVH27QMhYNVPVBNGZ
YiBT2kHd6VTP6WVuMN8YUzoJyPLvFbaCYDB1HzKyOT45ZxIu69oLP6QnzlGaE+JE
Qi4OX9F9SvXijFeYzGe+VH9DqIebY7OA1B0OyY7g6tBvCN8tIrdK+xo9l9UNzaOY
EJ3K4eECgYAEYqpilAZDVowLw6qBZF/CUp+voG9CD2MRapViDOFPhTMdlKYlaDbq
WCsj0yu/CdbW/nAet5kP0GMQZuuoIiPBoSwC0DRjExwr41LAiB20+bcBy53rgnLL
VYYcR3g3OYT5aw+Ay0mGAjTfa/GPSQCeaGZ+PHChMkHmQ530ly7NCQ==
-----END RSA PRIVATE KEY-----

Write this out to a .pem file with Unix line end encoding.

[TODO: Improve this]
Run Puttygen to turn this into a PPK file, and connect with Putty.


BOTH
When done, if you are not going to kill the fleet, then revoke your (Windows) RDP (3389) or (Linux) SSH (22) permissions.
    aws gamelift update-fleet-port-settings --fleet-id  "fleet-ce98ef9b-e9da-4927-bc56-785ccfcfc9d6" --inbound-permission-revocations "FromPort=3389,ToPort=3389,IpRange=0.0.0.0/0,Protocol=TCP"


Review the remaining permissions to ensure they are correct.
    aws gamelift describe-fleet-port-settings --fleet-id  "fleet-ce98ef9b-e9da-4927-bc56-785ccfcfc9d6"

    Should look something like this:
    {
        "InboundPermissions": [
            {
                "ToPort": 3333,
                "FromPort": 3333,
                "Protocol": "TCP",
                "IpRange": "0.0.0.0/0"
            }
        ]
    }


AUTOMATE THE UPLOADING OF A BUILD
1. Create a naiive deploy.bat to clear any logs and upload the package. Not absolutely vital, but saves a bunch of time.
    aws gamelift upload-build --name "Unity" --build-version "1.0.1" --build-root %ABS_ROOT%\Output\Server\Image

SEPARATE OUT GAMELIFT CODE INTO ITS OWN FILE
1. Pulled the budding GameLift code into another file to isolate it. Attached it to a static object in the Unity scene.
2. Develop code for both GameLift and GameLogic to be able to find each other based on them being in the scene, as a job of the Awake() function. This looks for the gamelogic code so that the gamelift code can find and call functions on it.
    GameObject gamelogicObj = GameObject.Find("/GameLogicStatic");
    Debug.Assert(gamelogicObj != null);
    gamelogic = gamelogicObj.GetComponent<GameLogic>();
    if (gamelogic == null) Debug.Log (":( GAMELOGIC CODE NOT AVAILABLE ON GAMELOGICSTATIC OBJECT");
    The idea is that if the GameLift code is not on a Unity object, the game can run only on a local server, but everything works. Make any calls between the modules contingent on them being present. We will test that as we go along.
3. Separate out gamelift server and client code into their own specific builds (properly)

GET SERVER SDK CALLS TO GAMELIFT WORKING
Anything more than the SDK version call will cause the AuxProxy (a daemon on the fleet instance) to make RESTful calls to the GameLift service itself. This must be coded to make it work correctly. E.g.
1. call the InitSDK() and read the outcome.
    try
    {
        var initOutcome = GameLiftServerAPI.InitSDK();
        if (initOutcome.Success)
        {
            Debug.Log (":) SERVER IS IN A GAMELIFT FLEET");
            ProcessReady();
        }
        else
        {
            if (gl.gamelogic != null) gl.gamelogic.GameliftStatus = false;
            Debug.Log (":( SERVER NOT IN A FLEET. GameLiftServerAPI.InitSDK() returned " + Environment.NewLine + initOutcome.Error.ErrorMessage);
        }
    }
    catch (Exception e)
    {
        Debug.Log (":( SERVER NOT IN A FLEET. GameLiftServerAPI.InitSDK() exception " + Environment.NewLine + e.Message);
    }
2. Upload new server code to GameLift each time. (Time consuming.)

FIX PLUGIN BUILDING
Remove code from buildplugins.bat that was being used for building Unity native plugins (no longer used)
Replace with code that builds the .NET version of the server SDK.

IMPLEMENT PROCESSREADY (NOT TIDY YET)
ProcessReady() is called server side to tell GameLift that the process is ready to accept a game. Doing such it provides gamelift with a few pieces of info.
1. Setup ProcessParameters structure
2. Tell GameLift the ProcessReady() using that API call.
3. Error checking
  See implementation. But the logging stuff doesn't work, and the ActivateGameSession part hadn't been implemented by here.


PULL AWS CREDENTIALS FROM THE LOCAL MACHINE INSTEAD OF HARD CODING THEM INTO THE SOFTWARE
Actually simple as removing the actual AWS access keys and using a default config:
    var config = new AmazonGameLiftConfig();
    config.RegionEndpoint = Amazon.RegionEndpoint.USEast1;
    using (AmazonGameLiftClient aglc = new AmazonGameLiftClient(config))
    {
        var dareq = new Amazon.GameLift.Model.DescribeAliasRequest ();
        dareq.AliasId = GameLift.alias;
        Amazon.GameLift.Model.DescribeAliasResponse dares = aglc.DescribeAlias (dareq);
        Amazon.GameLift.Model.Alias alias = dares.Alias;
        Debug.Log ((int)dares.HttpStatusCode + " ALIAS NAME: " + alias.Name);
    }

ADD STATUS TEXT
Text reads: SERVER/CLIENT, LOCAL/GAMELIFT, CONNECTED 3UP/DISCONNECTED (if CLIENT) or 3 CONNECTED (if SERVER)
1. Naiive implementation using a pull mechanism to figure out the sate each frame from wherever the status data was stored. Resulted in the string being calculated every frame.
2. Push the status changes into a new Status object in GameLogic, then at least calls across from Gamelift to Gamelogic are only done as needed.
3. Caught all the cases to make sure that the GameLift and GameLogic classes can call each other before notable status changes occur.
4. Then changing the status string only when the status changes, and set it on the Unity Object that it is drawn on. No more polling.
5. Generally go through and make all the status (local state) objects into properties instead of setting/getting them with ordinary functions.
6. Everywhere that the status changed needed to update the status object, so catch all those places too.

FIND OR CREATE A GAME SESSION (CLIENT SIDE)
1. Hook the NetworkClient::Connect() function in gamelogic.cs and pull out the actual connection attempt procedure into the TryConnect() function.
2. Make the code try to connect to the local server first, then make it try to connect to gamelift if that fails, i.e. there is no local server.
3. Get the connection details from the gamelift code (GetConnectionInfo()), which involves creating the game session and player session.
4. Get a game session:
    a. Create the AmazonGameLiftClient as usual.
    b. Search for existing game sessions with slots available.
    c. If there isn't one retry a few times.
    d. If there still isn't one, create a new one.
    e. If that didn't work, then retry that a few times
    f. If that still didn't work, give up and return failure.

IMPLEMENT PROCESSREADY LAMBDA FOR ONSTARTGAMESESSION (SERVER SIDE)
On the server, the onStartGameSession lambda is called when the GameLift service allocates a game session to a process. We don't need to do anything about that, as it happens, but we do need to accept the game session, acknowledging to GameLift that we are initialized and ready to have clients connect.
    /* onStartGameSession */ (gameSession) => {
        Debug.Log (":) GAMELIFT SESSION REQUESTED");
        try
        {
            var outcome = GameLiftServerAPI.ActivateGameSession();
            if (outcome.Success)
            {
                Debug.Log (":) GAME SESSION ACTIVATED");
            }
            else
            {
                Debug.Log (":( GAME SESSION ACTIVATION FAILED. ActivateGameSession() returned " + outcome.Error.ToString());
            }
        }
        catch (Exception e)
        {
            Debug.Log (":( GAME SESSION ACTIVATION FAILED. ActivateGameSession() exception " + Environment.NewLine + e.Message);
        }

CREATE A PLAYER SESSION (CLIENT SIDE)
1. Make a player session in the game session.
    a. Use the existing AmazonGameLiftClient and the Game Session ID.
    b. Try to make a player session
    c. If successful return the connection credentials to the connect function
    d. If that didn't work, retry a few times.
    e. if the player session can't be created, the client was probably beaten to the slot by another player. So we go back to wherever we were in the game session search/creation process to try again to get a game session again.

HANDLE FAILURE TO GET A GAME OR PLAYER SESSION (CLIENT SIDE)
1. Detect the condition, then exit the Connection function in NetworkClient without attempting to connect to GameLift. I.e. we are now in serverless (single player standalone mode).

VALIDATE PLAYERS (SERVER SIDE)
1. Add the authentication string to the client connect message and have the server side call the ValidatePlayer() function in the HandleConnect() function, passing along the session token.
2. Implement the ValidatePlayer() function in the GameLift server code
3. Return the validation result and disconnect the player if rejected

SUPPORT FOR TERMINATING GAME SESSIONS ON THE SERVER IF A CLIENT ENDS THE GAME (SERVER SIDE)
1. Implement TerminateGameSession()
2. Implement ProcessEnding() to notify GameLift of process end requests caused by the end of a game.
3. Call from HandleEnd() when the game ends.

HANDLE CLIENT DISCONNECTS (SERVER)
1. Trap socket errors from sending TCP messages, i.e. if a send fails, the client is gone.
2. Solidify disconnects so that if all players disconnect, the game ends automatically if it has not done so already.

IMPROVE ERROR HANDLING THROUGHOUT
1. Tidy up of exception handling, error messages and such to make it more useful.

HANDLE ONPROCESSTERMINATE (SERVER)
1. create code in the lambda in ProcessParameters prior to passing it to ProcessReady().
    /* onProcessTerminate */ () => {
        Debug.Log (":| GAMELIFT PROCESS TERMINATION REQUESTED (OK BYE)");
        GameLiftRequestedTermination = true;
        gl.TerminateServer();
        },

2. Implement TerminateServer, which calls the GameLogic quit functionality, as if the server application had been closed.
    public void TerminateServer()
    {
        if (gamelogic != null) gamelogic.OnApplicationQuit();
    }

3. the bool GameLiftRequestedTermination is a member that records is a force close by gameLift. We use this to test if there is a force quit by GameLift, as in a normal application quit, the application will try to notify GameLift to terminate the game session. This is not desired behavior
4. Notify clients by broadcasting a disconnect message. (Refactored transmitting state to all clients to broaden the usage of broadcast messages to include the use cases of broadcast disconnect, and broadcast of the log file, which I will implement to save me from looking at the server instance, most of the time.
5. Tidy up so that closing the client application or a local server application will always disconnect correctly in all circumstances.

DISTRO 16

DISTRO NUMS
1. Change distro nums so that they are one above the highest used one less than 100 instead of the lowest unused one. Means that i can clear out old backups without affecting the distro sequence.

Server: fleet-1088f383-b6b0-4148-87e7-5cd91e9913a1

SERVER LOG
Added test output to a server log.

BUG FOUND
JJ. The fifth client to connect (remember there are max four supported) gets a successful connection, but no further communication takes place, hanging the client. (red buttons)
- Fixed, server sends a reject message to the client that can then disconnect.

REUSABLE SLOTS
MM. Disconnecting a player from the server prevents any subsequent client from properly connecting to the server.
Add code to remove a player session and free up a slot if desired when a player leaves, to server code.

Add error handling to catch an exception if the game alias is not pointing to a valid fleet.

DISTRO 17: tested by levasa

Bug fixes
Separate clean.bat into clean.bat and distro.bat

DISTRO 18: 

AliasId can now be passed on the client command line and the client will connect to that alias instead of the default.

If the alias is a terminal type, the program will no longer crash. Obviously we can't connect to a fleet with a terminal alias, so the game will fall back to the standalone client.

Default server port is port 80, not port 3333 now.

Instructions and script written, notes updated, tasks listed (in markdown)

Removed non-distributable files from the Environment directory.

Added some (not all) license files.

Added CreateFleet tool to the distribution

Distro 19:

Added keys for Razer Blade (which doesn't have a numpad)
Y U I
H J K
N M ,

Fixed the buildsdk.bat so it can download the AWS SDK for .NET packages

Test the user's credentials to ensure that there are sufficient permissions
If a credentials file is passed as a command line parameter, then install the permissions as a named profile.

Server fixed to give GameLift the correct log file name. Pointing to a missing path is being newly checked for by GameLift, and caused the server to crash on ProcessReady().

Instructions updated, moved to lumberyard-fieldtech account

Distro 20

Removed from lumberyard-fieldtech account back to my account.

Distro 21

notes and instructions updated

These policies were set up in my account. If you set up your own server or download bucket, give access with these policies.
IAM policies, recommended:

demo-gamelift-unity-download
{
    "Version": "2012-10-17",
    "Statement": [
        {
            "Effect": "Allow",
            "Action": [
                "s3:GetBucketLocation",
                "s3:ListAllMyBuckets"
            ],
            "Resource": "arn:aws:s3:::*"
        },
        {
            "Effect": "Allow",
            "Action": [
                "s3:ListBucket"
            ],
            "Resource": [
                "arn:aws:s3:::demo-gamelift-unity"
            ]
        },
        {
            "Effect": "Allow",
            "Action": [
                "s3:GetObject"
            ],
            "Resource": [
                "arn:aws:s3:::demo-gamelift-unity/*"
            ]
        }
    ]
}

demo-gamelift-unity-client
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

demo-gamelift-unity-admin
run the build scripts and maintain and delete fleets
{
    (TBC) but full access to arn:aws:s3:::demo-gamelift-unity/*, Gamelift admin rights, and maybe create IAM users
}

Distro 22

Improved the appearance of the font, now uses better scale of Ember font instead of default Arial font.

Reduced play grid size a littl and moved score and status off the play grid for most resolutions, prefer 1024x768 windowed or other 4:3 aspect ratio resolution.

Updated CreateFleet script not to use deprecated t2.micro instances, built the combined DeployBuild script to upload the build in the script too.

Improved credentials search logic. Now always uses 'demo-gamelift_unity' profile if available, otherwise does general search including default profile.

Distro 23

Note that the server is not properly terminating if all the players leave. It is leaving the listener open (possibly for a long time) which eventually stops letting the server send to the clients, even though the listening part may be working okay.
The fix should probably be in onGameSessionStart callback to terminate and reinitiate listening behavior, for GameLift connected mode only.

Fixed the alias not being read off the command line and set in time for use by the connection.
Fixed deployment script. Was failing because build bucket had mixed case characters and underscores, not permitted by S3
Fixed deployment script. Scaling policy had rule with 4 days (5760 minute) evaluation period. Bug in GameLift had permitted that but it is restricted by a GameLift dependency. Now the maximum is one day, or 1440 evaluation periods. Don't know why, just fixed it.

Distro 24


## USING GAMELIFT LOCAL WITH THE SERVER

Install Java Runtime
Install GameLift Local: Unzip GameLift_04_11_2017.zip into a directory on the development PC.
In the GameLiftLocal-Release-1.0.0 folder, run a command prompt
Start the GameLiftLocal daemon by entering GameLiftLocal.jar at the command prompt. 
Start the server by running the server executable (rs.bat)
Server will report SERVER | GAMELIFT | 0 CONNECTED
C:\dev\GameLiftUnity\Output\Server\Image>aws gamelift create-game-session --endpoint-url http://localhost:8080 --maximum-player-
session-count 2 --fleet-id fleet-123 --game-session-id gsess-abc
{

```
"GameSession": {
    "Status": "ACTIVATING",
    "MaximumPlayerSessionCount": 2,
    "FleetId": "fleet-123",
    "GameSessionId": "gsess-abc",
    "IpAddress": "127.0.0.1",
    "Port": 3333
}
```

}

C:\dev\GameLiftUnity\Output\Server\Image>aws gamelift describe-game-sessions --endpoint-url http://localhost:8080 --game-session
-id gsess-abc
{

```
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
```

}

C:\dev\GameLiftUnity\Output\Server\Image>

## KNOWN ISSUES, OUTSTANDING BUGS AND UNACCEPTABLE CRAPNESSES

NN. Text is a little small now in some resolutions and score overlaps the playing board.
OO. All the game code is in one file.
PP. Only the release plugins are built, and their targets overlap. Enable server and client builds with both debug and release plugin options.
SS. Clients always log to the same file.
TT. Server log is not redirected to client.

ADDITIONAL THINGS TO DO
=================================================
Pull all unity specific code into its own file
Gamelift minimal code sample without Unity (WPF/console)
GameLift minimal linux server build
Gamelift logging and debugging instances white paper/walkthrough
Minimal sample installer

Brown bag lunch and internal exploration of the problems













































# GameLiftServerSdk C#
## Documention
You can find the official GameLift documentation [here](https://aws.amazon.com/documentation/gamelift/).
## Building the SDK
### Minimum Requirements:
* Visual Studio 2012 or later
* Xamarin Studio 6.1 or later
* Mono develop 6.1 or later

The package contains two solutions: 
* GameLiftServerSDKNet35.sln for .Net framework 3.5 
* GameLiftServerSDKNet45.sln for .Net framework 4.5

To build, simply load up the solution in one of the supported IDEs, restore the Nuget packages and build it from there.

## Unity
### Add GameLift libraries to Unity Editor
The .Net3.5 solution is compatible with Unity. In the Unity Editor, import the following libraries produced by the build. Be sure to pull all the DLLs into the Assets/Plugins directory:
* EngineIoClientDotNet.dll
* GameLiftServerSDKNet35.dll
* log4net.dll
* Newtonsoft.Json.dll
* protobuf-net.dll
* SocketIoClientDotNet.dll
* System.Threading.Tasks.NET35.dll
* WebSocket4Net.dll

Make sure to put all the DLLs in the Assets/Plugins directory.

###  Set API compatibility
You'll need to make sure the API compatibility level is set to .Net 2.0. Otherwise, Unity will throw errors when importing the DLLs.  
From the Unity editor, got to:  
File->Build Settings->Player Settings. Under the Optimization section, make sure the API compatibility Level is set to .Net 2.0.

At this point, you should be ready to start playing with the SDK!

### Example code
Below is a simple MonoBehavior that showcases a simple game server initialization with GameLift.
```csharp
using UnityEngine;
using Aws.GameLift.Server;
using System.Collections.Generic;

public class GameLiftServerExampleBehavior : MonoBehaviour
{
    //This is an example of a simple integration with GameLift server SDK that will make game server processes go active on GameLift!
    public void Start()
    {
        var listeningPort = 7777;

        //InitSDK will establish a local connection with GameLift's agent to enable further communication.
        var initSDKOutcome = GameLiftServerAPI.InitSDK();
        if (initSDKOutcome.Success)
        {
            ProcessParameters processParameters = new ProcessParameters(
                (gameSession) => {
                    //When a game session is created, GameLift sends an activation request to the game server and passes along the game session object containing game properties and other settings.
                    //Here is where a game server should take action based on the game session object.
                    //Once the game server is ready to receive incoming player connections, it should invoke GameLiftServerAPI.ActivateGameSession()
                    GameLiftServerAPI.ActivateGameSession();
                },
                (updateGameSession) => {
                    //When a game session is updated (e.g. by FlexMatch backfill), GameLiftsends a request to the game
                    //server containing the updated game session object.  The game server can then examine the provided
                    //matchmakerData and handle new incoming players appropriately.
                    //updateReason is the reason this update is being supplied.
                },
                () => {
                    //OnProcessTerminate callback. GameLift will invoke this callback before shutting down an instance hosting this game server.
                    //It gives this game server a chance to save its state, communicate with services, etc., before being shut down.
                    //In this case, we simply tell GameLift we are indeed going to shutdown.
                    GameLiftServerAPI.ProcessEnding();
                }, 
                () => {
                    //This is the HealthCheck callback.
                    //GameLift will invoke this callback every 60 seconds or so.
                    //Here, a game server might want to check the health of dependencies and such.
                    //Simply return true if healthy, false otherwise.
                    //The game server has 60 seconds to respond with its health status. GameLift will default to 'false' if the game server doesn't respond in time.
                    //In this case, we're always healthy!
                    return true;
                },
                listeningPort, //This game server tells GameLift that it will listen on port 7777 for incoming player connections.
                new LogParameters(new List<string>()
                {
                    //Here, the game server tells GameLift what set of files to upload when the game session ends.
                    //GameLift will upload everything specified here for the developers to fetch later.
                    "/local/game/logs/myserver.log"
                }));

            //Calling ProcessReady tells GameLift this game server is ready to receive incoming game sessions!
            var processReadyOutcome = GameLiftServerAPI.ProcessReady(processParameters);
            if (processReadyOutcome.Success)
            {
                print("ProcessReady success.");
            }
            else
            {
                print("ProcessReady failure : " + processReadyOutcome.Error.ToString());
            }
        }
        else
        {
            print("InitSDK failure : " + initSDKOutcome.Error.ToString());
        }
    }

    void OnApplicationQuit()
    {
        //Make sure to call GameLiftServerAPI.Destroy() when the application quits. This resets the local connection with GameLift's agent.
        GameLiftServerAPI.Destroy();
    }
}
```

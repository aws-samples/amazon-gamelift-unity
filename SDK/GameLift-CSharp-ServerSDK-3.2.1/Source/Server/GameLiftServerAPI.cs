/*
* All or portions of this file Copyright (c) Amazon.com, Inc. or its affiliates or
* its licensors.
*
* For complete copyright and license terms please see the LICENSE at the root of this
* distribution (the "License"). All use of this software is governed by the License,
* or, if provided, by the license below or the license accompanying this file. Do not
* remove or modify any license notices. This file is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
*
*/

using System;
using Aws.GameLift.Server.Model;

namespace Aws.GameLift.Server
{
    public static class GameLiftServerAPI
    {
        static readonly string sdkVersion = "3.2.1";

        /**
          * @return The current SDK version.
          */
        public static AwsStringOutcome GetSdkVersion()
        {
            return new AwsStringOutcome(sdkVersion);
        }

        /**
          * Initializes the GameLift server.
          * Should be called when the server starts, before any GameLift-dependent initialization happens.
          */
        public static GenericOutcome InitSDK()
        {
            return ServerState.Instance.InitializeNetworking();
        }

        /**
          * Signals GameLift that the process is ready to receive GameSessions.
          * The onStartGameSession callback will be invoked when the server is bound to a GameSession. Game-property-dependent initialization (such as loading a
          * user-requested map) should take place at that time. The onHealthCheck callback is invoked asynchronously. There is no mechanism to
          * to destroy the resulting thread. If it does not complete in a given time period the server status will be reported as unhealthy.
          * @param processParameters The parameters required to successfully run the process.
          */
        public static GenericOutcome ProcessReady(ProcessParameters processParameters)
        {
            return ServerState.Instance.ProcessReady(processParameters);
        }

        /**
          * Signals GameLift that the process is ending.
          * GameLift will eventually terminate the process and recycle the host. Once the process is marked as Ending,
          */
        public static GenericOutcome ProcessEnding()
        {
            return ServerState.Instance.ProcessEnding();
        }

        /**
          * Reports to GameLift that the server process is now ready to receive player sessions.
          * Should be called once all GameSession initialization has finished.
          */
        public static GenericOutcome ActivateGameSession()
        {
            return ServerState.Instance.ActivateGameSession();
        }

        /**
          * Reports to GameLift that the GameSession has now ended.
          * GameLift will now expect the server process to call either ProcessReady in order to launch a new GameSession
          * or ProcessEnding which will trigger this process and host to be recycled.
          */
        public static GenericOutcome TerminateGameSession()
        {
            return ServerState.Instance.TerminateGameSession();
        }

        /**
          * Update player session policy on the GameSession.
          */
        public static GenericOutcome UpdatePlayerSessionCreationPolicy(PlayerSessionCreationPolicy playerSessionPolicy)
        {
            return ServerState.Instance.UpdatePlayerSessionCreationPolicy(playerSessionPolicy);
        }

        /**
          * @return The server's bound GameSession Id, if the server is Active.
          */
        public static AwsStringOutcome GetGameSessionId()
        {
            return ServerState.Instance.GetGameSessionId();
        }

        /**
        * @return The server's processes Epoch TerminationTime.
        */
        public static AwsLongOutcome GetTerminationTime()
        {
            return ServerState.Instance.GetTerminationTime();
        }

        /**
          * Processes and validates a player session connection. This method should be called when a client requests a
          * connection to the server. The client should send the PlayerSessionID which it received from RequestPlayerSession
          * or GameLift::CreatePlayerSession to be passed into this function.
          * This method will return an UNEXPECTED_PLAYER_SESSION error if the player session ID is invalid.
          * @param playerSessionId the ID of the joining player's session.
          */
        public static GenericOutcome AcceptPlayerSession(String playerSessionId)
        {
            return ServerState.Instance.AcceptPlayerSession(playerSessionId);
        }

        /**
          * Processes a player session disconnection. Should be called when a player leaves or otherwise disconnects from
          * the server.
          * @param playerSessionId the ID of the joining player's session.
          */
        public static GenericOutcome RemovePlayerSession(String playerSessionId)
        {
            return ServerState.Instance.RemovePlayerSession(playerSessionId);
        }

		/// <summary>
		/// Retrieves properties for one or more player sessions.
		/// </summary>
		/// <returns>The player sessions.</returns>
		/// <param name="describePlayerSessionsRequest">Request specifying which player sessions to describe.</param>
		public static DescribePlayerSessionsOutcome DescribePlayerSessions(DescribePlayerSessionsRequest describePlayerSessionsRequest)
		{
            return ServerState.Instance.DescribePlayerSessions(describePlayerSessionsRequest);
		}

        /**
          * Submit a request to backfill the current match.
          */
        public static StartMatchBackfillOutcome StartMatchBackfill(StartMatchBackfillRequest request)
        {
            return ServerState.Instance.BackfillMatchmaking(request);
        }

        /**
          * Submit a request to stop an outstanding request to backfill the current match.
          */
        public static GenericOutcome StopMatchBackfill(StopMatchBackfillRequest request)
        {
            return ServerState.Instance.StopMatchmaking(request);
        }

        public static GenericOutcome Destroy()
        {
            ServerState.Instance.Shutdown();
            return new GenericOutcome();
        }
    }
}

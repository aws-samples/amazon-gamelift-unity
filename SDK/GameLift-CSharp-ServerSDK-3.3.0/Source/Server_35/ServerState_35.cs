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
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Quobject.SocketIoClientDotNet.Client;
using Aws.GameLift.Server.Model;
using log4net;
using Newtonsoft.Json;

namespace Aws.GameLift.Server
{
    public sealed class ServerState : IAuxProxyMessageHandler
    {
        static readonly string HOSTNAME = "127.0.0.1";
        static readonly string PORT = "5757";
        static readonly string PID_KEY = "pID";
        static readonly string SDK_VERSION_KEY = "sdkVersion";
        static readonly string FLAVOR_KEY = "sdkLanguage";
        static readonly string FLAVOR = "CSharp";
        static readonly double HEALTHCHECK_TIMEOUT_SECONDS = 60;

        AuxProxyMessageSender sender;
        Network network;

        ProcessParameters processParameters;
        bool processReady = false;
        string gameSessionId;
        long terminationTime = -1;

        //To make this thread safe
        static object networkLock = new Object();
        static volatile bool networkInitialized = false;
        static readonly ServerState instance = new ServerState();
        static readonly ILog log = LogManager.GetLogger(typeof(ServerState));

        static ServerState() { }
        ServerState() { }

        public static ServerState Instance { get { return instance; } }
        ~ServerState()
        {
            lock (networkLock)
            {
                networkInitialized = false;
            }
            network.Disconnect();
        }

        public GenericOutcome ProcessReady(ProcessParameters procParameters)
        {
            processReady = true;
            processParameters = procParameters;

            if (!networkInitialized)
            {
                return new GenericOutcome(new GameLiftError(GameLiftErrorType.NETWORK_NOT_INITIALIZED));
            }

            GenericOutcome result = sender.ProcessReady(processParameters.Port, processParameters.LogParameters.LogPaths);

            Task.Run(() => StartHealthCheck());

            return result;
        }

        public GenericOutcome ProcessEnding()
        {
            if (!networkInitialized)
            {
                return new GenericOutcome(new GameLiftError(GameLiftErrorType.NETWORK_NOT_INITIALIZED));
            }
            return sender.ProcessEnding();
        }

        public GenericOutcome ActivateGameSession()
        {
            if (!networkInitialized)
            {
                return new GenericOutcome(new GameLiftError(GameLiftErrorType.NETWORK_NOT_INITIALIZED));
            }
            if (String.IsNullOrEmpty(gameSessionId))
            {
                return new GenericOutcome(new GameLiftError(GameLiftErrorType.GAMESESSION_ID_NOT_SET));
            }
            return sender.ActivateGameSession(gameSessionId);
        }

        public GenericOutcome TerminateGameSession()
        {
            if (!networkInitialized)
            {
                return new GenericOutcome(new GameLiftError(GameLiftErrorType.NETWORK_NOT_INITIALIZED));
            }
            if (String.IsNullOrEmpty(gameSessionId))
            {
                return new GenericOutcome(new GameLiftError(GameLiftErrorType.GAMESESSION_ID_NOT_SET));
            }
            return sender.TerminateGameSession(gameSessionId);
        }

        public AwsStringOutcome GetGameSessionId()
        {
            if (String.IsNullOrEmpty(gameSessionId))
            {
                return new AwsStringOutcome(new GameLiftError(GameLiftErrorType.GAMESESSION_ID_NOT_SET));
            }
            return new AwsStringOutcome(gameSessionId);
        }

        public AwsLongOutcome GetTerminationTime()
        {
            if (terminationTime == -1)
            {
                return new AwsLongOutcome(new GameLiftError(GameLiftErrorType.TERMINATION_TIME_NOT_SET));
            }
            return new AwsLongOutcome(terminationTime);
        }

        public GenericOutcome UpdatePlayerSessionCreationPolicy(PlayerSessionCreationPolicy playerSessionPolicy)
        {
            if (!networkInitialized)
            {
                return new GenericOutcome(new GameLiftError(GameLiftErrorType.NETWORK_NOT_INITIALIZED));
            }
            if (String.IsNullOrEmpty(gameSessionId))
            {
                return new GenericOutcome(new GameLiftError(GameLiftErrorType.GAMESESSION_ID_NOT_SET));
            }
            return sender.UpdatePlayerSessionCreationPolicy(gameSessionId, playerSessionPolicy);
        }

        public GenericOutcome AcceptPlayerSession(string playerSessionId)
        {
            if (!networkInitialized)
            {
                return new GenericOutcome(new GameLiftError(GameLiftErrorType.NETWORK_NOT_INITIALIZED));
            }
            if (String.IsNullOrEmpty(gameSessionId))
            {
                return new GenericOutcome(new GameLiftError(GameLiftErrorType.GAMESESSION_ID_NOT_SET));
            }
            return sender.AcceptPlayerSession(playerSessionId, gameSessionId);
        }

        public GenericOutcome RemovePlayerSession(string playerSessionId)
        {
            if (!networkInitialized)
            {
                return new GenericOutcome(new GameLiftError(GameLiftErrorType.NETWORK_NOT_INITIALIZED));
            }
            if (String.IsNullOrEmpty(gameSessionId))
            {
                return new GenericOutcome(new GameLiftError(GameLiftErrorType.GAMESESSION_ID_NOT_SET));
            }
            return sender.RemovePlayerSession(playerSessionId, gameSessionId);
        }

        public DescribePlayerSessionsOutcome DescribePlayerSessions(DescribePlayerSessionsRequest request)
        {
            log.DebugFormat("Describing player sessions {0}", request);

            if (!networkInitialized)
            {
                return new DescribePlayerSessionsOutcome(new GameLiftError(GameLiftErrorType.NETWORK_NOT_INITIALIZED));
            }
            else
            {
                return sender.DescribePlayerSessions(request);
            }
        }

        void StartHealthCheck()
        {
            log.Debug("HealthCheck thread started.");
            while (processReady)
            {
                Task.Run(() => ReportHealth());
                Thread.Sleep(TimeSpan.FromSeconds(HEALTHCHECK_TIMEOUT_SECONDS));
            }
        }

        void ReportHealth()
        {
            log.Debug("Reporting health using the OnHealthCheck callback.");
            IAsyncResult result = processParameters.OnHealthCheck.BeginInvoke(null, null);

            if (!result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(HEALTHCHECK_TIMEOUT_SECONDS)))
            {
                log.Debug("Timed out waiting for health response from the server process. Reporting as unhealthy.");
                sender.ReportHealth(false);
            }
            else
            {
                bool healthCheckResult = processParameters.OnHealthCheck.EndInvoke(result);

                log.Debug(String.Format("Received health response from the server process: {0}", healthCheckResult));
                sender.ReportHealth(healthCheckResult);
            }
        }

        public StartMatchBackfillOutcome BackfillMatchmaking(StartMatchBackfillRequest request)
        {
            if (!networkInitialized)
            {
                return new StartMatchBackfillOutcome(new GameLiftError(GameLiftErrorType.NETWORK_NOT_INITIALIZED));
            }

            return sender.BackfillMatchmaking(request);
        }

        public GenericOutcome StopMatchmaking(StopMatchBackfillRequest request)
        {
            if (!networkInitialized)
            {
                return new GenericOutcome(new GameLiftError(GameLiftErrorType.NETWORK_NOT_INITIALIZED));
            }

            return sender.StopMatchmaking(request);
        }

        public GenericOutcome InitializeNetworking()
        {
            if (!networkInitialized)
            {
                lock (networkLock)
                {
                    var socketToAuxProxy = IO.Socket(CreateURI(), CreateDefaultOptions());
                    var socketFromAuxProxy = IO.Socket(CreateURI(), CreateDefaultOptions());
                    network = new Network(socketToAuxProxy, socketFromAuxProxy, this);

                    return Connect(socketToAuxProxy, network);
                }
            }
            //Idempotent
            return new GenericOutcome();
        }

        public GenericOutcome Connect(Socket socketToAuxProxy, Network network)
        {
            sender = new AuxProxyMessageSender(socketToAuxProxy);
            var outcome = network.Connect();
            networkInitialized = outcome.Success;

            return outcome;
        }

        string CreateURI()
        {
            var endpoint = string.Format("http://{0}:{1}", HOSTNAME, PORT);
            return endpoint;
        }

        IO.Options CreateDefaultOptions()
        {
            IO.Options options = new IO.Options();
            options.QueryString = String.Format("{0}={1}&{2}={3}&{4}={5}",
                                                PID_KEY,
                                                Process.GetCurrentProcess().Id.ToString(),
                                                SDK_VERSION_KEY,
                                                GameLiftServerAPI.GetSdkVersion().Result,
                                                FLAVOR_KEY,
                                                FLAVOR
                                               );
            options.AutoConnect = false;
            options.Transports = new List<string>() { "websocket" };

            return options;
        }

        public void OnStartGameSession(string rawGameSession, IAck ack)
        {
            log.DebugFormat("ServerState got the startGameSession signal. rawGameSession : {0}", rawGameSession);
            if (!processReady)
            {
                log.Debug("Got a game session on inactive process. Sending false ack.");
                ack.Call(false);
                return;
            }
            log.Debug("OnStartGameSession: Sending true ack.");
            ack.Call(true);

            Task.Run(() =>
            {
                GameSession gameSession = GameSessionParser.Parse(rawGameSession);
                gameSessionId = gameSession.GameSessionId;
                processParameters.OnStartGameSession(gameSession);
            });
        }

        public void OnTerminateProcess(string rawTerminationTime)
        {
            log.DebugFormat("ServerState got the terminateProcess signal.  rawTerminationTime : {0}", rawTerminationTime);
            Task.Run(() =>
            {
                var deserialized = JsonConvert.DeserializeObject<Aws.GameLift.Protobuf.TerminateProcess>(rawTerminationTime);
                  
                if (deserialized == null)
                {
                    //If termination time isn't sent from AuxProxy use now plus 5 minutes.
                    var defaultTerminationTime = DateTime.UtcNow;
                    defaultTerminationTime = defaultTerminationTime.AddSeconds(270);
                    terminationTime = defaultTerminationTime.Ticks;
                }
                else
                {
                    /* TerminationTime coming from AuxProxy is seconds that have elapsed since Unix epoch time begins (00:00:00 UTC Jan 1 1970).
                    * Since epoch time for dotNet starts at 0001-01-01T00:00:00 we need to create a DateTime at the beginning of Unix epoch time
                    * and add the TerminationTime to that date.
                    */
                    var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    terminationTime = epoch.AddSeconds(deserialized.TerminationTime).Ticks;
                }

                processParameters.OnProcessTerminate();
              });
        }

        public void OnUpdateGameSession(string rawUpdateGameSession, IAck ack)
        {
            log.DebugFormat("ServerState got the updateGameSession.  rawGameSessionUpdate : {0}", rawUpdateGameSession);
            if (!processReady)
            {
                log.Debug("Got a update game session call on inactive process. Sending false ack.");
                ack.Call(false);
                return;
            }
            log.Debug("OnUpdateGameSession: Sending true ack.");
            ack.Call(true);

            Task.Run(() =>
            {
                Protobuf.UpdateGameSession updateGameSession =
                    JsonConvert.DeserializeObject<Protobuf.UpdateGameSession>(rawUpdateGameSession);
                GameSession gameSession = GameSession.ParseFromBufferedGameSession(updateGameSession.GameSession);
                UpdateReason updateReason = UpdateReasonMapper.GetUpdateReasonForName(updateGameSession.UpdateReason);

                processParameters.OnUpdateGameSession(
                    new UpdateGameSession(gameSession, updateReason, updateGameSession.BackfillTicketId));
            });
        }

        public void Shutdown()
        {
            lock (networkLock)
            {
                networkInitialized = false;
            }
            network.Disconnect();
            processReady = false;
        }
    }
}

// Copyright 2018 Amazon
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Amazon;
using Amazon.GameLift;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Aws.GameLift.Server;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using UnityEngine;

public class GameLift : MonoBehaviour
{
    public GameLogic gamelogic = null;
#if SERVER
    public GameLiftServer server;
#endif
#if CLIENT
    public GameLiftClient client;
#endif

    public bool MyRemoteCertificateValidationCallback(System.Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        bool isOk = true;
        // If there are errors in the certificate chain, look at each error to determine the cause.
        if (sslPolicyErrors != SslPolicyErrors.None)
        {
            for (int i = 0; i < chain.ChainStatus.Length; i++)
            {
                if (chain.ChainStatus[i].Status != X509ChainStatusFlags.RevocationStatusUnknown)
                {
                    chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                    chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                    chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);
                    chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
                    bool chainIsValid = chain.Build((X509Certificate2)certificate);
                    if (!chainIsValid)
                    {
                        isOk = false;
                    }
                }
            }
        }

        return isOk;
    }

    public void Awake()
    {
        Debug.Log(":) GAMELIFT AWAKE");
        // Allow Unity to validate HTTPS SSL certificates; http://stackoverflow.com/questions/4926676
        ServicePointManager.ServerCertificateValidationCallback = MyRemoteCertificateValidationCallback;
#if SERVER
        Debug.Log (":) I AM SERVER");
        server = new GameLiftServer(this);
#endif
#if CLIENT
        Debug.Log(":) I AM CLIENT");
        client = new GameLiftClient(this);
#endif
        GameObject gamelogicObj = GameObject.Find("/GameLogicStatic");
        Debug.Assert(gamelogicObj != null);
        gamelogic = gamelogicObj.GetComponent<GameLogic>();
        if (gamelogic == null) Debug.Log(":( GAMELOGIC CODE NOT AVAILABLE ON GAMELOGICSTATIC OBJECT");
    }

    public void Start()
    {
        Debug.Log(":) GAMELIFT START");
#if SERVER
        server.Start();
#endif
#if CLIENT
        client.Start();
#endif
    }

    public void Update()
    {
    }

#if SERVER
    public bool ConnectPlayer(int playerIdx, string playerSessionId)
    {
        return server.ConnectPlayer(playerIdx, playerSessionId);
    }

    public void DisconnectPlayer(int playerIdx)
    {
        server.DisconnectPlayer(playerIdx);
    }

    public void TerminateGameSession(bool processEnding)
    {
        server.TerminateGameSession(processEnding);
    }

    // we received a force terminate request. Notify clients and gracefully exit.
    public void TerminateServer()
    {
        if (gamelogic != null) gamelogic.OnApplicationQuit();
    }

#endif

#if CLIENT
    public void GetConnectionInfo(ref string ip, ref int port, ref string auth)
    {
        client.GetConnectionInfo(ref ip, ref port, ref auth);
    }

#endif
}

#if SERVER
public class GameLiftServer
{
    private GameLift gl;
    private bool GameLiftRequestedTermination = false;
    private Dictionary<int, string> playerSessions;
    private int port = 1935;

    public GameLiftServer(GameLift _gl)
    {
        gl = _gl;
        playerSessions = new Dictionary<int, string>();
    }

    public void Start()
    {
        // Use command line port if possible, otherwise use default (hard coded port)
        string[] args = System.Environment.GetCommandLineArgs ();
        for (int i = 0; i < args.Length - 1; i++)
        {
            int value = 0;
            if (args[i] != "-port")
                continue;
            
            if (!int.TryParse(args[i+1], out value))
                continue;
            
            if (value < 1000 || value >= 65536)
                continue;
            
            port = value;
            Debug.Log (":) LISTEN PORT " + port + " FOUND ON COMMAND LINE");
            break;
        }

        string sdkVersion = GameLiftServerAPI.GetSdkVersion().Result;
        Debug.Log (":) SDK VERSION: " + sdkVersion);
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
    }

    public bool ConnectPlayer(int playerIdx, string playerSessionId)
    {
        try
        {
            var outcome = GameLiftServerAPI.AcceptPlayerSession(playerSessionId);
            if (outcome.Success)
            {
                Debug.Log (":) PLAYER SESSION VALIDATED");
            }
            else
            {
                Debug.Log (":( PLAYER SESSION REJECTED. AcceptPlayerSession() returned " + outcome.Error.ToString());
            }

            playerSessions.Add(playerIdx, playerSessionId);
            return outcome.Success;
        }
        catch (Exception e)
        {
            Debug.Log (":( REJECTED PLAYER SESSION. AcceptPlayerSession() exception " + Environment.NewLine + e.Message);
            return false;
        }
    }

    public void DisconnectPlayer(int playerIdx)
    {
        // if player slots never re-open, just skip this entire thing.
        try
        {
            string playerSessionId = playerSessions[playerIdx];
            try
            {
                var outcome = GameLiftServerAPI.RemovePlayerSession(playerSessionId);
                if (outcome.Success)
                {
                    Debug.Log (":) PLAYER SESSION REMOVED");
                }
                else
                {
                    Debug.Log (":( PLAYER SESSION REMOVE FAILED. RemovePlayerSession() returned " + outcome.Error.ToString());
                }
            }
            catch (Exception e)
            {
                Debug.Log (":( PLAYER SESSION REMOVE FAILED. RemovePlayerSession() exception " + Environment.NewLine + e.Message);
                throw;
            }

            playerSessions.Remove(playerIdx);
        }
        catch (KeyNotFoundException e)
        {
            Debug.Log (":( INVALID PLAYER SESSION. Exception " + Environment.NewLine + e.Message);
            throw; // should never happen
        }
    }

    public void TerminateGameSession(bool processEnding)
    {
        if (GameLiftRequestedTermination)
        {
            // don't terminate game session if gamelift initiated process termination, just exit.
            Environment.Exit(0);
        }

        try
        {
            var outcome = GameLiftServerAPI.TerminateGameSession();
            if (outcome.Success)
            {
                Debug.Log (":) GAME SESSION TERMINATED");
                if (processEnding)
                    ProcessEnding();
                else
                    ProcessReady();
            }
            else
            {
                Debug.Log (":( GAME SESSION TERMINATION FAILED. TerminateGameSession() returned " + outcome.Error.ToString());
            }
        }
        catch (Exception e)
        {
            Debug.Log (":( GAME SESSION TERMINATION FAILED. TerminateGameSession() exception " + Environment.NewLine + e.Message);
        }
    }

    private void ProcessReady()
    {
        try
        {
            ProcessParameters prParams = new ProcessParameters(
            /* onStartGameSession */ (gameSession) => {
                Debug.Log (":) GAMELIFT SESSION REQUESTED"); //And then do stuff with it maybe.
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
            },
            /* onProcessTerminate */ () => {
                Debug.Log (":| GAMELIFT PROCESS TERMINATION REQUESTED (OK BYE)");
                GameLiftRequestedTermination = true;
                gl.TerminateServer();
            },
            /* onHealthCheck */ () => {
                Debug.Log (":) GAMELIFT HEALTH CHECK REQUESTED (HEALTHY)");
                return true;
            },
            /* port */ port, // tell the GameLift service which port to connect to this process on.
            // unless we manage this there can only be one process per server.
                new LogParameters(new List<string>()
            {
                @"C:\game\GameLiftUnity_Data\output_log.txt" // must be different for each server if multiple servers on instance
            }));

            var processReadyOutcome = GameLiftServerAPI.ProcessReady(prParams);
            if (processReadyOutcome.Success)
            {
                if (gl.gamelogic != null) gl.gamelogic.GameliftStatus = true;
                Debug.Log (":) PROCESSREADY SUCCESS.");
            }
            else
            {
                if (gl.gamelogic != null) gl.gamelogic.GameliftStatus = false;
                Debug.Log (":( PROCESSREADY FAILED. ProcessReady() returned " + processReadyOutcome.Error.ToString());
            }
        }
        catch (Exception e)
        {
            Debug.Log (":( PROCESSREADY FAILED. ProcessReady() exception " + Environment.NewLine + e.Message);
        }
    }

    private void ProcessEnding()
    {
        try
        {
            var outcome = GameLiftServerAPI.ProcessEnding();
            if (outcome.Success)
            {
                Debug.Log (":) PROCESSENDING");
            }
            else
            {
                Debug.Log (":( PROCESSENDING FAILED. ProcessEnding() returned " + outcome.Error.ToString());
            }
        }
        catch (Exception e)
        {
            Debug.Log (":( PROCESSENDING FAILED. ProcessEnding() exception " + Environment.NewLine + e.Message);
        }
    }
}

#endif

#if CLIENT
public class GameLiftClient
{
    private GameLift gl;
    private string playerId; // this was once for DAU based billing.
    private Amazon.GameLift.Model.PlayerSession psession = null;
    // default alias - command line overrides, use --alias alias-0c67a845-bc6e-4885-a3f6-40f1d2268234
    // or change the default below and rebuild the client (case sensitive command line)
    //    buildconfig Client
    public string aliasId = "alias-0c67a845-bc6e-4885-a3f6-40f1d2268234";
    public static readonly string profileName = "demo-gamelift-unity";
    public AmazonGameLiftClient aglc = null;

    public void CreateGameLiftClient()
    {
        var config = new AmazonGameLiftConfig();
        config.RegionEndpoint = Amazon.RegionEndpoint.USEast1;
        Debug.Log("GL372");
        try
        {
            CredentialProfile profile = null;
            var nscf = new SharedCredentialsFile();
            nscf.TryGetProfile(profileName, out profile);
            AWSCredentials credentials = profile.GetAWSCredentials(null);
            Debug.Log("demo-gamelift-unity profile GL376");
            aglc = new AmazonGameLiftClient(credentials, config);
            Debug.Log("GL378");
        }
        catch (AmazonServiceException)
        {
            Debug.Log("regular profile search GL382");
            try
            {
                aglc = new AmazonGameLiftClient(config);
            }
            catch (AmazonServiceException e)
            {
                Debug.Log("AWS Credentials not found. Cannot connect to GameLift. Start application with -credentials <file> flag where credentials are the credentials.csv or accessKeys.csv file containing the access and secret key. GL390");
                Debug.Log(e.Message);
            }
        }
    }

    public void DisposeGameLiftClient()
    {
        aglc.Dispose();
    }

    public GameLiftClient(GameLift _gl)
    {
        gl = _gl;
        playerId = Guid.NewGuid().ToString();
        Credentials.Install();
        CreateGameLiftClient();

        // Use command line alias if possible, otherwise use default (hard coded alias)
        string[] args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] != "--alias")
            {
                continue;
            }

            string pattern = @"alias-[0-9A-Fa-f]{8}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{12}";
            Match m = Regex.Match(args[i + 1], pattern);
            if (m.Success)
            {
                aliasId = m.Value;
                Debug.Log(":) ALIAS RECOGNIZED. Alias " + aliasId + " found on command line");
                break;
            }
        }

        // verify alias exists
        if (aglc != null)
        {
            try
            {
                var dareq = new Amazon.GameLift.Model.DescribeAliasRequest();
                dareq.AliasId = aliasId;
                Amazon.GameLift.Model.DescribeAliasResponse dares = aglc.DescribeAlias(dareq);
                Amazon.GameLift.Model.Alias alias = dares.Alias;
                Debug.Log((int)dares.HttpStatusCode + " ALIAS NAME: " + alias.Name + " (" + aliasId + ")");
                if (alias.RoutingStrategy.Type == Amazon.GameLift.RoutingStrategyType.TERMINAL)
                    Debug.Log("             (TERMINAL ALIAS)");
            }
            catch (Exception e)
            {
                Debug.Log("AWS Credentials found but probably invalid. Check IAM permissions for the credentials.");
                Debug.Log(e.Message);
            }
        }
    }

    ~GameLiftClient()
    {
        DisposeGameLiftClient();
    }

    public void Start()
    {
    }

    public Amazon.GameLift.Model.PlayerSession CreatePlayerSession(Amazon.GameLift.Model.GameSession gsession)
    {
        try
        {
            var cpsreq = new Amazon.GameLift.Model.CreatePlayerSessionRequest();
            cpsreq.GameSessionId = gsession.GameSessionId;
            cpsreq.PlayerId = playerId;
            Amazon.GameLift.Model.CreatePlayerSessionResponse cpsres = aglc.CreatePlayerSession(cpsreq);
            string psid = cpsres.PlayerSession != null ? cpsres.PlayerSession.PlayerSessionId : "N/A";
            Debug.Log((int)cpsres.HttpStatusCode + " PLAYER SESSION CREATED: " + psid);
            return cpsres.PlayerSession;
        }
        catch (Amazon.GameLift.Model.InvalidGameSessionStatusException e)
        {
            Debug.Log(e.StatusCode.ToString() + " InvalidGameSessionStatusException: " + e.Message);
            return null;
        }
    }

    public Amazon.GameLift.Model.GameSession CreateGameSession()
    {
        try
        {
            var cgsreq = new Amazon.GameLift.Model.CreateGameSessionRequest();
            cgsreq.AliasId = aliasId;
            cgsreq.CreatorId = playerId;
            cgsreq.MaximumPlayerSessionCount = 4;
            Amazon.GameLift.Model.CreateGameSessionResponse cgsres = aglc.CreateGameSession(cgsreq);
            string gsid = cgsres.GameSession != null ? cgsres.GameSession.GameSessionId : "N/A";
            Debug.Log((int)cgsres.HttpStatusCode + " GAME SESSION CREATED: " + gsid);
            return cgsres.GameSession;
        }
        catch (Amazon.GameLift.Model.FleetCapacityExceededException e)
        {
            Debug.Log(e.StatusCode.ToString() + " FleetCapacityExceededException: " + e.Message);
            return null;
        }
        catch (Amazon.GameLift.Model.InvalidRequestException e)
        {
            Debug.Log(e.StatusCode.ToString() + " InvalidRequestException: " + e.Message);
            return null;
        }
    }

    public Amazon.GameLift.Model.GameSession SearchGameSessions()
    {
        try
        {
            var sgsreq = new Amazon.GameLift.Model.SearchGameSessionsRequest();
            sgsreq.AliasId = aliasId; // only our game
            sgsreq.FilterExpression = "hasAvailablePlayerSessions=true"; // only ones we can join
            sgsreq.SortExpression = "creationTimeMillis ASC"; // return oldest first
            sgsreq.Limit = 1; // only one session even if there are other valid ones
            Amazon.GameLift.Model.SearchGameSessionsResponse sgsres = aglc.SearchGameSessions(sgsreq);
            Debug.Log((int)sgsres.HttpStatusCode + " GAME SESSION SEARCH FOUND " + sgsres.GameSessions.Count + " SESSIONS (on " + aliasId + ")");
            if (sgsres.GameSessions.Count > 0)
                return sgsres.GameSessions[0];
            return null;
        }
        catch (Amazon.GameLift.Model.InvalidRequestException e)
        {
            // EXCEPTION HERE? Your alias does not point to a valid fleet, possibly.
            Debug.Log(e.StatusCode.ToString() + " :( SEARCHGAMESESSIONS FAILED. InvalidRequestException " + e.Message +
                Environment.NewLine + "Game alias " + aliasId + " may not point to a valid fleet" + Environment.NewLine);
            return null;
        }
    }

    public void GetConnectionInfo(ref string ip, ref int port, ref string auth)
    {
        Debug.Log("GetConnectionInfo()");
        if (aglc != null)
        {
            try
            {
                for (int retry = 0; retry < 4; retry++)
                {
                    Debug.Log("SearchGameSessions retry==" + retry);
                    Amazon.GameLift.Model.GameSession gsession = SearchGameSessions();
                    if (gsession != null)
                    {
                        Debug.Log("GameSession found " + gsession.GameSessionId);
                        Amazon.GameLift.Model.PlayerSession psession = CreatePlayerSession(gsession);
                        if (psession != null)
                        {
                            // created a player session in there
                            ip = psession.IpAddress;
                            port = psession.Port;
                            auth = psession.PlayerSessionId;
                            Debug.Log($"CLIENT CONNECT INFO: {ip}, {port}, {auth} GL545");
                            if (gl.gamelogic != null) gl.gamelogic.GameliftStatus = true;
                            aglc.Dispose();
                            return;
                        }

                        // player session creation failed (probably beaten to the session by another player)
                        retry = 0; // start over
                    }
                }

                // no game session, we should create one
                for (int retry = 0; retry < 4; retry++)
                {
                    Debug.Log("GameSession not found. CreateGameSession: retry==" + retry);
                    Amazon.GameLift.Model.GameSession gsession = CreateGameSession();
                    if (gsession != null)
                    {
                        for (int psretry = 0; psretry < 4; psretry++)
                        {
                            Debug.Log("CreatePlayerSession: retry==" + psretry);
                            psession = CreatePlayerSession(gsession);
                            if (psession != null)
                            {
                                // created a player session in there
                                ip = psession.IpAddress;
                                port = psession.Port;
                                auth = psession.PlayerSessionId;
                                Debug.Log($"CLIENT CONNECT INFO: {ip}, {port}, {auth} GL574");
                                if (gl.gamelogic != null) gl.gamelogic.GameliftStatus = true;
                                return;
                            }
                        }

                        // player session creation failed (probably beaten to the session by another player)
                        retry = 0; // start over
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log("AWS Credentials found but probably invalid. Check IAM permissions for the credentials. GL588");
                Debug.Log(e.Message);
            }
        }

        // something's not working. fall back to local client
        Debug.Log($"CLIENT CONNECT INFO (LOCAL): {ip}, {port}, {auth} GL594");
        if (gl.gamelogic != null) gl.gamelogic.GameliftStatus = false;
    }
}

#endif
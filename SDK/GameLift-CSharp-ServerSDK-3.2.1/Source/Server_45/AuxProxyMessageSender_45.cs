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
using System.Threading.Tasks;
using Quobject.SocketIoClientDotNet.Client;
using Aws.GameLift.Server.Model;
using Google.Protobuf;
using Com.Amazon.Whitewater.Auxproxy.Pbuffer;

namespace Aws.GameLift.Server
{
    public partial class AuxProxyMessageSender
    {
        AckImpl CreateAckFunction(TaskCompletionSource<StartMatchBackfillOutcome> future)
        {
            return new AckImpl((ack, response) =>
            {
                log.DebugFormat("Got ack {0} with response {1}", ack, response);

                if (null == ack)
                {
                    future.TrySetResult(new StartMatchBackfillOutcome(new GameLiftError(GameLiftErrorType.SERVICE_CALL_FAILED)));
                    return;
                }

                var success = (bool)ack;

                if (success)
                {
                    var deserialized = Com.Amazon.Whitewater.Auxproxy.Pbuffer.BackfillMatchmakingResponse.Parser.ParseJson(response as string);
                    var translation = BackfillDataMapper.ParseFromBufferedBackfillMatchmakingResponse(deserialized);
                    future.TrySetResult(new StartMatchBackfillOutcome(translation));
                }
                else
                {
                    future.TrySetResult(new StartMatchBackfillOutcome(new GameLiftError(GameLiftErrorType.SERVICE_CALL_FAILED)));
                }
            });
        }

        AckImpl CreateAckFunction(TaskCompletionSource<DescribePlayerSessionsOutcome> future)
        {
            return new AckImpl((ack, response) =>
            {
                log.DebugFormat("Got ack {0} with response {1}", ack, response);

                if (null == ack)
                {
                    future.TrySetResult(new DescribePlayerSessionsOutcome(new GameLiftError(GameLiftErrorType.SERVICE_CALL_FAILED)));
                    return;
                }

                var success = (bool)ack;

                if (success)
                {
                    var deserialized = Com.Amazon.Whitewater.Auxproxy.Pbuffer.DescribePlayerSessionsResponse.Parser.ParseJson(response as string);
                    var translation = DescribePlayerSessionsResult.ParseFromBufferedDescribePlayerSessionsResponse(deserialized);
                    future.TrySetResult(new DescribePlayerSessionsOutcome(translation));
                }
                else
                {
                    future.TrySetResult(new DescribePlayerSessionsOutcome(new GameLiftError(GameLiftErrorType.SERVICE_CALL_FAILED)));
                }
            });
        }

        public GenericOutcome ProcessReady(int port, List<string> logPathsToUpload)
        {
            var pReady = new ProcessReady
            {
                Port = port,
                LogPathsToUpload = { logPathsToUpload }
            };


            return EmitEvent(pReady);
        }

        public GenericOutcome ProcessEnding()
        {
            var pEnding = new ProcessEnding();

            return EmitEvent(pEnding);
        }

        public GenericOutcome ActivateGameSession(string gameSessionId)
        {
            var activateGameSession = new GameSessionActivate
            {
                GameSessionId = gameSessionId
            };

            return EmitEvent(activateGameSession);
        }

        public GenericOutcome TerminateGameSession(string gameSessionId)
        {
            var terminateGameSession = new GameSessionTerminate
            {
                GameSessionId = gameSessionId
            };

            return EmitEvent(terminateGameSession);
        }

        public GenericOutcome UpdatePlayerSessionCreationPolicy(string gameSessionId, PlayerSessionCreationPolicy playerSessionPolicy)
        {
            var policy = new UpdatePlayerSessionCreationPolicy
            {
                GameSessionId = gameSessionId,
                NewPlayerSessionCreationPolicy = PlayerSessionCreationPolicyMapper.GetNameForPlayerSessionCreationPolicy(playerSessionPolicy)
            };

            return EmitEvent(policy);
        }

        public GenericOutcome AcceptPlayerSession(string playerSessionId, string gameSessionId)
        {
            var acceptPlayerSession = new AcceptPlayerSession
            {
                PlayerSessionId = playerSessionId,
                GameSessionId = gameSessionId
            };

            return EmitEvent(acceptPlayerSession);
        }

        public GenericOutcome RemovePlayerSession(string playerSessionId, string gameSessionId)
        {
            var removePlayerSession = new RemovePlayerSession
            {
                PlayerSessionId = playerSessionId,
                GameSessionId = gameSessionId
            };

            return EmitEvent(removePlayerSession);
        }

        public DescribePlayerSessionsOutcome DescribePlayerSessions(Aws.GameLift.Server.Model.DescribePlayerSessionsRequest request)
        {
            log.DebugFormat("Describing player sessions {0}", request);
            var translation = DescribePlayerSessionsRequestMapper.ParseFromDescribePlayerSessionsRequest(request);
            
            var future = new TaskCompletionSource<DescribePlayerSessionsOutcome>();

            var ackFunction = CreateAckFunction(future);

            return EmitEvent(translation, ackFunction, future, DESCRIBE_PLAYER_SESSIONS_ERROR);
        }

        public StartMatchBackfillOutcome BackfillMatchmaking(Aws.GameLift.Server.Model.StartMatchBackfillRequest request)
        {
            var translation = BackfillDataMapper.CreateBufferedBackfillMatchmakingRequest(request);
            
            var future = new TaskCompletionSource<StartMatchBackfillOutcome>();

            var ackFunction = CreateAckFunction(future);

            return EmitEvent(translation, ackFunction, future, START_MATCH_BACKFILL_ERROR);
        }

        public GenericOutcome StopMatchmaking(Aws.GameLift.Server.Model.StopMatchBackfillRequest request)
        {
            var translation = BackfillDataMapper.CreateBufferedStopMatchmakingRequest(request);
            
            var future = new TaskCompletionSource<GenericOutcome>();

            var ackFunction = CreateAckFunction(future);

            return EmitEvent(translation, ackFunction, future, STOP_MATCH_BACKFILL_ERROR);
        }

        public GenericOutcome ReportHealth(bool healthStatus)
        {
            var rHealth = new ReportHealth
            {
                HealthStatus = healthStatus
            };

            return EmitEvent(rHealth);
        }

        AckImpl CreateAckFunction(TaskCompletionSource<GenericOutcome> future)
        {
            return new AckImpl((ack) =>
            {
                log.DebugFormat("Got ack {0}", ack);

                if(null == ack)
                {
                    future.TrySetResult(new GenericOutcome(new GameLiftError(GameLiftErrorType.SERVICE_CALL_FAILED)));
                    return;
                }

                var success = (bool)ack;

                if (success)
                {
                    future.TrySetResult(new GenericOutcome());
                }
                else
                {
                    future.TrySetResult(new GenericOutcome(new GameLiftError(GameLiftErrorType.SERVICE_CALL_FAILED)));
                }
            });
        }

        GenericOutcome EmitEvent(IMessage message)
        {
            var future = new TaskCompletionSource<GenericOutcome>();
            var ackFunction = CreateAckFunction(future);
            return EmitEvent(message, ackFunction, future, GENERIC_ERROR);
        }

        T EmitEvent<T>(IMessage message, IAck ackFunction, TaskCompletionSource<T> future, T error) where T : GenericOutcome
        {
            log.DebugFormat("Emitting event for message {0}", message);
            lock (emitLock)
            {
                socket.Emit(message.Descriptor.FullName, ackFunction, message.ToByteArray());
            }

            if (!future.Task.Wait(TimeSpan.FromSeconds(30)))
            {
                future.TrySetResult(error);
            }

            return future.Task.Result;
        }
    }
}

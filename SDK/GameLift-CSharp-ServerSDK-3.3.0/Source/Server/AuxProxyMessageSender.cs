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

using Quobject.SocketIoClientDotNet.Client;
using log4net;
using System;

namespace Aws.GameLift.Server
{
    public partial class AuxProxyMessageSender
    {
        static readonly ILog log = LogManager.GetLogger(typeof(AuxProxyMessageSender));
        static readonly GenericOutcome GENERIC_ERROR = new GenericOutcome(new GameLiftError(GameLiftErrorType.LOCAL_CONNECTION_FAILED));
        static readonly DescribePlayerSessionsOutcome DESCRIBE_PLAYER_SESSIONS_ERROR = new DescribePlayerSessionsOutcome(new GameLiftError(GameLiftErrorType.SERVICE_CALL_FAILED));
        static readonly StartMatchBackfillOutcome START_MATCH_BACKFILL_ERROR = new StartMatchBackfillOutcome(new GameLiftError(GameLiftErrorType.SERVICE_CALL_FAILED));
        static readonly GenericOutcome STOP_MATCH_BACKFILL_ERROR = new GenericOutcome(new GameLiftError(GameLiftErrorType.SERVICE_CALL_FAILED));

        Socket socket;
        private Object emitLock = new Object();

        public AuxProxyMessageSender(Socket socket)
        {
            this.socket = socket;
        }
    }
}

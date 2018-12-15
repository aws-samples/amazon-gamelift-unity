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

using Aws.GameLift.Server.Model;

namespace Aws.GameLift.Server
{
    public static partial class GameSessionParser
    {
        public static GameSession Parse(string rawGameSession)
        {
            log.DebugFormat("Parsing game session: '{0}'", rawGameSession);
            var deserialized = Com.Amazon.Whitewater.Auxproxy.Pbuffer.ActivateGameSession.Parser.ParseJson(rawGameSession as string);
            return GameSession.ParseFromBufferedGameSession(deserialized.GameSession);
        }
    }
}

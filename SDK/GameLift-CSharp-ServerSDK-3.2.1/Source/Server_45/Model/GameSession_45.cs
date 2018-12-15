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

namespace Aws.GameLift.Server.Model
{
    public partial class GameSession
    {
        public static GameSession ParseFromBufferedGameSession(Com.Amazon.Whitewater.Auxproxy.Pbuffer.GameSession gameSession)
        {
            var translation = new GameSession();

            translation.Name = gameSession.Name;
            translation.FleetId = gameSession.FleetId;
            translation.GameSessionId = gameSession.GameSessionId;
            translation.MaximumPlayerSessionCount = gameSession.MaxPlayers;
            translation.Port = gameSession.Port;
            translation.IpAddress = gameSession.IpAddress;
            translation.GameSessionData = gameSession.GameSessionData;
            translation.MatchmakerData = gameSession.MatchmakerData;
            translation.DnsName = gameSession.DnsName;

            foreach (var gameProperty in gameSession.GameProperties)
            {
                var translatedGameProperty = new GameProperty();

                translatedGameProperty.Key = gameProperty.Key;
                translatedGameProperty.Value = gameProperty.Value;

                translation.GameProperties.Add(translatedGameProperty);
            }

            return translation;
        }
    }
}

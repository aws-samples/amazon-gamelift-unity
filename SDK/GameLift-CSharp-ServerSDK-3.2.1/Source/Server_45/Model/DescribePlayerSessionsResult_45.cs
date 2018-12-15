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

using System.Collections.Generic;

namespace Aws.GameLift.Server.Model
{
	public partial class DescribePlayerSessionsResult
	{
        public static DescribePlayerSessionsResult ParseFromBufferedDescribePlayerSessionsResponse(Com.Amazon.Whitewater.Auxproxy.Pbuffer.DescribePlayerSessionsResponse response)
        {
            var translation = new DescribePlayerSessionsResult();

            translation.NextToken = response.NextToken;
            translation.PlayerSessions = new List<PlayerSession>();

            foreach (var playerSession in response.PlayerSessions)
            {
                var translatedPlayerSession = new PlayerSession();

                translatedPlayerSession.CreationTime = playerSession.CreationTime;
                translatedPlayerSession.FleetId = playerSession.FleetId;
                translatedPlayerSession.GameSessionId = playerSession.GameSessionId;
                translatedPlayerSession.IpAddress = playerSession.IpAddress;
                translatedPlayerSession.PlayerData = playerSession.PlayerData;
                translatedPlayerSession.PlayerId = playerSession.PlayerId;
                translatedPlayerSession.PlayerSessionId = playerSession.PlayerSessionId;
                translatedPlayerSession.Port = playerSession.Port;
                translatedPlayerSession.Status = PlayerSessionStatusMapper.GetPlayerSessionStatusForName(playerSession.Status);
                translatedPlayerSession.TerminationTime = playerSession.TerminationTime;
                translatedPlayerSession.DnsName = playerSession.DnsName;

                translation.AddPlayerSession(translatedPlayerSession);
            }

            return translation;
        }
	}
}


// AMAZON CONFIDENTIAL

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
using ProtoBuf;
using System.Collections.Generic;

namespace Aws.GameLift.Protobuf
{
    /*
     * Classes that implements IMessage are buffers we send from the SDK to AuxProxy.
     * Other classes are buffers the SDK receives from AuxProxy.
     */
    public abstract class IMessage
    {
        public string Tag { get; protected set; }
    }

    [ProtoContract]
    public class ProcessReady : IMessage
    {
        [ProtoMember(1)]
        public string[] LogPathsToUpload { get; set; }
        [ProtoMember(2)]
        public int Port { get; set; }

        public ProcessReady()
        {
            Tag = "com.amazon.whitewater.auxproxy.pbuffer.ProcessReady";
        }
    }

    [ProtoContract]
    public class ProcessEnding : IMessage
    {
        public ProcessEnding()
        {
            Tag = "com.amazon.whitewater.auxproxy.pbuffer.ProcessEnding";
        }
    }

    [ProtoContract]
    public class GameSessionActivate : IMessage
    {
        [ProtoMember(1)]
        public string GameSessionId { get; set; }
        [ProtoMember(2)]
        public int MaxPlayers { get; set; }

        public GameSessionActivate()
        {
            Tag = "com.amazon.whitewater.auxproxy.pbuffer.GameSessionActivate";
        }
    }

    [ProtoContract]
    public class BackfillMatchmakingRequest : IMessage
    {
        [ProtoMember(1)]
        public string TicketId { get; set; }
        [ProtoMember(2)]
        public string GameSessionArn { get; set; }
        [ProtoMember(3)]
        public string MatchmakingConfigurationArn { get; set; }
        [ProtoMember(4)]
        public Player[] Players { get; set; }

        public BackfillMatchmakingRequest()
        {
            Tag = "com.amazon.whitewater.auxproxy.pbuffer.BackfillMatchmakingRequest";
        }
    }

    [ProtoContract]
    public class BackfillMatchmakingResponse : IMessage
    {
        [ProtoMember(1)]
        public string TicketId { get; set; }
    }

    [ProtoContract]
    public class StopMatchmakingRequest : IMessage
    {
        [ProtoMember(1)]
        public string TicketId { get; set; }
        [ProtoMember(2)]
        public string GameSessionArn { get; set; }
        [ProtoMember(3)]
        public string MatchmakingConfigurationArn { get; set; }

        public StopMatchmakingRequest()
        {
            Tag = "com.amazon.whitewater.auxproxy.pbuffer.StopMatchmakingRequest";
        }
    }

    [ProtoContract]
    public class Player : IMessage
    {
        [ProtoMember(1)]
        public string PlayerId { get; set; }
        [ProtoMember(2)]
        public Dictionary<string, AttributeValue> PlayerAttributes { get; set; }
        [ProtoMember(3)]
        public string Team { get; set; }
        [ProtoMember(4)]
        public Dictionary<string, int> LatencyInMs { get; set; }
    }

    [ProtoContract]
    public class AttributeValue : IMessage
    {
        [ProtoMember(1)]
        public int type { get; set; }
        [ProtoMember(2)]
        public string S { get; set; }
        [ProtoMember(3)]
        public double N { get; set; }
        [ProtoMember(4)]
        public string[] SL { get; set; }
        [ProtoMember(5)]
        public Dictionary<string, double> SDM { get; set; }
    }

    [ProtoContract]
    public class GameSessionTerminate : IMessage
    {
        [ProtoMember(1)]
        public string GameSessionId { get; set; }

        public GameSessionTerminate()
        {
            Tag = "com.amazon.whitewater.auxproxy.pbuffer.GameSessionTerminate";
        }
    }

    [ProtoContract]
    public class UpdatePlayerSessionCreationPolicy : IMessage
    {
        [ProtoMember(1)]
        public string GameSessionId { get; set; }
        [ProtoMember(2)]
        public string NewPlayerSessionCreationPolicy { get; set; }

        public UpdatePlayerSessionCreationPolicy()
        {
            Tag = "com.amazon.whitewater.auxproxy.pbuffer.UpdatePlayerSessionCreationPolicy";
        }
    }

    [ProtoContract]
    public class AcceptPlayerSession : IMessage
    {
        [ProtoMember(1)]
        public string GameSessionId { get; set; }
        [ProtoMember(2)]
        public string PlayerSessionId { get; set; }

        public AcceptPlayerSession()
        {
            Tag = "com.amazon.whitewater.auxproxy.pbuffer.AcceptPlayerSession";
        }
    }

    [ProtoContract]
    public class RemovePlayerSession : IMessage
    {
        [ProtoMember(1)]
        public string GameSessionId { get; set; }
        [ProtoMember(2)]
        public string PlayerSessionId { get; set; }

        public RemovePlayerSession()
        {
            Tag = "com.amazon.whitewater.auxproxy.pbuffer.RemovePlayerSession";
        }
    }

    [ProtoContract]
    public class ReportHealth : IMessage
    {
        [ProtoMember(1, IsRequired = true)]
        public bool HealthStatus { get; set; }

        public ReportHealth()
        {
            Tag = "com.amazon.whitewater.auxproxy.pbuffer.ReportHealth";
        }
    }

    [ProtoContract]
    public class DescribePlayerSessionsRequest : IMessage
    {
        [ProtoMember(1)]
        public string GameSessionId { get; set; }
        [ProtoMember(2)]
        public string PlayerId { get; set; }
        [ProtoMember(3)]
        public string PlayerSessionId { get; set; }
        [ProtoMember(4)]
        public string PlayerSessionStatusFilter { get; set; }
        [ProtoMember(5)]
        public string NextToken { get; set; }
        [ProtoMember(6)]
        public int Limit { get; set; }

        public DescribePlayerSessionsRequest()
        {
            Tag = "com.amazon.whitewater.auxproxy.pbuffer.DescribePlayerSessionsRequest";
        }
    }

    [ProtoContract]
    public class ActivateGameSession
    {
        [ProtoMember(1)]
        public GameSession GameSession { get; set; }
    }

    [ProtoContract]
    public class TerminateProcess : IMessage
    {
        [ProtoMember(1)]
        public Int64 TerminationTime { get; set; }
    }

    [ProtoContract]
    public class UpdateGameSession : IMessage
    {
        [ProtoMember(1)]
        public GameSession GameSession { get; set; }
        [ProtoMember(2)]
        public string UpdateReason { get; set; }
        [ProtoMember(3)]
        public string BackfillTicketId { get; set; }
    }

    [ProtoContract]
    public class DescribePlayerSessionsResponse
    {
        [ProtoMember(1)]
        public string NextToken { get; set; }
        [ProtoMember(2)]
        public PlayerSession[] PlayerSessions { get; set; }
    }

    [ProtoContract]
    public class GameSession
    {
        [ProtoMember(1)]
        public string GameSessionId { get; set; }
        [ProtoMember(2)]
        public string FleetId { get; set; }
        [ProtoMember(3)]
        public string Name { get; set; }
        [ProtoMember(4)]
        public int MaxPlayers { get; set; }
        [ProtoMember(5)]
        public bool Joinable { get; set; }
        [ProtoMember(6)]
        public GameProperty[] GameProperties { get; set; }
        [ProtoMember(7)]
        public int Port { get; set; }
        [ProtoMember(8)]
        public string IpAddress { get; set; }
        [ProtoMember(9)]
        public string GameSessionData { get; set; }
        [ProtoMember(10)]
        public string MatchmakerData { get; set; }
        [ProtoMember(11)]
        public string DnsName { get; set; }
    }

    [ProtoContract]
    public class GameProperty
    {
        [ProtoMember(1)]
        public string Key { get; set; }
        [ProtoMember(2)]
        public string Value { get; set; }
    }

    [ProtoContract]
    public class PlayerSession
    {
        [ProtoMember(1)]
        public string PlayerSessionId { get; set; }
        [ProtoMember(2)]
        public string PlayerId { get; set; }
        [ProtoMember(3)]
        public string GameSessionId { get; set; }
        [ProtoMember(4)]
        public string FleetId { get; set; }
        [ProtoMember(5)]
        public string IpAddress { get; set; }
        [ProtoMember(6)]
        public string Status { get; set; }
        [ProtoMember(7)]
        public long CreationTime { get; set; }
        [ProtoMember(8)]
        public long TerminationTime { get; set; }
        [ProtoMember(9)]
        public int Port { get; set; }
        [ProtoMember(10)]
        public string PlayerData { get; set; }
        [ProtoMember(11)]
        public string DnsName { get; set; }
    }
}
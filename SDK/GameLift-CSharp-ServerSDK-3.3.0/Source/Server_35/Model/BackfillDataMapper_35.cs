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
    public static class BackfillDataMapper
    {
        public static Protobuf.BackfillMatchmakingRequest CreateBufferedBackfillMatchmakingRequest(
            StartMatchBackfillRequest request)
        {
            Protobuf.BackfillMatchmakingRequest translated = new Protobuf.BackfillMatchmakingRequest();

            translated.TicketId = request.TicketId;
            translated.GameSessionArn = request.GameSessionArn;
            translated.MatchmakingConfigurationArn = request.MatchmakingConfigurationArn;

            if (request.Players != null)
            {
                translated.Players = new Protobuf.Player[request.Players.Length];
                for (int i = 0; i < request.Players.Length; i++)
                {
                    translated.Players[i] = BackfillDataMapper.CreateBufferedPlayer(request.Players[i]);
                }
            }

            return translated;
        }

        public static Protobuf.AttributeValue CreateBufferedAttributeValue(AttributeValue attr)
        {
            Protobuf.AttributeValue translated = new Protobuf.AttributeValue();
            translated.type = (int)attr.attrType;

            switch (attr.attrType)
            {
                case AttributeValue.AttrType.STRING:
                    translated.S = attr.S;
                    break;

                case AttributeValue.AttrType.DOUBLE:
                    translated.N = attr.N;
                    break;

                case AttributeValue.AttrType.STRING_LIST:
                    translated.SL = new string[attr.SL.Length];
                    for (int i = 0; i < attr.SL.Length; i++)
                    {
                        translated.SL[i] = attr.SL[i];
                    }
                    break;

                case AttributeValue.AttrType.STRING_DOUBLE_MAP:
                    translated.SDM = new Dictionary<string, double>();
                    foreach (KeyValuePair<string, double> entry in attr.SDM)
                    {
                        translated.SDM.Add(entry.Key, entry.Value);
                    }
                    break;
            }

            return translated;
        }

        public static Protobuf.Player CreateBufferedPlayer(Player player)
        {
            var translation = new Protobuf.Player();

            translation.PlayerId = player.PlayerId;
            translation.Team = player.Team;

            if (player.LatencyInMS != null)
            {
                translation.LatencyInMs = new Dictionary<string, int>();
                foreach (KeyValuePair<string, int> entry in player.LatencyInMS)
                {
                    translation.LatencyInMs.Add(entry.Key, entry.Value);
                }
            }

            if (player.PlayerAttributes != null)
            {
                translation.PlayerAttributes = new Dictionary<string, Protobuf.AttributeValue>();
                foreach (KeyValuePair<string, AttributeValue> entry in player.PlayerAttributes)
                {
                    translation.PlayerAttributes.Add(entry.Key, BackfillDataMapper.CreateBufferedAttributeValue(entry.Value));
                }
            }

            return translation;
        }

        public static StartMatchBackfillResult ParseFromBufferedBackfillMatchmakingResponse(
            Protobuf.BackfillMatchmakingResponse response)
        {
            StartMatchBackfillResult result = new StartMatchBackfillResult();
            result.TicketId = response.TicketId;

            return result;
        }

        public static Protobuf.StopMatchmakingRequest CreateBufferedStopMatchmakingRequest(
            StopMatchBackfillRequest request)
        {
            Protobuf.StopMatchmakingRequest translated = new Protobuf.StopMatchmakingRequest();

            translated.TicketId = request.TicketId;
            translated.GameSessionArn = request.GameSessionArn;
            translated.MatchmakingConfigurationArn = request.MatchmakingConfigurationArn;

            return translated;
        }
    }
}

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

        public static Com.Amazon.Whitewater.Auxproxy.Pbuffer.AttributeValue CreateBufferedAttributeValue(AttributeValue attr)
        {
            Com.Amazon.Whitewater.Auxproxy.Pbuffer.AttributeValue translated =
                new Com.Amazon.Whitewater.Auxproxy.Pbuffer.AttributeValue();
            translated.Type = (int)attr.attrType;

            switch (attr.attrType)
            {
                case AttributeValue.AttrType.STRING:
                    translated.S = attr.S;
                    break;

                case AttributeValue.AttrType.DOUBLE:
                    translated.N = attr.N;
                    break;

                case AttributeValue.AttrType.STRING_LIST:
                    foreach (string str in attr.SL)
                    {
                        translated.SL.Add(str);
                    }
                    break;

                case AttributeValue.AttrType.STRING_DOUBLE_MAP:
                    foreach (KeyValuePair<string, double> entry in attr.SDM)
                    {
                        translated.SDM.Add(entry.Key, entry.Value);
                    }
                    break;
            }

            return translated;
        }

        public static Com.Amazon.Whitewater.Auxproxy.Pbuffer.Player CreateBufferedPlayer(Player player)
        {
            var translation = new Com.Amazon.Whitewater.Auxproxy.Pbuffer.Player();

            translation.PlayerId = player.PlayerId;
            translation.Team = player.Team;

            if (player.LatencyInMS != null)
            {
                foreach (KeyValuePair<string, int> entry in player.LatencyInMS)
                {
                    translation.LatencyInMs.Add(entry.Key, entry.Value);
                }
            }

            if (player.PlayerAttributes != null)
            {
                foreach (KeyValuePair<string, AttributeValue> entry in player.PlayerAttributes)
                {
                    translation.PlayerAttributes.Add(entry.Key, BackfillDataMapper.CreateBufferedAttributeValue(entry.Value));
                }
            }

            return translation;
        }

        public static Com.Amazon.Whitewater.Auxproxy.Pbuffer.BackfillMatchmakingRequest
            CreateBufferedBackfillMatchmakingRequest(StartMatchBackfillRequest request)
        {
            Com.Amazon.Whitewater.Auxproxy.Pbuffer.BackfillMatchmakingRequest translated =
                new Com.Amazon.Whitewater.Auxproxy.Pbuffer.BackfillMatchmakingRequest();

            translated.TicketId = request.TicketId;
            translated.GameSessionArn = request.GameSessionArn;
            translated.MatchmakingConfigurationArn = request.MatchmakingConfigurationArn;
            for (int i = 0; i < request.Players.Length; i++)
            {
                translated.Players.Add(BackfillDataMapper.CreateBufferedPlayer(request.Players[i]));
            }

            return translated;
        }

        public static StartMatchBackfillResult ParseFromBufferedBackfillMatchmakingResponse(
            Com.Amazon.Whitewater.Auxproxy.Pbuffer.BackfillMatchmakingResponse response)
        {
            StartMatchBackfillResult result = new StartMatchBackfillResult();
            result.TicketId = response.TicketId;

            return result;
        }

        public static Com.Amazon.Whitewater.Auxproxy.Pbuffer.StopMatchmakingRequest
            CreateBufferedStopMatchmakingRequest(StopMatchBackfillRequest request)
        {
            Com.Amazon.Whitewater.Auxproxy.Pbuffer.StopMatchmakingRequest translated =
                new Com.Amazon.Whitewater.Auxproxy.Pbuffer.StopMatchmakingRequest();

            translated.TicketId = request.TicketId;
            translated.GameSessionArn = request.GameSessionArn;
            translated.MatchmakingConfigurationArn = request.MatchmakingConfigurationArn;

            return translated;
        }
    }
}

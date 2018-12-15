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
    public static class DescribePlayerSessionsRequestMapper
    {
        public static Com.Amazon.Whitewater.Auxproxy.Pbuffer.DescribePlayerSessionsRequest ParseFromDescribePlayerSessionsRequest(DescribePlayerSessionsRequest request)
        {
            var pRequest = new Com.Amazon.Whitewater.Auxproxy.Pbuffer.DescribePlayerSessionsRequest();
            if (null != request.GameSessionId)
            {
                pRequest.GameSessionId = request.GameSessionId;
            }
            if(null != request.PlayerId)
            {
                pRequest.PlayerId = request.PlayerId;
            }
            if(null != request.PlayerSessionId)
            {
                pRequest.PlayerSessionId = request.PlayerSessionId;
            }
            if(null != request.PlayerSessionStatusFilter)
            {
                pRequest.PlayerSessionStatusFilter = request.PlayerSessionStatusFilter;
            }
            if (null != request.NextToken)
            {
                pRequest.NextToken = request.NextToken;
            }
            pRequest.Limit = request.Limit;
            return pRequest;
        }
    }
}


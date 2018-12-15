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
		static readonly int MAX_PLAYER_SESSIONS = 1024;

		public string NextToken { get; set; }
		public List<PlayerSession> PlayerSessions { get; set; }

		public DescribePlayerSessionsResult()
		{
			PlayerSessions = new List<PlayerSession>();
		}

		public void AddPlayerSession(PlayerSession value)
		{
			if (PlayerSessions.Count < MAX_PLAYER_SESSIONS)
			{
				PlayerSessions.Add(value);
			}
		}
	}
}


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
	public enum PlayerSessionStatus
	{
		NOT_SET,
		RESERVED,
		ACTIVE,
		COMPLETED,
		TIMEDOUT
	}

	public static class PlayerSessionStatusMapper
	{
		const string RESERVED = "RESERVED";
		const string ACTIVE = "ACTIVE";
		const string COMPLETED = "COMPLETED";
		const string TIMEDOUT = "TIMEDOUT";
		const string NOT_SET = "NOT_SET";

		public static PlayerSessionStatus GetPlayerSessionStatusForName(string name)
		{
			switch (name)
			{
				case RESERVED:
					return PlayerSessionStatus.RESERVED;
				case ACTIVE:
					return PlayerSessionStatus.ACTIVE;
				case COMPLETED:
					return PlayerSessionStatus.COMPLETED;
				case TIMEDOUT:
					return PlayerSessionStatus.TIMEDOUT;
				default:
					return PlayerSessionStatus.NOT_SET;
			}
		}

		public static string GetNameForPlayerSessionStatus(PlayerSessionStatus value)
		{
			switch (value)
			{
				case PlayerSessionStatus.RESERVED:
					return RESERVED;
				case PlayerSessionStatus.ACTIVE:
					return ACTIVE;
				case PlayerSessionStatus.COMPLETED:
					return COMPLETED;
				case PlayerSessionStatus.TIMEDOUT:
					return TIMEDOUT;
				default:
					return NOT_SET;
			}
		}
	}
}

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

namespace Aws.GameLift.Server.Model
{
    public enum PlayerSessionCreationPolicy
    {
        NOT_SET,
        ACCEPT_ALL,
        DENY_ALL
    }

    public static class PlayerSessionCreationPolicyMapper
    {
		const string ACCEPT_ALL = "ACCEPT_ALL";
		const string DENY_ALL = "DENY_ALL";
		const string NOT_SET = "NOT_SET";

        public static PlayerSessionCreationPolicy GetPlayerSessionCreationPolicyForName(string name)
        {
			switch (name)
			{
				case ACCEPT_ALL:
					return PlayerSessionCreationPolicy.ACCEPT_ALL;
				case DENY_ALL:
					return PlayerSessionCreationPolicy.DENY_ALL;
				default:
					return PlayerSessionCreationPolicy.NOT_SET;
			}
        }

        public static string GetNameForPlayerSessionCreationPolicy(PlayerSessionCreationPolicy value)
        {
            switch (value)
            {
                case PlayerSessionCreationPolicy.ACCEPT_ALL:
                    return ACCEPT_ALL;
                case PlayerSessionCreationPolicy.DENY_ALL:
                    return DENY_ALL;
                default:
                    return NOT_SET;
            }
        }
    }
}

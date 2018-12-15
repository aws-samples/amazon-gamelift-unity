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
    public class UpdateGameSession
    {
        public GameSession GameSession { get; private set; }
        public UpdateReason UpdateReason { get; private set; }
        public string BackfillTicketId { get; private set; }

        public UpdateGameSession(GameSession gameSession, UpdateReason updateReason, string backfillTicketId)
        {
            this.GameSession = gameSession;
            this.UpdateReason = updateReason;
            this.BackfillTicketId = backfillTicketId;
        }
    }
}

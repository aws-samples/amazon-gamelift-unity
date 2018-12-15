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
    public enum UpdateReason
    {
        MATCHMAKING_DATA_UPDATED,
        BACKFILL_FAILED,
        BACKFILL_TIMED_OUT,
        BACKFILL_CANCELLED,
        UNKNOWN
    }

    public static class UpdateReasonMapper
    {
        const string MATCHMAKING_DATA_UPDATED_REASON = "MATCHMAKING_DATA_UPDATED";
        const string BACKFILL_FAILED_REASON = "BACKFILL_FAILED";
        const string BACKFILL_TIMED_OUT_REASON = "BACKFILL_TIMED_OUT";
        const string BACKFILL_CANCELLED_REASON = "BACKFILL_CANCELLED";
        const string UNKNOWN_REASON = "UNKNOWN";

        public static UpdateReason GetUpdateReasonForName(string name)
        {
            switch (name)
            {
                case MATCHMAKING_DATA_UPDATED_REASON:
                    return UpdateReason.MATCHMAKING_DATA_UPDATED;
                case BACKFILL_FAILED_REASON:
                    return UpdateReason.BACKFILL_FAILED;
                case BACKFILL_TIMED_OUT_REASON:
                    return UpdateReason.BACKFILL_TIMED_OUT;
                case BACKFILL_CANCELLED_REASON:
                    return UpdateReason.BACKFILL_CANCELLED;
                default:
                    return UpdateReason.UNKNOWN;
            }
        }

        public static string GetNameForUpdateReason(UpdateReason value)
        {
            switch (value)
            {
                case UpdateReason.MATCHMAKING_DATA_UPDATED:
                    return MATCHMAKING_DATA_UPDATED_REASON;
                case UpdateReason.BACKFILL_FAILED:
                    return BACKFILL_FAILED_REASON;
                case UpdateReason.BACKFILL_TIMED_OUT:
                    return BACKFILL_TIMED_OUT_REASON;
                case UpdateReason.BACKFILL_CANCELLED:
                    return BACKFILL_CANCELLED_REASON;
                default:
                    return UNKNOWN_REASON;
            }
        }
    }
}

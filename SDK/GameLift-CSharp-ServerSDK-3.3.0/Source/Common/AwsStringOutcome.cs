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

namespace Aws.GameLift
{
    public class AwsStringOutcome : GenericOutcome
    {
        public string Result { get; set; }

        public AwsStringOutcome(string result) : base()
        {
            Result = result;
        }

        public AwsStringOutcome(GameLiftError error) : base(error)
        {
        }

        public AwsStringOutcome(GameLiftError error, string result) : base(error)
        {
            Result = result;
        }
    }
}

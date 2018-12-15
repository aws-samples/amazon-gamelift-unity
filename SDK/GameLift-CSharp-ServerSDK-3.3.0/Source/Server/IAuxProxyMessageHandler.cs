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
using Aws.GameLift.Server.Model;
using Quobject.SocketIoClientDotNet.Client;
namespace Aws.GameLift.Server
{
    public interface IAuxProxyMessageHandler
    {
        void OnStartGameSession(string rawGameSession, IAck ack);
        void OnUpdateGameSession(string rawGameSessionUpdate, IAck ack);
        void OnTerminateProcess(string rawTerminationTime);
    }
}

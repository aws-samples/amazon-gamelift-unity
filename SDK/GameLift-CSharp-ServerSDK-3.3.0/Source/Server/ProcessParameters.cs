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

using Aws.GameLift.Server.Model;

namespace Aws.GameLift.Server
{
    public class ProcessParameters
    {
        public delegate void OnStartGameSessionDelegate(GameSession gameSession);
        public delegate void OnUpdateGameSessionDelegate(UpdateGameSession updateGameSession);
        public delegate void OnProcessTerminateDelegate();
        public delegate bool OnHealthCheckDelegate();

        public OnStartGameSessionDelegate OnStartGameSession { get; set; }
        public OnUpdateGameSessionDelegate OnUpdateGameSession { get; set; }
        public OnProcessTerminateDelegate OnProcessTerminate { get; set; }
        public OnHealthCheckDelegate OnHealthCheck { get; set; }
        public int Port { get; set; }
        public LogParameters LogParameters { get; set; }

        public ProcessParameters()
        {
            Port = -1;
            OnHealthCheck = () => { return true; };
            LogParameters = new LogParameters ();
        }

        public ProcessParameters(OnStartGameSessionDelegate onStartGameSession,
                                OnUpdateGameSessionDelegate onUpdateGameSession,
                                OnProcessTerminateDelegate onProcessTerminate,
                                OnHealthCheckDelegate onHealthCheck,
                                int port,
                                LogParameters logParameters)
        {
            this.OnStartGameSession = onStartGameSession;
            this.OnUpdateGameSession = onUpdateGameSession;
            this.OnProcessTerminate = onProcessTerminate;
            this.OnHealthCheck = onHealthCheck;
            this.Port = port;
            this.LogParameters = logParameters;
        }

        public ProcessParameters(OnStartGameSessionDelegate onStartGameSession,
                                OnProcessTerminateDelegate onProcessTerminate,
                                OnHealthCheckDelegate onHealthCheck,
                                int port,
                                LogParameters logParameters)
        {
            this.OnStartGameSession = onStartGameSession;
            this.OnUpdateGameSession = (UpdateGameSession) => {};
            this.OnProcessTerminate = onProcessTerminate;
            this.OnHealthCheck = onHealthCheck;
            this.Port = port;
            this.LogParameters = logParameters;
        }
    }
}

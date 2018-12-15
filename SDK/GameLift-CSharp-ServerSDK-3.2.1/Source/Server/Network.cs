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
using System.Threading;
using Quobject.SocketIoClientDotNet.Client;
using Quobject.EngineIoClientDotNet.ComponentEmitter;
using log4net;

namespace Aws.GameLift.Server
{
    public class Network
    {
        Socket socketToAuxProxy;
        Socket socketFromAuxProxy;
        IAuxProxyMessageHandler handler;
        ManualResetEvent connected = new ManualResetEvent(false);

        static readonly ILog log = LogManager.GetLogger(typeof(Network));

        public Network(Socket socketToAuxProxy, Socket socketFromAuxProxy, IAuxProxyMessageHandler handler)
        {
            this.socketToAuxProxy = socketToAuxProxy;
            this.socketFromAuxProxy = socketFromAuxProxy;
            this.handler = handler;

            SetHandlerCallbacks(socketToAuxProxy);
            SetHandlerCallbacks(socketFromAuxProxy);
        }

        void SetHandlerCallbacks(Socket socket)
        {
            socket.On(Socket.EVENT_CONNECT, () =>
                {
                    log.Debug("Socket.io event triggered: EVENT_CONNECT");
                    connected.Set();
                });

            socket.On(Socket.EVENT_CONNECT_ERROR, (e) =>
                {
                    log.DebugFormat("Socket.io event triggered: EVENT_CONNECT_ERROR, with error: {0}", e);
                });

            socket.On(Socket.EVENT_ERROR, (e) =>
                {
                    log.DebugFormat("Socket.io event triggered: EVENT_ERROR, with error: {0}", e);
                });

            socket.On(Socket.EVENT_DISCONNECT, () =>
                {
                    log.Debug("Socket.io event triggered: EVENT_DISCONNECT");

                });

            socket.On(Socket.EVENT_CONNECT_TIMEOUT, () =>
                {
                    log.Debug("Socket.io event triggered: EVENT_CONNECT_TIMEOUT");

                });

            socket.On(Socket.EVENT_MESSAGE, (e) =>
                {
                    log.DebugFormat("Socket.io event triggered: EVENT_MESSAGE, with error: {0}", e);
                });

            socket.On("StartGameSession", new ServerStateListener((data, cb) =>
            {
                handler.OnStartGameSession(data as string, cb as IAck);
            }));

            socket.On("TerminateProcess", new ServerStateListener((data, cb) =>
            {
                handler.OnTerminateProcess(data as string);
            }));

            socket.On("UpdateGameSession", new ServerStateListener((data, cb) =>
            {
                handler.OnUpdateGameSession(data as string, cb as IAck);
            }));
        }

        public GenericOutcome Connect()
        {
            if (PerformConnect(socketToAuxProxy))
            {
                if (PerformConnect(socketFromAuxProxy))
                {
                    //Success
                    return new GenericOutcome();
                }
            }

            return new GenericOutcome(new GameLiftError(GameLiftErrorType.LOCAL_CONNECTION_FAILED));
        }

        public bool PerformConnect(Socket socket)
        {
            connected.Reset();
            socket.Connect();

            //Waits up to 5 seconds for the connection to complete.
            if (connected.WaitOne(5000))
            {
                return true;
            }

            return false;
        }

        public GenericOutcome Disconnect()
        {
            socketToAuxProxy.Close();
            socketFromAuxProxy.Close();

            return new GenericOutcome();
        }
    }

    public class ServerStateListener : IListener
    {
        static int IdCounter = 0;
        int Id;
        readonly Action<object, object> Fn;

        public ServerStateListener(Action<object, object> fn)
        {
            this.Fn = fn;
            this.Id = Interlocked.Increment(ref IdCounter);
        }

        public void Call(params object[] args)
        {
            var arg1 = args.Length > 0 ? args[0] : null;
            var arg2 = args.Length > 1 ? args[1] : null;

            Fn(arg1, arg2);
        }

        public int CompareTo(IListener other)
        {
            return this.GetId().CompareTo(other.GetId());
        }

        public int GetId()
        {
            return Id;
        }
    }
}

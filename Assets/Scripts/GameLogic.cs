// Copyright 2018 Amazon
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameLogic : MonoBehaviour
{
    // game logic classes
    private Input input;

    private Simulation simulation;
    public Render render;
#if SERVER
    private NetworkServer server;
#endif
#if CLIENT
    private NetworkClient client;
#endif
    public Status status;
    public Log log;
    public GameLift gamelift;

    public void Awake()
    {
        Debug.Log(":) GAMELOGIC AWAKE");
        // prevent the game going to sleep when the window loses focus
        Application.runInBackground = true;
        // Just 60 frames per second is enough
        Application.targetFrameRate = 60;

        // Get pointers to scripts on other objects
        GameObject gameliftObj = GameObject.Find("/GameLiftStatic");
        Debug.Assert(gameliftObj != null);
        gamelift = gameliftObj.GetComponent<GameLift>();
        if (gamelift == null) Debug.Log(":| GAMELIFT CODE NOT AVAILABLE ON GAMELIFTSTATIC OBJECT - ONLY LOCAL SERVER AND OFFLINE CONNECTION WILL WORK");
    }

    public void Start()
    {
        Debug.Log(":) GAMELOGIC START");
        // create owned objects
        log = new Log(this);
        input = new Input(this);
        status = new Status(this);
        simulation = new Simulation(this);
        render = new Render();
#if SERVER
        server = new NetworkServer(this);
#endif
#if CLIENT
        client = new NetworkClient(this);
#endif
        // Start Game
        input.Start();
        status.Start();
        simulation.ResetBoard();
        simulation.ResetScores();
#if SERVER
        server.TransmitState(); // if I am the server, I will send my state to all the clients so they have the same board and RNG state as I do
#endif
        log.WriteLine("HELLO WORLD!");
        render.SetMessage("HELLO WORLD!");
        ResetMessage(60);
        Render();
    }

    public void Update()
    {
        input.Update();
        Attract();
        Frame++;
#if SERVER
        server.Update();
#endif
#if CLIENT
        client.Update();
#endif
    }

    public void OnApplicationQuit()
    {
        Debug.Log("Application received quit signal");
#if SERVER
        server.Disconnect();
#endif
#if CLIENT
        client.Disconnect();
#endif
    }

    private void Attract()
    {
        if (!Playing && Authoritative && Frame % 60 == 0)
        {
            simulation.ResetBoard(); // randomize the board
#if SERVER
            server.TransmitState(); // if I am the server, I will send my state to all the clients so they have the same board and RNG state as I do
#endif
            Render();
        }
    }

    public void InputEvent(int playerIdx, Chord inputChord)
    {
#if CLIENT
        client.TransmitInput(playerIdx, inputChord);
#endif
        if (simulation.SimulateOnInput(playerIdx, inputChord))
        {
            Render();
#if SERVER
            server.TransmitState();
#endif
        }
    }

    public void ShowHighlight(int _keyIdx)
    {
        render.ShowHighlight(_keyIdx);
    }

    public void HideHighlight(int _keyIdx)
    {
        render.HideHighlight(_keyIdx);
    }

    public void Render()
    {
        render.RenderBoard(simulation, status);
    }

    public void ResetScore(int playerIdx)
    {
        simulation.ResetScore(playerIdx);
    }

    public void ZeroScore(int playerIdx)
    {
        simulation.ZeroScore(playerIdx);
    }

    // Is the specified player in the current game?
    public bool IsConnected(int playerIdx)
    {
#if SERVER
        return server.IsConnected(playerIdx);
#else
        if (playerIdx == 0) return true;
        return false;
#endif
    }

    // We pressed RETURN and we are ready to start the game
    public void Ready()
    {
#if CLIENT
        if (Authoritative)
        {
            StartGame(); // single player start
            Render();
        }
        else
            client.Ready();
#endif
    }

    public void StartGame()
    {
        simulation.ResetBoard();
        simulation.ResetScores();
        simulation.playing = true;
        log.WriteLine("GO");
        render.SetMessage("GO");
        FlashMessage(4, 4);
#if SERVER
        server.TransmitState();
#endif
        Render();
    }

    public void End()
    {
#if CLIENT
        if (Authoritative)
            EndGame(); // single player end
        else
            client.End();
#endif
    }

    public void EndGame()
    {
        simulation.playing = false;
        log.WriteLine("GAME OVER");
        render.SetMessage("GAME OVER");
        FlashMessage(10, 1);
#if SERVER
        server.TransmitState();
#endif
    }

    public string GetState(int playerIdx)
    {
        return simulation.Serialize(playerIdx);
    }

    public void SetState(string state)
    {
        bool priorPlayingState = simulation.Deserialize(state);
        if (priorPlayingState == false && simulation.playing == true) StartGame();
        if (priorPlayingState == true && simulation.playing == false) EndGame();
        Render();
    }

    public void TransmitLog()
    {
#if SERVER
        server.TransmitLog("THIS IS A TEST");
#endif
    }

    public ulong Frame
    {
        set
        {
            simulation.frame = value;
        }

        get
        {
            return simulation.frame;
        }
    }

    public int PlayerIdx
    {
        get
        {
            return simulation.playerIdx;
        }
    }

    public bool Playing
    {
        get
        {
            return simulation.playing;
        }
    }

    public bool Authoritative
    {
        get
        {
#if SERVER
            return true;
#else
            return client.Authoritative;
#endif
        }
    }

    public bool GameliftStatus
    {
        set
        {
            status.GameliftStatus = value;
        }

        get
        {
            return status.GameliftStatus;
        }
    }

    public void ResetMessage(int _timeSeconds)
    {
        StartCoroutine(render.ResetMessage(_timeSeconds));
    }

    public void FlashMessage(int _timeSeconds, int _rate)
    {
        StartCoroutine(render.FlashMessage(_timeSeconds, _rate));
    }
}

public class Log
{
    private GameLogic gl;

    public Log(GameLogic _gl)
    {
        gl = _gl;
    }

    public void WriteLine(string line)
    {
#if SERVER
        string me = "SERVER";
#else
        string me = "CLIENT " + (gl.PlayerIdx);
#endif
        Debug.Log(me + ": Frame: " + gl.Frame + " " + line);
    }
}

public class Chord
{
    public bool[] keys = new bool[9];
    private bool chordChanged = false;

    public void Reset()
    {
        for (int keyIdx = 0; keyIdx < keys.Length; keyIdx++)
        {
            keys[keyIdx] = false;
        }

        chordChanged = false;
    }

    public void Set(int keyIdx)
    {
        keys[keyIdx] = true;
        chordChanged = true;
    }

    public bool IsChanged()
    {
        return chordChanged;
    }

    public string Serialize()
    {
        var json = JsonUtility.ToJson(this);
        return json;
    }

    public void Deserialize(string json)
    {
        if (!string.IsNullOrEmpty(json))
        {
            JsonUtility.FromJsonOverwrite(json, this);
        }
    }

    public static Chord CreateFromSerial(string json)
    {
        Chord temp = new Chord();
        temp.Deserialize(json);
        return temp;
    }
}

public class Input
{
    // record what keys are in the current move that we are making
    private Chord chord = new Chord();
    private GameLogic gl;

    private Dictionary<KeyCode, int> keys = new Dictionary<KeyCode, int> {
      {KeyCode.Keypad1,0},
      {KeyCode.Keypad2,1},
      {KeyCode.Keypad3,2},
      {KeyCode.Keypad4,3},
      {KeyCode.Keypad5,4},
      {KeyCode.Keypad6,5},
      {KeyCode.Keypad7,6},
      {KeyCode.Keypad8,7},
      {KeyCode.Keypad9,8},
      {KeyCode.N,0},
      {KeyCode.M,1},
      {KeyCode.Comma,2},
      {KeyCode.H,3},
      {KeyCode.J,4},
      {KeyCode.K,5},
      {KeyCode.Y,6},
      {KeyCode.U,7},
      {KeyCode.I,8}
    };

    public Input(GameLogic _gl)
    {
        gl = _gl;
    }

    public void Start()
    {
        chord.Reset();
        for (int keyIdx = 0; keyIdx < 9; keyIdx++)
            gl.HideHighlight(keyIdx);
    }

    public void Update()
    {
        if (gl.Playing)
        {
            // quit?
            if (UnityEngine.Input.GetKeyUp(KeyCode.Escape))
            {
                gl.End();
                return;
            }

            // game move
            bool released = true;
            foreach (KeyValuePair<KeyCode, int> kv in keys)
            {
                if (UnityEngine.Input.GetKey(kv.Key))
                {
                    chord.Set(kv.Value);
                    gl.ShowHighlight(kv.Value);
                    released = false;
                }
            }

            if (released)
            {
                if (chord.IsChanged())
                {
                    // all keys are released, chord is complete
                    gl.InputEvent(gl.PlayerIdx, chord);
                    Start();
                }
            }
        }
        else
        {
            if (UnityEngine.Input.GetKeyUp(KeyCode.Return))
            {
                gl.Ready();
                return;
            }
        }
    }
}

// local state, such as whether we are connected, used for text display
public class Status
{
    private bool gameliftStatus = false;
    private int numConnected = 0;
    private int playerIdx = 0;
    private bool connected = false;
    private GameLogic gl;

    public Status(GameLogic _gl)
    {
        gl = _gl;
    }

    public void Start()
    {
        SetStatusText();
    }

    public bool GameliftStatus
    {
        set
        {
            if (gameliftStatus == value) return;
            gameliftStatus = value;
            SetStatusText();
        }

        get
        {
            return gameliftStatus;
        }
    }

    public int NumConnected
    {
        set
        {
            if (numConnected == value) return;
            numConnected = value;
            SetStatusText();
        }
    }

    public int PlayerIdx
    {
        set
        {
            if (playerIdx == value) return;
            playerIdx = value;
            SetStatusText();
        }
    }

    public bool Connected
    {
        set
        {
            if (connected == value) return;
            connected = value;
            SetStatusText();
        }
    }

    public void SetStatusText()
    {
        string glt = gameliftStatus ? "GAMELIFT | " : "LOCAL | ";
#if SERVER
        string svr = "SERVER | ";
    string con = numConnected + " CONNECTED";
#else
        string svr = "CLIENT | ";
        string con = connected ? "CONNECTED " + (playerIdx + 1) + "UP" : "DISCONNECTED";
#endif
        gl.render.SetStatusText(svr + glt + con);
    }
}

// Logic to initialize the board, receive the chords, determine matches, record scores, repopulate the board after a match, send the board for rendering, and send or receive the state of the class from the network code.
public class Simulation
{
    public int playerIdx = 0;
    public int[] boardColors = new int[9];
    public int[] scores = new int[4];
    public UnityEngine.Random.State rngState;
    public bool playing = false;
    public ulong frame = 0;
    // private: not serialized
    private GameLogic gl;

    public Simulation(GameLogic _gl)
    {
        gl = _gl;
    }

    public void ResetBoard()
    {
        if (gl.Authoritative)
        {
            gl.log.WriteLine("ResetBoard()");
            playerIdx = 0;
            for (int bcNum = 0; bcNum < boardColors.Length; bcNum++)
            {
                boardColors[bcNum] = UnityEngine.Random.Range(0, 7);
            }
        }
    }

    public void ResetScores()
    {
        if (gl.Authoritative)
        {
            for (int scNum = 0; scNum < scores.Length; scNum++)
            {
                scores[scNum] = gl.IsConnected(scNum) ? 0 : -1;
            }
        }
    }

    public void ResetScore(int _playerIdx)
    {
        scores[_playerIdx] = -1;
    }

    public void ZeroScore(int _playerIdx)
    {
        scores[_playerIdx] = 0;
    }

    public bool SimulateOnInput(int playerIdx, Chord inputChord)
    {
        gl.log.WriteLine("SimulateOnInput()");
        Debug.Assert(inputChord.keys.Length == boardColors.Length);
        // test for a match
        bool match = false;
        int matchColor = -1; // don't know yet
        for (int bcNum = 0; bcNum < boardColors.Length; bcNum++)
        {
            if (inputChord.keys[bcNum])
            {
                if (matchColor == -1)
                {
                    matchColor = boardColors[bcNum];
                }
                else
                {
                    if (boardColors[bcNum] == matchColor)
                        match = true;
                    else
                    {
                        match = false;
                        break;
                    }
                }
            }
        }

        if (match)
        {
            // yes, a match!
            for (int bcNum = 0; bcNum < boardColors.Length; bcNum++)
            {
                if (inputChord.keys[bcNum])
                {
                    boardColors[bcNum] = UnityEngine.Random.Range(0, 7);
                    scores[playerIdx]++;
                }
            }
        }

        return match;
    }

    public string Serialize(int _playerIdx)
    {
        playerIdx = _playerIdx;
        rngState = UnityEngine.Random.state;
        var json = JsonUtility.ToJson(this);
        return json;
    }

    public bool Deserialize(string json)
    {
        bool priorPlayingState = playing;
        if (!string.IsNullOrEmpty(json))
        {
            JsonUtility.FromJsonOverwrite(json, this);
            UnityEngine.Random.state = rngState;
            gl.status.PlayerIdx = playerIdx;
        }

        return priorPlayingState;
    }
}

public class Render
{
    private GameObject[] buttons = new GameObject[9];
    private GameObject[] highlights = new GameObject[9];
    private Material[] materials = new Material[8];
    private Text scoreText;
    private Text statusText;
    private Text msgText;

    public Render()
    {
        for (int butNum = 1; butNum <= buttons.Length; butNum++)
        {
            buttons[butNum - 1] = GameObject.Find("/Button" + butNum); // array index is one less than the keypad number it correlates to
            Debug.Assert(buttons[butNum - 1] != null); // test our button was found (debug only)
        }

        for (int hlNum = 1; hlNum <= highlights.Length; hlNum++)
        {
            highlights[hlNum - 1] = GameObject.Find("/Highlight" + hlNum); // array index is one less than the keypad number it correlates to
            Debug.Assert(highlights[hlNum - 1] != null); // test our highlight was found (debug only)
        }

        // Materials are not all active so we have to load them
        for (int matNum = 1; matNum <= materials.Length; matNum++)
        {
            materials[matNum - 1] = Resources.Load("Materials/Color" + matNum.ToString().PadLeft(3, '0'), typeof(Material)) as Material;
            Debug.Assert(materials[matNum - 1] != null);
        }

        GameObject score = GameObject.Find("/Canvas/Score");
        scoreText = score.GetComponent<Text>();
        GameObject status = GameObject.Find("/Canvas/Status");
        statusText = status.GetComponent<Text>();
        GameObject msg = GameObject.Find("/Canvas/MainMessage");
        msgText = msg.GetComponent<Text>();
    }

    public void ShowHighlight(int keyNum)
    {
        highlights[keyNum].SetActive(true);
    }

    public void HideHighlight(int keyNum)
    {
        highlights[keyNum].SetActive(false);
    }

    public void SetButtonColor(int butNum, int matNum)
    {
        Debug.Assert(butNum < buttons.Length);
        Debug.Assert(matNum < materials.Length);
        Renderer rend = buttons[butNum].GetComponent<Renderer>();
        rend.material = materials[matNum];
    }

    private void SetScoreText(int[] scores)
    {
        scoreText.text = "1UP " + (scores[0] < 0 ? "---" : scores[0].ToString().PadLeft(3, '0'));
        scoreText.text += "      2UP " + (scores[1] < 0 ? "---" : scores[1].ToString().PadLeft(3, '0'));
        scoreText.text += "      3UP " + (scores[2] < 0 ? "---" : scores[2].ToString().PadLeft(3, '0'));
        scoreText.text += "      4UP " + (scores[3] < 0 ? "---" : scores[3].ToString().PadLeft(3, '0'));
    }

    public void RenderBoard(Simulation _state, Status _status)
    {
        for (int bcNum = 0; bcNum < _state.boardColors.Length; bcNum++)
        {
            SetButtonColor(bcNum, _state.boardColors[bcNum]);
        }

        SetScoreText(_state.scores);
    }

    public void SetStatusText(string _text)
    {
        statusText.text = _text;
    }

    public void SetMessage(string _msg)
    {
        msgText.text = _msg;
    }

    internal IEnumerator ResetMessage(int _time)
    {
        string text = msgText.text;
        yield return new WaitForSeconds(_time);
        if (msgText.text == text) SetMessage("");
    }

    internal IEnumerator FlashMessage(int _time, int _rate)
    {
        string text = msgText.text;
        for (int i = 0; i < _time * _rate; i++)
        {
            SetMessage(text);
            yield return new WaitForSeconds(0.5f / _rate);
            if (msgText.text != text) break;
            SetMessage("");
            yield return new WaitForSeconds(0.5f / _rate);
            if (msgText.text != "") break;
        }
    }
}

public class NetworkProtocol
{
    public static string[] Receive(TcpClient client)
    {
        NetworkStream stream = client.GetStream();
        var messages = new List<string>();
        while (stream.DataAvailable)
        {
            byte[] bufferLength = new byte[4];
            stream.Read(bufferLength, 0, bufferLength.Length);
            int msgSize = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(bufferLength, 0));
            byte[] readBuffer = new Byte[msgSize];
            stream.Read(readBuffer, 0, readBuffer.Length);
            string msgStr = System.Text.Encoding.ASCII.GetString(readBuffer, 0, readBuffer.Length);
            messages.Add(msgStr);
        }

        return messages.ToArray();
    }

    public static void Send(TcpClient client, string msgStr)
    {
        if (client == null) return;
        NetworkStream stream = client.GetStream();
        byte[] writeBuffer = System.Text.Encoding.ASCII.GetBytes(msgStr);
        int msgSize = writeBuffer.Length;
        byte[] bufferLength = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(msgSize));
        stream.Write(bufferLength, 0, bufferLength.Length);
        stream.Write(writeBuffer, 0, msgSize);
    }
}

#if SERVER
public class NetworkServer
{
    private GameLogic gl;
    private TcpListener listener;
    private TcpClient[] clients = { null, null, null, null };
    private bool[] ready = { false, false, false, false };

    public NetworkServer(GameLogic _gl)
    {
        gl = _gl;
        listener = new TcpListener(IPAddress.Any, 1935);
        listener.Start();
    }

    public void Update()
    {
        // Are there any new connections pending?
        if (listener.Pending())
        {
            TcpClient client = listener.AcceptTcpClient();
            for (int x = 0; x < 4; x++)
            {
                if (clients[x] == null)
                {
                    clients[x] = client;
                    UpdateNumConnected();
                    gl.log.WriteLine("Connection accepted: playerIdx " + x + " joined");
                    return;
                }
            }

            // game already full, reject the connection
            gl.log.WriteLine("Connection rejected: game already full.");
            try
            {
                NetworkProtocol.Send(client, "REJECTED: game already full");
            }
            catch (SocketException) { }
        }

        // Have we received an input event message from any client?
        for (int x = 0; x < 4; x++)
        {
            if (clients[x] == null) continue;
            var messages = NetworkProtocol.Receive(clients[x]);
            foreach (string msgStr in messages)
            {
                gl.log.WriteLine("Msg rcvd from playerIdx " + x + " Msg: " + msgStr);
                HandleMessage(x, msgStr);
            }
        }
    }

    public void Disconnect()
    {
        // warn clients
        TransmitMessage("DISCONNECT:");
        // disconnect connections
        for (int x = 0; x < 4; x++) HandleDisconnect(x);
        // end listener
        listener.Stop();
        // warn GameLift
        if (gl.gamelift != null && gl.GameliftStatus) gl.gamelift.TerminateGameSession(true);
        // process is terminating so no other state cleanup required
    }

    public void TransmitLog(string msgStr)
    {
        TransmitMessage("LOG:" + msgStr);
    }

    public void TransmitState()
    {
        // update the state of all players
        for (int x = 0; x < 4; x++)
        {
            string msgStr = "STATE:" + gl.GetState(x);
            try
            {
                NetworkProtocol.Send(clients[x], msgStr);
            }
            catch (SocketException e)
            {
                HandleDisconnect(x);
                gl.log.WriteLine("TransmitState failed: Disconnected. " + e);
            }
        }
    }

    private void TransmitMessage(string msgStr)
    {
        // send the same message to all players
        for (int x = 0; x < 4; x++)
        {
            try
            {
                NetworkProtocol.Send(clients[x], msgStr);
            }
            catch (SocketException e)
            {
                HandleDisconnect(x);
                gl.log.WriteLine("TransmitMessage failed: Disconnected. " + e);
            }
        }
    }

    private void HandleMessage(int playerIdx, string msgStr)
    {
        // parse message and pass json string to relevant handler for deserialization
        gl.log.WriteLine("Msg rcvd from player " + playerIdx + ":" + msgStr);
        string delimiter = ":";
        string json = msgStr.Substring(msgStr.IndexOf(delimiter) + delimiter.Length);
        if (msgStr[0] == 'C') HandleConnect(playerIdx, json);
        if (msgStr[0] == 'R') HandleReady(playerIdx);
        if (msgStr[0] == 'I') HandleInput(playerIdx, json);
        if (msgStr[0] == 'E') HandleEnd();
        if (msgStr[0] == 'D') HandleDisconnect(playerIdx);
    }

    private void HandleConnect(int playerIdx, string json)
    {
        // respond with the player id and the current state.
        gl.log.WriteLine("CONNECT:" + json);
        if (gl.gamelift != null && gl.GameliftStatus)
            if (gl.gamelift.ConnectPlayer(playerIdx, json) == false)
                DisconnectPlayer(playerIdx);
        gl.ZeroScore(playerIdx);
        TransmitState();
    }

    private void HandleReady(int playerIdx)
    {
        // start the game once all connected clients have requested to start (RETURN key)
        gl.log.WriteLine("READY:");
        ready[playerIdx] = true;
        for (int x = 0; x < 4; x++)
        {
            if (clients[x] != null && ready[x] == false) return; // a client is not ready
        }

        gl.StartGame();
    }

    private void HandleInput(int playerIdx, string json)
    {
        // simulate the input then respond to all players with the current state
        gl.log.WriteLine("INPUT:" + json);
        Chord inputChord = Chord.CreateFromSerial(json);
        gl.InputEvent(playerIdx, inputChord);
    }

    private void HandleEnd()
    {
        // end the game at the request of any client (ESC key)
        gl.log.WriteLine("END:");
        for (int x = 0; x < 4; x++)
        {
            ready[x] = false; // all clients end now.
        }

        gl.EndGame();
        if (gl.gamelift != null && gl.GameliftStatus)
            gl.gamelift.TerminateGameSession(false);
    }

    private void HandleDisconnect(int playerIdx)
    {
        DisconnectPlayer(playerIdx);
        // if that was the last client to leave, then end the game.
        for (int x = 0; x < 4; x++)
        {
            if (clients[x] != null) return; // a client is still attached
        }

        HandleEnd();
    }

    private void DisconnectPlayer(int playerIdx)
    {
        gl.log.WriteLine("DISCONNECT: Player " + playerIdx);
        // remove the client and close the connection
        var client = clients[playerIdx];
        if (client != null)
        {
            NetworkStream stream = client.GetStream();
            stream.Close();
            client.Close();
            clients[playerIdx] = null;
        }

        // remove the player session from GameLift
        gl.gamelift.DisconnectPlayer(playerIdx);
        // clean up the game state
        gl.ResetScore(playerIdx);
        UpdateNumConnected();
    }

    public bool IsConnected(int playerIdx)
    {
        return clients[playerIdx] != null;
    }

    private void UpdateNumConnected()
    {
        int count = 0;
        for (int x = 0; x < 4; x++)
        {
            if (clients[x] != null) count++;
        }

        gl.status.NumConnected = count;
    }
}

#endif

#if CLIENT
public class NetworkClient
{
    private GameLogic gl;
    private TcpClient client = null;

    public NetworkClient(GameLogic _gl)
    {
        gl = _gl;
        Connect();
    }

    public void Update()
    {
        if (client == null) return;
        var messages = NetworkProtocol.Receive(client);
        foreach (string msgStr in messages)
        {
            gl.log.WriteLine("Msg rcvd: " + msgStr);
            HandleMessage(msgStr);
        }
    }

    private bool TryConnect(string ip, int port, string auth)
    {
        try
        {
            client = new TcpClient(ip, port);
            string msgStr = "CONNECT:" + auth;
            NetworkProtocol.Send(client, msgStr);
            gl.status.Connected = true;
            return true;
        }
        catch (ArgumentNullException e)
        {
            client = null;
            gl.status.Connected = false;
            gl.log.WriteLine(":( CONNECT TO SERVER " + ip + " FAILED: " + e);
            return false;
        }
        catch (SocketException e) // server not available
        {
            client = null;
            if (ip == "127.0.0.1")
                gl.log.WriteLine(":) CONNECT TO LOCAL SERVER FAILED: PROBABLY NO LOCAL SERVER RUNNING, TRYING GAMELIFT");
            else
                gl.log.WriteLine(":( CONNECT TO SERVER " + ip + "FAILED: " + e + " (ARE YOU ON THE *AMAZON*INTERNAL*NETWORK*?)");
            gl.status.Connected = false;
            return false;
        }
    }

    private void Connect()
    {
        // try to connect to a local server
        if (TryConnect("127.0.0.1", 1935, "") == false)
        {
            // try to connect to gamelift
            if (gl.gamelift)
            {
                string ip = null;
                int port = -1;
                string auth = null;
                gl.gamelift.GetConnectionInfo(ref ip, ref port, ref auth); // sets GameliftStatus
                if (gl.GameliftStatus) TryConnect(ip, port, auth);
            }
        }
    }

    public bool Authoritative
    {
        get
        {
            return client == null;
        }
    }

    public void Ready()
    {
        if (client == null) return;
        string msgStr = "READY:";
        try
        {
            NetworkProtocol.Send(client, msgStr);
        }
        catch (SocketException e)
        {
            HandleDisconnect();
            gl.log.WriteLine("Ready failed: Disconnected" + e);
        }
    }

    public void TransmitInput(int playerIdx, Chord chord)
    {
        if (client == null) return;
        string msgStr = "INPUT:" + chord.Serialize();
        try
        {
            NetworkProtocol.Send(client, msgStr);
        }
        catch (SocketException e)
        {
            HandleDisconnect();
            gl.log.WriteLine("TransmitInput failed: Disconnected" + e);
        }
    }

    public void End()
    {
        if (client == null) return;
        string msgStr = "END:";
        try
        {
            NetworkProtocol.Send(client, msgStr);
        }
        catch (SocketException e)
        {
            HandleDisconnect();
            gl.log.WriteLine("End failed: Disconnected" + e);
        }
    }

    public void Disconnect()
    {
        if (client == null) return;
        string msgStr = "DISCONNECT:";
        try
        {
            NetworkProtocol.Send(client, msgStr);
        }

        finally
        {
            HandleDisconnect();
        }
    }

    private void HandleMessage(string msgStr)
    {
        // parse message and pass json string to relevant handler for deserialization
        //gl.log.WriteLine("Msg rcvd:" + msgStr);
        string delimiter = ":";
        string json = msgStr.Substring(msgStr.IndexOf(delimiter) + delimiter.Length);
        if (msgStr[0] == 'S') HandleState(json);
        if (msgStr[0] == 'L') HandleLog(json);
        if (msgStr[0] == 'R') HandleReject();
        if (msgStr[0] == 'D') HandleDisconnect();
    }

    private void HandleState(string msgStr)
    {
        gl.SetState(msgStr);
    }

    private void HandleLog(string msgStr)
    {
        gl.log.WriteLine(msgStr);
    }

    private void HandleReject()
    {
        gl.log.WriteLine(":( CONNECT TO SERVER REJECTED: game already full");
        NetworkStream stream = client.GetStream();
        stream.Close();
        client.Close();
        client = null;
        gl.status.Connected = false;
    }

    private void HandleDisconnect()
    {
        gl.EndGame();
        NetworkStream stream = client.GetStream();
        stream.Close();
        client.Close();
        client = null;
        gl.status.Connected = false;
    }
}

#endif
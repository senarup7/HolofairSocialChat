

using Sfs2X;
using Sfs2X.Core;
using Sfs2X.Entities;
using Sfs2X.Entities.Data;
using Sfs2X.Logging;
using Sfs2X.Requests;
using UnityEngine;

// Statics for holding the connection to the SFS server end
// Can then be queried from the entire game to get the connection


    public class ConnectionManager : MonoBehaviour
    {
        private static ConnectionManager instance;
        private static SmartFox smartFox;
        private static string serverHost;
        private static int serverPort;
        private static string userName;
        private static string playerJoinRoomName;
    /// <summary>
    /// When true user is logged in. If false user is not logged in but may be connected.
    /// </summary>
    private static bool isLoggedIn;
        private static Sfs2X.Entities.Room activeRoom;

        #region EVENTS

        public delegate void RequestAccepted();
        public static event RequestAccepted ConnectedToServer;
        public static event RequestAccepted ConnectedToZone;
        public static event RequestAccepted ConnectedToRoom;

        /// <summary>
        /// Event for received extension responses received.
        /// </summary>
        /// <param name="pms">Parameters of the response.</param>
        public delegate void EventResponseReceived(string cmd, ISFSObject pms);
        public static event EventResponseReceived ResponseReceived;
        #endregion

        public static SmartFox Instance
        {
            get
            {
                if (instance == null)
                    instance = new GameObject("ConnectionManager").AddComponent(typeof(ConnectionManager)) as ConnectionManager;
                return smartFox;
            }
            set
            {
                if (instance == null)
                    instance = new GameObject("ConnectionManager").AddComponent(typeof(ConnectionManager)) as ConnectionManager;
                smartFox = value;
            }
        }


        public static SmartFox sfsServer
        {
            get { return smartFox; }
            

        }
        public static string UserName
        {
            get { return userName; }
            set { userName = value; }
        }
    public static string RoomName
    {
        get { return playerJoinRoomName.ToString(); }
       // set { userName = value; }
    }

    public static bool IsLoggedIn
        {
            get { return isLoggedIn; }
        }

        /// <summary>
        /// True if there is connection. False if not
        /// </summary>
        public static bool IsConnected
        {
            get
            {
                return smartFox != null && smartFox.IsConnected;
            }
        }

        /// <summary>
        /// Checks if smartfox exists.
        /// </summary>
        public static bool IsInitialized
        {
            get
            {
                return (smartFox != null);
            }
        }

        /// <summary>
        /// Initialises smartfox.
        /// </summary>
        /// <param name="serverName"></param>
        /// <param name="port"></param>
        /// <param name="logLevel"></param>
        public static void Initialize(string newServerName, int newServerPort, LogLevel logLevel)
        {
            serverHost = newServerName;
            serverPort = newServerPort;

            // Register for basic callbacks
            smartFox = new SmartFox(true);
            smartFox.AddEventListener(SFSEvent.CONNECTION, OnConnection);
            smartFox.AddEventListener(SFSEvent.CONNECTION_LOST, OnConnectionLost);

            // Also register for all debug messages from the API at the given log level
            smartFox.AddLogListener(logLevel, OnDebugMessage);

            if (instance == null)
                instance = new GameObject("SmartFoxConnection").AddComponent(typeof(ConnectionManager)) as ConnectionManager;
        }

        public static void Connect()
        {
            smartFox.Connect(serverHost, serverPort);
        }

        /// <summary>
        /// Request log in to a zone for the user.
        /// </summary>
        /// <param name="userName">The name of the user to register.</param>
        /// <param name="zoneName">The name of the zone you will request log in.</param>
        public static void LogInToZone(string zoneName)
        {
            smartFox.AddEventListener(SFSEvent.LOGIN, OnZoneLogin);
            smartFox.AddEventListener(SFSEvent.LOGIN_ERROR, OnLoginError);
            smartFox.AddEventListener(SFSEvent.LOGOUT, OnLogout);
            smartFox.AddEventListener(SFSEvent.USER_EXIT_ROOM, OnUserExitRoom);

            smartFox.Send(new LoginRequest(userName, "", zoneName));
        }

        /// <summary>
        /// Send request to server.
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="parameters"></param>
        /// <param name="roomName">Adds room name to the parameters for the server to propagate the event to the room.</param>
        public static void SendRequest(string cmd, ISFSObject parameters, string roomName)
        {
            Debug.Log("Sending command: " + cmd);
            parameters.PutUtfString("room", roomName);
            smartFox.Send(new ExtensionRequest(cmd, parameters));
        }

        /// <summary>
        /// Send request to server.
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="parameters"></param>
        public static void SendRequest(string cmd, ISFSObject parameters)
        {
            Debug.Log("Sending command: " + cmd);
            smartFox.Send(new ExtensionRequest(cmd, parameters));
        }

        /// <summary>
        /// Send request to server.
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="parameters"></param>
        public static void SendRequest(string cmd)
        {
            Debug.Log("Sending command: " + cmd);
            smartFox.Send(new ExtensionRequest(cmd, new SFSObject()));
        }

        /// <summary>
        /// As Unity is not thread safe, we process the queued up callbacks every physics tick
        /// </summary>
        void FixedUpdate()
        {
            if (smartFox != null)
                smartFox.ProcessEvents();
        }

        /// <summary>
        /// Don't destroy smartfox component by scene load.
        /// </summary>
        void Awake()
        {
            DontDestroyOnLoad(this);
        }

        // Disconnect from the socket when shutting down the game
        // ** Important for Windows users - can cause crashes otherwise
        public void OnApplicationQuit()
        {
            if (smartFox != null)
            {
                if (smartFox.IsConnected)
                    smartFox.Disconnect();
            }

            smartFox = null;
        }

        // Disconnect from the socket when ordered by the main Panel scene
        // ** Important for Windows users - can cause crashes otherwise
        public void Disconnect()
        {
            OnApplicationQuit();
        }

        /// <summary>
        /// User logged in successfully. Can now receive responses from the server.
        /// </summary>
        /// <param name="evt"></param>
        public static void OnZoneLogin(BaseEvent evt)
        {
            
            User user = evt.Params["user"] as User;
            OnDebugMessage("Zone login success: " + user);
            OnDebugMessage(smartFox.UserManager.UserCount.ToString());

            isLoggedIn = true;

           // smartFox.AddEventListener(SFSEvent.EXTENSION_RESPONSE, extensionResponses);

            if (ConnectedToZone != null)
                ConnectedToZone(); //ChangeServerInfo(user.Id, user.Name);
            // Chat Panel On
            
            FindObjectOfType<Chat_UIManager>().InitSFXConnection(user.Name);
    }

        /// <summary>
        /// Request join in room.
        /// </summary>
        /// <param name="roomName"></param>
        public static void LogInToRoom(string roomName)
        {
            activeRoom = smartFox.GetRoomByName(roomName);
            playerJoinRoomName = roomName;
            smartFox.AddEventListener(SFSEvent.ROOM_JOIN, onRoomJoin);
            smartFox.AddEventListener(SFSEvent.ROOM_JOIN_ERROR, onJoinError);
            smartFox.Send(new JoinRoomRequest(roomName));
            OnDebugMessage("User Logged In");
        }
         
        /// <summary>
        /// Log out from active room.
        /// </summary>
        public static void LogoutFromRoom()
        {
            if (activeRoom != null)
                smartFox.Send(new LeaveRoomRequest(activeRoom));
        }

        private static void onJoinError(BaseEvent evt)
        {
            isLoggedIn = false;
            OnDebugMessage("Join room error");
        }

        /// <summary>
        /// User just joined a room. Fires event.
        /// </summary>
        /// <param name="evt"></param>
        private static void onRoomJoin(BaseEvent evt)
        {
            if (ConnectedToRoom != null)
                ConnectedToRoom();
        }

        /// <summary>
        /// Receives network responses.
        /// </summary>
        /// <param name="evt"></param>
        private static void extensionResponses(BaseEvent evt)
        {
            string cmd = evt.Params["cmd"] as string;
            Debug.Log("Received response with command: " + cmd);
            ISFSObject parameters = evt.Params["params"] as SFSObject;
            if (ResponseReceived != null)
                ResponseReceived(cmd, parameters);
        }

        public static void OnLoginError(BaseEvent evt)
        {
            OnDebugMessage("Login failed: " + evt.Params["errorMessage"]);
        }

        public static void OnLogout(BaseEvent evt)
        {
            isLoggedIn = false;
            OnDebugMessage("Logged out");
        }

        /// <summary>
        /// Displays the connection view in case the current user is kicked from the Room.
        /// An MMORoom kicks users automatically in case their initial position is not set within the configured time (see userMaxLimboSeconds setting on the Room).
        /// 
        /// Actually this will never happen in this game as the position is set in the server side Extension as soon as the user enters the game
        /// </summary>
        /// <param name="evt"></param>
        public static void OnUserExitRoom(BaseEvent evt)
        {
            // Show the title screen
            if (evt.Params["user"] == smartFox.MySelf) // Set a warning to be displayed in connection view
                OnDebugMessage("You have been kicked out of the room because your initial position wasn't set in time");
        }

        //----------------------------------------------------------
        // Handle connection response from server
        //----------------------------------------------------------
        public static void OnConnection(BaseEvent evt)
        {
            bool success = (bool)evt.Params["success"];
            if (success)
            {
                OnDebugMessage("Connection successful!");
                if (ConnectedToServer != null)
                    ConnectedToServer();
                //LogInToZone(userName, "WorldExtension");
            }
            else
                OnDebugMessage("Can't connect to server!");
        }

        public static void OnConnectionLost(BaseEvent evt)
        {
            OnDebugMessage("Connection lost; reason: " + (string)evt.Params["reason"]);
        }

        //----------------------------------------------------------
        // Show the debug messages both in window as well as console log
        //----------------------------------------------------------
        public static void OnDebugMessage(BaseEvent evt)
        {
            string message = (string)evt.Params["message"];
            Debug.Log("[SFS DEBUG] " + message);
        }

        public static void OnDebugMessage(string message)
        {
            Debug.Log("[SFS DEBUG] " + message);
        }
    }

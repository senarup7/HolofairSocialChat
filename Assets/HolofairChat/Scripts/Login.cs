using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;
using Sfs2X;
using Sfs2X.Logging;
using Sfs2X.Util;
using Sfs2X.Core;
using Sfs2X.Entities;
using Sfs2X.Entities.Data;
using Sfs2X.Entities.Variables;
using UnityEngine.SceneManagement;
using Sfs2X.Requests;

public class Login : MonoBehaviour
{

	//----------------------------------------------------------
	// Editor public properties
	//----------------------------------------------------------

	[Tooltip("IP address or domain name of the SmartFoxServer 2X instance")]
	public string Host = "127.0.0.1";

	[Tooltip("TCP port listened by the SmartFoxServer 2X instance; used for regular socket connection in all builds except WebGL")]
	public int TcpPort = 9933;

	[Tooltip("WebSocket port listened by the SmartFoxServer 2X instance; used for in WebGL build only")]
	public int WSPort = 8080;

	[Tooltip("Name of the SmartFoxServer 2X Zone to join")]
	public string Zone = "BasicExamples";

	[Tooltip("Name of the Room")]
	public string roomName = "GameRoom";
	//----------------------------------------------------------
	// UI elements
	//----------------------------------------------------------

	// Login panel components

	public InputField zoneInput;
	public InputField nameInput;
	public Button loginButton;
	public Text errorText;

	// User details panel components

	public Text loggedInText;

	//----------------------------------------------------------
	// Private properties
	//----------------------------------------------------------

	private SmartFox sfs;

	//----------------------------------------------------------
	// Unity calback methods
	//----------------------------------------------------------

	void Start()
	{
		ConnectionManager.Initialize(Host, TcpPort, LogLevel.DEBUG);
		// Initialize UI
		zoneInput.text = Zone;
		errorText.text = "";
		ConnectionManager.Connect();
	}

	// Update is called once per frame
	void Update()
	{
		if (sfs != null)
			sfs.ProcessEvents();
		

	}
	public  void SceneChange(string Scene)
	{
		if (Scene != null || Scene != "")
		{
			SceneManager.LoadScene(Scene);
		}
	}
	//----------------------------------------------------------
	// Public interface methods for UI
	//----------------------------------------------------------

	public void OnLoginButtonClick()
	{
		ConnectionManager.UserName = nameInput.text;
		ConnectionManager.LogInToZone(zoneInput.text);
		//ConnectionManager.LogInToRoom(roomName);
		
		ConnectionManager.sfsServer.Send(new Sfs2X.Requests.Buddylist.InitBuddyListRequest());

	}

	/**
	 * Disconnects from server.
	 */
	public void OnDisconnectButtonClick()
	{
		sfs.Disconnect();
	}
	//----------------------------------------------------------
	// SmartFoxServer event listeners
	//----------------------------------------------------------

	private void OnConnection(BaseEvent evt)
	{
		if ((bool)evt.Params["success"])
		{
			Debug.Log("SFS2X API version: " + sfs.Version);
			Debug.Log("Connection mode is: " + sfs.ConnectionMode);

			// Login
			sfs.Send(new Sfs2X.Requests.LoginRequest(nameInput.text));
		}
		else
		{
			// Remove SFS2X listeners and re-enable interface
			reset();

			// Show error message
			errorText.text = "Connection failed; is the server running at all?";
		}
	}

	private void OnConnectionLost(BaseEvent evt)
	{

		// Remove SFS2X listeners and re-enable interface
		reset();

		string reason = (string)evt.Params["reason"];

		if (reason != ClientDisconnectionReason.MANUAL)
		{
			// Show error message
			errorText.text = "Connection was lost; reason is: " + reason;
		}
	}
	private void reset()
	{
		// Remove SFS2X listeners
		sfs.RemoveEventListener(SFSEvent.CONNECTION, OnConnection);
		sfs.RemoveEventListener(SFSEvent.CONNECTION_LOST, OnConnectionLost);
		sfs.RemoveEventListener(SFSEvent.LOGIN, OnLogin);
		sfs.RemoveEventListener(SFSEvent.LOGIN_ERROR, OnLoginError);


		sfs = null;


	}
	private void OnLogin(BaseEvent evt)
	{
		Debug.Log("On Login");

		// Initialize buddy list system
		sfs.Send(new Sfs2X.Requests.Buddylist.InitBuddyListRequest());

		
	}
	private void OnLoginError(BaseEvent evt)
	{
		// Disconnect
		sfs.Disconnect();

		// Remove SFS2X listeners and re-enable interface
		reset();

		// Show error message
		errorText.text = "Login failed: " + (string)evt.Params["errorMessage"];
	}
	
}
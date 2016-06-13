// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WorkerMenu.cs" company="Exit Games GmbH">
//   Part of: Photon Unity Networking
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using UnityEngine;
using Random = UnityEngine.Random;


public class LobbyMenu : MonoBehaviour
{
	private string roomName = "myRoom";
	private bool MessageRoomNameTaken = false;
	private float MessageRoomTakenTimeToDisplay = 0;
	private Vector2 scrollPos = Vector2.zero;
	
	private bool connectFailed = false;
	
	public static readonly string SceneNameMenu = "LobbyScene";
	
	public static readonly string SceneNameGame = "GameScene";
	
	public void Awake()
	{
		PhotonNetwork.automaticallySyncScene = true;
		
		if (PhotonNetwork.connectionStateDetailed == PeerState.PeerCreated)
		{
			PhotonNetwork.ConnectUsingSettings("1.0");
		}
		
		if (String.IsNullOrEmpty(PhotonNetwork.playerName))
		{
			PhotonNetwork.playerName = MainMenu.username;
		}
	}
	
	public void OnGUI()
	{
		if (!PhotonNetwork.connected)
		{
			if (PhotonNetwork.connecting)
			{
				GUILayout.Label("Connecting to: " + PhotonNetwork.ServerAddress);
			}
			else
			{
				GUILayout.Label("Not connected. Check console output. Detailed connection state: " + PhotonNetwork.connectionStateDetailed + " Server: " + PhotonNetwork.ServerAddress);
			}
			
			if (this.connectFailed)
			{
				GUILayout.Label("Connection failed. Check setup and use Setup Wizard to fix configuration.");
				GUILayout.Label(String.Format("Server: {0}", new object[] {PhotonNetwork.ServerAddress}));
				GUILayout.Label("AppId: " + PhotonNetwork.PhotonServerSettings.AppID);
				
				if (GUILayout.Button("Try Again", GUILayout.Width(100)))
				{
					this.connectFailed = false;
					PhotonNetwork.ConnectUsingSettings("1.0");
				}
			}
			
			return;
		}
		
		
		
		
		GUI.skin.box.fontStyle = FontStyle.Bold;
		GUI.Box(new Rect((Screen.width - 400) / 2, (Screen.height - 350) / 2, 400, 300), "Join or Create a Room");
		GUILayout.BeginArea(new Rect((Screen.width - 400) / 2, (Screen.height - 350) / 2, 400, 300));
		
		GUILayout.Space(25);

		GUILayout.BeginHorizontal();
		GUILayout.Label("Player name:", GUILayout.Width(100));
		GUILayout.Label(PhotonNetwork.playerName);
		GUILayout.Space(105);
		if (GUI.changed)
		{
			PlayerPrefs.SetString("playerName", PhotonNetwork.playerName);
		}
		GUILayout.EndHorizontal();
		
		GUILayout.Space(15);
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Roomname:", GUILayout.Width(100));
		this.roomName = GUILayout.TextField(this.roomName);
		
		if (GUILayout.Button("Create Room", GUILayout.Width(100)))
		{
			
			foreach (RoomInfo roomInfo in PhotonNetwork.GetRoomList())
			{
				if (roomInfo.name == this.roomName) {MessageRoomNameTaken = true; break;}
				
			}
			if (MessageRoomNameTaken==false) PhotonNetwork.CreateRoom(this.roomName, new RoomOptions() { maxPlayers = 2 }, null);
			
			Debug.Log("OnJoinedRoom");
			
		}
		
		
		
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("Join Room", GUILayout.Width(100)))
		{
			PhotonNetwork.JoinRoom(this.roomName);
		}
		
		GUILayout.EndHorizontal();
		
		
		GUILayout.Space(15);
		
		GUILayout.BeginHorizontal();
		
		GUILayout.Label(PhotonNetwork.countOfPlayers + " users are online in " + PhotonNetwork.countOfRooms + " rooms.");
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("Join Random", GUILayout.Width(100)))
		{
			PhotonNetwork.JoinRandomRoom();
		}
		
		
		GUILayout.EndHorizontal();
		
		GUILayout.Space(15);
		if (PhotonNetwork.GetRoomList().Length == 0)
		{
			GUILayout.Label("Currently no games are available.");
			GUILayout.Label("Rooms will be listed here, when they become available.");
		}
		else
		{
			int roomcount = PhotonNetwork.GetRoomList().Length;
			if (roomcount==1 )GUILayout.Label("1 room is currently available:");
			else GUILayout.Label(PhotonNetwork.GetRoomList().Length + " rooms are currently available:");
			this.scrollPos = GUILayout.BeginScrollView(this.scrollPos);
			foreach (RoomInfo roomInfo in PhotonNetwork.GetRoomList())
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label(roomInfo.name + " " + roomInfo.playerCount + "/" + roomInfo.maxPlayers);
				if (GUILayout.Button("Join"))
				{
					PhotonNetwork.JoinRoom(roomInfo.name);
					
				}
				
				GUILayout.EndHorizontal();
			}
			
			GUILayout.EndScrollView();
		}
		
		GUILayout.EndArea();
		
		if (MessageRoomNameTaken == true) {
			
			MessageRoomTakenTimeToDisplay = 5;
			MessageRoomNameTaken = false;
		}
		if (MessageRoomTakenTimeToDisplay >0 ) { GUI.contentColor = Color.red;  
			GUI.Label(new Rect(400,50,300,60), "The room with this name already exists");
			MessageRoomTakenTimeToDisplay = MessageRoomTakenTimeToDisplay - Time.deltaTime;
		}
	}
	
	public void OnJoinedRoom()
	{
		
		Debug.Log("OnJoinedRoom");
		
	}
	
	public void OnCreatedRoom()
	{
		Debug.Log("OnCreatedRoom");
		PhotonNetwork.LoadLevel(SceneNameGame);
	}
	
	public void OnDisconnectedFromPhoton()
	{
		Debug.Log("Disconnected from Photon.");
	}
	
	public void OnFailedToConnectToPhoton(object parameters)
	{
		this.connectFailed = true;
		Debug.Log("OnFailedToConnectToPhoton. StatusCode: " + parameters + " ServerAddress: " + PhotonNetwork.networkingPeer.ServerAddress);
	}
}

using UnityEngine;
using System.Collections;

public class NetworkMenu : MonoBehaviour {
    public string connectionIP = "127.0.0.1";
    public int portNumber = 8632;

    private bool connected = false;
    private void OnConnectedToServer()
    {
        connected = true;
    }
    private void OnServerInitialized()
    {
        connected = true;
    }

    private void OnDisconnectedFromSercer()
    {
        connected = false;
    }
    private void OnGUI()

    {
        if (!connected)
        {

            connectionIP = GUILayout.TextField(connectionIP);
            int.TryParse(GUILayout.TextField(portNumber.ToString()), out portNumber);

            if (GUILayout.Button("Connect"))
                Network.Connect(connectionIP, portNumber);
            if (GUILayout.Button("Server"))
                Network.InitializeServer(4, portNumber, true);
        }
        else
        {
            GUILayout.Label("Connections: " + Network.connections.Length.ToString());
            GUILayout.Label("Player: " + Network.connections.ToString());
        }
    }
}

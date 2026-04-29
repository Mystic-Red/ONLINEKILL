namespace OnlineKill;

using System;
using UnityEngine;

public sealed class OnlineKillHud : MonoBehaviour
{
    private bool visible;
    private string host = "127.0.0.1";
    private string port = "27040";
    private string playerName = Environment.UserName;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F7))
        {
            visible = !visible;
        }
    }

    private void OnGUI()
    {
        if (!visible)
        {
            return;
        }

        GUI.Box(new Rect(20, 20, 360, 190), "OnlineKill");
        GUI.Label(new Rect(36, 50, 90, 24), "Server");
        host = GUI.TextField(new Rect(126, 50, 220, 24), host);
        GUI.Label(new Rect(36, 80, 90, 24), "Port");
        port = GUI.TextField(new Rect(126, 80, 220, 24), port);
        GUI.Label(new Rect(36, 110, 90, 24), "Name");
        playerName = GUI.TextField(new Rect(126, 110, 220, 24), playerName);

        if (GUI.Button(new Rect(36, 145, 145, 28), "Connect") && int.TryParse(port, out int parsedPort))
        {
            Plugin.Client.Connect(host, parsedPort, playerName);
        }

        if (GUI.Button(new Rect(201, 145, 145, 28), "Disconnect"))
        {
            Plugin.Client.Disconnect();
            Plugin.RemotePlayers.Clear();
        }

        GUI.Label(new Rect(36, 176, 310, 24), Plugin.Client.Status);
    }
}

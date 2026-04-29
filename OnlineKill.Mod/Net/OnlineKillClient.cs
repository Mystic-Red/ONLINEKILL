namespace OnlineKill.Net;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using BepInEx.Logging;
using OnlineKill.Game;
using UnityEngine;

public sealed class OnlineKillClient : IDisposable
{
    private const string Hello = "HELLO";
    private const string Welcome = "WELCOME";
    private const string Pose = "POSE";
    private const string Snapshot = "SNAP";
    private const string Leave = "LEAVE";
    private const string Bye = "BYE";

    private readonly ManualLogSource log;
    private readonly Queue<PlayerSnapshot> snapshots = new();
    private readonly Queue<int> disconnectedPlayers = new();
    private readonly byte[] packetBuffer = new byte[8192];
    private Socket socket;
    private EndPoint serverEndPoint;
    private string playerName = Environment.UserName;
    private int localId;

    public bool IsConnected { get; private set; }
    public string Status { get; private set; } = "Offline";

    public OnlineKillClient(ManualLogSource log)
    {
        this.log = log;
    }

    public void Connect(string host, int port, string name)
    {
        Disconnect();

        playerName = string.IsNullOrWhiteSpace(name) ? Environment.UserName : name.Trim();
        IPAddress[] addresses = Dns.GetHostAddresses(host);
        IPAddress address = Array.Find(addresses, x => x.AddressFamily == AddressFamily.InterNetwork) ?? IPAddress.Loopback;

        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Blocking = false;
        serverEndPoint = new IPEndPoint(address, port);
        IsConnected = true;
        Status = $"Connected to {host}:{port}";
        SendRaw($"{Hello}|{Escape(playerName)}|{Application.version}");
        log.LogInfo(Status);
    }

    public void Disconnect()
    {
        if (socket != null)
        {
            try
            {
                SendRaw(Bye);
                socket.Close();
            }
            catch
            {
            }
        }

        socket = null;
        serverEndPoint = null;
        IsConnected = false;
        localId = 0;
        Status = "Offline";
        snapshots.Clear();
        disconnectedPlayers.Clear();
    }

    public void SendPose(PlayerPose pose)
    {
        if (!IsConnected)
        {
            return;
        }

        Vector3 p = pose.Position;
        Quaternion r = pose.Rotation;
        SendRaw(FormattableString.Invariant($"{Pose}|{p.x:0.###}|{p.y:0.###}|{p.z:0.###}|{r.x:0.#####}|{r.y:0.#####}|{r.z:0.#####}|{r.w:0.#####}|{Escape(pose.Scene)}"));
    }

    public void Pump()
    {
        if (socket == null)
        {
            return;
        }

        while (socket.Available > 0)
        {
            EndPoint remote = new IPEndPoint(IPAddress.Any, 0);
            int count = socket.ReceiveFrom(packetBuffer, ref remote);
            string packet = Encoding.UTF8.GetString(packetBuffer, 0, count);
            Handle(packet);
        }
    }

    public IReadOnlyList<PlayerSnapshot> ConsumeSnapshots()
    {
        if (snapshots.Count == 0)
        {
            return Array.Empty<PlayerSnapshot>();
        }

        List<PlayerSnapshot> list = new(snapshots.Count);
        while (snapshots.Count > 0)
        {
            list.Add(snapshots.Dequeue());
        }

        return list;
    }

    public IReadOnlyList<int> ConsumeDisconnectedPlayers()
    {
        if (disconnectedPlayers.Count == 0)
        {
            return Array.Empty<int>();
        }

        List<int> list = new(disconnectedPlayers.Count);
        while (disconnectedPlayers.Count > 0)
        {
            list.Add(disconnectedPlayers.Dequeue());
        }

        return list;
    }

    public void Dispose()
    {
        Disconnect();
    }

    private void Handle(string packet)
    {
        string[] parts = packet.Split('|');
        if (parts.Length == 0)
        {
            return;
        }

        if (parts[0] == Welcome && parts.Length >= 2 && int.TryParse(parts[1], out int assigned))
        {
            localId = assigned;
            Status = $"Online as #{localId}";
            return;
        }

        if (parts[0] == Snapshot && parts.Length >= 11 && int.TryParse(parts[1], out int id) && id != localId)
        {
            string name = Unescape(parts[2]);
            if (TryFloat(parts[3], out float px) && TryFloat(parts[4], out float py) && TryFloat(parts[5], out float pz) &&
                TryFloat(parts[6], out float rx) && TryFloat(parts[7], out float ry) && TryFloat(parts[8], out float rz) && TryFloat(parts[9], out float rw))
            {
                snapshots.Enqueue(new PlayerSnapshot(id, name, new Vector3(px, py, pz), new Quaternion(rx, ry, rz, rw)));
            }
        }

        if (parts[0] == Leave && parts.Length >= 2 && int.TryParse(parts[1], out int goneId))
        {
            disconnectedPlayers.Enqueue(goneId);
        }
    }

    private void SendRaw(string text)
    {
        if (socket == null || serverEndPoint == null)
        {
            return;
        }

        byte[] data = Encoding.UTF8.GetBytes(text);
        socket.SendTo(data, serverEndPoint);
    }

    private static bool TryFloat(string value, out float result)
    {
        return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result);
    }

    private static string Escape(string value)
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(value ?? string.Empty));
    }

    private static string Unescape(string value)
    {
        try
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(value));
        }
        catch
        {
            return "Player";
        }
    }
}

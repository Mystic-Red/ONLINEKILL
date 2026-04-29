namespace OnlineKill;

using BepInEx;
using BepInEx.Logging;
using OnlineKill.Game;
using OnlineKill.Net;
using UnityEngine;

[BepInPlugin(Guid, Name, Version)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string Guid = "com.matiu.onlinekill";
    public const string Name = "OnlineKill";
    public const string Version = "0.1.0";
    private const float SendRate = 20f;

    internal static ManualLogSource Log { get; private set; }
    internal static OnlineKillClient Client { get; private set; }
    internal static PlayerTracker PlayerTracker { get; private set; }
    internal static RemotePlayerManager RemotePlayers { get; private set; }

    private OnlineKillHud menu;
    private float sendTimer;

    private void Awake()
    {
        Log = Logger;
        Client = new OnlineKillClient(Logger);
        PlayerTracker = new PlayerTracker(Logger);
        RemotePlayers = new RemotePlayerManager(Logger);
        menu = gameObject.AddComponent<OnlineKillHud>();
        DontDestroyOnLoad(gameObject);
        Logger.LogInfo("OnlineKill loaded. Press F7 to open the quick connect menu.");
    }

    private void Update()
    {
        Client.Pump();
        RemotePlayers.Apply(Client.ConsumeSnapshots());
        RemotePlayers.Remove(Client.ConsumeDisconnectedPlayers());

        sendTimer += Time.unscaledDeltaTime;
        if (Client.IsConnected && sendTimer >= 1f / SendRate && PlayerTracker.TryReadLocalPose(out PlayerPose pose))
        {
            sendTimer = 0f;
            Client.SendPose(pose);
        }
    }

    private void OnDestroy()
    {
        Client.Dispose();
        RemotePlayers.Clear();
    }
}

namespace OnlineKill.Game;

using System.Collections.Generic;
using BepInEx.Logging;
using OnlineKill.Net;
using UnityEngine;

public sealed class RemotePlayerManager
{
    private readonly ManualLogSource log;
    private readonly Dictionary<int, RemotePlayerView> players = new();

    public RemotePlayerManager(ManualLogSource log)
    {
        this.log = log;
    }

    public void Apply(IReadOnlyList<PlayerSnapshot> snapshots)
    {
        for (int i = 0; i < snapshots.Count; i++)
        {
            PlayerSnapshot snapshot = snapshots[i];
            if (!players.TryGetValue(snapshot.Id, out RemotePlayerView view))
            {
                view = RemotePlayerView.Create(snapshot.Id, snapshot.Name);
                players.Add(snapshot.Id, view);
                log.LogInfo($"Remote player joined: {snapshot.Name} ({snapshot.Id})");
            }

            view.SetTarget(snapshot.Position, snapshot.Rotation, snapshot.Name);
        }
    }

    public void Remove(IReadOnlyList<int> ids)
    {
        for (int i = 0; i < ids.Count; i++)
        {
            int id = ids[i];
            if (!players.TryGetValue(id, out RemotePlayerView view))
            {
                continue;
            }

            players.Remove(id);
            if (view != null)
            {
                Object.Destroy(view.gameObject);
            }
        }
    }

    public void Clear()
    {
        foreach (RemotePlayerView view in players.Values)
        {
            if (view != null)
            {
                Object.Destroy(view.gameObject);
            }
        }

        players.Clear();
    }
}

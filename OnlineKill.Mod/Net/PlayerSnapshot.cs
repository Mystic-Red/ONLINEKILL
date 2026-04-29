namespace OnlineKill.Net;

using UnityEngine;

public readonly struct PlayerSnapshot
{
    public readonly int Id;
    public readonly string Name;
    public readonly Vector3 Position;
    public readonly Quaternion Rotation;

    public PlayerSnapshot(int id, string name, Vector3 position, Quaternion rotation)
    {
        Id = id;
        Name = name;
        Position = position;
        Rotation = rotation;
    }
}

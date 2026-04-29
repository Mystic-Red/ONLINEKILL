namespace OnlineKill.Game;

using UnityEngine;

public readonly struct PlayerPose
{
    public readonly Vector3 Position;
    public readonly Quaternion Rotation;
    public readonly string Scene;

    public PlayerPose(Vector3 position, Quaternion rotation, string scene)
    {
        Position = position;
        Rotation = rotation;
        Scene = scene;
    }
}

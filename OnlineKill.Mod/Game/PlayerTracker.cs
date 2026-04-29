namespace OnlineKill.Game;

using BepInEx.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class PlayerTracker
{
    private readonly ManualLogSource log;
    private Transform cachedTransform;
    private Transform cachedCamera;
    private float nextSearchTime;

    public PlayerTracker(ManualLogSource log)
    {
        this.log = log;
    }

    public bool TryReadLocalPose(out PlayerPose pose)
    {
        pose = default;

        if (!ResolveTransforms())
        {
            return false;
        }

        Quaternion rotation = cachedCamera != null ? cachedCamera.rotation : cachedTransform.rotation;
        pose = new PlayerPose(cachedTransform.position, rotation, SceneManager.GetActiveScene().name);
        return true;
    }

    private bool ResolveTransforms()
    {
        if (cachedTransform != null)
        {
            return true;
        }

        if (Time.unscaledTime < nextSearchTime)
        {
            return false;
        }

        nextSearchTime = Time.unscaledTime + 1f;

        NewMovement movement = Object.FindObjectOfType<NewMovement>();
        if (movement != null)
        {
            cachedTransform = movement.transform;
            cachedCamera = Camera.main != null ? Camera.main.transform : null;
            log.LogInfo("Found the local player transform.");
            return true;
        }

        Camera main = Camera.main;
        if (main != null)
        {
            cachedTransform = main.transform;
            cachedCamera = main.transform;
            return true;
        }

        return false;
    }
}

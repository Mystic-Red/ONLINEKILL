namespace OnlineKill.Game;

using UnityEngine;

public sealed class RemotePlayerView : MonoBehaviour
{
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private TextMesh label;

    public static RemotePlayerView Create(int id, string playerName)
    {
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = $"OnlineKill Remote Player {id}";
        Object.DontDestroyOnLoad(body);

        RemotePlayerView view = body.AddComponent<RemotePlayerView>();
        view.targetPosition = body.transform.position;
        view.targetRotation = body.transform.rotation;

        Renderer renderer = body.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = new Color(0.1f, 0.85f, 1f, 0.85f);
        }

        GameObject labelObject = new GameObject("Name");
        labelObject.transform.SetParent(body.transform, false);
        labelObject.transform.localPosition = new Vector3(0f, 1.35f, 0f);
        view.label = labelObject.AddComponent<TextMesh>();
        view.label.anchor = TextAnchor.MiddleCenter;
        view.label.alignment = TextAlignment.Center;
        view.label.characterSize = 0.18f;
        view.label.text = playerName;
        view.label.color = Color.cyan;

        return view;
    }

    public void SetTarget(Vector3 position, Quaternion rotation, string playerName)
    {
        targetPosition = position;
        targetRotation = rotation;
        if (label != null)
        {
            label.text = playerName;
        }
    }

    private void Update()
    {
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 18f);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 18f);

        if (label != null && Camera.main != null)
        {
            label.transform.rotation = Quaternion.LookRotation(label.transform.position - Camera.main.transform.position);
        }
    }
}

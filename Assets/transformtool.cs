using UnityEngine;

public class DebugAttachPointAdjuster : MonoBehaviour
{
    public Transform target;
    public Vector3 positionOffset;
    public Vector3 rotationOffset;

    void Update()
    {
        if (target)
        {
            target.localPosition = positionOffset;
            target.localEulerAngles = rotationOffset;
        }
    }
}

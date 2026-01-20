using UnityEngine;

public class CameraInGame : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 8f;
    public Vector3 offset = new Vector3(0, 0, -10);

    void LateUpdate()
    {
        if (!target) return;

        Vector3 desiredPos = target.position + offset;
        transform.position = Vector3.Lerp(
            transform.position,
            desiredPos,
            smoothSpeed * Time.deltaTime
        );
    }
}

using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target to follow")]
    public Transform target;

    [Header("Camera offset from target")]
    public Vector3 offset = new Vector3(0f, 10f, -5f);

    [Header("Follow smoothness")]
    [Range(1f, 10f)] public float smoothSpeed = 5f;
    private void LateUpdate()
    {
        if (target == null) return;

        // Desired position (target + offset)
        Vector3 desiredPosition = target.position + offset;

        // Smoothly move the camera toward the desired position
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // Make the camera look at the target (optional)
        transform.LookAt(target);
    }
}

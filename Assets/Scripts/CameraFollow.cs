using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform target; // The player to follow
    
    [Header("Follow Settings")]
    [SerializeField] private float smoothSpeed = 5f; // How quickly the camera catches up
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f); // Camera offset from target
    
    [Header("Height Constraints")]
    [SerializeField] private bool useHeightConstraints = true;
    [SerializeField] private float minHeight = -5f; // Minimum Y position for camera
    [SerializeField] private float maxHeight = 10f; // Maximum Y position for camera
    
    [Header("Horizontal Constraints (Optional)")]
    [SerializeField] private bool useHorizontalConstraints = false;
    [SerializeField] private float minX = -10f; // Minimum X position for camera
    [SerializeField] private float maxX = 10f; // Maximum X position for camera

    void LateUpdate()
    {
        if (target == null)
        {
            Debug.LogWarning("CameraFollow: No target assigned!");
            return;
        }

        // Calculate the desired position
        Vector3 desiredPosition = target.position + offset;

        // Apply height constraints
        if (useHeightConstraints)
        {
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, minHeight, maxHeight);
        }

        // Apply horizontal constraints (optional)
        if (useHorizontalConstraints)
        {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);
        }

        // Smoothly interpolate between current position and desired position
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // Update camera position
        transform.position = smoothedPosition;
    }
}

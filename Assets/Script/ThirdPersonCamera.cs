using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform target; // The character to follow
    public Vector3 offset = new Vector3(0f, 2f, -5f); // Distance from character (x, y, z)
                                                      // Adjust y for height, z for distance behind
    public float smoothSpeed = 10f; // How quickly the camera catches up

    void LateUpdate() // Using LateUpdate is best for follow cameras
    {
        if (target == null)
        {
            Debug.LogWarning("Camera target is not set!");
            return;
        }

        // Calculate the desired position for the camera
        Vector3 desiredPosition = target.position + target.rotation * offset;
        // The 'target.rotation * offset' makes the offset relative to the player's facing direction.
        // If you want a fixed world offset, just use 'target.position + offset'.

        // Smoothly move the camera towards the desired position
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;

        // Make the camera look at the target
        transform.LookAt(target.position + Vector3.up * 1.5f); // Looks slightly above the target's feet
    }
}
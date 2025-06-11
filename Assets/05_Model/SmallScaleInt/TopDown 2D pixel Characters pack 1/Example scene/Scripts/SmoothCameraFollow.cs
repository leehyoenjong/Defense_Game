using UnityEngine;

namespace SmallScaleInc.TopDownPixelCharactersPack1
{
    public class SmoothCameraFollow : MonoBehaviour
    {
        public Transform target; // The target the camera should follow
        public float smoothSpeed = 0.125f; // How smooth the camera follows the player. Lower is smoother.
        public Vector3 offset; // The offset from the player. Adjust as needed.

        void LateUpdate()
        {
            // Desired position the camera tries to reach
            Vector3 desiredPosition = target.position + offset;
            
            // Smoothly interpolate between the camera's current position and the desired position
            // Ensure Z position remains constant if needed
            desiredPosition.z = transform.position.z; // Keep the camera's Z position constant
            
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            
            // Update the camera's position
            transform.position = smoothedPosition;
        }
    }
}
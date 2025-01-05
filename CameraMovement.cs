using System.Collections;
using UnityEngine;

namespace PlayerCamera
{
    public class CameraMovement : MonoBehaviour
    {    
        [SerializeField] Transform target;
        [SerializeField] Vector2 camPosMinLimits;
        [SerializeField] Vector2 camPosMaxLimits;
        [SerializeField] Vector2 targetOffset;
            
        void Start()
        {
            StartCoroutine(FollowingTarget());
        }
            
        /* The current basic camera is locked to the player quite tightly, and there's no "dead zone" in the middle, inside which the player could move freely without moving the camera as well.
         * The camera will lock to the position of the followed target (usually player) with a certain offset, and camera min and max positions will also be reinforced.
         * For example, if we never want the camera to be below y = 0, we will keep the camera at that y position, even if the player is not perfectly centered to the camera.
         */
        IEnumerator FollowingTarget()
        {
            Vector2 camPos = default;
    
            while (true)
            {
                camPos.x = Mathf.Clamp(target.position.x + targetOffset.x, camPosMinLimits.x, camPosMaxLimits.x);
                camPos.y = Mathf.Clamp(target.position.y + targetOffset.y, camPosMinLimits.y, camPosMaxLimits.y);
    
                transform.position = new Vector3(camPos.x, camPos.y, transform.position.z);
                yield return null;
            }
        }
    
        public void SetNewCameraAttributes(Vector2 newMinLimits, Vector2 newMaxLimits, Vector2 newTargetOffset)
        {
            camPosMinLimits = newMinLimits;
            camPosMaxLimits = newMaxLimits;
            targetOffset = newTargetOffset;
        }
    }
}
}

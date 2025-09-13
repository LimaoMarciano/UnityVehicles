using UnityEngine;
using UnityEngine.UIElements;

namespace UnityVehicles.SimpleCar {

    public class WhellColliderMesh : MonoBehaviour
    {

        public WheelCollider WheelCollider;
        public bool FlipMesh = false;
        
        Vector3 position;
        Quaternion rotation;

        private void OnEnable()
        {

            if (!WheelCollider)
            {
                enabled = false;
                Debug.LogWarning("Wheel Collider not set. Disabling component.");
            } 
            else
            {
                UpdateTransform();
            }
        }

        // Update is called once per frame
        void Update()
        {
            UpdateTransform();
        }

        void UpdateTransform()
        {
            WheelCollider.GetWorldPose(out position, out rotation);
            transform.position = position;
            if (FlipMesh)
            {
                transform.rotation = Quaternion.Inverse(rotation);
            }
            else
            {
                transform.rotation = rotation;
            }
        }
    }

}

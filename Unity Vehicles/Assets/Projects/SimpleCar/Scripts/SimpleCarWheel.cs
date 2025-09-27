using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityVehicles.SimpleCar
{
    [RequireComponent(typeof(WheelCollider))]
    public class SimpleCarWheel : MonoBehaviour
    {
        public Transform VisualWheel;
        public WheelHit WheelHit;

        public bool isGrounded { get; private set; } = false;
        public float SuspensionTravel { get; private set; } = 0f;

        public WheelCollider WheelCollider { get; private set; }
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            WheelCollider = GetComponent<WheelCollider>();
        }

        // Update is called once per frame
        void Update()
        {
            if (VisualWheel)
            {
                Vector3 position;
                Quaternion rotation;
                WheelCollider.GetWorldPose(out position, out rotation);
                VisualWheel.transform.position = position;
                VisualWheel.transform.rotation = rotation;
            }
            
        }

        public void UpdateValues()
        {
            WheelHit wheelHit;
            isGrounded = WheelCollider.GetGroundHit(out wheelHit);
            WheelHit = wheelHit;
            
            if (isGrounded)
            {
                SuspensionTravel = (-transform.InverseTransformPoint(WheelHit.point).y - WheelCollider.radius) / WheelCollider.suspensionDistance;               
            } 
            else
            {
                SuspensionTravel = 1f;
            }
        }
    }

}

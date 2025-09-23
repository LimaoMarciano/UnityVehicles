using UnityEngine;
using UnityEngine.UIElements;

namespace UnityVehicles.SimpleCar
{
    [RequireComponent(typeof(WheelCollider))]
    public class SimpleCarWheel : MonoBehaviour
    {
        public Transform VisualWheel;
        public WheelHit WheelHit;

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

        void FixedUpdate()
        {
            WheelHit wheelHit;
            WheelCollider.GetGroundHit(out wheelHit);
            WheelHit = wheelHit;
        }

    }

}

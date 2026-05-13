using UnityEngine;

namespace UnityVehicles.SimpleCar
{
    [RequireComponent(typeof(WheelCollider))]
    public class SimpleCarWheel : MonoBehaviour
    {
        public Transform visualWheel;
        public WheelHit WheelHit;

        public bool isGrounded { get; private set; }
        public float suspensionTravel { get; private set; }

        public WheelCollider wheelCollider { get; private set; }
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Start()
        {
            wheelCollider = GetComponent<WheelCollider>();
        }

        // Update is called once per frame
        void Update()
        {
            if (!visualWheel) return;
            wheelCollider.GetWorldPose(out var position, out var rotation);
            visualWheel.transform.position = position;
            visualWheel.transform.rotation = rotation;
        }

        public void UpdateValues()
        {
            isGrounded = wheelCollider.GetGroundHit(out var wheelHit);
            WheelHit = wheelHit;
            
            if (isGrounded)
            {
                suspensionTravel = (-transform.InverseTransformPoint(WheelHit.point).y - wheelCollider.radius) / wheelCollider.suspensionDistance;               
            } 
            else
            {
                suspensionTravel = 1f;
            }
        }

        public void SetWheelProperties(TiresSettings tiresSettings, SuspensionSettings suspensionSettings, WheelMountData wheelMountData, bool isFront, bool isLeft)
        {
            //Tires
            var tireData = isFront ? tiresSettings.front : tiresSettings.rear;
            
            wheelCollider.radius = tireData.radius;
            wheelCollider.mass = tireData.mass;
            wheelCollider.wheelDampingRate = tireData.dampingRate;
            
            //Wheel mount
            wheelCollider.forceAppPointDistance = isFront ? wheelMountData.frontForceAppPointDistance : wheelMountData.rearForceAppPointDistance;
            
            var x = isFront ? wheelMountData.frontAxleWidth / 2f : wheelMountData.rearAxleWidth / 2f;
            x = isLeft ? -x : x;
            var y = (isFront) ? wheelMountData.frontWheelsCenter.y : wheelMountData.rearWheelsCenter.y;
            var z = (isFront) ? wheelMountData.frontWheelsCenter.x : wheelMountData.rearWheelsCenter.x;
            wheelCollider.center = new Vector3(x, y, z);
            
            //Suspension
            var suspensionData = isFront ? suspensionSettings.front : suspensionSettings.rear;
            
            wheelCollider.suspensionDistance = suspensionData.distance;
            var jointSpring = new JointSpring
            {
                spring = suspensionData.spring,
                damper = suspensionData.damper,
                targetPosition = suspensionData.targetPosition
            };

            wheelCollider.suspensionSpring = jointSpring;
            
            var fwdFriction = new WheelFrictionCurve
            {
                extremumSlip = tireData.forwardFriction.extremumSlip,
                extremumValue = tireData.forwardFriction.extremumValue,
                asymptoteSlip = tireData.forwardFriction.asymptoteSlip,
                asymptoteValue = tireData.forwardFriction.asymptoteValue,
                stiffness = tireData.forwardFriction.stiffness
            };

            var sideFriction = new WheelFrictionCurve
            {
                extremumSlip = tireData.sideFriction.extremumSlip,
                extremumValue = tireData.sideFriction.extremumValue,
                asymptoteSlip = tireData.sideFriction.asymptoteSlip,
                asymptoteValue = tireData.sideFriction.asymptoteValue,
                stiffness = tireData.sideFriction.stiffness
            };

            wheelCollider.forwardFriction = fwdFriction;
            wheelCollider.sidewaysFriction = sideFriction;
        }
        
    }

}

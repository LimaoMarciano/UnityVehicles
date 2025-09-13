using UnityEditor;
using UnityEngine;

namespace UnityVehicles.SimpleCar
{
    public enum DriveTrainType
    {
        FWD,
        RWD,
        AWD
    }

    public class SimpleCar : MonoBehaviour
    {
    
        Rigidbody rb;
        public Vector3 CenterOfMass;
        public DriveTrainType DriveTrain;
        [Header("Steering")]
        public float TurnRadius = 10f;
        public float SteeringWheelRange = 900f;

        [Header("Engine")]
        public float HorsePower = 78f;
        public float RpmRange = 9000f;
        public float IdleRpm = 800f;
        public AnimationCurve PowerCurve;

        [Header("GearBox")]
        public float[] GearRatios = new float[5] { 4.27f, 2.35f, 1.48f, 1.05f, 0.8f};
        public float ReverseGearRatio = 3.31f;
        public float DifferentialGearRatio = 4.87f;

        
        [Header("Wheels")]
        public WheelCollider FrontRightWheel;
        public WheelCollider FrontLeftWheel;
        public WheelCollider RearRightWheel;
        public WheelCollider RearLeftWheel;

        //Input
        [HideInInspector]
        [Range(-1f,1f)]
        public float SteeringInput = 0f;
        [HideInInspector]
        [Range(0f, 1f)]
        public float AcceleratorInput = 0f;
        
        float wheelBase;
        float rearAxleTrack;
        WheelCollider[] drivenWheels;
        int currentGear = 1;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            rb = GetComponent<Rigidbody>();
            rb.centerOfMass = CenterOfMass;

            (wheelBase, rearAxleTrack) = SetWheelBaseAndRearAxleTrack();

            switch(DriveTrain)
            {
                case DriveTrainType.FWD:
                    drivenWheels = new WheelCollider[2];
                    drivenWheels[0] = FrontLeftWheel;
                    drivenWheels[1] = FrontRightWheel;
                    break;
                case DriveTrainType.RWD:
                    drivenWheels = new WheelCollider[2];
                    drivenWheels[0] = RearLeftWheel;
                    drivenWheels[1] = RearRightWheel;
                    break;
                case DriveTrainType.AWD:
                    drivenWheels = new WheelCollider[4];
                    drivenWheels[0] = FrontLeftWheel;
                    drivenWheels[1] = FrontRightWheel;
                    drivenWheels[2] = RearLeftWheel;
                    drivenWheels[3] = RearRightWheel;
                break;
            }
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            ApplySteering(SteeringInput);

            float wheelAverageRpm = Mathf.Min(IdleRpm,GetDrivenWheelsAverageRpm());
            float rpmCurrentRange = wheelAverageRpm / RpmRange;
            float currentPower = PowerCurve.Evaluate(Mathf.Clamp01(rpmCurrentRange)) * HorsePower;
            float torque = (currentPower / wheelAverageRpm) * GearRatios[currentGear] * DifferentialGearRatio * 5252f;
            ApplyTorqueToDrivenWheels(torque);
        }

        public void ApplySteering(float input)
        {
            Vector2 steeringAngles = CalculateAckermannSteering(input, wheelBase, TurnRadius, rearAxleTrack);
            FrontLeftWheel.steerAngle = steeringAngles.x;
            FrontRightWheel.steerAngle = steeringAngles.y;
        }

        void ApplyTorqueToDrivenWheels(float torque)
        {
            foreach (WheelCollider wheelCollider in drivenWheels)
            {
                wheelCollider.motorTorque = torque * AcceleratorInput;             
            }

            Debug.Log(torque * AcceleratorInput);
        }

        /// <summary>
        /// Calculates wheel base (distance between car's two axles) and rear axle track (distance between rear wheels).
        /// This is necessary for Ackermann steering calculation
        /// </summary>
        /// <returns>WheelBase, RearAxleTrack</returns>
        (float,float) SetWheelBaseAndRearAxleTrack()
        {
            Vector3 frontAxleMidPoint = (FrontLeftWheel.transform.position + FrontRightWheel.transform.position) / 2f;
            Vector3 rearAxleMidPoint = (RearLeftWheel.transform.position + RearRightWheel.transform.position) / 2f;
            wheelBase = Vector3.Distance(frontAxleMidPoint, rearAxleMidPoint);
            rearAxleTrack = Vector3.Distance(RearLeftWheel.transform.position, RearRightWheel.transform.position);

            return (wheelBase, rearAxleTrack);
        }

        /// <summary>
        /// Calculate steering angle. Input value is in range between -1 (left) and +1 (right).
        /// The maximum turn radius is set on turnRadius variable.
        /// </summary>
        /// <param name="steeringInput">Value between -1 (left) and +1 (right)</param>
        Vector2 CalculateAckermannSteering(float steeringInput, float wheelBase, float turnRadius, float rearAxleTrack)
        {
            Vector2 steeringAngles = Vector2.zero;

            if (steeringInput > 0) //Turning right
            {
                steeringAngles.x = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + (rearAxleTrack / 2))) * steeringInput;
                steeringAngles.y = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius - (rearAxleTrack / 2))) * steeringInput;
            }
            else if (steeringInput < 0) //Turning left
            {
                steeringAngles.x = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius - (rearAxleTrack / 2))) * steeringInput;
                steeringAngles.y = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + (rearAxleTrack / 2))) * steeringInput;
            }

            return steeringAngles;
        }

        float GetDrivenWheelsAverageRpm ()
        {
            float wheelRpmAvg = 0f;
            foreach (WheelCollider wheelCollider in drivenWheels)
            {
                wheelRpmAvg += wheelCollider.rpm;
            }

            wheelRpmAvg = Mathf.Max(IdleRpm,wheelRpmAvg) / drivenWheels.Length;

            return wheelRpmAvg;
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 gizmoPos = transform.TransformPoint(CenterOfMass);
            Gizmos.DrawWireSphere(gizmoPos, 0.1f);
            //Gizmos.DrawSphere(gizmoPos, 0.1f);
        
        }
    }
}

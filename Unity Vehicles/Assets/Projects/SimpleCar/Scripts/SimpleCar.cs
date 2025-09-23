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

        public Vector3 CenterOfMassOffset;
        public DriveTrainType DriveTrain;

        [Header("Steering")]
        public float TurnRadius = 10f;
        public float SteeringWheelRange = 900f;

        [Header("Engine")]
        public float HorsePower = 78f;
        public float RpmRange = 9000f;
        public float IdleRpm = 500f;
        public float EngineBrake = 100f;
        public AnimationCurve PowerCurve;

        [Header("GearBox")]
        public float[] GearRatios = new float[5] { 4.27f, 2.35f, 1.48f, 1.05f, 0.8f };
        public float ReverseGearRatio = 3.31f;
        public float DifferentialGearRatio = 4.87f;
        [Range(0f, 1f)] public float DifferentialLock = 0f;

        [Header("Brakes")]
        public float BrakePower = 1000f;
        [Range(0f, 1f)]
        public float BrakeBias = 0.5f;

        [Header("Wheels")]
        public SimpleCarWheel FrontRightWheel;
        public SimpleCarWheel FrontLeftWheel;
        public SimpleCarWheel RearRightWheel;
        public SimpleCarWheel RearLeftWheel;

        //Input
        [HideInInspector] public float SteeringInput = 0f;
        [HideInInspector] public float AcceleratorInput = 0f;
        [HideInInspector] public float BrakesInput = 0f;

        public int CurrentGear { get; private set; } = 0;
        public float EngineRpm { get; private set; } = 0f;
        public float CurrentSpeed { get; private set; } = 0f;
        public float EngineTorque { get; private set; } = 0f;
        public float DriveTrainTorque { get; private set; } = 0f;
        
        float wheelBase;
        float rearAxleTrack;
        float drivetrainEfficiency = 1f;
        SimpleCarWheel[] drivenWheels;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            rb = GetComponent<Rigidbody>();
            rb.centerOfMass = CenterOfMassOffset;

            (wheelBase, rearAxleTrack) = GetWheelBaseAndRearAxleTrack();

            switch(DriveTrain)
            {
                case DriveTrainType.FWD:
                    drivenWheels = new SimpleCarWheel[2];
                    drivenWheels[0] = FrontLeftWheel;
                    drivenWheels[1] = FrontRightWheel;
                    drivetrainEfficiency = 0.92f;
                    break;
                case DriveTrainType.RWD:
                    drivenWheels = new SimpleCarWheel[2];
                    drivenWheels[0] = RearLeftWheel;
                    drivenWheels[1] = RearRightWheel;
                    drivetrainEfficiency = 0.88f;
                    break;
                case DriveTrainType.AWD:
                    drivenWheels = new SimpleCarWheel[4];
                    drivenWheels[0] = FrontLeftWheel;
                    drivenWheels[1] = FrontRightWheel;
                    drivenWheels[2] = RearLeftWheel;
                    drivenWheels[3] = RearRightWheel;
                    drivetrainEfficiency = 0.85f;
                    break;
            }
        }

        // Update is called once per frame
        void FixedUpdate()
        {          
            ApplySteering(SteeringInput);
            ApplyBrakes(BrakesInput);
            ApplyTorqueToDrivenWheels(AcceleratorInput);

            /*
            EngineRpm = GetDrivenWheelsAverageRpm() * GearRatios[CurrentGear] * DifferentialGearRatio;
            EngineRpm = Mathf.Max(IdleRpm, EngineRpm);
            currentPower = PowerCurve.Evaluate(Mathf.Clamp01(EngineRpm / RpmRange)) * HorsePower;
            EngineTorque = (currentPower * 5252f) / EngineRpm;
            ApplyTorqueToDrivenWheels(EngineTorque * GearRatios[CurrentGear] * DifferentialGearRatio);
            */
            CalculateCurrentSpeed();
        }

        void ApplySteering(float input)
        {
            Vector2 steeringAngles = CalculateAckermannSteering(input, wheelBase, TurnRadius, rearAxleTrack);
            FrontLeftWheel.WheelCollider.steerAngle = steeringAngles.x;
            FrontRightWheel.WheelCollider.steerAngle = steeringAngles.y;
        }

        void ApplyBrakes(float input)
        {
            FrontLeftWheel.WheelCollider.brakeTorque = FrontRightWheel.WheelCollider.brakeTorque = Mathf.Max(0f, BrakePower * BrakeBias * input);
            RearLeftWheel.WheelCollider.brakeTorque = RearRightWheel.WheelCollider.brakeTorque = Mathf.Max(0f, BrakePower * (1f- BrakeBias) * input);
        }

        void ApplyTorqueToDrivenWheels(float acceleratorInput)
        {
            EngineRpm = GetDrivenWheelsAverageRpm() * GearRatios[CurrentGear] * DifferentialGearRatio;
            EngineRpm = Mathf.Max(IdleRpm, EngineRpm);

            float currentRpmRange = Mathf.Clamp01(EngineRpm / RpmRange);

            if (AcceleratorInput > 0)
            {
                float currentPower = PowerCurve.Evaluate(currentRpmRange) * HorsePower;
                EngineTorque = currentPower * 5252f / EngineRpm * acceleratorInput;
            } 
            else
            {
                EngineTorque = -EngineBrake * currentRpmRange;
            }

            DriveTrainTorque = EngineTorque * GearRatios[CurrentGear] * DifferentialGearRatio * drivetrainEfficiency;

            foreach (SimpleCarWheel wheelCollider in drivenWheels)
            {
                wheelCollider.WheelCollider.motorTorque = DriveTrainTorque * 0.5f;
            }
        }

        void CalculateCurrentSpeed()
        {
            if (GearRatios[CurrentGear] == 0f || DifferentialGearRatio == 0f)
            {
                CurrentSpeed = 0f;
            } 
            else
            {
                CurrentSpeed = EngineRpm / GearRatios[CurrentGear] / DifferentialGearRatio / 60f * FrontLeftWheel.WheelCollider.radius * 2f * Mathf.PI;
            }
        }

        float GetDrivenWheelsAverageRpm ()
        {
            float wheelRpmAvg = 0f;
            foreach (SimpleCarWheel simpleCarWheel in drivenWheels)
            {
                wheelRpmAvg += Mathf.Abs(simpleCarWheel.WheelCollider.rpm);
            }

            wheelRpmAvg = wheelRpmAvg / drivenWheels.Length;
            return wheelRpmAvg;
        }

        public void IncreaseGear()
        {
            if (CurrentGear < GearRatios.Length - 1)
            {
                CurrentGear += 1;
            }
        }

        public void DecreaseGear()
        {
            if (CurrentGear > 0)
            {
                CurrentGear -= 1;
            }
        }

        /// <summary>
        /// Calculates wheel base (distance between car's two axles) and rear axle track (distance between rear wheels).
        /// This is necessary for Ackermann steering calculation
        /// </summary>
        /// <returns>WheelBase, RearAxleTrack</returns>
        (float, float) GetWheelBaseAndRearAxleTrack()
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

        private void OnDrawGizmosSelected()
        {
            Vector3 gizmoPos = transform.TransformPoint(CenterOfMassOffset);
            Gizmos.DrawWireSphere(gizmoPos, 0.1f);    
        }
    }
}

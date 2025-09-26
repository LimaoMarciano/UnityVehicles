using UnityEditor;
using UnityEngine;

namespace UnityVehicles.SimpleCar
{
    public enum DriveTrainType
    {
        FWD,
        RWD
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
        public float UnclutchedResponse = 0.8f;
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
        [HideInInspector]
        public float ClutchInput
        {
            set
            { clutchGrip = Mathf.Clamp01(1 - value); }
            get
            { return 1 - clutchGrip; }
        }

        public int CurrentGear { get; private set; } = 0;
        public float EngineRpm { get; private set; } = 0f;
        public float ActualRpm { get; private set; } = 0f;
        public float Speedometer { get; private set; } = 0f;
        public float EngineTorque { get; private set; } = 0f;
        public float DriveTrainTorque { get; private set; } = 0f;
       
        float wheelBase;
        float rearAxleTrack;
        float drivetrainEfficiency = 1f;
        SimpleCarWheel[] drivenWheels;

        float clutchGrip = 1f;
        float clutchSmoothDampVel = 0f;
        float unclutchedRpm = 0f;

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
            }
        }

        void FixedUpdate()
        {
            ApplySteering(SteeringInput);
            ApplyBrakes(BrakesInput);
            ApplyTorqueToDrivenWheels(AcceleratorInput);

            SetSpeedometerReading();
        }

        /// <summary>
        /// Applies steering angle to front wheels based on Ackermann geometry.
        /// </summary>
        /// <param name="input">Steering input in -1/+1 range</param>
        /// <see cref="Ackerman Steering Explained" href="https://www.youtube.com/watch?v=oYMMdjbmQXc"/>
        void ApplySteering(float input)
        {
            
            Vector2 steeringAngles = CalculateAckermannSteering(input, wheelBase, TurnRadius, rearAxleTrack);
            FrontLeftWheel.WheelCollider.steerAngle = steeringAngles.x;
            FrontRightWheel.WheelCollider.steerAngle = steeringAngles.y;
        }

        /// <summary>
        /// Applies brakes torque according to set brake bias and handbrake torque
        /// </summary>
        /// <param name="input"></param>
        void ApplyBrakes(float input)
        {
            FrontLeftWheel.WheelCollider.brakeTorque = FrontRightWheel.WheelCollider.brakeTorque = Mathf.Max(0f, BrakePower * BrakeBias * input);
            RearLeftWheel.WheelCollider.brakeTorque = RearRightWheel.WheelCollider.brakeTorque = Mathf.Max(0f, BrakePower * (1f- BrakeBias) * input);
        }

        /// <summary>
        /// Applies torque to the driven wheels based on current engine RPM. This includes engine braking when not accelerating.
        /// </summary>
        /// <param name="acceleratorInput"></param>
        /// <see cref="What is Engine Braking" href="https://www.youtube.com/watch?v=o8Cta2cC2Co"/>
        void ApplyTorqueToDrivenWheels(float acceleratorInput)
        {
            /* Engine RPM when completly unclutched using a fake smoothed function that follows how much the accelerator is pressed.
             * Min value is set to idle rpm, since realistically, the engine would stall and turn off below that without clutch input.
             * This hack kinda simulates a trained driver behavior, where you would press the clutch at low speed or at a stop.
             */
            float targetRpm = Mathf.Max(IdleRpm, RpmRange * Mathf.Clamp01(acceleratorInput));
            unclutchedRpm = Mathf.SmoothDamp(unclutchedRpm, targetRpm, ref clutchSmoothDampVel, UnclutchedResponse);

            /* Engine RPM when clutched, completly locked to the driven wheels
             */
            float clutchedRpm = GetDrivenWheelsAverageRpm() * GearRatios[CurrentGear] * DifferentialGearRatio;
            
            /*We also store the actual RPM without any clamping to use further for engine braking calculation.
             */
            ActualRpm = clutchedRpm;

            /*Final RPM is interpolated between clutched and the fake unclutched behavior depending on how much the clutch is pressed.
             *Like the fake unclutched RPM, we set idle rpm as minimum.
             *(This is a very simplistic aproximation of the slipping nature between engine and drivetrain when clutch is halfway pressed)
             */
            EngineRpm = Mathf.Lerp(unclutchedRpm, Mathf.Max(IdleRpm, Mathf.Abs(ActualRpm)), clutchGrip);

            /*If we have any accelerator input, calculate torque based on power curve;
             *If not, we apply negative torque proportional to current RPM, simulating an engine braking effect.
             *In a real car, engine braking comes from friction between the moving parts of the engine/drivetrain and vacuum inside the engine chamber when not accelerating.
             *Since this braking force goes throught the drivetrain, it's multiplied by gear ratios, being stronger on lower gears. It's also stronger on higher RPMs (by friction)
            */
            if (AcceleratorInput > 0.01f)
            {
                float currentRpmRange = Mathf.Clamp01(EngineRpm / RpmRange);
                float currentPower = PowerCurve.Evaluate(currentRpmRange) * HorsePower;
                EngineTorque = currentPower * 5252f / EngineRpm * acceleratorInput;
            } 
            else
            {
                /* Engine braking
                 */
                EngineTorque = -EngineBrake * (ActualRpm / RpmRange);    
            }

            /* The torque produced by the engine is multiplied by the current gear and diffential.
             * We also apply an efficiency value, since a real car suffers some energy dissipation through the drivetrain.
             * How much of the produced torque is actually transmitted to the wheel depends on how much the clutch is pressed.
             */
             DriveTrainTorque = EngineTorque * GearRatios[CurrentGear] * DifferentialGearRatio * drivetrainEfficiency * clutchGrip;

            /* This is a approximation of how a open differential distributes torque between the driven wheels. 
             */
            foreach (SimpleCarWheel wheelCollider in drivenWheels)
            {
                wheelCollider.WheelCollider.motorTorque = DriveTrainTorque * 0.5f;
            }
        }

        /// <summary>
        /// Sets speedometer speed reading based on drivetrain RPM.
        /// </summary>
        void SetSpeedometerReading()
        {
            /* Most road cars calculate speed by measuring the drivetrain rotation speed and wheel radius to deduce the tire surface speed 
             * (that's why changing tire radius throws off speed readings on a real car)
             * Conclusion: the speedometer reads the speed of the spinning wheels surface, not the actual physical speed that the car is travelling.
             * This is just a touch to make speed readings more immersive, since a burnout in real life would cause the speedometer to spike even though the car is not moving.
             * Dials going crazy are cool for the player :D
             */
            
            if (GearRatios[CurrentGear] == 0f || DifferentialGearRatio == 0f)
            {
                Speedometer = 0f;
            } 
            else
            {
                Speedometer = EngineRpm / GearRatios[CurrentGear] / DifferentialGearRatio / 60f * FrontLeftWheel.WheelCollider.radius * 2f * Mathf.PI * clutchGrip;
            }
        }

        /// <summary>
        /// Returns average RPM of the driven wheels. Useful to deduce engine RPM on an open differential car.
        /// </summary>
        /// <returns></returns>
        float GetDrivenWheelsAverageRpm ()
        {

            /* Engine and wheels are locked together when the car is clutched. Getting an average of the driven wheels
             * is a good approximation of engine RPM in a car with an open differential, but innacurate for locked or limited slip differentials.
             */
            float wheelRpmAvg = 0f;
            foreach (SimpleCarWheel simpleCarWheel in drivenWheels)
            {
                wheelRpmAvg += simpleCarWheel.WheelCollider.rpm;
            }

            wheelRpmAvg = wheelRpmAvg / drivenWheels.Length;
            return wheelRpmAvg;
        }

        /// <summary>
        /// Increases current gear sequentially
        /// </summary>
        public void IncreaseGear()
        {
            if (CurrentGear < GearRatios.Length - 1)
            {
                CurrentGear += 1;
            }
        }

        /// <summary>
        /// Decreases current gear sequentially
        /// </summary>
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
        /// Calculate steering angle for both wheels. Input value is in range between -1 (left) and +1 (right).
        /// The maximum turn radius is set by the turnRadius variable.
        /// </summary>
        /// <param name="steeringInput">Value between -1 (left) and +1 (right)</param>
        /// <returns>Steering angle for both wheels packaged in a Vector2 (x = left, y = right)</returns>
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

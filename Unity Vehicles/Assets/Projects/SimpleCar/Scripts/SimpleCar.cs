using UnityEngine;
using System;

namespace UnityVehicles.SimpleCar
{
    public enum DriveTrainType
    {
        Fwd,
        Rwd,
        Awd
    }

    public enum Axle
    {
        Front,
        Rear
    }

    public class SimpleCar : MonoBehaviour
    {
        private Rigidbody rb;
        
        public ChassisData chassisData;
        public SuspensionSettings suspension;
        public TiresSettings tires;
        
        public EngineData engineData;
        public GearboxData gearboxData;
        public AntirollBarData antirollBarData;
        public BrakesData brakesData;

        [Header("Wheels")]
        public SimpleCarWheel frontLeftWheel;
        public SimpleCarWheel frontRightWheel;
        public SimpleCarWheel rearLeftWheel;
        public SimpleCarWheel rearRightWheel;

        //Input
        [NonSerialized] public float SteeringInput;
        [NonSerialized] public float AcceleratorInput;
        [NonSerialized] public float BrakesInput;
        [NonSerialized] public float HandbrakeInput;
        
        public float clutchInput
        {
            get => 1 - clutchGrip;
            set => clutchGrip = Mathf.Clamp01(1 - value);
        }

        public int currentGear { get; private set; }
        public float currentGearRatio { get; private set; }
        public float engineRpm { get; private set; }
        public float driveTrainRpm { get; private set; }
        public float speedometer { get; private set; }
        public float engineTorque { get; private set; }
        public float driveTrainTorque { get; private set; }
        public float frSuspensionTravel { get; private set; }
        public float flSuspensionTravel { get; private set; }
        public float rrSuspensionTravel { get; private set; } 
        public float rlSuspensionTravel { get; private set; }

        private float wheelBase;
        private float rearAxleTrack;
        private float drivetrainEfficiency = 1f;
        SimpleCarWheel[] drivenWheels;

        private float clutchGrip = 1f;
        private float clutchSmoothDampVel;
        private float unclutchedRpm;
        private float wheelAverageRpm;

        private void Start()
        {
            
            rb = GetComponent<Rigidbody>();
            rb.mass = chassisData.physicalProperties.mass;
            rb.linearDamping = chassisData.physicalProperties.linearDamping;
            rb.angularDamping = chassisData.physicalProperties.angularDamping;
            rb.centerOfMass = chassisData.physicalProperties.centerOfMass;
            
            currentGearRatio = gearboxData.gearRatios[currentGear];
            
            (wheelBase, rearAxleTrack) = GetWheelBaseAndRearAxleTrack();
            
            SetWheelsPosition();

            switch(chassisData.driveTrainType)
            {
                case DriveTrainType.Fwd:
                    drivenWheels = new SimpleCarWheel[2];
                    drivenWheels[0] = frontLeftWheel;
                    drivenWheels[1] = frontRightWheel;
                    drivetrainEfficiency = 0.92f;
                    break;
                case DriveTrainType.Rwd:
                    drivenWheels = new SimpleCarWheel[2];
                    drivenWheels[0] = rearLeftWheel;
                    drivenWheels[1] = rearRightWheel;
                    drivetrainEfficiency = 0.88f;
                    break;
                case DriveTrainType.Awd:
                    drivenWheels = new SimpleCarWheel[4];
                    drivenWheels[0] = frontLeftWheel;
                    drivenWheels[1] = frontRightWheel;
                    drivenWheels[2] = rearLeftWheel;
                    drivenWheels[3] = rearRightWheel;
                    drivetrainEfficiency = 0.83f;
                    break;
            }
            drivenWheels[0].wheelCollider.ConfigureVehicleSubsteps(10,4,4);
        }

        private void FixedUpdate()
        {
            UpdateWheelsValues();
            
            ApplySteering(SteeringInput);
            ApplyBrakes(BrakesInput, HandbrakeInput);
            ApplyTorqueToDrivenWheels(AcceleratorInput);
            ApplyAntirollBarForce(Axle.Front, antirollBarData.frontStrength);
            ApplyAntirollBarForce(Axle.Rear, antirollBarData.rearStrength);

            SetSpeedometerReading();
        }

        /// <summary>
        /// Applies steering angle to front wheels based on Ackermann geometry.
        /// </summary>
        /// <param name="input">Steering input in -1/+1 range</param>
        /// <see href="https://www.youtube.com/watch?v=oYMMdjbmQXc">
        ///     <cref>Ackerman Steering Explained</cref>
        /// </see>
        private void ApplySteering(float input)
        {
            
            var steeringAngles = CalculateAckermannSteering(input, wheelBase, chassisData.wheelMount.turnRadius, rearAxleTrack);
            frontLeftWheel.wheelCollider.steerAngle = steeringAngles.x;
            frontRightWheel.wheelCollider.steerAngle = steeringAngles.y;
        }

        /// <summary>
        /// Applies brakes torque according to set brake bias and handbrake torque
        /// </summary>
        /// <param name="input">Brakes input</param>
        /// <param name="handbrakeInput">Handbrake input</param>
        private void ApplyBrakes(float input, float handbrakeInput)
        {
            frontLeftWheel.wheelCollider.brakeTorque = frontRightWheel.wheelCollider.brakeTorque = Mathf.Max(0f, brakesData.torque * brakesData.bias * input);
            rearLeftWheel.wheelCollider.brakeTorque = rearRightWheel.wheelCollider.brakeTorque = Mathf.Max(0f, brakesData.torque * (1f- brakesData.bias) * input) + handbrakeInput * brakesData.handbrakeTorque;
        }

        /// <summary>
        /// Applies torque to the driven wheels based on current engine RPM. This includes engine braking when not accelerating.
        /// </summary>
        /// <param name="acceleratorInput"></param>
        /// <see href="https://www.youtube.com/watch?v=o8Cta2cC2Co">
        ///     <cref>What is Engine Braking</cref>
        /// </see>
        private void ApplyTorqueToDrivenWheels(float acceleratorInput)
        {
            if (currentGear == -1)
            {
                clutchGrip = 0f;
            }

            /* Engine RPM when completely unclutched using a fake smoothed function that follows how much the accelerator is pressed.
             * Min value is set to idle rpm, since realistically, the engine would stall and turn off below that without clutch input.
             * This hack kinda simulates a trained driver behavior, where you would press the clutch at low speed or at a stop.
             */
            if (clutchGrip < 0.99f)
            {
                var targetRpm = Mathf.Max(engineData.idleRpm, engineData.rpmRange * Mathf.Clamp01(acceleratorInput));
                unclutchedRpm = Mathf.SmoothDamp(unclutchedRpm, targetRpm, ref clutchSmoothDampVel, engineData.unclutchedResponse);
            } 
            else
            {
                /* Keep updating unclutched rpm to match EngineRpm, so when we disengage clutch, the fake RPM starts where the real RPM was
                 * This avoids a weird "RPM reset" when transitioning from clutched to unclutched.
                 */
                unclutchedRpm = engineRpm;
                clutchSmoothDampVel = 0f;
            }

            /* Engine RPM when clutched, completely locked to the driven wheels
             */
            wheelAverageRpm = GetDrivenWheelsAverageRpm();
            driveTrainRpm = wheelAverageRpm * currentGearRatio * gearboxData.differentialGearRatio;

            /*Final RPM is interpolated between clutched and the fake unclutched behavior depending on how much the clutch is pressed.
             *Like the fake unclutched RPM, we set idle rpm as minimum.
             *(This is a very simplistic approximation of the slipping nature between engine and drivetrain when clutch is halfway pressed)
             */
            engineRpm = Mathf.Lerp(unclutchedRpm, Mathf.Max(engineData.idleRpm, Mathf.Abs(driveTrainRpm)), clutchGrip);

            /*If we have any accelerator input, calculate torque based on power curve;
             *If not, we apply negative torque proportional to current RPM, simulating an engine braking effect.
             *In a real car, engine braking comes from friction between the moving parts of the engine/drivetrain and vacuum inside the engine chamber when not accelerating.
             *Since this braking force goes through the drivetrain, it's multiplied by gear ratios, being stronger on lower gears. It's also stronger on higher RPMs (by friction)
            */
            if (AcceleratorInput > 0.01f)
            {
                var currentRpmRange = Mathf.Clamp01(engineRpm / engineData.rpmRange);
                var currentPower = engineData.powerCurve.Evaluate(currentRpmRange) * engineData.horsePower;
                engineTorque = currentPower * 5252f / engineRpm * acceleratorInput;
            } 
            else
            {
                /* Engine braking
                 */
                engineTorque = -engineData.engineBrake * (driveTrainRpm / engineData.rpmRange);    
            }

            /* The torque produced by the engine is multiplied by the current gear and differential.
             * We also apply an efficiency value, since a real car suffers some energy dissipation through the drivetrain.
             * How much of the produced torque is actually transmitted to the wheel depends on how much the clutch is pressed.
             */
             driveTrainTorque = engineTorque * currentGearRatio * gearboxData.differentialGearRatio * drivetrainEfficiency * clutchGrip;

            /* This is a approximation of how a open differential distributes torque between the driven wheels. 
             */
            foreach (var wheelCollider in drivenWheels)
            {
                wheelCollider.wheelCollider.motorTorque = driveTrainTorque / drivenWheels.Length;
            }
        }

        /// <summary>
        /// Applies antiroll force to the selected wheel axle
        /// </summary>
        /// <param name="axle">Which axle will the force be applied</param>
        /// <param name="strength">How strong is the max roll reaction force on the opposing wheel</param>
        /// <see href="https://www.youtube.com/watch?v=_liGnV3PTiQ">
        ///     <cref>How Anti-Roll Bars Work</cref>
        /// </see>
        private void ApplyAntirollBarForce (Axle axle, float strength)
        {
            /* When the suspension on one side compresses, the antiroll bar applies compression force to the opposing side suspension.
             * This creates a force that fights against the car body leaning to the sides, increasing roll stability.
             * 
             * Besides protecting the car against rolling, the antiroll bars have a big influence on handling.
             * By fighting against roll on cornering, the weight is distributed more evenly between left and right tires, resulting in better cornering grip and stability.
             * There's an optimal balance, though. 
             * Too strong antiroll bars can compromise suspension independence, resulting in lower grip and worse bump absorption, making the car twitchy on uneven terrain.
             * On cornering, it can also cause the outside wheel to lift off the ground in extreme cases.
             * 
             * Balance between front and rear roll-bars are also important. A stiffer front increases understeer, a stiffer rear increases oversteer.
             * Changing this balance is very helpful to achieve the desired handling characteristic.
             */
            
            float leftTravel;
            SimpleCarWheel leftWheel;
            SimpleCarWheel rightWheel;

            if (axle == Axle.Front)
            {
                leftWheel = frontLeftWheel;
                leftTravel = leftWheel.suspensionTravel;

                rightWheel = frontRightWheel;
            }
            else
            {
                leftWheel = rearLeftWheel;
                leftTravel = leftWheel.suspensionTravel;

                rightWheel = rearRightWheel;
            }

            var rightTravel = rightWheel.suspensionTravel;

            var antiRollForce = (leftTravel - rightTravel) * strength;

            if (leftWheel.isGrounded)
            {
                rb.AddForceAtPosition(leftWheel.transform.up * -antiRollForce, leftWheel.transform.position, ForceMode.Force);
            }

            if (rightWheel.isGrounded)
            {
                rb.AddForceAtPosition(rightWheel.transform.up * antiRollForce, rightWheel.transform.position, ForceMode.Force);
            }
        }

        /// <summary>
        /// Updates wheels internal values. This should be called at the beginning of FixedUpdate
        /// to guarantee that all wheels values are up to date for subsequent calculations.
        /// </summary>
        void UpdateWheelsValues()
        {
            frontRightWheel.UpdateValues();
            frontLeftWheel.UpdateValues();
            rearRightWheel.UpdateValues();
            rearLeftWheel.UpdateValues();
        }

        /// <summary>
        /// Sets speedometer speed reading based on drivetrain RPM.
        /// </summary>
        void SetSpeedometerReading()
        {
            /* Most road cars calculate speed by measuring the drivetrain rotation speed and wheel radius to deduce the tire surface speed 
             * (that's why changing tire radius throws off speed readings on a real car)
             * Conclusion: the speedometer reads the speed of the spinning wheels surface, not the actual physical speed that the car is traveling.
             * This is just a touch to make speed readings more immersive, since a burnout in real life would cause the speedometer to spike even though the car is not moving.
             * Dials going crazy are cool for the player :D
             */
            speedometer = wheelAverageRpm / 60f * drivenWheels[0].wheelCollider.radius * 2f * Mathf.PI;
        }

        /// <summary>
        /// Returns average RPM of the driven wheels. Useful to deduce engine RPM on an open differential car.
        /// </summary>
        /// <returns></returns>
        float GetDrivenWheelsAverageRpm ()
        {

            /* Engine and wheels are locked together when the car is clutched. Getting an average of the driven wheels
             * is a good approximation of engine RPM in a car with an open differential, but inaccurate for locked or limited slip differentials.
             */
            float wheelRpmAvg = 0f;
            foreach (SimpleCarWheel simpleCarWheel in drivenWheels)
            {
                wheelRpmAvg += simpleCarWheel.wheelCollider.rpm;
            }

            wheelRpmAvg = wheelRpmAvg / drivenWheels.Length;
            return wheelRpmAvg;
        }

        /// <summary>
        /// Sets gear to a specific value. First gear is zero.
        /// -1 is neutral gear and -2 is reverse gear. Values are clamped between existing gears.
        /// </summary>
        /// <param name="gear"></param>
        private void SetGear(int gear)
        {
            if (wheelAverageRpm > 2f && gear == -2)
            {
                return;
            }

            /* If the gear we're changing into would cause the RPM to be above the limit, the gear change is rejected. 
             */
            if (gear >= 0)
            {
                float predictedRpm =  gearboxData.gearRatios[Mathf.Clamp(gear, 0, gearboxData.gearRatios.Length - 1)] * gearboxData.differentialGearRatio * wheelAverageRpm;

                if (Mathf.Abs(predictedRpm) > engineData.rpmRange)
                {
                    return;
                }
            }

            currentGear = Mathf.Clamp(gear, -2, gearboxData.gearRatios.Length - 1);
            
            if (gear >= 0)
            {
                currentGearRatio = gearboxData.gearRatios[currentGear];
            }
            else if (gear == -1)
            {
                //Neutral
                currentGearRatio = 0f;
            }
            else
            {
                //Reverse
                currentGearRatio = -gearboxData.reverseGearRatio;
            }
        }

        /// <summary>
        /// Increases current gear sequentially
        /// </summary>
        public void IncreaseGear()
        {
            SetGear(currentGear + 1);
        }

        /// <summary>
        /// Decreases current gear sequentially
        /// </summary>
        public void DecreaseGear()
        {
            SetGear(currentGear - 1);
        }

        /// <summary>
        /// Calculates wheelbase (distance between car's two axles) and rear axle track (distance between rear wheels).
        /// This is necessary for Ackermann steering calculation
        /// </summary>
        /// <returns>WheelBase, RearAxleTrack</returns>
        (float, float) GetWheelBaseAndRearAxleTrack()
        {
            frontLeftWheel.wheelCollider.GetWorldPose(out var flWheelPos, out _);
            frontLeftWheel.wheelCollider.GetWorldPose(out var frWheelPos, out _);
            rearLeftWheel.wheelCollider.GetWorldPose(out var rlWheelPos, out _);
            rearRightWheel.wheelCollider.GetWorldPose(out var rrWheelPos, out _);
            
            var frontAxleMidPoint = (flWheelPos + frWheelPos) / 2f;
            var rearAxleMidPoint = (rlWheelPos + rrWheelPos) / 2f;
            wheelBase = Vector3.Distance(frontAxleMidPoint, rearAxleMidPoint);
            rearAxleTrack = Vector3.Distance(rearLeftWheel.transform.position, rearRightWheel.transform.position);
            

            return (wheelBase, rearAxleTrack);
        }

        /// <summary>
        /// Calculate steering angle for both wheels. Input value is in range between -1 (left) and +1 (right).
        /// The maximum turn radius is set by the turnRadius variable.
        /// </summary>
        /// <param name="steeringInput">Value between -1 (left) and +1 (right)</param>
        /// <returns>Steering angle for both wheels packaged in a Vector2 (x = left, y = right)</returns>
        private Vector2 CalculateAckermannSteering(float steeringInput, float wheelBase, float turnRadius, float rearAxleTrack)
        {
            var steeringAngles = Vector2.zero;

            switch (steeringInput)
            {
                //Turning right
                case > 0:
                    steeringAngles.x = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + (rearAxleTrack / 2))) * steeringInput;
                    steeringAngles.y = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius - (rearAxleTrack / 2))) * steeringInput;
                    break;
                //Turning left
                case < 0:
                    steeringAngles.x = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius - (rearAxleTrack / 2))) * steeringInput;
                    steeringAngles.y = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + (rearAxleTrack / 2))) * steeringInput;
                    break;
            }

            return steeringAngles;
        }

        void SetWheelsPosition()
        {
            frontLeftWheel.SetWheelProperties(tires, suspension, chassisData.wheelMount, true, true);
            frontRightWheel.SetWheelProperties(tires, suspension, chassisData.wheelMount, true, false);
            rearLeftWheel.SetWheelProperties(tires, suspension, chassisData.wheelMount, false, true);
            rearRightWheel.SetWheelProperties(tires, suspension, chassisData.wheelMount, false, false);
        }
        
        private void OnDrawGizmosSelected()
        {
            Vector3 gizmoPos = transform.TransformPoint(chassisData.physicalProperties.centerOfMass);
            Gizmos.DrawWireSphere(gizmoPos, 0.1f);    
        }
    }
}

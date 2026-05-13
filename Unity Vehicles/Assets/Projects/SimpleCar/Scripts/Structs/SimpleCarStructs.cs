using UnityEngine;

namespace UnityVehicles.SimpleCar
{
    [System.Serializable]
    public struct EngineData
    {
        public float horsePower;
        public float rpmRange;
        public float idleRpm;
        public float engineBrake;
        public float unclutchedResponse;
        public AnimationCurve powerCurve;

        public EngineData(float horsePower, float rpmRange, float idleRpm, float engineBrake, float unclutchedResponse,
            AnimationCurve powerCurve)
        {
            this.horsePower = horsePower;
            this.rpmRange = rpmRange;
            this.idleRpm = idleRpm;
            this.engineBrake = engineBrake;
            this.unclutchedResponse = unclutchedResponse;
            this.powerCurve = powerCurve;
        }
    }

    [System.Serializable]
    public struct GearboxData
    {
        public float[] gearRatios;
        public float reverseGearRatio;
        public float differentialGearRatio;

        public GearboxData(float[] gearRatios, float reverseGearRatio, float differentialGearRatio)
        {
            this.gearRatios = gearRatios;
            this.reverseGearRatio = reverseGearRatio;
            this.differentialGearRatio = differentialGearRatio;
        }
    }

    [System.Serializable]
    public struct SuspensionData
    {
        public float spring;
        public float damper;
        public float distance;
        [Range(0, 1)] public float targetPosition;

        public SuspensionData(float spring, float damper, float distance, float targetPosition)
        {
            this.spring = spring;
            this.damper = damper;
            this.distance = distance;
            this.targetPosition = targetPosition;
        }
    }
    
    [System.Serializable]
    public struct SuspensionSettings
    {
        public SuspensionData front;
        public SuspensionData rear;

        public SuspensionSettings(SuspensionData front, SuspensionData rear)
        {
            this.front = front;
            this.rear = rear;
        }
    }
    
    [System.Serializable]
    public struct AntirollBarData
    {
        public float frontStrength;
        public float rearStrength;

        public AntirollBarData(float frontStrength, float rearStrength)
        {
            this.frontStrength = frontStrength;
            this.rearStrength = rearStrength;
        }
    }

    [System.Serializable]
    public struct BrakesData
    {
        public float torque;
        [Range(0, 1)] public float bias;
        public float handbrakeTorque;

        public BrakesData(float torque, float bias, float handbrakeTorque)
        {
            this.torque = torque;
            this.bias = bias;
            this.handbrakeTorque = handbrakeTorque;
        }
    }

    [System.Serializable]
    public struct ChassisPhysicalPropertiesData
    {
        public float mass;
        public float linearDamping;
        public float angularDamping;
        public Vector3 centerOfMass;

        public ChassisPhysicalPropertiesData(float mass, float linearDamping, float angularDamping, Vector3 centerOfMass)
        {
            this.mass = mass;
            this.linearDamping = linearDamping;
            this.angularDamping = angularDamping;
            this.centerOfMass = centerOfMass;
        }
    }

    [System.Serializable]
    public struct WheelMountData
    {
        public Vector2 frontWheelsCenter;
        public Vector2 rearWheelsCenter;
        public float frontAxleWidth;
        public float rearAxleWidth;
        public float frontForceAppPointDistance;
        public float rearForceAppPointDistance;
        public float turnRadius;
        public float steeringRange;

        public WheelMountData(Vector2 frontWheelsCenter, Vector2 rearWheelsCenter, float frontAxleWidth, float rearAxleWidth, float frontForceAppPointDistance, float rearForceAppPointDistance, float turnRadius, float steeringRange)
        {
            this.frontWheelsCenter = frontWheelsCenter;
            this.rearWheelsCenter = rearWheelsCenter;
            this.frontAxleWidth = frontAxleWidth;
            this.rearAxleWidth = rearAxleWidth;
            this.frontForceAppPointDistance = frontForceAppPointDistance;
            this.rearForceAppPointDistance = rearForceAppPointDistance;
            this.turnRadius = turnRadius;
            this.steeringRange = steeringRange;
        }
    }
    
    [System.Serializable]
    public struct ChassisData
    {
        public ChassisPhysicalPropertiesData physicalProperties;
        public WheelMountData wheelMount;
        public DriveTrainType driveTrainType;

        public ChassisData(ChassisPhysicalPropertiesData physicalProperties, DriveTrainType driveTrainType, WheelMountData wheelMount)
        {
            this.physicalProperties = physicalProperties;
            this.wheelMount = wheelMount;
            this.driveTrainType = driveTrainType;
        }
    }

    [System.Serializable]
    public struct FrictionData
    {
        public float extremumSlip;
        public float extremumValue;
        public float asymptoteSlip;
        public float asymptoteValue;
        public float stiffness;

        public FrictionData(float extremumSlip, float extremumValue, float asymptoteSlip, float asymptoteValue, float stiffness)
        {
            this.extremumSlip = extremumSlip;
            this.extremumValue = extremumValue;
            this.asymptoteSlip = asymptoteSlip;
            this.asymptoteValue = asymptoteValue;
            this.stiffness = stiffness;
        }
    }

    [System.Serializable]
    public struct TireData
    {
        public float mass;
        public float radius;
        public float dampingRate;
        public FrictionData forwardFriction;
        public FrictionData sideFriction;

        public TireData(float mass, float radius, float dampingRate, FrictionData forwardFriction, FrictionData rearFriction)
        {
            this.mass = mass;
            this.radius = radius;
            this.dampingRate = dampingRate;
            this.forwardFriction = forwardFriction;
            this.sideFriction = rearFriction;
        }
    }

    [System.Serializable]
    public struct TiresSettings
    {
        public TireData front;
        public TireData rear;

        public TiresSettings(TireData front, TireData rear)
        {
            this.front = front;
            this.rear = rear;
        }
    }
}

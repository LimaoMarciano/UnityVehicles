using UnityEngine;
using UnityEngine.Serialization;

namespace UnityVehicles.SimpleCar
{
    [CreateAssetMenu(fileName = "CarSetupDataChassis", menuName = "SimpleCar/Setup/Chassis")]
    public class CarSetupDataChassis : ScriptableObject
    {
        public ChassisData chassisData = new ChassisData(
            new ChassisPhysicalPropertiesData(
                860f,
                0.01f,
                0.05f, 
                new Vector3(0f, -0.53f, 0.35f)
                ), 
            DriveTrainType.Fwd,
            new WheelMountData(new Vector2(1.262f, -0.603f),
                new Vector2(-1.431f, -0.603f), 
                1.504f, 
                1.504f, 
                0.3f, 
                0.3f, 
                10f,
                900f)
            );
    }
}

using UnityEngine;

namespace UnityVehicles.SimpleCar
{
    [CreateAssetMenu(fileName = "CarSetupSuspensionData", menuName = "SimpleCar/Setup/Suspension")]
    public class CarSetupDataSuspension : ScriptableObject
    {
        public SuspensionSettings suspension = new SuspensionSettings(
                new SuspensionData(11000f, 950f, 0.2f, 0.3f),
                new SuspensionData(10000f, 900f, 0.2f, 0.3f)
            );
    }
}
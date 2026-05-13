using UnityEngine;

namespace UnityVehicles.SimpleCar
{
    [CreateAssetMenu(fileName = "CarSetupDataGearbox", menuName = "SimpleCar/Setup/Gearbox")]
    public class CarSetupDataGearbox : ScriptableObject
    {
        public GearboxData gearbox = new GearboxData(new float[]{4.27f, 2.35f, 1.48f, 1.05f, 0.8f}, 3.31f, 4.87f);
    }
}

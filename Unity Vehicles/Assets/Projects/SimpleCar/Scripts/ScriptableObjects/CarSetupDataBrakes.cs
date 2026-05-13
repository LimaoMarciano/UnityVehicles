using UnityEngine;

namespace UnityVehicles.SimpleCar
{
    [CreateAssetMenu(fileName = "CarSetupDataBrakes", menuName = "SimpleCar/Setup/Brakes")]
    public class CarSetupDataBrakes : ScriptableObject
    {
        public BrakesData brakesData = new BrakesData(1700f,0.65f,2000);
    }
}

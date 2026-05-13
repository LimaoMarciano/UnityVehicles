using UnityEngine;

namespace UnityVehicles.SimpleCar
{
    [CreateAssetMenu(fileName = "CarSetupDataTire", menuName = "SimpleCar/Setup/Tires")]
    public class CarSetupDataTire : ScriptableObject
    {
        public TireData tires = new TireData(20f,0.28f,0.25f,new FrictionData(0.2f,0.7f,1f,0.65f,1f),new FrictionData(0.2f,0.75f,1f,0.7f,1f));
    }
}


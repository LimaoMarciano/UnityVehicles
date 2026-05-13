using UnityEngine;

namespace UnityVehicles.SimpleCar
{
    [CreateAssetMenu(fileName = "CarSetupDataBrakes", menuName = "SimpleCar/Setup/Engine")]
    public class CarSetupDataEngine : ScriptableObject
    {
        public EngineData engine = new EngineData(78f, 9000f, 500f, 30f, 1f,AnimationCurve.Linear(0f,0f,1f,1f));
    }
}

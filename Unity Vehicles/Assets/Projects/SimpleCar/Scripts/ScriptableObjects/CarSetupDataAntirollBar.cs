using UnityEngine;


namespace UnityVehicles.SimpleCar
{
    [CreateAssetMenu(fileName = "CarSetupDataAntirollBar", menuName = "SimpleCar/Setup/Antiroll Bar")]
    public class CarSetupDataAntirollBar : ScriptableObject
    {
        public AntirollBarData antirollBarData = new AntirollBarData(1000f,900f);
    }
}

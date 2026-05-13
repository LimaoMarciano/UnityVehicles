using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace UnityVehicles.SimpleCar
{
    [RequireComponent(typeof(SimpleCar))]
    public class SimpleCarApplySetup : MonoBehaviour
    {
        public bool applyOnStart = true;
        public CarSetupDataChassis chassisSetup;
        public CarSetupDataEngine engineSetup;
        public CarSetupDataGearbox gearboxSetup;
        public CarSetupDataBrakes brakesSetup;
        public CarSetupDataSuspension suspensionSetup;
        public CarSetupDataAntirollBar antirollBarSetup;
        public CarSetupDataTire tireSetup;
        
        private SimpleCar simpleCar;

        private void OnEnable()
        {
            simpleCar = GetComponent<SimpleCar>();
            if (applyOnStart) Apply();
            
        }

        public void Apply()
        {
            if (chassisSetup)
                simpleCar.chassisData = chassisSetup.chassisData;
            if (engineSetup)
                simpleCar.engineData = engineSetup.engine;
            if (gearboxSetup)
                simpleCar.gearboxData = gearboxSetup.gearbox;
            if (brakesSetup)
                simpleCar.brakesData = brakesSetup.brakesData;
            if (suspensionSetup)
                simpleCar.suspension = suspensionSetup.suspension;
            if (antirollBarSetup)
                simpleCar.antirollBarData = antirollBarSetup.antirollBarData;
            if (tireSetup)
                simpleCar.tires = new TiresSettings(tireSetup.tires, tireSetup.tires);
        }
        
    }
}

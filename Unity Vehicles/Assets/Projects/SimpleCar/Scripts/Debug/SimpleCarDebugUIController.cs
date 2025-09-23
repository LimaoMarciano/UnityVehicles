using UnityEngine;
using UnityVehicles.SimpleCar;


public class SimpleCarDebugUIController : MonoBehaviour
{
    public SimpleCar Car;
    public UIDebugBar RPMBar;
    public UIDebugBar GearBar;
    public UIDebugBar SpeedBar;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        RPMBar.Range = Car.RpmRange;
    }

    // Update is called once per frame
    void Update()
    {
        RPMBar.SetCurrentValue(Car.EngineRpm);
        GearBar.SetCurrentValue(Car.CurrentGear + 1);
        SpeedBar.SetCurrentValue(Car.CurrentSpeed * 3.6f);
    }
}

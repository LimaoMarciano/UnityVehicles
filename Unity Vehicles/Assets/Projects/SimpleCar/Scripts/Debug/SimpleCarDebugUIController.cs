using UnityEngine;
using UnityVehicles.SimpleCar;


public class SimpleCarDebugUIController : MonoBehaviour
{
    public SimpleCar car;
    public UIDebugBar rpmBar;
    public UIDebugBar gearBar;
    public UIDebugBar speedBar;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rpmBar.Range = car.engineData.rpmRange;
    }

    // Update is called once per frame
    void Update()
    {
        rpmBar.SetCurrentValue(car.engineRpm);
        gearBar.SetCurrentValue(car.currentGear + 1);
        speedBar.SetCurrentValue(car.speedometer * 3.6f);
    }
}

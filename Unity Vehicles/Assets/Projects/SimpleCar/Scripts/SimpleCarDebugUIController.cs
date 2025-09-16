using UnityEngine;
using UnityVehicles.SimpleCar;


public class SimpleCarDebugUIController : MonoBehaviour
{
    public SimpleCar Car;
    public UIDebugBar RPMBar;
    public UIDebugBar FRWheelBar;
    public UIDebugBar FLWheelBar;
    public WheelCollider FRWheel;
    public WheelCollider FLWheel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        RPMBar.Range = Car.RpmRange;
    }

    // Update is called once per frame
    void Update()
    {
        RPMBar.SetCurrentValue(Car.EngineRpm);
        FRWheelBar.SetCurrentValue(FRWheel.rpm);
        FLWheelBar.SetCurrentValue(FLWheel.rpm);
    }
}

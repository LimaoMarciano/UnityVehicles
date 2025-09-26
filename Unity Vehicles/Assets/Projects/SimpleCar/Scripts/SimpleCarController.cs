using UnityEngine;
using UnityEngine.InputSystem;

namespace UnityVehicles.SimpleCar
{
    [RequireComponent(typeof(SimpleCar))]
    public class SimpleCarController : MonoBehaviour
    {

        CarInputActions carInputActions;
        SimpleCar car;

        InputAction GearUpShift;
        InputAction GearDownShift;

        private void Awake()
        {
            carInputActions = new CarInputActions();
        }

        private void OnEnable()
        {
            carInputActions.Enable();
        }

        private void OnDisable()
        {
            carInputActions.Disable();
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            car = GetComponent<SimpleCar>();
            GearUpShift = carInputActions.FindAction("UpShift");
            GearDownShift = carInputActions.FindAction("DownShift");
        }

        // Update is called once per frame
        void Update()
        {
            Vector2 steering = carInputActions.Car.Steering.ReadValue<Vector2>();
            car.SteeringInput = steering.x;

            car.AcceleratorInput = carInputActions.Car.Throttle.ReadValue<float>();
            car.BrakesInput = carInputActions.Car.Brakes.ReadValue<float>();
            car.ClutchInput = carInputActions.Car.Clutch.ReadValue<float>();

            if (GearUpShift.WasPressedThisFrame())
            {
                car.IncreaseGear();
            }

            if (GearDownShift.WasPressedThisFrame())
            {
                car.DecreaseGear();
            }

            
        }
    }
}

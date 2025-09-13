using UnityEngine;

namespace UnityVehicles.SimpleCar
{
    [RequireComponent(typeof(SimpleCar))]
    public class SimpleCarController : MonoBehaviour
    {

        CarInputActions carInputActions;
        SimpleCar car;

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
        }

        // Update is called once per frame
        void Update()
        {
            Vector2 steering = carInputActions.Car.Steering.ReadValue<Vector2>();
            car.SteeringInput = steering.x;

            car.AcceleratorInput = carInputActions.Car.Throttle.ReadValue<float>();
        }
    }
}

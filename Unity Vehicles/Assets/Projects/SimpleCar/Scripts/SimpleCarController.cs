using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UnityVehicles.SimpleCar
{
    [RequireComponent(typeof(SimpleCar))]
    public class SimpleCarController : MonoBehaviour
    {

        public bool isAutoclutchEnabled = true;

        private CarInputActions carInputActions;
        private SimpleCar car;

        private InputAction gearUpShift;
        private InputAction gearDownShift;

        private const float ClutchPressTime = 0.25f;
        private const float ClutchDepressTime = 0.5f;
        private float autoClutchAccOverride = 1f;
        private float autoClutchInput;
        private bool isExecutingAutoClutch;
        private Coroutine autoClutchCoroutine;

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
        private void Start()
        {
            car = GetComponent<SimpleCar>();
            gearUpShift = carInputActions.FindAction("UpShift");
            gearDownShift = carInputActions.FindAction("DownShift");
        }

        // Update is called once per frame
        private void Update()
        {
            var steering = carInputActions.Car.Steering.ReadValue<Vector2>();
            car.SteeringInput = steering.x;

            car.AcceleratorInput = carInputActions.Car.Throttle.ReadValue<float>() * autoClutchAccOverride;
            car.BrakesInput = carInputActions.Car.Brakes.ReadValue<float>();
            car.HandbrakeInput = carInputActions.Car.Handbrake.ReadValue<float>();
            
            car.clutchInput = isAutoclutchEnabled ? autoClutchInput : carInputActions.Car.Clutch.ReadValue<float>();

            if (gearUpShift.WasPressedThisFrame())
            {
                if (isAutoclutchEnabled)
                {
                    if (isExecutingAutoClutch)
                        StopCoroutine(autoClutchCoroutine);
                    autoClutchCoroutine = StartCoroutine(AutoClutchChangeGear(1, ClutchPressTime, ClutchDepressTime));
                }
                else
                    car.IncreaseGear();
            }

            if (gearDownShift.WasPressedThisFrame())
            {
                if (isAutoclutchEnabled)
                {
                    if (isExecutingAutoClutch)
                        StopCoroutine(autoClutchCoroutine);
                    autoClutchCoroutine = StartCoroutine(AutoClutchChangeGear(-1, ClutchPressTime, ClutchDepressTime));
                } 
                else
                    car.DecreaseGear();
            }
        }

        private IEnumerator AutoClutchChangeGear(int gearChange, float clutchPressDuration, float clutchDepressDuration)
        {            
            isExecutingAutoClutch = true;
            
            for (var i = autoClutchInput; i <= 1f; i += Time.deltaTime * 1f/ clutchPressDuration) 
            {
                autoClutchInput = Mathf.Clamp01(i);
                autoClutchAccOverride = 1f - autoClutchInput;
                yield return true;
            }
            
            autoClutchInput = 1f;
            autoClutchAccOverride = 0f;
            
            if (gearChange >= 0)
                car.IncreaseGear();
            else
                car.DecreaseGear();

            for (var i = autoClutchInput; i >= 0f; i -= Time.deltaTime * 1f/ clutchDepressDuration)
            {
                autoClutchInput = Mathf.Clamp01(i);
                autoClutchAccOverride = 1f - autoClutchInput;
                yield return true;
            }

            autoClutchInput = 0f;
            autoClutchAccOverride = 1f;

            isExecutingAutoClutch = false;

        }
    }
}

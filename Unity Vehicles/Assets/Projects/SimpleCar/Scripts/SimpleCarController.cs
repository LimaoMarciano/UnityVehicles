using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UnityVehicles.SimpleCar
{
    [RequireComponent(typeof(SimpleCar))]
    public class SimpleCarController : MonoBehaviour
    {

        public bool IsAutoclutchEnabled = true;
        
        CarInputActions carInputActions;
        SimpleCar car;

        InputAction GearUpShift;
        InputAction GearDownShift;

        float clutchPressTime = 0.25f;
        float clutchDepressTime = 0.5f;
        float autoClutchAccOverride = 1f;
        float autoClutchInput = 0f;
        bool isExecutingAutoClutch = false;
        Coroutine AutoClutchCoroutine;

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

            car.AcceleratorInput = carInputActions.Car.Throttle.ReadValue<float>() * autoClutchAccOverride;
            car.BrakesInput = carInputActions.Car.Brakes.ReadValue<float>();
            car.HandbrakeInput = carInputActions.Car.Handbrake.ReadValue<float>();
            
            if (IsAutoclutchEnabled)
            {
                car.ClutchInput = autoClutchInput;
            }
            else
            {
                car.ClutchInput = carInputActions.Car.Clutch.ReadValue<float>();
            }

            if (GearUpShift.WasPressedThisFrame())
            {
                if (IsAutoclutchEnabled)
                {
                    if (isExecutingAutoClutch)
                    {
                        StopCoroutine(AutoClutchCoroutine);
                        Debug.Log("Interrupting gear change");
                    }
                    AutoClutchCoroutine = StartCoroutine(AutoClutchChangeGear(1, clutchPressTime, clutchDepressTime));
                }
                else
                {
                    car.IncreaseGear();
                }
            }

            if (GearDownShift.WasPressedThisFrame())
            {
                if (IsAutoclutchEnabled)
                {
                    if (isExecutingAutoClutch)
                    {
                        StopCoroutine(AutoClutchCoroutine);
                        Debug.Log("Interrupting gear change");
                    }
                    AutoClutchCoroutine = StartCoroutine(AutoClutchChangeGear(-1, clutchPressTime, clutchDepressTime));
                } 
                else
                {
                    car.DecreaseGear();
                }
            }

            
        }

        IEnumerator AutoClutchChangeGear(int gearChange, float clutchPressDuration, float clutchDepressDuration)
        {            
            isExecutingAutoClutch = true;
            
            for (float i = autoClutchInput; i <= 1f; i += Time.deltaTime * 1f/ clutchPressDuration) 
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

            for (float i = autoClutchInput; i >= 0f; i -= Time.deltaTime * 1f/ clutchDepressDuration)
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

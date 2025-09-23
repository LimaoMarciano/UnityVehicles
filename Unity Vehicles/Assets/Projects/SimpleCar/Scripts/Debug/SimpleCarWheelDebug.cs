using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UnityVehicles.SimpleCar
{
    public class SimpleCarWheelDebug : MonoBehaviour
    {
        public TMP_Text Value;
        public SimpleCarWheel SimpleCarWheel;
        public Image WheelImage;
        public Gradient gradient;

        Color IdleColor = new Color(0.2f, 0.2f, 0.2f);
        Color OptimumColor = new Color(0.1f, 0.6f, 0.3f);
        Color BadColor = new Color(0.7f, 0.3f, 0.1f);

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
            float slip = SimpleCarWheel.WheelHit.sidewaysSlip / SimpleCarWheel.WheelCollider.sidewaysFriction.extremumSlip;
            if (Mathf.Abs(slip) <= 1f)
            {
                WheelImage.color = Color.Lerp(IdleColor, OptimumColor, Mathf.Abs(slip));
            }
            else
            {
                WheelImage.color = Color.Lerp(OptimumColor, BadColor, Mathf.Abs(slip) - 1f);
            }

            Value.text = slip.ToString("F1");

            WheelImage.rectTransform.rotation = Quaternion.Euler(new Vector3(0f,0f,SimpleCarWheel.WheelCollider.steerAngle));
        }
    }

}


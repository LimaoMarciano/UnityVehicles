using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UnityVehicles.SimpleCar
{
    public class SimpleCarWheelDebug : MonoBehaviour
    {
        public TMP_Text value;
        public SimpleCarWheel simpleCarWheel;
        public Image wheelImage;
        public Gradient gradient;

        private readonly Color idleColor = new Color(0.2f, 0.2f, 0.2f);
        private readonly Color optimumColor = new Color(0.1f, 0.6f, 0.3f);
        private readonly Color badColor = new Color(0.7f, 0.3f, 0.1f);

        // Update is called once per frame
        private void Update()
        {
            var slip = simpleCarWheel.WheelHit.sidewaysSlip / simpleCarWheel.wheelCollider.sidewaysFriction.extremumSlip;
            if (Mathf.Abs(slip) <= 1f)
                wheelImage.color = Color.Lerp(idleColor, optimumColor, Mathf.Abs(slip));
            else
                wheelImage.color = Color.Lerp(optimumColor, badColor, Mathf.Abs(slip) - 1f);

            value.text = slip.ToString("F1");

            wheelImage.rectTransform.rotation = Quaternion.Euler(new Vector3(0f,0f,simpleCarWheel.wheelCollider.steerAngle));
        }
    }

}


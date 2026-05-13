using UnityEngine;

[RequireComponent (typeof(Rigidbody))]
public class SimpleCarAerodynamics : MonoBehaviour
{
    public float frontalCdA = 0.654f;
    public float sideCdA = 5.7f;
    public float backCdA = 0.8f;
    public float airDensity = 1.225f;

    public float FrontDrag { get; private set; }
    public float BackDrag { get; private set; }
    public float SideDrag { get; private set; }

    Rigidbody rb;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        Vector3 localVelocity = transform.InverseTransformDirection(rb.linearVelocity);

        float forwardDrag;
        if (localVelocity.z > 0f)
        {
            FrontDrag = 0.5f * airDensity * frontalCdA * localVelocity.z * localVelocity.z;
            BackDrag = 0f;

            forwardDrag = FrontDrag;
        } else
        {
            FrontDrag = 0f;
            BackDrag = 0.5f * airDensity * backCdA * localVelocity.z * localVelocity.z;

            forwardDrag = -BackDrag;
        }

        SideDrag = 0.5f * airDensity * sideCdA * localVelocity.x * localVelocity.x * Mathf.Sign(localVelocity.x);

        Vector3 dragForce = new Vector3(forwardDrag, SideDrag, 0f);
        Debug.Log(-dragForce);
        rb.AddForce(transform.TransformDirection(-dragForce), ForceMode.Force);
    }
}

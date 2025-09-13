using UnityEditor;
using UnityEngine;

public class SimpleCar : MonoBehaviour
{
    
    Rigidbody rb;
    public Vector3 CenterOfMass;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = CenterOfMass;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 gizmoPos = transform.TransformPoint(CenterOfMass);
        Gizmos.DrawWireSphere(gizmoPos, 0.1f);
        //Gizmos.DrawSphere(gizmoPos, 0.1f);
        
    }
}

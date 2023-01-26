using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollCage : MonoBehaviour
{
    public CarController Car;

    Rigidbody rb;

    public Rigidbody RB => rb ??= GetComponent<Rigidbody>();

    public bool OnGround => Colliders.Count > 0;

    public HashSet<Collider> Colliders = new HashSet<Collider>();

    void OnCollisionEnter(Collision collision)
    {
        Colliders.Add(collision.collider);
        if (Car != null)
        {
            Car.OnCollisionEnter(collision);
        }
    }

    void OnCollisionExit(Collision collision)
    {
        Colliders.Remove(collision.collider);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (Car != null)
        {
            Car.OnTriggerEnter(other);
        }
    }
}

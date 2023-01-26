using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireball : NetworkBehaviour
{
    bool started = false;

    Vector3 direction;
    Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public override void OnStartServer()
    {
        started = true;
    }


    private void OnCollisionEnter(Collision collision)
	{
        if (started)
        {
            var otherRB = collision.rigidbody;

            if (otherRB != null && otherRB.TryGetComponent<RollCage>(out var rc))
            {
                Debug.Log("EXPLOSION TRIGGERED");
                rc.Car.AddForce((transform.forward * 50) + (Vector3.up * 100), ForceMode.Force);
                //otherRB.AddExplosionForce(10, transform.position, 1f);
            }

            NetworkServer.Destroy(gameObject);
        }
		Debug.Log("Fireball Collision = " + collision.gameObject);
		//collision.rigidbody?.AddExplosionForce(10, transform.position, 1f);
		//Destroy(gameObject);
	}
}


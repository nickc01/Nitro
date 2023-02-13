using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireball : NetworkBehaviour
{
    private void OnCollisionEnter(Collision collision)
	{
        if (NetworkServer.active)
        {
            var otherRB = collision.rigidbody;

            if (otherRB != null)
            {
                var force = (transform.forward * 50) + (Vector3.up * 100);
                if (otherRB.TryGetComponent<RollCage>(out var rc))
                {
                    rc.Car.AddForce(force, ForceMode.Force);
                }
                else
                {
                    otherRB.AddForce(force, ForceMode.Force);
                }
            }

            NetworkServer.Destroy(gameObject);
        }
	}
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireball : MonoBehaviour
{
	private void OnCollisionEnter(Collision collision)
	{
		collision.rigidbody?.AddExplosionForce(10, transform.position, 1f);
		Destroy(gameObject);
	}
}


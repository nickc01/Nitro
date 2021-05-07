using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GroundChecker : MonoBehaviour
{
	HashSet<GameObject> touchedObjects = new HashSet<GameObject>();

	public bool IsTouchingGround
	{
		get
		{
			touchedObjects.RemoveWhere(g => g == null);

			return touchedObjects.Count > 0;
		}
	}


	private void OnCollisionEnter(Collision collision)
	{
		touchedObjects.Add(collision.gameObject);
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		touchedObjects.Add(collision.gameObject);
	}

	private void OnCollisionExit(Collision collision)
	{
		touchedObjects.Remove(collision.gameObject);
	}

	private void OnCollisionExit2D(Collision2D collision)
	{
		touchedObjects.Remove(collision.gameObject);
	}
}


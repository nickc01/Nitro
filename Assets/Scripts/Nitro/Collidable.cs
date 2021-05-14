using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Nitro
{
	/// <summary>
	/// A component that makes it easier to keep track of objects that have collided with an object.
	/// </summary>
	public abstract class Collidable : MonoBehaviour
	{
		List<Rigidbody> collidedBodies = new List<Rigidbody>();

		/// <summary>
		/// Returns a list of all the collided objects
		/// </summary>
		public IEnumerable<Rigidbody> CollidedBodies => collidedBodies.Distinct();

		/// <summary>
		/// Called when an object collides with this object. Guaranteed to never be called multiple times for the same object
		/// </summary>
		/// <param name="body">The rigidbody on the collided object</param>
		protected abstract void OnCollideStart(Rigidbody body);

		/// <summary>
		/// Called when an object is no longer colliding with this object. Guaranteed to never be called multiple times for the same object
		/// </summary>
		/// <param name="body">The rigidbody on the collided object</param>
		protected abstract void OnCollideStop(Rigidbody body);


		protected virtual void OnTriggerEnter(Collider other)
		{
			if (other.attachedRigidbody != null && !collidedBodies.Contains(other.attachedRigidbody))
			{
				collidedBodies.Add(other.attachedRigidbody);
				OnCollideStart(other.attachedRigidbody);
			}
			else
			{
				collidedBodies.Add(other.attachedRigidbody);
			}
		}

		protected virtual void OnTriggerExit(Collider other)
		{
			collidedBodies.Remove(other.attachedRigidbody);
			if (other.attachedRigidbody != null && !collidedBodies.Contains(other.attachedRigidbody))
			{
				OnCollideStop(other.attachedRigidbody);
			}
		}

		protected virtual void OnCollisionEnter(Collision collision)
		{
			if (collision.rigidbody != null && !collidedBodies.Contains(collision.rigidbody))
			{
				collidedBodies.Add(collision.rigidbody);
				OnCollideStart(collision.rigidbody);
			}
			else
			{
				collidedBodies.Add(collision.rigidbody);
			}
		}

		protected virtual void OnCollisionExit(Collision collision)
		{
			collidedBodies.Remove(collision.rigidbody);
			if (collision.rigidbody != null && !collidedBodies.Contains(collision.rigidbody))
			{
				OnCollideStop(collision.rigidbody);
			}
		}

		protected virtual void OnDisable()
		{
			foreach (var body in collidedBodies.Distinct())
			{
				if (body != null)
				{
					OnCollideStop(body);
				}
			}
			collidedBodies.Clear();
		}

		protected virtual void OnDestroy()
		{
			foreach (var body in collidedBodies.Distinct())
			{
				if (body != null)
				{
					OnCollideStop(body);
				}
			}
			collidedBodies.Clear();
		}
	}
}

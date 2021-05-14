using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Nitro
{
	/// <inheritdoc cref="Collidable"/>
	public abstract class Collidable2D : MonoBehaviour
	{
		List<Rigidbody2D> collidedBodies = new List<Rigidbody2D>();

		/// <inheritdoc cref="Collidable.CollidedBodies"/>
		public IEnumerable<Rigidbody2D> CollidedBodies => collidedBodies.Distinct();

		/// <inheritdoc cref="Collidable.OnCollideStart"/>
		protected abstract void OnCollideStart(Rigidbody2D body);

		/// <inheritdoc cref="Collidable.OnCollideStop"/>
		protected abstract void OnCollideStop(Rigidbody2D body);


		private void OnTriggerEnter2D(Collider2D collision)
		{
			if (collision.attachedRigidbody != null && !collidedBodies.Contains(collision.attachedRigidbody))
			{
				collidedBodies.Add(collision.attachedRigidbody);
				OnCollideStart(collision.attachedRigidbody);
			}
			else
			{
				collidedBodies.Add(collision.attachedRigidbody);
			}
		}

		private void OnTriggerExit2D(Collider2D collision)
		{
			collidedBodies.Remove(collision.attachedRigidbody);
			if (collision.attachedRigidbody != null && !collidedBodies.Contains(collision.attachedRigidbody))
			{
				OnCollideStop(collision.attachedRigidbody);
			}
		}

		private void OnCollisionEnter2D(Collision2D collision)
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

		private void OnCollisionExit2D(Collision2D collision)
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
				OnCollideStop(body);
			}
			collidedBodies.Clear();
		}
	}
}

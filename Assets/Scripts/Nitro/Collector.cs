using UnityEngine;
using UnityEngine.Events;

namespace Nitro
{
	/// <summary>
	/// Used for collecting powerups
	/// </summary>
	public abstract class Collector : MonoBehaviour
	{
		[SerializeField]
		bool collectorEnabled = true;

		public bool CollectorEnabled { get => collectorEnabled; set => collectorEnabled = value; }

		[SerializeField]
		[Tooltip("If set to true, all powerups that collide with this object will automatically be collected")]
		bool collectOnContact = true;

		/// <summary>
		/// If set to true, all powerups that collide with this object will automatically be collected
		/// </summary>
		public bool CollectOnContact { get => collectOnContact; set => collectOnContact = value; }

		/// <summary>
		/// Whether the powerup can be collected or not
		/// </summary>
		/// <param name="powerup">The powerup to test if it can be collected</param>
		/// <returns>Returns whether or not the powerup can be picked up</returns>
		public abstract bool CanCollectPowerup(Powerup powerup);

		/// <summary>
		/// A function that is triggered when a powerup is collected
		/// </summary>
		/// <param name="powerup">The powerup that has been collected</param>
		protected abstract void OnCollect(Powerup powerup);

		/// <summary>
		/// Executes the powerup(s) that the collector has collected
		/// </summary>
		public abstract void Execute();

		/// <summary>
		/// An event that is triggered when a powerup is collected
		/// </summary>
		public UnityEvent<Powerup> PowerupCollectEvent;

		/// <summary>
		/// Collects a powerup
		/// </summary>
		/// <param name="powerup">The powerup to collect</param>
		/// <returns>Returns whether the collector was able to pick up the <see cref="Powerup"/></returns>
		public bool CollectPowerup(Powerup powerup)
		{
			if (!collectorEnabled)
			{
				return false;
			}
			if (CanCollectPowerup(powerup))
			{
				OnCollect(powerup);
				powerup.OnCollect(this);
				PowerupCollectEvent?.Invoke(powerup);
				return true;
			}
			return false;
		}

		protected virtual void OnTriggerEnter(Collider other)
		{
			if (collectOnContact)
			{
				var powerup = other.GetComponent<Powerup>();
				if (powerup != null)
				{
					CollectPowerup(powerup);
				}
			}
		}

		protected virtual void OnTriggerEnter2D(Collider2D collision)
		{
			if (collectOnContact)
			{
				var powerup = collision.GetComponent<Powerup>();
				if (powerup != null)
				{
					CollectPowerup(powerup);
				}
			}
		}

		protected virtual void OnCollisionEnter(Collision collision)
		{
			if (collectOnContact)
			{
				var powerup = collision.gameObject.GetComponent<Powerup>();
				if (powerup != null)
				{
					CollectPowerup(powerup);
				}
			}
		}

		protected virtual void OnCollisionEnter2D(Collision2D collision)
		{
			if (collectOnContact)
			{
				var powerup = collision.gameObject.GetComponent<Powerup>();
				if (powerup != null)
				{
					CollectPowerup(powerup);
				}
			}
		}
	}
}

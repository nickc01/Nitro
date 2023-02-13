using System;
using System.Collections;
using UnityEngine;

namespace Nitro
{

    /// <summary>
    /// The base class for all powerups
    /// </summary>
    public abstract class Powerup : MonoBehaviour, IPowerup
	{
		/// <summary>
		/// The collector that collected the powerup
		/// </summary>
		public ICollector Collector { get; private set; }

		[SerializeField]
		[Tooltip("If set to true, all colliders and renderers on the powerup will be disabled when collected")]
		protected bool hideObjectOnCollect = true;

		public bool HideObjectOnCollect => hideObjectOnCollect;

		/// <summary>
		/// The main action of the powerup
		/// </summary>
		/// <param name="collector">The collector that collected the powerup</param>
		public abstract void DoAction();

		/// <summary>
		/// Called when the powerup has been collected
		/// </summary>
		/// <param name="collector">The collector that has collected the powerup</param>
		public virtual void OnCollect(ICollector collector)
		{
			Collector = collector;
			if (hideObjectOnCollect)
			{
				foreach (var collider in GetComponentsInChildren<Collider>())
				{
					collider.enabled = false;
				}

				foreach (var collider in GetComponentsInChildren<Collider2D>())
				{
					collider.enabled = false;
				}

				foreach (var renderer in GetComponentsInChildren<Renderer>())
				{
					renderer.enabled = false;
				}
			}
			if (collector is Component component)
			{
                transform.SetParent(component.transform);
            }
			transform.localRotation = Quaternion.identity;
			transform.localPosition = default;
		}

		/// <summary>
		/// Used to dispose of the powerup when done
		/// </summary>
		public virtual void DoneUsingPowerup()
		{
			Collector = null;
			Destroy(gameObject);
		}

		/// <summary>
		/// Used to dispose of the powerup after a set amount of time
		/// </summary>
		/// <param name="lifetime">How long before the powerup is destroyed</param>
		public void DoneUsingPowerupAfter(float lifetime, Action onDone = null)
		{
			StartCoroutine(Routine());

			IEnumerator Routine()
			{
				yield return new WaitForSeconds(lifetime);
				onDone?.Invoke();
                DoneUsingPowerup();
			}
		}
	}
}
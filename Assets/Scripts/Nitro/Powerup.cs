using System.Collections;
using UnityEngine;

namespace Nitro
{
	public abstract class Powerup : MonoBehaviour
	{
		[SerializeField]
		[Tooltip("If set to true, all colliders and renderers on the powerup will be disabled when collected")]
		protected bool hideObjectOnCollect = true;

		public bool HideObjectOnCollect => hideObjectOnCollect;

		/// <summary>
		/// The main action of the powerup
		/// </summary>
		/// <param name="collector">The collector that collected the powerup</param>
		public abstract void DoAction(Collector collector);

		/// <summary>
		/// Called when the powerup has been collected
		/// </summary>
		/// <param name="collector">The collector that has collected the powerup</param>
		public virtual void OnCollect(Collector collector)
		{
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
			transform.SetParent(collector.transform);
			transform.localRotation = Quaternion.identity;
			transform.localPosition = default;
		}

		/// <summary>
		/// Used to dispose of the powerup when done
		/// </summary>
		public virtual void DoneUsingPowerup()
		{
			Destroy(gameObject);
		}

		/// <summary>
		/// Used to dispose of the powerup after a set amount of time
		/// </summary>
		/// <param name="lifetime">How long before the powerup is destroyed</param>
		public void DoneUsingPowerupAfter(float lifetime)
		{
			StartCoroutine(Routine());

			IEnumerator Routine()
			{
				yield return new WaitForSeconds(lifetime);
				DoneUsingPowerup();
			}
		}
	}
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class CombinablePowerup : MonoBehaviour
{
	Renderer[] renderers;
	Collider[] colliders;
	Collider2D[] colliders2D;


	[Tooltip(@"The priority of the powerup, which determines how the powerup will be done when in a combo

For example, if you have a fire powerup that has a higher priority than a water powerup, then the fire effect will be applied before the water effect")]
	public int Priority;

	/// <summary>
	/// Whether the powerup has been collected or not
	/// </summary>
	public bool Collected => SourceCollector != null;
	public IEnumerable<CombinablePowerup> AuxillaryPowerups { get; private set; }

	/// <summary>
	/// The collector that collected the powerup. This is null when the powerup has not been collected yet
	/// </summary>
	public PowerupCollector SourceCollector { get; internal set; }

	/// <summary>
	/// This is the action the powerup will take if it has not been combined with anything else
	/// </summary>
	public abstract IEnumerator MainAction();

	/// <summary>
	/// This is an auxillary effect that will be applied on top of an existing powerup
	/// </summary>
	/// <param name="position">The position where the effect should take place</param>
	public abstract void AuxillaryAction(Vector3 position);

	/// <summary>
	/// Called when the powerup is collected by a collector
	/// </summary>
	protected virtual void OnPowerupCollect()
	{

	}

	internal void CollectPowerup(PowerupCollector collector)
	{
		if (Collected)
		{
			return;
		}

		SourceCollector = collector;

		if (renderers == null)
		{
			renderers = GetComponentsInChildren<Renderer>();
			colliders = GetComponentsInChildren<Collider>();
			colliders2D = GetComponentsInChildren<Collider2D>();
		}

		foreach (var renderer in renderers)
		{
			renderer.enabled = false;
		}

		foreach (var collider in colliders)
		{
			collider.enabled = false;
		}

		foreach (var collider2D in colliders2D)
		{
			collider2D.enabled = false;
		}

		transform.SetParent(collector.transform);
		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.identity;
		OnPowerupCollect();
	}

	internal void ExecutePowerup(IEnumerable<CombinablePowerup> auxillaryPowerups)
	{
		AuxillaryPowerups = auxillaryPowerups;
		StartCoroutine(DoMainAction());
	}

	IEnumerator DoMainAction()
	{
		yield return MainAction();
		SourceCollector = null;
		foreach (var aux in AuxillaryPowerups)
		{
			Destroy(aux.gameObject);
		}
		AuxillaryPowerups = null;
		Destroy(gameObject);
	}

	/// <summary>
	/// Executes all other auxillary attacks
	/// </summary>
	/// <param name="position">The position where the effect should take place</param>
	protected void ExecuteAuxillaryPowerups(Vector3 position)
	{
		if (AuxillaryPowerups != null)
		{
			foreach (var aux in AuxillaryPowerups)
			{
				aux.AuxillaryAction(position);
			}
		}
	}
}

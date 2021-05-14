using MyBox;
using Nitro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricPowerup : DemoPowerup
{
	[SerializeField]
	Cloud CloudPrefab;

	public Vector3 CloudOffset;
	public GameObject BoltPrefab;


	public float MinWaitTime;
	public float MaxWaitTime;
	public float LifeTime = 10f;
	public float StrikeTime = 0.1f;

	public float ExplosionForce;
	public float ExplosionRadius;

	Cloud cloudInstance;

	public LayerMask collisionMask;

	bool IsAuxillary = false;

	public override void DoMainAction(Collector collector, IEnumerable<CombinablePowerup> AuxillaryPowerups)
	{
		IsAuxillary = false;
		StartCoroutine(MainRoutine());

		IEnumerator MainRoutine()
		{
			cloudInstance = GameObject.Instantiate(CloudPrefab, transform.position, Quaternion.identity);
			cloudInstance.SourcePowerup = this;
			cloudInstance.SourceCollector = collector;
			cloudInstance.DoMainAction();

			for (float i = 0; i < LifeTime + StrikeTime; i += Time.deltaTime)
			{
				cloudInstance.transform.position = transform.TransformPoint(CloudOffset);
				yield return null;
			}
			DoneUsingPowerup();
		}
	}

	public void OnStrike(Rigidbody hitObject)
	{
		if (!IsAuxillary)
		{
			foreach (var powerup in AuxillaryPowerups)
			{
				powerup.DoAuxillaryAction(this, hitObject.transform.position, SourceCollector);
			}
		}
	}

	public override void DoneUsingPowerup()
	{
		Destroy(cloudInstance);
		base.DoneUsingPowerup();
	}

	public override void DoAuxillaryAction(CombinablePowerup sourcePowerup, Vector3 position, Collector collector)
	{
		IsAuxillary = true;
		cloudInstance = GameObject.Instantiate(CloudPrefab, position, Quaternion.identity);
		cloudInstance.SourcePowerup = this;
		cloudInstance.SourceCollector = collector;
		cloudInstance.DoSingleStrike(position);
	}
}

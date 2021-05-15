using MyBox;
using Nitro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ElectricPowerup : CombinablePowerup
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

	Cloud CreateCloud()
	{
		var instance = GameObject.Instantiate(CloudPrefab, transform.position, Quaternion.identity);
		instance.SourcePowerup = this;
		instance.SourceCollector = Collector;
		return instance;
	}

	//The main action of the electric powerup
	public override void DoMainAction(AuxPowerups AuxillaryPowerups)
	{
		IsAuxillary = false;
		StartCoroutine(MainRoutine());

		IEnumerator MainRoutine()
		{
			cloudInstance = CreateCloud();
			cloudInstance.DoMainAction();

			for (float i = 0; i < LifeTime + StrikeTime; i += Time.deltaTime)
			{
				cloudInstance.transform.position = transform.TransformPoint(CloudOffset);
				yield return null;
			}
			DoneUsingPowerup();
		}
	}

	//The auxiliary action of the electric powerup
	public override void DoAuxillaryAction(CombinablePowerup sourcePowerup, Vector3 position)
	{
		IsAuxillary = true;
		cloudInstance = CreateCloud();
		cloudInstance.DoSingleStrike(position);
	}

	//This is called when the cloud object strikes an object. This is used to execute the auxiliary powerups where the lightining struck
	public void OnStrike(Rigidbody hitObject)
	{
		if (!IsAuxillary)
		{
			AuxillaryPowerups.Execute(this, hitObject.transform.position);
		}
	}

	public override void DoneUsingPowerup()
	{
		if (cloudInstance != null)
		{
			Destroy(cloudInstance);
		}
		base.DoneUsingPowerup();
	}
}

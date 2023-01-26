using Mirror;
using Nitro;
using System;
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

	bool IsFirst = false;

	public float MaxDistance = 3f;

	[Server]
	Cloud CreateCloud()
	{
		var instance = GameObject.Instantiate(CloudPrefab);
		instance.SourcePowerup = this;
		//instance.SourceCar = (Collector as Component).GetComponent<CarController>();
		instance.SourceCollector = (Collector as Component).GetComponent<NetworkIdentity>();
		instance.transform.localPosition = CloudOffset;

		NetworkServer.Spawn(instance.gameObject, (Collector as Component).gameObject);
		return instance;
	}

	Action<Vector3, Quaternion> RunNextPowerup;

    public override void Execute(ICombinablePowerup previous, Vector3 position, Quaternion rotation, Action<Vector3, Quaternion> runNextPowerup)
    {
        RunNextPowerup = runNextPowerup;
        IsFirst = previous == null;


		IEnumerator Routine()
		{
            if (IsFirst)
            {
                cloudInstance = CreateCloud();
				var strikeRoutine = cloudInstance.StartCoroutine(cloudInstance.DoMultipleStrikes());

                /*for (float i = 0; i < LifeTime + StrikeTime; i += Time.deltaTime)
                {
					//cloudInstance.transform.position = transform.TransformPoint(CloudOffset);
					cloudInstance.transform.rotation = Quaternion.identity;
                    yield return null;
                }*/

				yield return strikeRoutine;
            }
            else
            {
                cloudInstance = CreateCloud();
				yield return cloudInstance.DoSingleStrike(position);
            }

            if (cloudInstance != null)
            {
				NetworkServer.Destroy(cloudInstance.gameObject);
                //Destroy(cloudInstance.gameObject);
            }
            DoneUsingPowerup();
        }

		StartCoroutine(Routine());

    }

    //This is called when the cloud object strikes an object. This is used to execute the auxiliary powerups where the lightining struck
    public void OnStrike(Rigidbody hitObject)
	{
		if (IsFirst)
		{
			RunNextPowerup(hitObject.transform.position, hitObject.transform.rotation);
        }
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThunderstormAction : MonoBehaviour
{
	public ElectricPowerup Powerup;


	public GameObject CloudPrefab;
	public Vector3 CloudOffset;
	public GameObject BoltPrefab;


	public float MinWaitTime;
	public float MaxWaitTime;
	public float LifeTime = 10f;
	public float StrikeTime = 0.1f;

	public float ExplosionForce;
	public float ExplosionRadius;

	/*public override void Activate()
	{
		StartCoroutine(Routine());
	}*/

	GameObject cloudInstance;

	IEnumerator Routine()
	{
		//cloudInstance = GameObject.Instantiate(CloudPrefab,Car.transform);
		cloudInstance.transform.localPosition = CloudOffset;
		cloudInstance.transform.localRotation = Quaternion.identity;


		float waitCounter = 0f;
		float waitAmount = Random.Range(MinWaitTime, MaxWaitTime);
		for (float i = 0; i < LifeTime; i += Time.deltaTime)
		{
			waitCounter += Time.deltaTime;
			if (waitCounter >= waitAmount)
			{
				waitCounter -= waitAmount;
				waitAmount = Random.Range(MinWaitTime, MaxWaitTime);
				Debug.Log("STRIKE!");
				StartCoroutine(DoStrike());
			}

			yield return null;
		}

		yield return new WaitForSeconds(StrikeTime);

		Destroy(gameObject);
		Destroy(cloudInstance);
	}


	IEnumerator DoStrike()
	{
		yield return null;
		if (Targets.Count == 0)
		{
			yield break;
		}
		var target = Targets[Random.Range(0,Targets.Count)];

		/*if (target == Car.gameObject)
		{
			if (Targets.Count == 1)
			{
				yield break;
			}
			else
			{
				yield return DoStrike();
				yield break;
			}
		}*/

		var bolt = GameObject.Instantiate(BoltPrefab,Vector3.zero,Quaternion.identity);

		var distance = Vector3.Distance(cloudInstance.transform.position,target.transform.position);

		var midpoint = Vector3.Lerp(cloudInstance.transform.position,target.transform.position,0.5f);

		bolt.transform.position = midpoint;
		bolt.transform.localScale = new Vector3(distance / 16f, distance,1f);
		bolt.transform.LookAt(target.transform.position);
		bolt.transform.rotation *= Quaternion.Euler(90f,0f,0f);

		var targetBody = target.GetComponent<Rigidbody>();

		var nearby = Vector3.Lerp(cloudInstance.transform.position,target.transform.position,0.7f);

		targetBody.AddExplosionForce(ExplosionForce, target.transform.position - Vector3.one,distance - (distance * 0.7f));

		Destroy(bolt,StrikeTime);

		yield return new WaitForSeconds(StrikeTime);

		yield break;
	}

	List<GameObject> Targets = new List<GameObject>();


	private void OnCollisionEnter(Collision collision)
	{
		if (!Targets.Contains(collision.gameObject))
		{
			//Debug.Log("Adding Object_A = " + collision.gameObject);
			Targets.Add(collision.gameObject);
		}
	}

	private void OnCollisionExit(Collision collision)
	{
		//Debug.Log("Removing Object_A = " + collision.gameObject);
		Targets.Remove(collision.gameObject);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!Targets.Contains(other.gameObject))
		{
			//Debug.Log("Adding Object_B = " + other.gameObject);
			Targets.Add(other.gameObject);
		}
		//Targets.Add(other.gameObject);
	}

	private void OnTriggerExit(Collider other)
	{
		//Debug.Log("Removing Object_B = " + other.gameObject);
		Targets.Remove(other.gameObject);
	}
}

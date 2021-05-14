using Nitro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Cloud : MonoBehaviour
{
	List<Rigidbody> Targets = new List<Rigidbody>();

	[HideInInspector]
	public ElectricPowerup SourcePowerup;
	[HideInInspector]
	public Collector SourceCollector;

	IEnumerator Action()
	{
		float waitCounter = 0f;
		float waitAmount = Random.Range(SourcePowerup.MinWaitTime, SourcePowerup.MaxWaitTime);
		for (float i = 0; i < SourcePowerup.LifeTime; i += Time.deltaTime)
		{
			waitCounter += Time.deltaTime;
			if (waitCounter >= waitAmount)
			{
				waitCounter -= waitAmount;
				waitAmount = Random.Range(SourcePowerup.MinWaitTime, SourcePowerup.MaxWaitTime);
				StartCoroutine(DoStrike(transform.position));
			}

			yield return null;
		}

		yield return new WaitForSeconds(SourcePowerup.StrikeTime);

		Destroy(gameObject);
	}

	public void DoMainAction()
	{
		StartCoroutine(Action());
	}

	public void DoSingleStrike(Vector3 source)
	{
		foreach (var renderer in GetComponentsInChildren<Renderer>())
		{
			renderer.enabled = false;
		}
		StartCoroutine(DoStrike(source));
		Destroy(gameObject, SourcePowerup.StrikeTime);
	}

	IEnumerator DoStrike(Vector3 source)
	{
		yield return null;
		if (Targets.Count == 0)
		{
			yield break;
		}

		var sourceBody = SourceCollector.GetComponentInChildren<Rigidbody>();

		var selectableTargets = new List<Rigidbody>(Targets.Where(t => t != sourceBody));

		if (selectableTargets.Count == 0)
		{
			yield break;
		}

		var selectedTarget = selectableTargets[Random.Range(0,selectableTargets.Count)];

		var bolt = GameObject.Instantiate(SourcePowerup.BoltPrefab, Vector3.zero, Quaternion.identity);

		var distance = Vector3.Distance(source, selectedTarget.transform.position);

		var midpoint = Vector3.Lerp(source, selectedTarget.transform.position, 0.5f);

		bolt.transform.position = midpoint;
		bolt.transform.localScale = new Vector3(distance / 16f, distance, 1f);
		bolt.transform.LookAt(selectedTarget.transform.position);
		bolt.transform.rotation *= Quaternion.Euler(90f, 0f, 0f);

		var distanceVector = (new Vector3(selectedTarget.transform.position.x, source.y, selectedTarget.transform.position.z) - source);
		selectedTarget.velocity = (Vector3.up * SourcePowerup.ExplosionForce) + (distanceVector).normalized * SourcePowerup.ExplosionRadius;

		Destroy(bolt, SourcePowerup.StrikeTime);

		SourcePowerup.OnStrike(selectedTarget);

		yield return new WaitForSeconds(SourcePowerup.StrikeTime);

		yield break;
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (((1 << collision.gameObject.layer) & SourcePowerup.collisionMask.value) != 0 && !Targets.Contains(collision.rigidbody))
		{
			Targets.Add(collision.rigidbody);
		}
	}

	private void OnCollisionExit(Collision collision)
	{
		Targets.Remove(collision.rigidbody);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (((1 << other.gameObject.layer) & SourcePowerup.collisionMask.value) != 0 && !Targets.Contains(other.attachedRigidbody))
		{
			Targets.Add(other.attachedRigidbody);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		Targets.Remove(other.attachedRigidbody);
	}

	static Transform GetTopParent(Transform transform)
	{
		while (true)
		{
			if (transform.parent == null)
			{
				return transform;
			}
			else
			{
				transform = transform.parent;
			}
		}
	}
}

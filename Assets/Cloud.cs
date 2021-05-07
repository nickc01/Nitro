using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cloud : MonoBehaviour
{
	List<GameObject> Targets = new List<GameObject>();

	[HideInInspector]
	public ElectricPowerup SourcePowerup;


	// Start is called before the first frame update
	void Start()
    {
		StartCoroutine(Action());
    }

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
				StartCoroutine(DoStrike());
			}

			yield return null;
		}

		yield return new WaitForSeconds(SourcePowerup.StrikeTime);

		Destroy(gameObject);
	}

	IEnumerator DoStrike()
	{
		if (Targets.Count == 0)
		{
			yield break;
		}

		int selectedIndex = Random.Range(0, Targets.Count);
		GameObject selectedTarget = Targets[selectedIndex];

		var topMostParent = GetTopParent(selectedTarget.transform);

		var collector = topMostParent.GetComponentInChildren<PowerupCollector>();

		if (collector == SourcePowerup.SourceCollector)
		{
			if (Targets.Count == 1)
			{
				yield break;
			}
			else
			{
				selectedIndex = (selectedIndex + 1) % Targets.Count;
				selectedTarget = Targets[selectedIndex];
				topMostParent = GetTopParent(selectedTarget.transform);
			}
		}

		//Do a check for the source collector object

		/*var target = Targets[Random.Range(0, Targets.Count)];

		if (target == SourceCollector.gameObject)
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

		var bolt = GameObject.Instantiate(SourcePowerup.BoltPrefab, Vector3.zero, Quaternion.identity);

		var distance = Vector3.Distance(transform.position, selectedTarget.transform.position);

		var midpoint = Vector3.Lerp(transform.position, selectedTarget.transform.position, 0.5f);

		bolt.transform.position = midpoint;
		bolt.transform.localScale = new Vector3(distance / 16f, distance, 1f);
		bolt.transform.LookAt(selectedTarget.transform.position);
		bolt.transform.rotation *= Quaternion.Euler(90f, 0f, 0f);

		var targetBody = topMostParent.GetComponentInChildren<Rigidbody>();

		//Debug.Log("selectedTarget = " + selectedTarget);
		//Debug.Log("selectedTarget Body = " + targetBody);

		//var nearby = Vector3.Lerp(cloudInstance.transform.position, selectedTarget.transform.position, 0.7f);

		//targetBody.AddExplosionForce(ExplosionForce, selectedTarget.transform.position - Vector3.one, distance - (distance * 0.7f));
		//targetBody.AddExplosionForce(SourcePowerup.ExplosionForce * distance, transform.position, distance * 2f, 2.0f);
		//targetBody.AddRelativeForce((Vector3.up * SourcePowerup.ExplosionForce) + (selectedTarget.transform.position - transform.position).normalized * SourcePowerup.ExplosionRadius);
		//var distanceVector = selectedTarget.transform.position - transform.position;

		var distanceVector = (new Vector3(selectedTarget.transform.position.x, transform.position.y, selectedTarget.transform.position.z) - transform.position);
		targetBody.velocity = (Vector3.up * SourcePowerup.ExplosionForce) + (distanceVector).normalized * SourcePowerup.ExplosionRadius;

		Destroy(bolt, SourcePowerup.StrikeTime);

		yield return new WaitForSeconds(SourcePowerup.StrikeTime);

		yield break;
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (((1 << collision.gameObject.layer) & SourcePowerup.collisionMask.value) != 0 && !Targets.Contains(collision.gameObject))
		{
			//Debug.Log("Adding Target_A = " + collision.gameObject);
			Targets.Add(collision.gameObject);
		}
	}

	private void OnCollisionExit(Collision collision)
	{
		//Debug.Log("Removing Target_A = " + collision.gameObject);
		Targets.Remove(collision.gameObject);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (((1 << other.gameObject.layer) & SourcePowerup.collisionMask.value) != 0 && !Targets.Contains(other.gameObject))
		{
			//Debug.Log("Adding Target_B = " + other.gameObject);
			Targets.Add(other.gameObject);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		//Debug.Log("Removing Target_B = " + other.gameObject);
		Targets.Remove(other.gameObject);
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

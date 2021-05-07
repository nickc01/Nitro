using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AuxBolt : MonoBehaviour
{
	public ElectricPowerup SourcePowerup;

	SphereCollider sphere;
	Renderer renderer;

	Vector3 sourcePosition;

	[SerializeField]
	float strikeDelay = 0.1f;


	private void Awake()
	{
		sourcePosition = transform.position;
		renderer = GetComponent<Renderer>();
		renderer.enabled = false;
	}

	private void Start()
	{
		sphere = GetComponent<SphereCollider>();
		StartCoroutine(Strike());
	}


	IEnumerator Strike()
	{
		//yield return null;
		yield return new WaitForSeconds(strikeDelay);

		Debug.Log("Beginning Strike");

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

		sphere.enabled = false;
		renderer.enabled = true;

		Debug.Log("Striking");
		Debug.Log("Object = " + selectedTarget.gameObject.name);
		Debug.Log("Top Most Parent = " + topMostParent);

		var distance = Vector3.Distance(sourcePosition, selectedTarget.transform.position);

		var midpoint = Vector3.Lerp(sourcePosition, selectedTarget.transform.position, 0.5f);

		transform.position = midpoint;
		transform.localScale = new Vector3(distance / 16f, distance, 1f);
		transform.LookAt(selectedTarget.transform.position);
		transform.rotation *= Quaternion.Euler(90f, 0f, 0f);

		var targetBody = topMostParent.GetComponentInChildren<Rigidbody>();

		//Debug.Log("selectedTarget = " + selectedTarget);
		//Debug.Log("selectedTarget Body = " + targetBody);

		//var nearby = Vector3.Lerp(cloudInstance.transform.position, selectedTarget.transform.position, 0.7f);

		//targetBody.AddExplosionForce(ExplosionForce, selectedTarget.transform.position - Vector3.one, distance - (distance * 0.7f));
		//targetBody.AddExplosionForce(SourcePowerup.ExplosionForce * distance, sourcePosition, distance * 2f, 2.0f);

		Debug.Log("Rightbody = " + targetBody.gameObject.name);
		//targetBody.AddRelativeForce(Vector3.right * SourcePowerup.ExplosionForce);
		//targetBody.velocity += (Vector3.up * SourcePowerup.ExplosionForce);
		//targetBody.velocity = (Vector3.up * SourcePowerup.ExplosionForce) + (selectedTarget.transform.position - transform.position).normalized * SourcePowerup.ExplosionRadius;

		var distanceVector = (new Vector3(selectedTarget.transform.position.x, transform.position.y, selectedTarget.transform.position.z) - transform.position);
		targetBody.velocity = (Vector3.up * SourcePowerup.ExplosionForce) + (distanceVector).normalized * SourcePowerup.ExplosionRadius;

		Destroy(gameObject, SourcePowerup.StrikeTime);
	}


	List<GameObject> Targets = new List<GameObject>();

	private void OnCollisionEnter(Collision collision)
	{
		if (((1 << collision.gameObject.layer) & SourcePowerup.collisionMask.value) != 0 && !Targets.Contains(collision.gameObject))
		{
			Debug.Log("fdsafdsafAdding Target_A = " + collision.gameObject);
			Targets.Add(collision.gameObject);
		}
	}

	private void OnCollisionExit(Collision collision)
	{
		Debug.Log("fsdfsdfRemoving Target_A = " + collision.gameObject);
		Targets.Remove(collision.gameObject);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (((1 << other.gameObject.layer) & SourcePowerup.collisionMask.value) != 0 && !Targets.Contains(other.gameObject))
		{
			Debug.Log("fsdfsdfAdding Target_B = " + other.gameObject);
			Targets.Add(other.gameObject);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		Debug.Log("fsdfsdfRemoving Target_B = " + other.gameObject);
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

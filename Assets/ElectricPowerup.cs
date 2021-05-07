using MyBox;
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

	//new SphereCollider collider;

	[Header("Auxillary Action")]
	[SerializeField]
	AuxBolt AuxillaryBoltPrefab;

	//private void Awake()
	//{
		//collider = GetComponentInChildren<SphereCollider>();
	//}


	public override void AuxillaryAction(Vector3 position)
	{
		var auxBolt = GameObject.Instantiate(AuxillaryBoltPrefab,position,Quaternion.identity);
		auxBolt.SourcePowerup = this;
	}

	public override IEnumerator MainAction()
	{
		cloudInstance = GameObject.Instantiate(CloudPrefab,transform.position,Quaternion.identity);
		cloudInstance.SourcePowerup = this;

		//yield return new WaitForSeconds(LifeTime + StrikeTime);

		for (float i = 0; i < LifeTime + StrikeTime; i += Time.deltaTime)
		{
			cloudInstance.transform.position = transform.TransformPoint(CloudOffset);
			yield return null;
		}

		Destroy(cloudInstance);
		//Destroy(cloudInstance.gameObject, LifeTime);
		/*collider.enabled = true;
		cloudInstance = GameObject.Instantiate(CloudPrefab,transform);
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
				StartCoroutine(DoStrike());
			}

			yield return null;
		}

		yield return new WaitForSeconds(StrikeTime);

		Destroy(gameObject);
		Destroy(cloudInstance);*/
	}

	/*IEnumerator DoStrike()
	{
		if (Targets.Count == 0)
		{
			yield break;
		}

		int selectedIndex = Random.Range(0, Targets.Count);
		GameObject selectedTarget = Targets[selectedIndex];

		var topMostParent = GetTopParent(selectedTarget.transform);

		var collector = topMostParent.GetComponentInChildren<PowerupCollector>();

		if (collector == SourceCollector)
		{
			if (Targets.Count == 1)
			{
				yield break;
			}
			else
			{
				selectedIndex = (selectedIndex + 1) % Targets.Count;
				selectedTarget = Targets[selectedIndex];
			}
		}

		//Do a check for the source collector object

		var bolt = GameObject.Instantiate(BoltPrefab, Vector3.zero, Quaternion.identity);

		var distance = Vector3.Distance(cloudInstance.transform.position, selectedTarget.transform.position);

		var midpoint = Vector3.Lerp(cloudInstance.transform.position, selectedTarget.transform.position, 0.5f);

		bolt.transform.position = midpoint;
		bolt.transform.localScale = new Vector3(distance / 16f, distance, 1f);
		bolt.transform.LookAt(selectedTarget.transform.position);
		bolt.transform.rotation *= Quaternion.Euler(90f, 0f, 0f);

		var targetBody = topMostParent.GetComponentInChildren<Rigidbody>();

		//Debug.Log("selectedTarget = " + selectedTarget);
		//Debug.Log("selectedTarget Body = " + targetBody);

		//var nearby = Vector3.Lerp(cloudInstance.transform.position, selectedTarget.transform.position, 0.7f);

		//targetBody.AddExplosionForce(ExplosionForce, selectedTarget.transform.position - Vector3.one, distance - (distance * 0.7f));
		targetBody.AddExplosionForce(ExplosionForce * distance, transform.position, distance * 2f,2.0f);

		Destroy(bolt, StrikeTime);

		yield return new WaitForSeconds(StrikeTime);

		yield break;
	}

	public List<GameObject> Targets = new List<GameObject>();


	private void OnCollisionEnter(Collision collision)
	{
		if (((1 << collision.gameObject.layer) & collisionMask.value) != 0 && !Targets.Contains(collision.gameObject))
		{
			Debug.Log("Adding Target_A = " + collision.gameObject);
			Targets.Add(collision.gameObject);
		}
	}

	private void OnCollisionExit(Collision collision)
	{
		Debug.Log("Removing Target_A = " + collision.gameObject);
		Targets.Remove(collision.gameObject);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (((1 << other.gameObject.layer) & collisionMask.value) != 0 && !Targets.Contains(other.gameObject))
		{
			Debug.Log("Adding Target_B = " + other.gameObject);
			Targets.Add(other.gameObject);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		Debug.Log("Removing Target_B = " + other.gameObject);
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
	}*/
}

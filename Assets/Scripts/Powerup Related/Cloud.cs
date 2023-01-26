using Assets;
using Mirror;
using Nitro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Cloud : NetworkBehaviour
{
	//List<Rigidbody> Targets = new List<Rigidbody>();

	[HideInInspector]
	public ElectricPowerup SourcePowerup;

	[SyncVar]
	public NetworkIdentity SourceCollector;

    //[SyncVar]
	//public CarController SourceCar;

    public override void OnStartServer()
    {
        StartCoroutine(PositionRoutine());
    }

    public override void OnStartClient()
    {
		if (!NetworkServer.active)
		{
			SourcePowerup = GameSettings.Instance.PossiblePowerups.First(p => p is ElectricPowerup) as ElectricPowerup;
			StartCoroutine(PositionRoutine());
		}
    }

	IEnumerator PositionRoutine()
	{
		while (true)
		{
			if (SourceCollector != null)
			{
                transform.position = SourceCollector.transform.TransformPoint(SourcePowerup.CloudOffset);
                transform.rotation = Quaternion.identity;
            }
            yield return null;
		}
	}

    public IEnumerator DoMultipleStrikes()
	{
		//Debug.Log("Starting Multiple Strikes");
		float waitCounter = 0f;
		float waitAmount = Random.Range(SourcePowerup.MinWaitTime, SourcePowerup.MaxWaitTime);
		for (float i = 0; i < SourcePowerup.LifeTime; i += Time.deltaTime)
		{
			waitCounter += Time.deltaTime;
			if (waitCounter >= waitAmount)
			{
				waitCounter -= waitAmount;
				waitAmount = Random.Range(SourcePowerup.MinWaitTime, SourcePowerup.MaxWaitTime);
				StartCoroutine(StrikeRoutine(transform.position));
			}

			yield return null;
		}

		yield return new WaitForSeconds(SourcePowerup.StrikeTime);
	}

	public IEnumerator DoSingleStrike(Vector3 source)
	{
        foreach (var renderer in GetComponentsInChildren<Renderer>())
        {
            renderer.enabled = false;
        }
		yield return StrikeRoutine(source);
    }

	IEnumerator StrikeRoutine(Vector3 source)
	{
		yield return new WaitForFixedUpdate();
		yield return null;


		var players = PlayerManager.Players.ToList();

		if (players.Count <= 1)
		{
			yield break;
		}
		/*if (Targets.Count == 0)
		{
			yield break;
		}*/

        //var sourceBody = SourceCollector.GetComponent<CarController>().RollCage.GetComponentInChildren<Rigidbody>();

		var selectableTargets = new List<CarController>(players.Where(t => t.CarController.gameObject != SourceCollector.gameObject).Select(p => p.CarController));

		Debug.Log("Selectable Targets = " + selectableTargets.Count);

		if (selectableTargets.Count == 0)
		{
			yield break;
		}

		//var selectedTarget = selectableTargets[Random.Range(0, selectableTargets.Count)].RollCage;
		var selectedTarget = selectableTargets.OrderBy(t => Vector3.Distance(t.RollCage.transform.position, transform.position)).First().RollCage;

		if (Vector3.Distance(selectedTarget.transform.position,transform.position) > SourcePowerup.MaxDistance)
		{
			yield break;
        }

		var bolt = GameObject.Instantiate(SourcePowerup.BoltPrefab, Vector3.zero, Quaternion.identity);

		var distance = Vector3.Distance(source, selectedTarget.transform.position);

		var midpoint = Vector3.Lerp(source, selectedTarget.transform.position, 0.5f);

		bolt.transform.position = midpoint;
		bolt.transform.localScale = new Vector3(distance / 16f, distance, 1f);
		bolt.transform.LookAt(selectedTarget.transform.position);
		bolt.transform.rotation *= Quaternion.Euler(90f, 0f, 0f);
		NetworkServer.Spawn(bolt, SourceCollector.gameObject);

		var distanceVector = (new Vector3(selectedTarget.transform.position.x, source.y, selectedTarget.transform.position.z) - source);
        //selectedTarget.velocity = (Vector3.up * SourcePowerup.ExplosionForce) + (distanceVector).normalized * SourcePowerup.ExplosionRadius;

        selectedTarget.Car.AddForce((Vector3.up * SourcePowerup.ExplosionForce) + (distanceVector).normalized * SourcePowerup.ExplosionRadius, ForceMode.VelocityChange);

        //Destroy(bolt, SourcePowerup.StrikeTime);
        //NetworkServer.Destroy();
        StartCoroutine(DestroyAfterTime(SourcePowerup.StrikeTime, bolt));

		SourcePowerup.OnStrike(selectedTarget.RB);

		yield return new WaitForSeconds(SourcePowerup.StrikeTime);

		yield break;
	}

	IEnumerator DestroyAfterTime(float time, GameObject obj)
	{
		yield return new WaitForSeconds(time);
		NetworkServer.Destroy(obj);
	}

	/*private void OnCollisionEnter(Collision collision)
	{
		if (!Targets.Contains(collision.rigidbody) && NetworkServer.active && collision.gameObject.GetComponent<RollCage>() != null)
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
		if (!Targets.Contains(other.attachedRigidbody) && NetworkServer.active && other.gameObject.GetComponent<RollCage>() != null)
		{
			Targets.Add(other.attachedRigidbody);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		Targets.Remove(other.attachedRigidbody);
	}*/

	/*static Transform GetTopParent(Transform transform)
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

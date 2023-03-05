using Assets;
using Mirror;
using Nitro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Unity.Mathematics;
using UnityEngine;

public class Cloud : NetworkBehaviour
{
    [HideInInspector]
    public ElectricPowerup SourcePowerup;

    [SyncVar] 
    [HideInInspector]
    public NetworkIdentity SourceCollector;

    [SerializeField]
    float strikeRadius;

    [SerializeField]
    LayerMask strikeMask;

    [SerializeField]
    AudioClip strikeClip;

    public override void OnStartServer()
    {
        StartCoroutine(PositionRoutine());
    }

    //Method that is called when the object is selected and the gizmos are visible
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow; //set the color of the gizmos to yellow
        Gizmos.DrawWireSphere(transform.position, strikeRadius); //draw a wire sphere at the object's position with the specified radius
    }

    public override void OnStartClient()
    {
        if (!NetworkServer.active)
        {
            SourcePowerup = GameSettings.Instance.PossiblePowerups.First(p => p is ElectricPowerup) as ElectricPowerup; //set the SourcePowerup to the first ElectricPowerup in the PossiblePowerups list
            StartCoroutine(PositionRoutine());
        }
    }

    //coroutine for positioning the object
    IEnumerator PositionRoutine()
    {
        while (true) //loop forever
        {
            if (SourceCollector != null) //if the SourceCollector is not null
            {
                transform.position = SourceCollector.transform.TransformPoint(SourcePowerup.CloudOffset); //set the position of the object to the SourceCollector's position plus the CloudOffset of the SourcePowerup
                transform.rotation = Quaternion.identity; //set the rotation of the object to identity
            }
            yield return null;
        }
    }

    public IEnumerator DoMultipleStrikes()
    {
        // Initialize the wait counter to 0
        float waitCounter = 0f;
        // Randomly generate a wait amount between the minimum and maximum wait time
        float waitAmount = UnityEngine.Random.Range(SourcePowerup.MinWaitTime, SourcePowerup.MaxWaitTime);
        // Loop for the duration of the powerup's life time
        for (float i = 0; i < SourcePowerup.LifeTime; i += Time.deltaTime)
        {
            // Increase the wait counter by the delta time
            waitCounter += Time.deltaTime;
            // If the wait counter is greater than or equal to the wait amount
            if (waitCounter >= waitAmount)
            {
                // Subtract the wait amount from the wait counter
                waitCounter -= waitAmount;
                // Generate a new random wait amount
                waitAmount = UnityEngine.Random.Range(SourcePowerup.MinWaitTime, SourcePowerup.MaxWaitTime);
                // Start the strike routine using the current transform position
                StartCoroutine(StrikeRoutine(transform.position));
            }


            yield return null;
        }

        // Wait for the strike time before ending the coroutine
        yield return new WaitForSeconds(SourcePowerup.StrikeTime);
    }

    public IEnumerator DoSingleStrike(Vector3 source)
    {
        // Disable all renderers on children game objects
        foreach (var renderer in GetComponentsInChildren<Renderer>())
        {
            renderer.enabled = false;
        }
        // Start the strike routine using the given source position
        yield return StrikeRoutine(source);
    }

    IEnumerator StrikeRoutine(Vector3 source)
    {
        yield return new WaitForFixedUpdate();
        yield return null;

        // Get all objects within a certain radius, with a certain mask, and store them in the variable "hits"
        var hits = Physics.SphereCastAll(transform.position, strikeRadius, Vector3.down, 100f, strikeMask);
        // If there are no hits, the function ends
        if (hits.Length == 0)
        {
            yield break;
        }

        // Filter the hits to only include objects that have a rigidbody and are not the source collector's roll cage
        // and store the selected objects in "selectableTargets"
        var selectableTargets = hits.Where(h => h.rigidbody != null && h.collider.GetComponent<RollCage>() != SourceCollector.GetComponent<CarController>().RollCage).Select(h => h.rigidbody).ToList();
        // If there are no selectable targets, the function ends
        if (selectableTargets.Count == 0)
        {
            yield break;
        }

        // Select the closest selectable target
        var selectedTarget = selectableTargets.OrderBy(t => Vector3.Distance(t.transform.position, transform.position)).First();

        // Spawn a lightning bolt between the source and the target
        SpawnLightningBolt(source, selectedTarget.transform.position);

        PlaySound(selectedTarget.transform.position);

        // Create a distance vector between the source and the selected target
        var distanceVector = new Vector3(selectedTarget.transform.position.x, source.y, selectedTarget.transform.position.z) - source;
        // Calculate the force to apply to the target
        Vector3 force = (Vector3.up * SourcePowerup.ExplosionForce) + distanceVector.normalized * SourcePowerup.ExplosionRadius;
        // If the selected target has a roll cage, apply the force to the cage's car
        if (selectedTarget.TryGetComponent<RollCage>(out var cage))
        {
            cage.Car.AddForce(force, ForceMode.VelocityChange);
        }
        else
        {
            selectedTarget.AddForce(force, ForceMode.VelocityChange);
        }

        // Call the function OnStrike on the source powerup, passing in the selected target
        SourcePowerup.OnStrike(selectedTarget);

        // Wait for a certain amount of time
        yield return new WaitForSeconds(SourcePowerup.StrikeTime);

        // End the function
        yield break;
    }

    [ClientRpc]
    void PlaySound(Vector3 target)
    {
        AudioPool.PlaySoundTillDone(strikeClip, target);
    }

    [Server]
	void SpawnLightningBolt(Vector3 source, Vector3 destination)
	{
        var bolt = GameObject.Instantiate(SourcePowerup.BoltPrefab, Vector3.zero, Quaternion.identity);

        var distance = Vector3.Distance(source, destination);
        var midpoint = Vector3.Lerp(source, destination, 0.5f);

        bolt.transform.position = midpoint;
        bolt.transform.localScale = new Vector3(distance / 16f, distance, 1f);
        bolt.transform.LookAt(destination);
        bolt.transform.rotation *= Quaternion.Euler(90f, 0f, 0f);
        NetworkServer.Spawn(bolt, SourceCollector.gameObject);

        StartCoroutine(DestroyAfterTime(SourcePowerup.StrikeTime, bolt));
    }

	IEnumerator DestroyAfterTime(float time, GameObject obj)
	{
		yield return new WaitForSeconds(time);
		NetworkServer.Destroy(obj);
	}
}

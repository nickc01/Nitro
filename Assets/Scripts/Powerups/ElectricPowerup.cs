using Mirror;
using Nitro;
using System;
using System.Collections;
using UnityEngine;


// ElectricPowerup is a class that inherits from CombinablePowerup
public class ElectricPowerup : CombinablePowerup
{
    [SerializeField]
    private Cloud CloudPrefab;

    // Variable for the offset of the cloud prefab
    public Vector3 CloudOffset;
    // Variable for the bolt prefab
    public GameObject BoltPrefab;

    // Variables for the wait time between strikes, lifetime of the powerup, strike time, explosion force, and explosion radius
    public float MinWaitTime;
    public float MaxWaitTime;
    public float LifeTime = 10f;
    public float StrikeTime = 0.1f;
    public float ExplosionForce;
    public float ExplosionRadius;

    // Flag to check if this is the first powerup in the chain
    private bool isFirstPowerup = false;
    // Reference to the instantiated cloud
    private Cloud cloudInstance;
    // Action to run the next powerup
    private Action<Vector3, Quaternion> RunNextPowerup;

    [Server]
    private Cloud CreateCloud()
    {
        //Instantiating a new Cloud object, assign it to instance variable
        Cloud instance = GameObject.Instantiate(CloudPrefab);
        //Assign this ElectricPowerup as the source powerup for the cloud instance
        instance.SourcePowerup = this;
        //Assign the collector as the source collector for the cloud instance
        instance.SourceCollector = Collector.GetGameObject().GetComponent<NetworkIdentity>();
        //Set the local position of the cloud instance to the CloudOffset
        instance.transform.localPosition = CloudOffset;

        //Spawn the cloud instance on the network, with the collector as the parent object
        NetworkServer.Spawn(instance.gameObject, Collector.GetGameObject().gameObject);
        //Return the created cloud instance
        return instance;
    }

    public override void Execute(ICombinablePowerup previous, Vector3 position, Quaternion rotation, Action<Vector3, Quaternion> runNextPowerup)
    {
        //Assign the runNextPowerup action to the RunNextPowerup variable
        RunNextPowerup = runNextPowerup;
        //Check if this is the first powerup
        isFirstPowerup = previous == null;

        //Routine function to be called using StartCoroutine
        IEnumerator Routine()
        {
            if (isFirstPowerup)
            {
                //If this is the first powerup, create a cloud, start the multiple strikes coroutine, and wait for it to finish
                cloudInstance = CreateCloud();
                Coroutine strikeRoutine = cloudInstance.StartCoroutine(cloudInstance.DoMultipleStrikes());

                yield return strikeRoutine;
            }
            else
            {
                //If this is not the first powerup, create a cloud and perform a single strike at the given position
                cloudInstance = CreateCloud();
                yield return cloudInstance.DoSingleStrike(position);
            }

            //if cloudInstance is not null, destroy it on the network
            if (cloudInstance != null)
            {
                NetworkServer.Destroy(cloudInstance.gameObject);
            }

            DoneUsingPowerup();
        }

        //Start the Routine() coroutine
        StartCoroutine(Routine());
    }

    //This is called when the cloud object strikes an object. This is used to execute the auxiliary powerups where the lightining struck
    public void OnStrike(Rigidbody hitObject)
    {
        if (isFirstPowerup)
        {
            RunNextPowerup(hitObject.transform.position, hitObject.transform.rotation);
        }
    }
}

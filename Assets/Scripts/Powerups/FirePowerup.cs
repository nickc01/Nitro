using Mirror;
using Nitro;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class FirePowerup : CombinablePowerup
{
	[SerializeField]
	[Tooltip("How long the fireball will last")]
	float lifeTime = 5f;
	[SerializeField]
	[Tooltip("How fast the fireball will travel")]
	float fireballVelocity = 10f;
	[SerializeField]
	Vector3 spawnOffset;
	[SerializeField]
	GameObject FireballPrefab;


	GameObject fireballInstance;
	[Header("Auxiliary Action")]
	[SerializeField]
	[Tooltip("How fast will auxiliary powerups spawn as the fireball is travelling forwards")]
	float auxillaryExecutionRate = 0.25f;
	[SerializeField]
	float auxillaryLifeTime = 5f;
	[SerializeField]
	float particleSize = 5f;
	[SerializeField]
	FireParticles FireParticles;
	public float dragMultiplier = 5f;

    [SerializeField]
    float forwardAmount = 0.25f;

    [Server]
    void DoFireball(Vector3 position, Quaternion rotation, Action<Vector3, Quaternion> runNextPowerup)
    {
        // spawn fireball at specific position, rotation and offset
        var spawnPosition = transform.TransformPoint(transform.localPosition + spawnOffset);
        fireballInstance = GameObject.Instantiate(FireballPrefab, spawnPosition + (transform.forward * forwardAmount) + new Vector3(0f, 0.025f, 0f), rotation);

        // get the rigidbody of the fireball
        Rigidbody fireballBody = fireballInstance.GetComponent<Rigidbody>();

        // set the velocity of the fireball
        fireballBody.velocity = Collector.GetGameObject().transform.forward * fireballVelocity;

        // spawn fireball on the network
        NetworkServer.Spawn(fireballInstance, Collector.GetGameObject().gameObject);

        // start the coroutine
        StartCoroutine(Routine());

        IEnumerator Routine()
        {
            // loop for the lifetime of the fireball
            for (float t = 0f; t < lifeTime; t += auxillaryExecutionRate)
            {
                // wait for auxillary execution rate
                yield return new WaitForSeconds(auxillaryExecutionRate);
                if (fireballInstance == null)
                {
                    // if fireball is null, break the loop
                    break;
                }
                // run next powerup
                runNextPowerup(fireballInstance.transform.position, rotation);
            }

            if (fireballInstance != null)
            {
                // destroy the fireball object on the network
                NetworkServer.Destroy(fireballInstance);
            }
            // call DoneUsingPowerup function
            DoneUsingPowerup();
        }
    }

    [Server]
    void DoFirepit(Vector3 position)
    {
        //instantiate the fire particles object
        var particleInstance = GameObject.Instantiate(FireParticles, position, Quaternion.identity);
        //set the source of the fire particles to this powerup
        particleInstance.SourcePowerup = this;

        //get all the particle systems in the children of the particle instance
        var particleSystems = particleInstance.GetComponentsInChildren<ParticleSystem>();

        //iterate through all the particle systems 
        foreach (var particle in particleSystems)
        {
            //get the shape component of the particle system
            var shape = particle.shape;
            //set the radius of the shape to the particle size variable
            shape.radius = particleSize;
        }

        //spawn the particle instance on the network server
        NetworkServer.Spawn(particleInstance.gameObject, (Collector as Component).gameObject);
    }


    public override void Execute(ICombinablePowerup previous, Vector3 position, Quaternion rotation, Action<Vector3, Quaternion> runNextPowerup)
    {
        //If this is the first powerup in the chain
		if (previous == null)
		{
            //Spawn a fireball
			DoFireball(position, rotation, runNextPowerup);
        }
        //If this powerup isn't the first in the powerup chain
		else
		{
            //Spawn a circular pit of flames
			DoFirepit(position);
            //Trigger the next powerup in the chain to run
			runNextPowerup(position, rotation);

            DoneUsingPowerup();
        }
    }
}

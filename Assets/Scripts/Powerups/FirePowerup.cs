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
	public float auxillaryTerminalVelocityMultiplier = 0.5f;

	void DoFireball(Vector3 position, Quaternion rotation, Action<Vector3,Quaternion> runNextPowerup)
	{
        var spawnPosition = transform.TransformPoint(transform.localPosition + spawnOffset);
        fireballInstance = GameObject.Instantiate(FireballPrefab, spawnPosition, rotation);

        Rigidbody fireballBody = fireballInstance.GetComponent<Rigidbody>();

        fireballBody.velocity = Collector.transform.forward * fireballVelocity;

        StartCoroutine(Routine());

        IEnumerator Routine()
        {
			for (float t = 0f; t < lifeTime; t += auxillaryExecutionRate)
			{
                yield return new WaitForSeconds(auxillaryExecutionRate);
                runNextPowerup(fireballInstance.transform.position, rotation);
            }

            if (fireballInstance != null)
            {
                Destroy(fireballInstance.gameObject);
            }
			DoneUsingPowerup();
        }
    }

	void DoFirepit(Vector3 position)
	{
        var particleInstance = GameObject.Instantiate(FireParticles, position, Quaternion.identity);
        particleInstance.SourcePowerup = this;

        var particleSystems = particleInstance.GetComponentsInChildren<ParticleSystem>();

        foreach (var particle in particleSystems)
        {
            var shape = particle.shape;
            shape.radius = particleSize;
        }

        particleInstance.StartCoroutine(Routine());


        IEnumerator Routine()
        {
			DoneUsingPowerupAfter(auxillaryLifeTime);
            yield return new WaitForSeconds(auxillaryLifeTime);

            foreach (var particle in particleSystems)
            {
                particle.Stop(false, ParticleSystemStopBehavior.StopEmitting);
            }

            particleInstance.GetComponent<Collider>().enabled = false;
            //particleInstance.Stop();
        }
    }


    public override void Execute(CombinablePowerup previous, Vector3 position, Quaternion rotation, Action<Vector3, Quaternion> runNextPowerup)
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
        }
    }
}

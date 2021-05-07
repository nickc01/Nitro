﻿using System.Collections;
using UnityEngine;

public class FirePowerup : DemoPowerup
{
	[SerializeField]
	[Tooltip("How fast will auxillary powerups spawn as the fireball is travelling forwards")]
	float auxillaryExecutionRate = 0.25f;

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
	[Header("Auxillary Action")]
	[SerializeField]
	float auxillaryLifeTime = 5f;
	[SerializeField]
	float particleSize = 5f;
	[SerializeField]
	FireParticles FireParticles;
	public float auxillaryTerminalVelocityMultiplier = 0.5f;

	/// <summary>
	/// The auxillary action will spawn fire at the position specified
	/// </summary>
	/// <param name="position"></param>
	public override void AuxillaryAction(Vector3 position)
	{
		var particleInstance = GameObject.Instantiate(FireParticles, position, Quaternion.identity);
		particleInstance.SourcePowerup = this;

		var particleSystems = particleInstance.GetComponentsInChildren<ParticleSystem>();

		foreach (var particle in particleSystems)
		{
			var shape = particle.shape;
			shape.radius = particleSize;
		}

		StartCoroutine(AuxillaryRoutine());




		IEnumerator AuxillaryRoutine()
		{
			yield return new WaitForSeconds(auxillaryLifeTime);

			foreach (var particle in particleSystems)
			{
				particle.Stop(false, ParticleSystemStopBehavior.StopEmitting);
			}

			particleInstance.GetComponent<Collider>().enabled = false;

			yield return new WaitForSeconds(10f);


			Destroy(particleInstance.gameObject);
		}


		//Destroy(particleInstance.gameObject, auxillaryLifeTime);
	}

	/// <summary>
	/// The main action shoots a fireball in front of the player
	/// </summary>
	public override IEnumerator MainAction()
	{
		var spawnPosition = transform.TransformPoint(transform.localPosition + spawnOffset);
		fireballInstance = GameObject.Instantiate(FireballPrefab, spawnPosition, SourceCollector.transform.rotation);

		Rigidbody fireballBody = fireballInstance.GetComponent<Rigidbody>();

		fireballBody.velocity = SourceCollector.transform.forward * fireballVelocity;

		Destroy(gameObject, lifeTime);

		while (true)
		{
			yield return new WaitForSeconds(auxillaryExecutionRate);
			ExecuteAuxillaryPowerups(fireballInstance.transform.position);
		}
	}

	void OnDestroy()
	{
		if (fireballInstance != null)
		{
			Destroy(fireballInstance.gameObject);
		}
	}
}

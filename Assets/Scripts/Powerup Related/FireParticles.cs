using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Nitro;

public class FireParticles : Collidable
{
	[HideInInspector]
	public FirePowerup SourcePowerup;

	List<Player> collidedPlayers = new List<Player>();

	/*private void OnTriggerEnter(Collider other)
	{
		var player = other.attachedRigidbody.GetComponent<Player>();
		if (player != null)
		{
			if (!collidedPlayers.Contains(player))
			{
				
			}
			collidedPlayers.Add(player);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		var player = other.attachedRigidbody.GetComponent<Player>();
		if (player != null)
		{
			collidedPlayers.Remove(player);
			if (!collidedPlayers.Contains(player))
			{
				
			}
		}
	}*/

	public void Stop()
	{
		foreach (var player in collidedPlayers.Distinct())
		{
			player.Movement.TerminalVelocity.Revert(this);
		}
	}

	private void OnParticleSystemStopped()
	{
		Destroy(gameObject);
	}

	protected override void OnCollideStart(Rigidbody body)
	{
		var player = body.GetComponent<Player>();
		if (player != null)
		{
			player.Movement.TerminalVelocity.Modify(player.Movement.TerminalVelocity.BaseValue * SourcePowerup.auxillaryTerminalVelocityMultiplier, this, 20);
		}
	}

	protected override void OnCollideStop(Rigidbody body)
	{
		var player = body.GetComponent<Player>();
		if (player != null)
		{
			player.Movement.TerminalVelocity.Revert(this);
		}
	}
}

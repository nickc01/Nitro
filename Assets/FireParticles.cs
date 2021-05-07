using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireParticles : MonoBehaviour, IPlayerDetector
{
	[HideInInspector]
	public FirePowerup SourcePowerup;


	public void OnPlayerTouch(Player player)
	{
		player.Movement.TerminalVelocity.Modify(this, player.Movement.TerminalVelocity * SourcePowerup.auxillaryTerminalVelocityMultiplier, 10);
	}

	public void OnPlayerUnTouch(Player player)
	{
		player.Movement.TerminalVelocity.Revert(this);
	}
}

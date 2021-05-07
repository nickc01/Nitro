using System.Collections.Generic;
using UnityEngine;

public class Puddle : MonoBehaviour, IPlayerDetector
{
	[SerializeField]
	float terminalVelocityMultiplier = 0.5f;

	[SerializeField]
	Vector3 spawnOffset;


	private void Awake()
	{
		Debug.DrawLine(transform.position, transform.position + (Vector3.down * 100f), Color.red, 10f);
		if (Physics.Raycast(transform.position, Vector3.down, out var info, 100f))
		{
			transform.position = info.point + spawnOffset;
		}
	}

	public void OnPlayerTouch(Player player)
	{
		player.Movement.TerminalVelocity.Modify(this, player.Movement.TerminalVelocity * terminalVelocityMultiplier, 10);
	}

	public void OnPlayerUnTouch(Player player)
	{
		player.Movement.TerminalVelocity.Revert(this);
	}
}

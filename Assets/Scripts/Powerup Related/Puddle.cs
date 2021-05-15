using Nitro;
using System.Collections.Generic;
using UnityEngine;

public class Puddle : Collidable
{
	[SerializeField]
	float terminalVelocityMultiplier = 0.5f;

	[SerializeField]
	Vector3 spawnOffset;

	[SerializeField]
	float puddleLifeTime = 5f;

	private void Awake()
	{
		Debug.DrawLine(transform.position, transform.position + (Vector3.down * 100f), Color.red, 10f);
		if (Physics.Raycast(transform.position, Vector3.down, out var info, 100f))
		{
			transform.position = info.point + spawnOffset;
		}

		Destroy(gameObject, puddleLifeTime);
	}

	protected override void OnCollideStart(Rigidbody body)
	{
		var player = body.GetComponent<Player>();
		if (player != null)
		{
			player.Movement.TerminalVelocity.Modify(player.Movement.TerminalVelocity.BaseValue * terminalVelocityMultiplier, this, 10);
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

using Nitro;
using UnityEngine;

public class OilSlick : Collidable
{
	protected override void OnCollideStart(Rigidbody body)
	{
		var player = body.GetComponent<Player>();
		player?.Movement.TerminalVelocity.Modify(1f, this, 10);
	}

	protected override void OnCollideStop(Rigidbody body)
	{
		var player = body.GetComponent<Player>();
		player?.Movement.TerminalVelocity.Revert(this);
	}
}

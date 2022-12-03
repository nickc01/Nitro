using Nitro;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

public class WaterPowerup : CombinablePowerup
{
	[SerializeField]
	GameObject PuddlePrefab;

	[SerializeField]
	[FormerlySerializedAs("normalPuddleSize")]
	float largePuddleSize = 1f;

	[SerializeField]
	[FormerlySerializedAs("auxillaryPuddleSize")]
	float smallPuddleSize = 0.5f;

	//Spawns a puddle with the specified size and at the specified position
	public void SpawnPuddle(float size, Vector3 position)
	{
		var puddle = GameObject.Instantiate(PuddlePrefab, position, Quaternion.identity);

		puddle.transform.localScale = new Vector3(size,puddle.transform.localScale.y,size);
	}

    public override void Execute(CombinablePowerup previous, Vector3 position, Quaternion rotation, Action<Vector3, Quaternion> runNextPowerup)
    {
		//If this powerup is the first in the powerup chain
		if (previous == null)
		{
			//Spawn a large puddle
            SpawnPuddle(largePuddleSize, position);
        }
		else
		{
			//Spawn a small puddle
            SpawnPuddle(smallPuddleSize, position);
        }
		//Trigger the next powerup in the powerup chain. You can tell it to execute at a certain position and rotation
		runNextPowerup(position, rotation);

		//Signal that this powerup is done with its execution
        DoneUsingPowerup();
    }
}


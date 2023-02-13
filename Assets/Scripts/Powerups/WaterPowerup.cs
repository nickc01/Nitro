using Mirror;
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
	float largePuddleSize = 1f;

	[SerializeField]
	float smallPuddleSize = 0.5f;

	[SerializeField]
	float placementOffset = -0.2f;

	//Spawns a puddle with the specified size and at the specified position
	public Vector3 SpawnPuddle(float size, Vector3 position)
	{
		var sourcePlayer = Collector.GetGameObject();

		var spawnPosition = position + sourcePlayer.transform.forward * placementOffset;

        var puddle = GameObject.Instantiate(PuddlePrefab, spawnPosition, Quaternion.identity);

		puddle.transform.localScale = new Vector3(size,puddle.transform.localScale.y,size);

		NetworkServer.Spawn(puddle, sourcePlayer);

		return spawnPosition;
    }

    public override void Execute(ICombinablePowerup previous, Vector3 position, Quaternion rotation, Action<Vector3, Quaternion> runNextPowerup)
    {
		Vector3 spawnPos;

		//If this powerup is the first in the powerup chain
		if (previous == null)
		{
            //Spawn a large puddle
            spawnPos = SpawnPuddle(largePuddleSize, position);
        }
		else
		{
            //Spawn a small puddle
            spawnPos = SpawnPuddle(smallPuddleSize, position);
        }
		//Trigger the next powerup in the powerup chain. You can tell it to execute at a certain position and rotation
		runNextPowerup(spawnPos, rotation);

		//Signal that this powerup is done with its execution
        DoneUsingPowerup();
    }
}


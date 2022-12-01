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

	//The auxiliary action of the water powerup
	/*public override void DoAuxillaryAction(CombinablePowerup sourcePowerup, Vector3 position)
	{
		SpawnPuddle(smallPuddleSize, position);
	}

	//The main action of the water powerup
	public override void DoMainAction(AuxPowerups AuxillaryPowerups)
	{
		SpawnPuddle(largePuddleSize, transform.position);
		AuxillaryPowerups.Execute(this, transform.position);
		DoneUsingPowerup();
	}*/

	//Spawns a puddle with the specified size and at the specified position
	public void SpawnPuddle(float size, Vector3 position)
	{
		var puddle = GameObject.Instantiate(PuddlePrefab, position, Quaternion.identity);

		puddle.transform.localScale = new Vector3(size,puddle.transform.localScale.y,size);
	}

    public override void Execute(CombinablePowerup previous, Vector3 position, Quaternion rotation, Action<Vector3, Quaternion> runNextPowerup)
    {
		if (previous == null)
		{
            SpawnPuddle(largePuddleSize, position);
			//AuxillaryPowerups.Execute(this, transform.position);
        }
		else
		{
            SpawnPuddle(smallPuddleSize, position);
        }
		runNextPowerup(position, rotation);
        DoneUsingPowerup();
    }
}


using Nitro;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class WaterPowerup : CombinablePowerup
{
	[SerializeField]
	GameObject PuddlePrefab;

	[SerializeField]
	float normalPuddleSize = 1f;

	[SerializeField]
	float auxillaryPuddleSize = 0.5f;

	//The auxiliary action of the water powerup
	public override void DoAuxillaryAction(CombinablePowerup sourcePowerup, Vector3 position)
	{
		SpawnPuddle(auxillaryPuddleSize, position);
	}

	//The main action of the water powerup
	public override void DoMainAction(AuxPowerups AuxillaryPowerups)
	{
		SpawnPuddle(normalPuddleSize, transform.position);
		AuxillaryPowerups.Execute(this, transform.position);
		DoneUsingPowerup();
	}

	//Spawns a puddle with the specified size and at the specified position
	public void SpawnPuddle(float size, Vector3 position)
	{
		var puddle = GameObject.Instantiate(PuddlePrefab, position, Quaternion.identity);

		puddle.transform.localScale = new Vector3(size,puddle.transform.localScale.y,size);
	}
}


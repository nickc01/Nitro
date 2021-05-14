using Nitro;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class WaterPowerup : DemoPowerup
{
	[SerializeField]
	GameObject PuddlePrefab;

	[SerializeField]
	float normalPuddleSize = 1f;

	[SerializeField]
	float auxillaryPuddleSize = 0.5f;

	public override void DoAuxillaryAction(CombinablePowerup sourcePowerup, Vector3 position, Collector collector)
	{
		SpawnPuddle(auxillaryPuddleSize, position);
	}

	public override void DoMainAction(Collector collector, IEnumerable<CombinablePowerup> AuxillaryPowerups)
	{
		SpawnPuddle(normalPuddleSize, transform.position);
		foreach (var aux in AuxillaryPowerups)
		{
			aux.DoAuxillaryAction(this, transform.position, collector);
		}
		DoneUsingPowerup();
	}

	public void SpawnPuddle(float size, Vector3 position)
	{
		var puddle = GameObject.Instantiate(PuddlePrefab, position, Quaternion.identity);

		puddle.transform.localScale = new Vector3(size,puddle.transform.localScale.y,size);
	}
}


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

	[SerializeField]
	float puddleLifeTime = 5f;


	public override void AuxillaryAction(Vector3 position)
	{
		SpawnPuddle(auxillaryPuddleSize, position, false);
	}

	public override IEnumerator MainAction()
	{
		SpawnPuddle(normalPuddleSize, transform.position, true);
		Destroy(gameObject);
		yield break;
	}


	public void SpawnPuddle(float size, Vector3 position, bool doAuxillaryActions)
	{
		var puddle = GameObject.Instantiate(PuddlePrefab, position, Quaternion.identity);

		puddle.transform.localScale = new Vector3(size,puddle.transform.localScale.y,size);

		if (doAuxillaryActions)
		{
			ExecuteAuxillaryPowerups(transform.position);
		}

		Destroy(puddle, puddleLifeTime);
	}
}


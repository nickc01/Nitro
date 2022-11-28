using Nitro;
using UnityEngine;

public class PowerupExecuter : MonoBehaviour
{
	PowerupCollector collector;

	private void Awake()
	{
		collector = GetComponent<PowerupCollector>();
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.P))
		{
			collector.Execute();
		}
	}
}

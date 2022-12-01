using Nitro;
using UnityEngine;

public class PowerupExecuter : MonoBehaviour
{
	SinglePowerupCollector collector;

	private void Awake()
	{
		collector = GetComponent<SinglePowerupCollector>();
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.P))
		{
			collector.Execute();
		}
	}
}

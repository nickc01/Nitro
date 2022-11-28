using Nitro;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Player : MonoBehaviour
{
	MultiplePowerupCollector collector;
	PowerupCollector singleCollector;

	CarMovement _movement;
	public CarMovement Movement
	{
		get
		{
			if (_movement == null)
			{
				_movement = GetComponent<CarMovement>();
			}
			return _movement;
		}
	}

	private void Awake()
	{
		collector = GetComponent<MultiplePowerupCollector>();
		singleCollector = GetComponent<PowerupCollector>();
		if (!collector.CollectorEnabled)
		{
			PowerupColorDisplay.Instance.gameObject.SetActive(false);
		}
		collector.PowerupCollectEvent.AddListener(OnPowerupCollect);
	}

	static void OnPowerupCollect(Powerup powerup)
	{
		var colorizer = powerup.GetComponent<Colorizer>();
		if (colorizer != null)
		{
			PowerupColorDisplay.AddColor(colorizer.Color);
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			if (collector.CollectorEnabled)
			{
				if (collector.HeldPowerups.Count > 0)
				{
					PowerupColorDisplay.Clear();
				}
				collector.Execute();
			}
			else
			{
				singleCollector.Execute();
			}
		}

		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Application.Quit();
		}
	}
}

using Nitro;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Player : MonoBehaviour
{
	MultiplePowerupCollector collector;

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
		collector.PowerupCollectEvent.AddListener(OnPowerupCollect);
	}

	static void OnPowerupCollect(Powerup powerup)
	{
		if (powerup is DemoPowerup dPowerup)
		{
			PowerupColorDisplay.AddColor(dPowerup.Color);
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			if (collector.HeldPowerups.Count > 0)
			{
				PowerupColorDisplay.Clear();
			}
			collector.Execute();
		}
	}
}

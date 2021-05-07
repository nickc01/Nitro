using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Player : MonoBehaviour
{
	PowerupCollector collector;

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
		collector = GetComponent<PowerupCollector>();
	}

	private void OnTriggerEnter(Collider other)
	{
		var powerup = other.gameObject.GetComponent<DemoPowerup>();
		if (powerup != null && collector.CollectPowerup(powerup))
		{
			PowerupColorDisplay.AddColor(powerup.Color);
		}

		other.gameObject.GetComponent<IPlayerDetector>()?.OnPlayerTouch(this);
	}

	private void OnTriggerExit(Collider other)
	{
		other.gameObject.GetComponent<IPlayerDetector>()?.OnPlayerUnTouch(this);
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			if (collector.ExecutePowerups() > 0)
			{
				PowerupColorDisplay.Clear();
			}
		}
	}
}

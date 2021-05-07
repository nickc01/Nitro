using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Used for collecting powerups
/// </summary>
public class PowerupCollector : MonoBehaviour
{
	[Tooltip("Represents the maximum amount of powerups the collector can hold at once")]
	public int MaxPowerupsHeld = 3;

	[Tooltip("If set to true, the collector will require that the powerups collected are different types")]
	public bool DifferingTypesRequired = true;
	/// <summary>
	/// The total powerups collected
	/// </summary>
	public IEnumerable<CombinablePowerup> CollectedPowerups => _collectedPowerups;
	public IEnumerable<Type> CollectedPowerupTypes
	{
		get
		{
			foreach (var p in _collectedPowerups)
			{
				yield return p.GetType();
			}
		}
	}
	public int CollectedPowerupCount => _collectedPowerups.Count;
	List<CombinablePowerup> _collectedPowerups = new List<CombinablePowerup>();

	/// <summary>
	/// Collects a powerup
	/// </summary>
	/// <param name="powerup">The powerup to collect</param>
	/// <returns>Returns true if the powerup was collected, and false otherwise</returns>
	public bool CollectPowerup(CombinablePowerup powerup)
	{
		if (CollectedPowerupCount >= MaxPowerupsHeld)
		{
			return false;
		}

		if (DifferingTypesRequired)
		{
			var powerupType = powerup.GetType();
			if (CollectedPowerupTypes.Any(t => t == powerupType))
			{
				return false;
			}
		}
		_collectedPowerups.Add(powerup);
		powerup.CollectPowerup(this);
		OnPowerupChange();

		return true;
	}

	/// <summary>
	/// Clears the collected powerups
	/// </summary>
	public void ClearPowerups()
	{
		foreach (var powerup in CollectedPowerups)
		{
			Destroy(powerup.gameObject);
		}
		_collectedPowerups.Clear();
		OnPowerupChange();
	}

	/// <summary>
	/// Executes the collected powerups
	/// </summary>
	/// <returns>Returns how many powerups were executed</returns>
	public int ExecutePowerups()
	{
		if (_collectedPowerups.Count == 0)
		{
			return 0;
		}

		CombinablePowerup prioritizedPowerup = null;
		int highestPriority = int.MinValue;
		foreach (var pUp in _collectedPowerups)
		{
			if (prioritizedPowerup == null || pUp.Priority > highestPriority)
			{
				prioritizedPowerup = pUp;
				highestPriority = pUp.Priority;
			}
		}

		var powerups = _collectedPowerups;
		_collectedPowerups = new List<CombinablePowerup>();

		prioritizedPowerup.ExecutePowerup(powerups.Where(p => p != prioritizedPowerup));

		OnPowerupChange();

		return powerups.Count;
	}

	/// <summary>
	/// Called when a powerup is collected or removed from the <see cref="CollectedPowerups"/> list
	/// </summary>
	protected virtual void OnPowerupChange()
	{

	}
}


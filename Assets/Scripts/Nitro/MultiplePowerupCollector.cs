using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Nitro
{
	/// <summary>
	/// A collector that can collect multiple powerups
	/// </summary>
	public class MultiplePowerupCollector : Collector
	{
		[Tooltip("Represents the maximum amount of powerups the collector can hold at once")]
		public int MaxPowerupsHeld = 3;

		[Tooltip("If set to true, the collector will require that the powerups collected are different types")]
		public bool DifferingTypesRequired = true;

		/// <summary>
		/// A list of all the currently held powerups, sorted by <see cref="CombinablePowerup.Priority"/>
		/// </summary>
		[NonSerialized]
		public SortedSet<CombinablePowerup> HeldPowerups = new SortedSet<CombinablePowerup>(new CombinablePowerup.Comparer());

		/// <inheritdoc/>
		public override bool CanCollectPowerup(Powerup powerup)
		{
			if (powerup is CombinablePowerup && HeldPowerups.Count < MaxPowerupsHeld)
			{
				if (!DifferingTypesRequired || (DifferingTypesRequired && !HeldPowerups.Any(p => p.GetType() == powerup.GetType())))
				{
					return true;
				}
			}
			return false;
		}

		/// <inheritdoc/>
		public override void Execute()
		{
			if (HeldPowerups.Count > 0)
			{
				HeldPowerups.Max.DoAction();
				HeldPowerups.Clear();
			}
		}

		/// <inheritdoc/>
		protected override void OnCollect(Powerup powerup)
		{
			HeldPowerups.Add(powerup as CombinablePowerup);
		}
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Nitro
{


    /// <summary>
    /// A collector that can collect multiple powerups
    /// </summary>
    public class MultiplePowerupCollector : Collector, IMultiplePowerupCollector
	{
		[Tooltip("Represents the maximum amount of powerups the collector can hold at once")]
		public int maxPowerupsHeld = 3;

		[Tooltip("If set to true, the collector will require that the powerups collected are different types")]
		public bool DifferingTypesRequired = true;

		public int MaxPowerupsHeld => maxPowerupsHeld;

		/// <summary>
		/// A list of all the currently held powerups, sorted by <see cref="CombinablePowerup.Priority"/>
		/// </summary>
		[NonSerialized]
		SortedSet<ICombinablePowerup> heldPowerups = new SortedSet<ICombinablePowerup>(new CombinablePowerup.Comparer());

		public IEnumerable<ICombinablePowerup> CollectedPowerups => heldPowerups;

		/// <inheritdoc/>
		public override bool CanCollectPowerup(IPowerup powerup)
		{
			if (powerup is ICombinablePowerup && heldPowerups.Count < maxPowerupsHeld)
			{
				if (!DifferingTypesRequired || (DifferingTypesRequired && !heldPowerups.Any(p => p.GetType() == powerup.GetType())))
				{
					return true;
				}
			}
			return false;
		}

		/// <inheritdoc/>
		public override void Execute()
		{
			if (heldPowerups.Count > 0)
			{
				heldPowerups.Max.DoAction();
				heldPowerups.Clear();
			}
		}

		/// <inheritdoc/>
		protected override void OnCollect(IPowerup powerup)
		{
			heldPowerups.Add(powerup as ICombinablePowerup);
		}
	}
}
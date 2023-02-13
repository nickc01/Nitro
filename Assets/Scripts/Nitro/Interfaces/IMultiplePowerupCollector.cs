using System.Collections.Generic;

namespace Nitro
{
	/// <summary>
	/// The base class for a powerup collector that can collect multiple powerups
	/// </summary>
    public interface IMultiplePowerupCollector : ICollector
	{
		/// <summary>
		/// The maximum amount of powerups this collector can hold
		/// </summary>
		int MaxPowerupsHeld { get; }

		/// <summary>
		/// A list of all collected powerups
		/// </summary>
		IEnumerable<ICombinablePowerup> CollectedPowerups { get; }
	}
}
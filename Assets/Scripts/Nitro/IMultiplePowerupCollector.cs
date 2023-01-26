using System.Collections.Generic;

namespace Nitro
{
    public interface IMultiplePowerupCollector : ICollector
	{
		int MaxPowerupsHeld { get; }

		IEnumerable<ICombinablePowerup> CollectedPowerups { get; }
	}
}
using System;
using UnityEngine;

namespace Nitro
{
    /// <summary>
    /// The base class for all combinable powerups
    /// </summary>
    public interface ICombinablePowerup : IPowerup
	{
        /// <summary>
        /// The priority of the powerup, which determines whether or not this powerup will get executed before others
        ///
        ///For example, if you have a fire powerup that has a higher priority than a water powerup, then the fire effect will be executed before the water effect.
        /// </summary>
		int Priority { get; }


        /// <summary>
        /// The main action of the combinable powerup
        /// </summary>
        /// <param name="previous">The previous powerup in the chain. If this is null, then the currently executing powerup is first in the chain</param>
        /// <param name="position">The position of the collector the powerup is from</param>
        /// <param name="rotation">The rotation of the collector the powerup is from</param>
        /// <param name="runNextPowerup">A delegate used to execute the next powerup in the chain. Be sure to call this to make sure all the powerups in the chain get executed</param>
        void Execute(ICombinablePowerup previous, Vector3 position, Quaternion rotation, Action<Vector3, Quaternion> runNextPowerup);
    }
}
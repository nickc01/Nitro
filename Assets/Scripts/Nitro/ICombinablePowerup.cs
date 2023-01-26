using System;
using UnityEngine;

namespace Nitro
{
    public interface ICombinablePowerup : IPowerup
	{
		int Priority { get; }

        void Execute(ICombinablePowerup previous, Vector3 position, Quaternion rotation, Action<Vector3, Quaternion> runNextPowerup);
    }
}
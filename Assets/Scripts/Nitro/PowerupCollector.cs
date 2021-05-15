using Nitro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Nitro
{
	/// <summary>
	/// A simple powerup collector that collects and stores a single powerup
	/// </summary>
	public class PowerupCollector : Collector
	{
		/// <summary>
		/// The currently collected powerup
		/// </summary>
		public Powerup CurrentPowerup { get; private set; }

		/// <inheritdoc/>
		public override bool CanCollectPowerup(Powerup powerup)
		{
			return CurrentPowerup == null;
		}

		/// <inheritdoc/>
		public override void Execute()
		{
			if (CurrentPowerup != null)
			{
				CurrentPowerup.DoAction();
				CurrentPowerup = null;
			}
		}

		/// <inheritdoc/>
		protected override void OnCollect(Powerup powerup)
		{
			CurrentPowerup = powerup;
		}

		/// <summary>
		/// Removes the <see cref="CurrentPowerup"/>
		/// </summary>
		public void RemovePowerup()
		{
			if (CurrentPowerup != null)
			{
				CurrentPowerup.DoneUsingPowerup();
				CurrentPowerup = null;
			}
		}
	}
}
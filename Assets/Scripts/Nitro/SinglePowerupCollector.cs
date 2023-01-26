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
	public class SinglePowerupCollector : Collector
	{
		/// <summary>
		/// The currently collected powerup
		/// </summary>
		public IPowerup CurrentPowerup { get; private set; }

		/// <inheritdoc/>
		public override bool CanCollectPowerup(IPowerup powerup)
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
		protected override void OnCollect(IPowerup powerup)
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
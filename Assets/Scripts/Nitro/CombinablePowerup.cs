using Nitro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Nitro
{
	/// <summary>
	/// A powerup that can have its effects combined with other powerups. Combinable powerups work by having 1 powerup act as the primary powerup, where it does its main action. All other powerups will act as secondary powerups, and they will do auxiliary actions on top of the primary's main action
	/// </summary>
	public abstract class CombinablePowerup : Powerup
	{
		/// <summary>
		/// A comparer used for sorting combinable powerups by priority
		/// </summary>
		public class Comparer : IComparer<CombinablePowerup>
		{
			Comparer<int> intComparer = Comparer<int>.Default;
			public int Compare(CombinablePowerup x, CombinablePowerup y)
			{
				if (x.priority == y.priority)
				{
					return intComparer.Compare(x.GetInstanceID(), y.GetInstanceID());
				}
				else
				{
					return intComparer.Compare(x.priority, y.priority);
				}
			}
		}

		[SerializeField]
		[Tooltip(@"The priority of the powerup, which determines how the powerup will be done when in a combo

For example, if you have a fire powerup that has a higher priority than a water powerup, then the fire effect will be applied before the water effect. The fire powerup will be the primary powerup doing its primary action, while the water powerup is secondary and does an auxiliary action on top of the fire effect")]
		protected int priority;

		public int Priority => priority;

		/// <summary>
		/// A list of all the secondary/auxiliary powerups that are to be combined with the main powerup. This list is empty of the current powerup is not the primary powerup
		/// </summary>
		protected AuxPowerups AuxillaryPowerups = new AuxPowerups();

		/// <inheritdoc/>
		public override sealed void DoAction()
		{
			if (Collector is MultiplePowerupCollector mpc)
			{
				AuxillaryPowerups = GetAuxillaryPowerups(mpc);
			}
			DoMainAction(AuxillaryPowerups);
		}

		/// <summary>
		/// The main action that is only called if this powerup is the primary powerup
		/// </summary>
		/// <param name="collector">The source collector that collected this powerup</param>
		/// <param name="auxillaryPowerups">All the other auxiliary actions that are to be applied</param>
		public abstract void DoMainAction(AuxPowerups auxillaryPowerups);

		/// <summary>
		/// The auxiliary action that is only called if this powerup is a secondary powerup
		/// </summary>
		/// <param name="primaryPowerup">The primary powerup</param>
		/// <param name="position">The position that the effects are to be played at</param>
		public abstract void DoAuxillaryAction(CombinablePowerup primaryPowerup, Vector3 position);

		private AuxPowerups GetAuxillaryPowerups(MultiplePowerupCollector collector)
		{
			return new AuxPowerups(collector.HeldPowerups.Where(p => p != this));
		}

		/// <inheritdoc/>
		public override void DoneUsingPowerup()
		{
			foreach (var aux in AuxillaryPowerups)
			{
				Destroy(aux.gameObject);
			}
			base.DoneUsingPowerup();
		}
	}
}
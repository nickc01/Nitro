using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Nitro
{
	/// <summary>
	/// Represents a collection of auxiliary powerups
	/// </summary>
	public class AuxPowerups : List<CombinablePowerup>
	{
		public AuxPowerups(IEnumerable<CombinablePowerup> collection) : base(collection) { }
		public AuxPowerups(int capacity) : base(capacity) { }
		public AuxPowerups() { }

		/// <summary>
		/// Executes all the auxiliary powerups in the collection
		/// </summary>
		/// <param name="primaryPowerup">The primary powerup that is doing the primary action</param>
		/// <param name="position">The position of where the auxiliary effects should take place</param>
		/// <param name="sourceCollector">The collector that collected the powerups</param>
		public void Execute(CombinablePowerup primaryPowerup, Vector3 position)
		{
			foreach (var aux in this)
			{
				aux.DoAuxillaryAction(primaryPowerup, position);
			}
		}

		/// <summary>
		/// Call this to property dispose of the auxiliary powerups
		/// </summary>
		public void DoneUsingPowerups()
		{
			foreach (var aux in this)
			{
				aux.DoneUsingPowerup();
			}
		}

		/// <summary>
		/// Call this to properly dispose of the auxiliary powerups after a set amount of time
		/// </summary>
		/// <param name="time">The time before the powerups are disposed</param>
		public void DoneUsingPowerupsAfter(float time)
		{
			foreach (var aux in this)
			{
				aux.DoneUsingPowerupAfter(time);
			}
		}

		/// <summary>
		/// Attempts to retrieve a powerup from the collection that derives from type T
		/// </summary>
		/// <typeparam name="T">The type filter</typeparam>
		/// <returns>Returns the found powerup. Returns null if no powerup was found</returns>
		public T GetPowerup<T>() where T : CombinablePowerup
		{
			if (TryGetPowerup<T>(out var powerup))
			{
				return powerup;
			}
			else
			{
				return default;
			}
		}

		/// <summary>
		/// Attempts to retrieve a powerup from the collection that derives from type T
		/// </summary>
		/// <typeparam name="T">The type filter</typeparam>
		/// <param name="powerup">The found powerup</param>
		/// <returns>Returns whether or not a powerup has been found</returns>
		public bool TryGetPowerup<T>(out T powerup) where T : CombinablePowerup
		{
			var index = FindIndex(p => typeof(T).IsAssignableFrom(p.GetType()));
			if (index == -1)
			{
				powerup = null;
				return false;
			}
			else
			{
				powerup = (T)this[index];
				return true;
			}
		}

		/// <summary>
		/// Gets whether the collection has a powerup that derives from type T
		/// </summary>
		/// <typeparam name="T">The type filter</typeparam>
		/// <returns>Returns whether the collection has a powerup that derives from type T</returns>
		public bool HasPowerup<T>() where T : CombinablePowerup
		{
			return TryGetPowerup<T>(out _);
		}

		/// <summary>
		/// Gets how many powerups that derive from type T
		/// </summary>
		/// <typeparam name="T">The type filter</typeparam>
		/// <returns>Returns how many powerups that derive from type T</returns>
		public int HasPowerups<T>() where T : CombinablePowerup
		{
			return this.Count(p => typeof(T).IsAssignableFrom(p.GetType()));
		}

		/// <summary>
		/// Gets the powerups that are derived from type T
		/// </summary>
		/// <typeparam name="T">The type filter</typeparam>
		/// <returns>Returns all the powerups that are derived from type T</returns>
		public IEnumerable<T> GetPowerups<T>() where T : CombinablePowerup
		{
			return this.OfType<T>();
		}
	}
}

using Nitro;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;


namespace Nitro
{
    /// <summary>
    /// A powerup that can have its effects combined with other powerups. When several powerups are collected via a <see cref="MultiplePowerupCollector"/>, they will form a chain that is sorted based on the <see cref="Priority"/> of the powerups.
    /// 
    /// The powerups with the highest priority will be executed first, while the ones with the lowest priority will be executed last. When a powerup is executed, it will have it's <see cref="Execute(CombinablePowerup, Vector3, Quaternion, Action{Vector3, Quaternion})"/> function called.
    /// 
    /// With the execute function, you will be able to know the previous powerup in the chain, the position and rotation of where the powerup is affecting, and a delegate called runNextPowerup, which when called, will execute the next powerup in the chain. You can use this delegate to control where and when the next powerups in the chain are executed"/>
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
                    return intComparer.Compare(y.GetInstanceID(), x.GetInstanceID());
                }
                else
                {
                    return intComparer.Compare(y.priority, x.priority);
                }
            }
        }

        [SerializeField]
        [Tooltip(@"The priority of the powerup, which determines whether or not this powerup will get executed before others

For example, if you have a fire powerup that has a higher priority than a water powerup, then the fire effect will be executed before the water effect.")]
        protected int priority;

        /// <summary>
        /// The priority of the powerup, which determines whether or not this powerup will get executed before others
        ///
        ///For example, if you have a fire powerup that has a higher priority than a water powerup, then the fire effect will be executed before the water effect.
        /// </summary>
		public int Priority => priority;

        [NonSerialized]
        private CombinablePowerup[] _powerups;
        [NonSerialized]
        private bool[] _completedPowerups;
        [NonSerialized]
        private int _index;
        [NonSerialized]
        private Dictionary<int, Action<Vector3, Quaternion>> _lambdaCache;

        /// <summary>
        /// Gets a list of all the powerups in the chain
        /// </summary>
        protected ReadOnlySpan<CombinablePowerup> GetPowerupChain() => _powerups != null ? ReadOnlySpan<CombinablePowerup>.Empty : new ReadOnlySpan<CombinablePowerup>(_powerups);

        /// <summary>
        /// Checks if a certain powerup is within the powerup chain
        /// </summary>
        /// <typeparam name="T">The type of powerup to check for</typeparam>
        /// <returns>Returns true if the powerup type is within the chain</returns>
        protected bool HasPowerupInChain<T>() => HasPowerupInChain(typeof(T));

        /// <summary>
        /// Checks if a certain powerup is within the powerup chain
        /// </summary>
        /// <typeparam name="T">The type of powerup to check for</typeparam>
        /// <param name="powerup">The resulting powerup</param>
        /// <returns>Returns true if the powerup type is within the chain</returns>
        protected bool HasPowerupInChain<T>(out T powerup)
        {
            CombinablePowerup resultPowerup;
            var result = HasPowerupInChain(typeof(T), out resultPowerup);
            if (resultPowerup != null)
            {
                powerup = (T)(object)resultPowerup;
            }
            else
            {
                powerup = default;
            }
            return result;
        }

        /// <summary>
        /// Checks if a certain powerup is within the powerup chain
        /// </summary>
        /// <param name="powerupType">The type of powerup to check for</param>
        /// <returns>Returns true if the powerup type is within the chain</returns>
        protected bool HasPowerupInChain(Type powerupType)
        {
            foreach (var p in GetPowerupChain())
            {
                if (powerupType.IsAssignableFrom(p.GetType()))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if a certain powerup is within the powerup chain
        /// </summary>
        /// <param name="powerupType">The type of powerup to check for</param>
        /// <param name="powerup">The resulting powerup</param>
        /// <returns>Returns true if the powerup type is within the chain</returns>
        protected bool HasPowerupInChain(Type powerupType, out CombinablePowerup powerup)
        {
            foreach (var p in GetPowerupChain())
            {
                if (powerupType.IsAssignableFrom(p.GetType()))
                {
                    powerup = p;
                    return true;
                }
            }
            powerup = null;
            return false;
        }

        /// <summary>
        /// Retrieves the index of the current powerup within the powerup chain.
        /// </summary>
        /// <returns></returns>
        protected int GetPowerupIndex() => _index;

		/// <inheritdoc/>
		public override sealed void DoAction()
		{
			_powerups = (Collector as MultiplePowerupCollector).HeldPowerups.ToArray();
			_completedPowerups = new bool[_powerups.Length];
            _lambdaCache = new Dictionary<int, Action<Vector3, Quaternion>>();

            for (int i = 0; i < _powerups.Length; i++)
            {
                var p = _powerups[i];
                p._powerups = _powerups;
                p._completedPowerups = _completedPowerups;
                p._index = i;
                p._lambdaCache = _lambdaCache;
            }

			var runNextPowerup = GetCallToNextPowerup(null, 0);

			runNextPowerup(Collector.transform.position, Collector.transform.rotation);
        }

        /// <summary>
        /// The main action of the combinable powerup
        /// </summary>
        /// <param name="previous">The previous powerup in the chain. If this is null, then the currently executing powerup is first in the chain</param>
        /// <param name="position">The position of the collector the powerup is from</param>
        /// <param name="rotation">The rotation of the collector the powerup is from</param>
        /// <param name="runNextPowerup">A delegate used to execute the next powerup in the chain. Be sure to call this to make sure all the powerups in the chain get executed</param>
		public abstract void Execute(CombinablePowerup previous, Vector3 position, Quaternion rotation, Action<Vector3, Quaternion> runNextPowerup);

        /// <summary>
        /// Creates a delegate to the next powerup in the chain.
        /// </summary>
        /// <param name="currentIndex">The current index in the powerup chain. A delegate will be created that will call the next powerup</param>
        /// <returns>Returns a delgate that executes the next powerup in the chain</returns>
        public Action<Vector3, Quaternion> GetCallToNextPowerup(int currentIndex)
        {
            CombinablePowerup previous = null;
            if (currentIndex > 0 && currentIndex <= _powerups.Length)
            {
                previous = _powerups[currentIndex - 1];
            }

            return GetCallToNextPowerup(previous, currentIndex);
        }

        private Action<Vector3, Quaternion> GetCallToNextPowerup(CombinablePowerup previous, int currentIndex)
        {
            if (_lambdaCache.TryGetValue(currentIndex, out var result))
            {
                return result;
            }
            else
            {
                Action<Vector3, Quaternion> func = (pos, rot) =>
                {
                    if (currentIndex < _powerups.Length)
                    {
                        var currentPowerup = _powerups[currentIndex];
                        currentPowerup.Execute(previous, pos, rot, GetCallToNextPowerup(currentPowerup, currentIndex + 1));
                    }
                };
                _lambdaCache.Add(currentIndex, func);
                return func;
            }
        }

        /// <inheritdoc/>
        public override sealed void DoneUsingPowerup()
		{
            if (_completedPowerups == null)
            {
                base.DoneUsingPowerup();
                return;
            }
            else
            {
            }
            for (int i = 0; i < _powerups.Length; i++)
            {
                if (_powerups[i] == this)
                {
                    if (_completedPowerups[i] == false)
                    {
                        _completedPowerups[i] = true;
                        if (_completedPowerups.All(b => b == true))
                        {
                            foreach (var p in _powerups)
                            {
                                if (p == this)
                                {
                                    continue;
                                }
                                p._completedPowerups = null;
                                p._powerups = null;
                                p._lambdaCache = null;
                                p.DoneUsingPowerup();
                            }
                            _completedPowerups = null;
                            _powerups = null;
                            _lambdaCache = null;
                            DoneUsingPowerup();
                        }
                    }
                    break;
                }
            }
        }
	}
}
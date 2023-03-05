using Mirror;
using Nitro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets
{
    public class NetworkedMultiplePowerupCollector : NetworkBehaviour, IMultiplePowerupCollector
    {
        /// <summary>
        /// A comparer used for sorting combinable powerups by priority
        /// </summary>
        class Comparer : IComparer<PowerupIndex>
        {
            Comparer<int> intComparer = Comparer<int>.Default;
            public int Compare(PowerupIndex xIndex, PowerupIndex yIndex)
            {
                var x = GameSettings.Instance.PossiblePowerups[xIndex.Index];
                var y = GameSettings.Instance.PossiblePowerups[yIndex.Index];
                if (x.Priority == y.Priority)
                {
                    if (yIndex.Index == xIndex.Index)
                    {
                        return 1;
                    }
                    else
                    {
                        return intComparer.Compare(yIndex.Index, xIndex.Index);
                    }
                }
                else
                {
                    if (y.Priority == x.Priority)
                    {
                        return 1;
                    }
                    else
                    {
                        return intComparer.Compare(y.Priority, x.Priority);
                    }
                }
            }
        }

        class PowerupIndex
        {
            public PowerupIndex(int index)
            {
                Index = index;
            }

            public int Index;
        }



        /// <summary>
		/// A list of all the currently held powerups, sorted by <see cref="CombinablePowerup.Priority"/>
		/// </summary>
		[NonSerialized]
        SortedSet<PowerupIndex> heldPowerups = new SortedSet<PowerupIndex>(new Comparer());

        [NonSerialized]
        readonly SyncList<int> addedPowerups = new SyncList<int>();

        [field: SerializeField]
        public bool CollectorEnabled { get; private set; }

        [field: SerializeField]
        public bool CollectOnContact { get; private set; }

        [field: SerializeField]
        public int MaxPowerupsHeld { get; private set; } = 3;

        public int HeldPowerupCount = 0;

        [SerializeField]
        AudioSource collectPowerupSound;

        [Tooltip("If set to true, the collector will require that the powerups collected are different types")]
        public bool DifferingTypesRequired = true;

        public IEnumerable<ICombinablePowerup> CollectedPowerups { get; private set; } = Enumerable.Empty<ICombinablePowerup>();

        public bool CanCollectPowerup(IPowerup powerup)
        {
            if (powerup is ICombinablePowerup && heldPowerups.Count < MaxPowerupsHeld)
            {
                if (!DifferingTypesRequired || (DifferingTypesRequired && !heldPowerups.Any(p => GameSettings.Instance.PossiblePowerups[p.Index].GetType() == powerup.GetType())))
                {
                    return true;
                }
            }
            return false;
        }

        public bool CollectPowerup(IPowerup powerup)
        {
            if (!CollectorEnabled)
            {
                return false;
            }
            if (CanCollectPowerup(powerup))
            {
                if (NetworkServer.active)
                {
                    var component = (powerup as Component);
                    MapGenerator.Instance.SpawnNewPowerup(component.transform.position, component.transform.rotation,3f);
                    powerup.OnCollect(this);

                    var powerupIndex = GameSettings.Instance.PossiblePowerupTypes.IndexOf(powerup.GetType());
                    addedPowerups.Add(powerupIndex);
                    HeldPowerupCount++;

                    Debug.Log("PLAYING SOUND");
                    PlaySound();
                }
                else
                {
                    powerup.OnCollect(this);
                }

                if (NetworkServer.active && powerup is Component c)
                {
                    if (c.GetComponent<NetworkIdentity>() != null)
                    {
                        NetworkServer.Destroy(c.gameObject);
                    }
                    else
                    {
                        Destroy(c.gameObject);
                    }
                }

                return true;
            }
            return false;
        }

        public void Execute()
        {
            if (!NetworkServer.active)
            {
                Execute_Server();
            }
            else
            {
                if (heldPowerups.Count > 0)
                {
                    CollectedPowerups = heldPowerups.Select(i =>
                    {
                        var instance = GameObject.Instantiate(GameSettings.Instance.PossiblePowerups[i.Index], transform);
                        instance.OnCollect(this);
                        return instance;
                    }).ToArray();

                    var maxIndex = heldPowerups.Max;
                    CollectedPowerups.First().DoAction();
                    addedPowerups.Clear();
                }
            }
        }

        [TargetRpc]
        void PlaySound()
        {
            Debug.Log("A");
            if (collectPowerupSound != null)
            {
                Debug.Log("B");
                collectPowerupSound.Play();
            }
        }

        [Command]
        void Execute_Server()
        {
            Execute();
        }

        void OnPowerupCollected(int powerupIndex, Powerup powerupPrefab)
        {
            if (isOwned)
            {
                PowerupColorDisplay.AddColor(powerupPrefab.GetComponent<Colorizer>().Color);
            }
        }

        void OnPowerupsExecuted()
        {
            if (isOwned)
            {
                PowerupColorDisplay.Clear();
            }
        }


        public override void OnStartClient()
        {
            addedPowerups.Callback += OnAddedPowerupsUpdated;

            for (int i = 0; i < addedPowerups.Count; i++)
            {
                OnAddedPowerupsUpdated(SyncList<int>.Operation.OP_ADD, i, 0, addedPowerups[i]);
            }
        }

        void OnAddedPowerupsUpdated(SyncList<int>.Operation op, int indexInList, int oldPowerupIndex, int newPowerupIndex)
        {
            switch (op)
            {
                case SyncList<int>.Operation.OP_ADD:
                    heldPowerups.Add(new PowerupIndex(newPowerupIndex));
                    OnPowerupCollected(newPowerupIndex, GameSettings.Instance.PossiblePowerups[newPowerupIndex]);
                    break;
                case SyncList<int>.Operation.OP_CLEAR:
                    OnPowerupsExecuted();
                    heldPowerups.Clear();
                    break;
                case SyncList<int>.Operation.OP_INSERT:
                    heldPowerups.Add(new PowerupIndex(newPowerupIndex));
                    OnPowerupCollected(newPowerupIndex, GameSettings.Instance.PossiblePowerups[newPowerupIndex]);
                    break;
                case SyncList<int>.Operation.OP_REMOVEAT:
                    heldPowerups.RemoveWhere(p => p.Index == oldPowerupIndex);
                    break;
                case SyncList<int>.Operation.OP_SET:
                    heldPowerups.RemoveWhere(p => p.Index == oldPowerupIndex);
                    heldPowerups.Add(new PowerupIndex(newPowerupIndex));
                    break;
                default:
                    break;
            }
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (CollectOnContact)
            {
                var powerup = other.GetComponent<IPowerup>();
                if (powerup != null)
                {
                    CollectPowerup(powerup);
                }
            }
        }

        protected virtual void OnTriggerEnter2D(Collider2D collision)
        {
            if (CollectOnContact)
            {
                var powerup = collision.GetComponent<IPowerup>();
                if (powerup != null)
                {
                    CollectPowerup(powerup);
                }
            }
        }

        protected virtual void OnCollisionEnter(Collision collision)
        {
            if (CollectOnContact)
            {
                var powerup = collision.gameObject.GetComponent<IPowerup>();
                if (powerup != null)
                {
                    CollectPowerup(powerup);
                }
            }
        }

        protected virtual void OnCollisionEnter2D(Collision2D collision)
        {
            if (CollectOnContact)
            {
                var powerup = collision.gameObject.GetComponent<IPowerup>();
                if (powerup != null)
                {
                    CollectPowerup(powerup);
                }
            }
        }
    }
}

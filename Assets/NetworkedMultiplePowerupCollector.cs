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
        SyncList<int> addedPowerups = new SyncList<int>();

        [field: SerializeField]
        public bool CollectorEnabled { get; private set; }

        [field: SerializeField]
        public bool CollectOnContact { get; private set; }

        [field: SerializeField]
        public int MaxPowerupsHeld { get; private set; } = 3;

        public int HeldPowerupCount = 0;

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
                //heldPowerups.Add(powerup as CombinablePowerup);
                //OnCollect(powerup);

                if (NetworkServer.active)
                {
                    powerup.OnCollect(this);

                    Debug.Log("Collected Powerup = " + powerup.GetType());
                    var powerupIndex = GameSettings.Instance.PossiblePowerupTypes.IndexOf(powerup.GetType());
                    Debug.Log("Powerup Index = " + powerupIndex);
                    addedPowerups.Add(powerupIndex);
                    //heldPowerups.Add(new PowerupIndex(powerupIndex));
                    HeldPowerupCount++;



                    //Debug.Log("Instance ID = " + GetInstanceID());
                    //Debug.Log("Held Powerup Counts = " + heldPowerups.Count);
                    //Debug.Log("Held Powerup Count 2 = " + HeldPowerupCount);
                    //Debug.Log("MaxPowerupsHeld = " + MaxPowerupsHeld);

                    //OnPowerupCollected(powerupIndex, GameSettings.Instance.PossiblePowerups[powerupIndex]);
                }
                else
                {
                    powerup.OnCollect(this);
                }

                Debug.Log("PUP AFTERWARDS");
                if (NetworkServer.active && powerup is Component c)
                {
                    if (c.GetComponent<NetworkIdentity>() != null)
                    {
                        Debug.Log("DESTROYING POWERUP = " + powerup.GetType());
                        NetworkServer.Destroy(c.gameObject);
                    }
                    else
                    {
                        Destroy(c.gameObject);
                    }
                }
                //Destroy(powerup.)
                //PowerupCollectEvent?.Invoke(powerup);


                return true;
            }
            return false;
        }

        public void Execute()
        {
            Debug.Log("Executing 1");
            if (!NetworkServer.active)
            {
                Debug.Log("Executing 2");
                Execute_Server();
            }
            else
            {
                Debug.Log("Executing 3 = " + heldPowerups.Count);
                if (heldPowerups.Count > 0)
                {
                    Debug.Log("Executing!");
                    CollectedPowerups = heldPowerups.Select(i =>
                    {
                        var instance = GameObject.Instantiate(GameSettings.Instance.PossiblePowerups[i.Index], transform);
                        instance.OnCollect(this);
                        return instance;
                    }).ToArray();

                    foreach (var powerup in CollectedPowerups)
                    {
                        Debug.Log("POWERUP Type = " + powerup.GetType());
                    }

                    //heldPowerups.Max.DoAction();
                    var maxIndex = heldPowerups.Max;
                    Debug.Log("Max Index = " + maxIndex);
                    //GameSettings.Instance.PossiblePowerups[maxIndex.Index].DoAction();
                    CollectedPowerups.First().DoAction();
                    Debug.Log("2");
                    addedPowerups.Clear();
                    //heldPowerups.Clear();
                    //Debug.Log("3");
                    //OnPowerupsExecuted();
                }
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
            Debug.Log("Updated = " + op);
            Debug.Log("New Powerup Index = " + newPowerupIndex);
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


        /*public override void Execute()
        {
            base.Execute();
        }*/
    }
}

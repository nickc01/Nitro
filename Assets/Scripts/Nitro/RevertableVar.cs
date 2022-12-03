using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nitro
{
    /// <summary>
    /// A variable that allows you to easily modify it's value and revert it back to it's original value
    /// </summary>
    /// <typeparam name="T">The type of variable the RevertableVar is going to hold</typeparam>
    [Serializable]
    public class RevertableVar<T> : IRevertableVar
    {
        private static ulong idCounter;

        private class RoutineRunner : MonoBehaviour
        {
            public List<Modifier<T>> modifiersToCheck = new List<Modifier<T>>();
            private static RoutineRunner _instance;
            public static RoutineRunner Instance
            {
                get
                {
                    if (_instance == null)
                    {
                        _instance = new GameObject("ROUTINE_RUNNER").AddComponent<RoutineRunner>();
                        _instance.gameObject.hideFlags = HideFlags.HideAndDontSave;
                        UnityEngine.Object.DontDestroyOnLoad(_instance.gameObject);
                    }
                    return _instance;
                }
            }

            private void Awake()
            {
                StartCoroutine(CheckerRoutine());
            }

            private IEnumerator CheckerRoutine()
            {
                while (true)
                {
                    for (int i = modifiersToCheck.Count - 1; i >= 0; i--)
                    {
                        Modifier<T> mod = modifiersToCheck[i];
                        if ((mod.TimeActive > 0f && Time.unscaledTime >= mod.TimeActive + mod.TimeActive) || (mod.HasBoundObject && mod.BoundObject == null))
                        {
                            modifiersToCheck.RemoveAt(i);
                            mod.Revert();
                        }
                    }
                    yield return null;
                }
            }
        }

        /// <summary>
        /// Constructs a new revertable variable
        /// </summary>
        /// <param name="baseValue">The base value of the variable</param>
        public RevertableVar(T baseValue)
        {
            this.baseValue = baseValue;
            value = baseValue;
        }

        /// <summary>
        /// Constructs a new revertable variable
        /// </summary>
        public RevertableVar() : this(default) { }


        /// <inheritdoc/>
        object IRevertableVar.BaseValue { get => BaseValue; set => BaseValue = (T)value; }

        /// <inheritdoc/>
        object IRevertableVar.Value => Value;

        /// <inheritdoc/>
        int IRevertableVar.ModifiersApplied => modifiers.Count;

        /// <inheritdoc/>
        Type IRevertableVar.ValueType => typeof(T);

        [NonSerialized]
        private SortedSet<Modifier<T>> modifiers = new SortedSet<Modifier<T>>(new Modifier<T>.Sorter());

        [SerializeField]
        private T baseValue;

        [SerializeField]
        private T value;

        /// <summary>
        /// The current value of the revertable variable, with all of it's modifiers applied
        /// </summary>
        public T Value => value;


        /// <summary>
        /// The base value of the revertable variable
        /// </summary>
        public T BaseValue
        {
            get => baseValue;
            set
            {
                if (!baseValue.Equals(value))
                {
                    baseValue = value;
                    UpdateCurrentValue();
                }
            }
        }

        /// <summary>
        /// How many modifiers are currently being applied to this revertible variable
        /// </summary>
        public int ModifiersApplied => modifiers.Count;

        /// <summary>
        /// Applies a multiplication to the variable.
        /// </summary>
        /// <param name="value">The value to multiply the variable with</param>
        /// <param name="priority">The priority of the modifier. The lower the priority, the sooner it will be applied to the variable before other modifiers</param>
        /// <param name="boundObject">The object this modifier is bound to. When the bound object gets destroyed, the modifier gets reverted</param>
        /// <returns>Returns a reference to the modifier currently applied to this variable. Use <see cref="Modifier{T}.Revert"/> to revert the modification</returns>
        public Modifier<T> MultiplyBy(T value, UnityEngine.Object boundObject = null, int priority = 0)
        {
            return MultiplyBy(value, boundObject, priority, 0f);
        }

        /// <summary>
        /// Applies a division to the variable.
        /// </summary>
        /// <param name="value">The value to divide the variable with</param>
        /// <param name="priority">The priority of the modifier. The lower the priority, the sooner it will be applied to the variable before other modifiers</param>
        /// <param name="boundObject">The object this modifier is bound to. When the bound object gets destroyed, the modifier gets reverted</param>
        /// <returns>Returns a reference to the modifier currently applied to this variable. Use <see cref="Modifier{T}.Revert"/> to revert the modification</returns>
        public Modifier<T> DivideBy(T value, UnityEngine.Object boundObject = null, int priority = 0)
        {
            return DivideBy(value, boundObject, priority, 0f);
        }

        /// <summary>
        /// Applies an addition to the variable.
        /// </summary>
        /// <param name="value">The value to add onto the variable</param>
        /// <param name="priority">The priority of the modifier. The lower the priority, the sooner it will be applied to the variable before other modifiers</param>
        /// <param name="boundObject">The object this modifier is bound to. When the bound object gets destroyed, the modifier gets reverted</param>
        /// <returns>Returns a reference to the modifier currently applied to this variable. Use <see cref="Modifier{T}.Revert"/> to revert the modification</returns>
        public Modifier<T> AddBy(T value, UnityEngine.Object boundObject = null, int priority = 0)
        {
            return AddBy(value, boundObject, priority, 0f);
        }

        /// <summary>
        /// Applies an subtraction to the variable.
        /// </summary>
        /// <param name="value">The value to subtract from the variable</param>
        /// <param name="priority">The priority of the modifier. The lower the priority, the sooner it will be applied to the variable before other modifiers</param>
        /// <param name="boundObject">The object this modifier is bound to. When the bound object gets destroyed, the modifier gets reverted</param>
        /// <returns>Returns a reference to the modifier currently applied to this variable. Use <see cref="Modifier{T}.Revert"/> to revert the modification</returns>
        public Modifier<T> SubtractBy(T value, UnityEngine.Object boundObject = null, int priority = 0)
        {
            return SubtractBy(value, boundObject, priority, 0f);
        }

        /// <summary>
        /// Applies an subtraction to the variable.
        /// </summary>
        /// <param name="value">The value to subtract from the variable</param>
        /// <param name="priority">The priority of the modifier. The lower the priority, the sooner it will be applied to the variable before other modifiers</param>
        /// <param name="boundObject">The object this modifier is bound to. When the bound object gets destroyed, the modifier gets reverted</param>
        /// <returns>Returns a reference to the modifier currently applied to this variable. Use <see cref="Modifier{T}.Revert"/> to revert the modification</returns>
        public Modifier<T> Set(T value, UnityEngine.Object boundObject = null, int priority = 0)
        {
            return Set(value, boundObject, priority, 0f);
        }


        /// <summary>
        /// Applies a multiplication to the variable.
        /// </summary>
        /// <param name="value">The value to multiply the variable with</param>
        /// <param name="priority">The priority of the modifier. The lower the priority, the sooner it will be applied to the variable before other modifiers</param>
        /// <param name="boundObject">The object this modifier is bound to. When the bound object gets destroyed, the modifier gets reverted</param>
        /// <param name="timeActive">The amount of time this modifier should remain active. Once the time is up, the modifier will be reverted</param>
        /// <returns>Returns a reference to the modifier currently applied to this variable. Use <see cref="Modifier{T}.Revert"/> to revert the modification</returns>
        public Modifier<T> MultiplyBy(T value, UnityEngine.Object boundObject, int priority, float timeActive)
        {
            return ModifyInternal(Modifier<T>.Operation.Multiply, value, boundObject, priority, timeActive);
        }

        /// <summary>
        /// Applies a division to the variable.
        /// </summary>
        /// <param name="value">The value to divide the variable with</param>
        /// <param name="priority">The priority of the modifier. The lower the priority, the sooner it will be applied to the variable before other modifiers</param>
        /// <param name="boundObject">The object this modifier is bound to. When the bound object gets destroyed, the modifier gets reverted</param>
        /// <param name="timeActive">The amount of time this modifier should remain active. Once the time is up, the modifier will be reverted</param>
        /// <returns>Returns a reference to the modifier currently applied to this variable. Use <see cref="Modifier{T}.Revert"/> to revert the modification</returns>
        public Modifier<T> DivideBy(T value, UnityEngine.Object boundObject, int priority, float timeActive)
        {
            return ModifyInternal(Modifier<T>.Operation.Divide, value, boundObject, priority, timeActive);
        }

        /// <summary>
        /// Applies an addition to the variable.
        /// </summary>
        /// <param name="value">The value to add onto the variable</param>
        /// <param name="priority">The priority of the modifier. The lower the priority, the sooner it will be applied to the variable before other modifiers</param>
        /// <param name="boundObject">The object this modifier is bound to. When the bound object gets destroyed, the modifier gets reverted</param>
        /// <param name="timeActive">The amount of time this modifier should remain active. Once the time is up, the modifier will be reverted</param>
        /// <returns>Returns a reference to the modifier currently applied to this variable. Use <see cref="Modifier{T}.Revert"/> to revert the modification</returns>
        public Modifier<T> AddBy(T value, UnityEngine.Object boundObject, int priority, float timeActive)
        {
            return ModifyInternal(Modifier<T>.Operation.Add, value, boundObject, priority, timeActive);
        }

        /// <summary>
        /// Applies an subtraction to the variable.
        /// </summary>
        /// <param name="value">The value to subtract from the variable</param>
        /// <param name="priority">The priority of the modifier. The lower the priority, the sooner it will be applied to the variable before other modifiers</param>
        /// <param name="boundObject">The object this modifier is bound to. When the bound object gets destroyed, the modifier gets reverted</param>
        /// /// <param name="timeActive">The amount of time this modifier should remain active. Once the time is up, the modifier will be reverted</param>
        /// <returns>Returns a reference to the modifier currently applied to this variable. Use <see cref="Modifier{T}.Revert"/> to revert the modification</returns>
        public Modifier<T> SubtractBy(T value, UnityEngine.Object boundObject, int priority, float timeActive)
        {
            return ModifyInternal(Modifier<T>.Operation.Subtract, value, boundObject, priority, timeActive);
        }

        /// <summary>
        /// Applies an subtraction to the variable.
        /// </summary>
        /// <param name="value">The value to subtract from the variable</param>
        /// <param name="priority">The priority of the modifier. The lower the priority, the sooner it will be applied to the variable before other modifiers</param>
        /// <param name="timeActive">The amount of time this modifier should remain active. Once the time is up, the modifier will be reverted</param>
        /// <param name="boundObject">The object this modifier is bound to. When the bound object gets destroyed, the modifier gets reverted</param>
        /// <returns>Returns a reference to the modifier currently applied to this variable. Use <see cref="Modifier{T}.Revert"/> to revert the modification</returns>
        public Modifier<T> Set(T value, UnityEngine.Object boundObject, int priority, float timeActive)
        {
            return ModifyInternal(Modifier<T>.Operation.Set, value, boundObject, priority, timeActive);
        }


        private Modifier<T> ModifyInternal(Modifier<T>.Operation op, T value, UnityEngine.Object boundObject, int priority, float timeActive)
        {
            if (!Application.isPlaying)
            {
                throw new Exception("Nitro Variables can only be modified during play mode");
            }

            Modifier<T> modifier = new Modifier<T>
            {
                SourceVar = this,
                Op = op,
                Value = value,
                Priority = priority,
                TimeActive = timeActive,
                TimeAdded = Time.unscaledTime,
                BoundObject = boundObject,
                HasBoundObject = boundObject != null,
                ID = ++idCounter
            };

            modifiers.Add(modifier);

            if (timeActive > 0f || boundObject != null)
            {
                RoutineRunner.Instance.modifiersToCheck.Add(modifier);
            }

            UpdateCurrentValue();

            return modifier;

        }

        /// <summary>
        /// Reverts a modifier
        /// </summary>
        /// <param name="modifier">The modifier to revert</param>
        public void Revert(Modifier<T> modifier)
        {
            if (modifier.ID != 0)
            {
                modifiers.Remove(modifier);
                modifier.ID = 0;
                UpdateCurrentValue();
            }
        }

        private void UpdateCurrentValue()
        {
            T newValue = baseValue;
            foreach (Modifier<T> mod in modifiers)
            {
                switch (mod.Op)
                {
                    case Modifier<T>.Operation.Set:
                        newValue = mod.Value;
                        break;
                    case Modifier<T>.Operation.Multiply:
                        newValue = GenericMath.Mul(newValue, mod.Value);
                        break;
                    case Modifier<T>.Operation.Divide:
                        newValue = GenericMath.Div(newValue, mod.Value);
                        break;
                    case Modifier<T>.Operation.Add:
                        newValue = GenericMath.Add(newValue, mod.Value);
                        break;
                    case Modifier<T>.Operation.Subtract:
                        newValue = GenericMath.Sub(newValue, mod.Value);
                        break;
                    default:
                        break;
                }
            }
            value = newValue;
        }

        /// <summary>
		/// Implicitly converts from a revertable variable to the variable of type T
		/// </summary>
		/// <param name="v">The revertable variable</param>
		public static implicit operator T(RevertableVar<T> v)
        {
            return v.Value;
        }

        /// <summary>
        /// Implicitly converts from a variable of type T to a revertable variable
        /// </summary>
        /// <param name="value">The variable</param>
        public static implicit operator RevertableVar<T>(T value)
        {
            return new RevertableVar<T>(value);
        }

        public static bool operator ==(RevertableVar<T> a, RevertableVar<T> b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(RevertableVar<T> a, RevertableVar<T> b)
        {
            return !a.Equals(b);
        }

        public static bool operator ==(RevertableVar<T> a, T b)
        {
            return a.Value.Equals(b);
        }

        public static bool operator !=(RevertableVar<T> a, T b)
        {
            return !a.Value.Equals(b);
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}

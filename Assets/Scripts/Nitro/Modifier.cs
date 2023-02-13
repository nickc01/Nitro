using System.Collections.Generic;
using UnityEngine;
using static Nitro.IModifier;

namespace Nitro
{
    /// <summary>
    /// Represents a modification applied to a <see cref="RevertableVar{T}"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class Modifier<T> : IModifier
    {
        public Modifier(IRevertableVar sourceVar, Operation op, T value, int priority, float timeActive, UnityEngine.Object boundObject, ulong id)
        {
            SourceVariable = sourceVar;
            Op = op;
            Value = value;
            Priority = priority;
            TimeActive = timeActive;
            TimeAdded = Time.unscaledTime;
            BoundObject = boundObject;
            HasBoundObject = boundObject != null;
            ID = id;
        }

        /// <inheritdoc/>
        public Operation Op { get; private set; }

        /// <summary>
        /// The right-hand operand of the modifier
        /// </summary>
        public T Value { get; private set; }

        /// <inheritdoc/>
        public int Priority { get; private set; }

        /// <inheritdoc/>
        public float TimeAdded { get; private set; }

        /// <inheritdoc/>
        public IRevertableVar SourceVariable { get; private set; }

        /// <inheritdoc/>
        public UnityEngine.Object BoundObject { get; private set; }

        /// <inheritdoc/>
        public bool HasBoundObject { get; private set; }

        public float TimeActive { get; private set; }

        /// <inheritdoc/>
        public ulong ID { get; private set; }

        /// <inheritdoc/>
        object IModifier.Value => Value;

        public class Sorter : IComparer<Modifier<T>>
        {
            private Comparer<int> numberComparer = Comparer<int>.Default;
            private Comparer<float> floatComparer = Comparer<float>.Default;
            private Comparer<ulong> idComparer = Comparer<ulong>.Default;
            public int Compare(Modifier<T> x, Modifier<T> y)
            {
                if (x.Priority == y.Priority)
                {
                    if (x.TimeAdded == y.TimeAdded)
                    {
                        return idComparer.Compare(y.ID, x.ID);
                    }
                    else
                    {
                        return floatComparer.Compare(y.TimeAdded, x.TimeAdded);
                    }
                }
                else
                {
                    return numberComparer.Compare(y.Priority, x.Priority);
                }
            }
        }

        /// <inheritdoc/>
        public void Revert()
        {
            SourceVariable.Revert(this);
        }

        public void Dispose()
        {
            Revert();
        }
    }
}

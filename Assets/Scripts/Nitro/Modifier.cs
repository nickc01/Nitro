using System.Collections.Generic;

namespace Nitro
{
    /// <summary>
    /// Represents a modification applied to a <see cref="RevertableVar{T}"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class Modifier<T> : IModifier
    {
        internal enum Operation
        {
            Set,
            Multiply,
            Divide,
            Add,
            Subtract
        }

        /// <inheritdoc/>
        public RevertableVar<T> SourceVar { get; internal set; }

        internal Operation Op;

        internal T Value;

        /// <inheritdoc/>
        public int Priority { get; internal set; }

        /// <inheritdoc/>
        public float TimeAdded { get; internal set; }

        /// <inheritdoc/>
        IRevertableVar IModifier.SourceVariable => SourceVar;

        internal float TimeActive;

        internal ulong ID;

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
                        return idComparer.Compare(x.ID, y.ID);
                    }
                    else
                    {
                        return floatComparer.Compare(x.TimeAdded, y.TimeAdded);
                    }
                }
                else
                {
                    return numberComparer.Compare(x.Priority, y.Priority);
                }
            }
        }

        /// <inheritdoc/>
        public void Revert()
        {
            SourceVar.Revert(this);
        }

        public void Dispose()
        {
            Revert();
        }
    }
}

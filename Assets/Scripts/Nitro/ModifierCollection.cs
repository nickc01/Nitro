using System;
using System.Collections;
using System.Collections.Generic;

namespace Nitro
{
    /// <summary>
    /// Represents a collection of modifiers. This can be used to easily group modifiers together and revert them together as well
    /// </summary>
    public class ModifierCollection : ICollection<IModifier>, IEnumerable<IModifier>, IEnumerable, IReadOnlyList<IModifier>, IReadOnlyCollection<IModifier>, ICollection
    {
        List<IModifier> modifiers = new List<IModifier>();
        List<UnityEngine.Object> modBindings = new List<UnityEngine.Object>();

        public int Count => modifiers.Count;

        public bool IsReadOnly => false;

        public bool IsSynchronized => false;

        public object SyncRoot => null;

        public bool IsFixedSize => false;

        public IModifier this[int index] { get => modifiers[index]; }

        /// <summary>
        /// Adds a modifier to the collection
        /// </summary>
        /// <param name="modifier">The modifier to add</param>
        public void Add(IModifier modifier)
        {
            Add(modifier, null);
        }

        /// <summary>
        /// Adds a modifier to the collection
        /// </summary>
        /// <param name="modifier">The modifier to add</param>
        /// <param name="tiedObject">If an object is specified, then this modifier will become tied to this object. You can then use <see cref="RevertAllByObject(UnityEngine.Object)"/> to revert all modifiers that are tied to that object</param>
        public void Add(IModifier modifier, UnityEngine.Object tiedObject)
        {
            modifiers.Add(modifier);
            modBindings.Add(tiedObject);
        }

        /// <summary>
        /// Reverts all modifiers and removes them from the collection
        /// </summary>
        public void RevertAll()
        {
            for (int i = modifiers.Count - 1; i >= 0; i--)
            {
                modifiers[i].Revert();
            }
            modifiers.Clear();
            modBindings.Clear();
        }

        /// <summary>
        /// Reverts all modifiers for a specific <see cref="RevertableVar{T}"/>, and removes them from the list
        /// </summary>
        /// <typeparam name="T">The type of variable</typeparam>
        /// <param name="var">The revertable variable</param>
        public void RevertAllFor<T>(RevertableVar<T> var)
        {
            for (int i = modifiers.Count - 1; i >= 0; i--)
            {
                IModifier mod = modifiers[i];
                if (mod.SourceVariable is RevertableVar<T> source && source == var)
                {
                    mod.Revert();
                    modifiers.RemoveAt(i);
                    modBindings.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Reverts all modifiers for a specific <see cref="RevertableVar{T}"/>, and removes them from the list
        /// </summary>
        /// <typeparam name="T">The type of variable</typeparam>
        /// <param name="var">The revertable variable</param>
        public void RevertAllFor(IRevertableVar var)
        {
            for (int i = modifiers.Count - 1; i >= 0; i--)
            {
                IModifier mod = modifiers[i];
                if (mod.SourceVariable == var)
                {
                    mod.Revert();
                    modifiers.RemoveAt(i);
                    modBindings.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Reverts all modifiers that have been tied to a specific object
        /// </summary>
        /// <param name="tiedObject">The object the modifiers are tied to</param>
        public void RevertAllByObject(UnityEngine.Object tiedObject)
        {
            for (int i = modifiers.Count - 1; i >= 0; i--)
            {
                IModifier mod = modifiers[i];
                if (modBindings[i] == tiedObject)
                {
                    mod.Revert();
                    modifiers.RemoveAt(i);
                    modBindings.RemoveAt(i);
                }
            }
        }

        public void Clear()
        {
            modifiers.Clear();
            modBindings.Clear();
        }

        public bool Contains(IModifier item)
        {
            return modifiers.Contains(item);
        }

        public void CopyTo(IModifier[] array, int arrayIndex)
        {
            modifiers.CopyTo(array, arrayIndex);
        }

        public bool Remove(IModifier item)
        {
            var index = modifiers.IndexOf(item);
            if (index >= 0)
            {
                modifiers.RemoveAt(index);
                modBindings.RemoveAt(index);
                return true;
            }
            else
            {
                return false;
            }
        }

        public IEnumerator<IModifier> GetEnumerator()
        {
            return modifiers.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return modifiers.GetEnumerator();
        }

        public int IndexOf(IModifier item)
        {
            return modifiers.IndexOf(item);
        }

        public void CopyTo(Array array, int index)
        {
            ((ICollection)modifiers).CopyTo(array, index);
        }

        public bool Contains(object value)
        {
            if (value is IModifier mod)
            {
                return modifiers.Contains(mod);
            }
            return false;
        }

        public int IndexOf(object value)
        {
            if (value is IModifier mod)
            {
                return modifiers.IndexOf(mod);
            }
            return -1;
        }

        public void Remove(object value)
        {
            if (value is IModifier mod)
            {
                Remove(mod);
            }
        }
    }
}

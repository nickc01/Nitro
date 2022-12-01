using System.Collections.Generic;

namespace Nitro
{
    /// <summary>
    /// Represents a collection of modifiers. This can be used to easily group modifiers together and revert them together as well
    /// </summary>
    public class ModifierCollection : List<IModifier>
    {
        /// <summary>
        /// Reverts all modifiers and removes them from the collection
        /// </summary>
        public void RevertAll()
        {
            for (int i = Count - 1; i >= 0; i--)
            {
                this[i].Revert();
            }
            Clear();
        }

        /// <summary>
        /// Reverts all modifiers for a specific <see cref="RevertableVar{T}"/>, and removes them from the list
        /// </summary>
        /// <typeparam name="T">The type of variable</typeparam>
        /// <param name="var">The revertable variable</param>
        public void RevertAllFor<T>(RevertableVar<T> var)
        {
            for (int i = Count - 1; i >= 0; i--)
            {
                IModifier mod = this[i];
                if (mod.SourceVariable is RevertableVar<T> source && source == var)
                {
                    mod.Revert();
                    RemoveAt(i);
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
            for (int i = Count - 1; i >= 0; i--)
            {
                IModifier mod = this[i];
                if (mod.SourceVariable == var)
                {
                    mod.Revert();
                    RemoveAt(i);
                }
            }
        }
    }
}

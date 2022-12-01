using System;

namespace Nitro
{
    /// <summary>
    /// Base class for all modifiers
    /// </summary>
    public interface IModifier : IDisposable
    {
        /// <summary>
        /// The priority of the modifier. The lower the number, the sooner it will be processed before other modifiers
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// The time this modifer was created and added to a revertable variable
        /// </summary>
        float TimeAdded { get; }

        /// <summary>
        /// The revertable variable that this modifier is a part of
        /// </summary>
        IRevertableVar SourceVariable { get; }

        /// <summary>
        /// Reverts the modifier and removes it from the revertable variable it is a part of
        /// </summary>
        void Revert();
    }
}

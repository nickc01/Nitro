using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nitro
{
    public interface IRevertableVar
    {
        /// <summary>
		/// The base value of the revertible variable
		/// </summary>
		object BaseValue { get; set; }
        /// <summary>
        /// The current value of the revertible variable
        /// </summary>
        object Value { get; }
        /// <summary>
        /// How many variable modifiers are currently applied to the revertible variable
        /// </summary>
        int ModifiersApplied { get; }
        /// <summary>
        /// The type of the value being held by the revertible variable
        /// </summary>
        Type ValueType { get; }

        /// <summary>
        /// Gets a list of all the added modifiers to this revertable variable
        /// </summary>
        /// <returns></returns>
        IEnumerable<IModifier> GetModifiers();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Nitro
{

    public static class Extensions
    {
        /// <summary>
        /// Gets the <see cref="GameObject"/> this collector is attached to
        /// </summary>
        public static GameObject GetGameObject(this ICollector collector)
        {
            if (collector is Component c)
            {
                return c.gameObject;
            }
            return null;
        }

        /// <summary>
        /// Gets the <see cref="Transform"/> this collector is attached to
        /// </summary>
        public static Transform GetTransform(this ICollector collector)
        {
            if (collector is Component c)
            {
                return c.transform;
            }
            return null;
        }
    }
}

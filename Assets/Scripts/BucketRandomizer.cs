using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets
{
    /// <summary>
    /// Contains methods for selecting random pieces from lists
    /// </summary>
    public static class BucketRandomizer
    {
        /// <summary>
        /// Picks a random road piece from the given list of road pieces
        /// </summary>
        /// <param name="pieces">List of road pieces</param>
        /// <param name="randomizer">Reference to the randomizer</param>
        /// <returns>A random road piece</returns>
        public static RoadPiece PickRandomRoadPiece(List<RoadPiece> pieces, ref Unity.Mathematics.Random randomizer)
        {
            float total = pieces.Sum(p => p.RandomChance);

            var number = randomizer.NextFloat(total);

            for (int i = 0; i < pieces.Count; i++)
            {
                if (number < pieces[i].RandomChance)
                {
                    return pieces[i];
                }
                number -= pieces[i].RandomChance;
            }
            return null;
        }

        /// <summary>
        /// Picks a random piece from the given list of pieces
        /// </summary>
        /// <typeparam name="T">Type of the pieces</typeparam>
        /// <param name="pieces">List of pieces</param>
        /// <param name="randomAmountGetter">Function for getting the random amount for each piece</param>
        /// <param name="randomizer">Reference to the randomizer</param>
        /// <returns>A random piece</returns>
        public static T PickRandomPiece<T>(List<T> pieces, Func<T, float> randomAmountGetter, ref Unity.Mathematics.Random randomizer)
        {
            float total = pieces.Sum(p => randomAmountGetter(p));

            var number = randomizer.NextFloat(total);

            for (int i = 0; i < pieces.Count; i++)
            {
                if (number < randomAmountGetter(pieces[i]))
                {
                    return pieces[i];
                }
                number -= randomAmountGetter(pieces[i]);
            }
            return default;
        }
    }
}

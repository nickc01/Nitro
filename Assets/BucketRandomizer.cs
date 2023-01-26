using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets
{
    public static class BucketRandomizer
    {
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

        public static T PickRandomPiece<T>(List<T> pieces, Func<T,float> randomAmountGetter, ref Unity.Mathematics.Random randomizer)
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

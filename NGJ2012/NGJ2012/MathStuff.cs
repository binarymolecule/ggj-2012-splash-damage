using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace NGJ2012
{
    public static class MathStuff
    {
        public static float Wrap(float value, float min, float max)
        {
            Debug.Assert(max > min, "Max must be larger than min.");

            var d = (max - min);
            var toMod = (value - min);

            if (toMod < 0) {
                toMod = d - ((-toMod) % max);
            }

            return toMod % d + min;
        }

        public static float WorldDistance(float from, float to, float w)
        {
            from = from % w;
            to = to % w;

            if (to == from)
            {
                return 0;
            }
            else if (to < from)
            {
                return (w - from) + to;
            }
            else
            {
                return to - from;
            }
        }
    }
}

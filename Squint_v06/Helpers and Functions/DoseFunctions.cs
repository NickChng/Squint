using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SquintScript
{
    public static class DoseFunctions
    {
        public static double BED(int f1, int f2, double dose1, double abRatio)
        {
            // Note that this rounds to the nearest 10 cGy
            if (!(abRatio == 0))
            {
                double BED = dose1 / 100 * (1 + dose1 / 100 / f1 / abRatio);
                double dose2 = (double)f2 * 1 / 2 * (-abRatio + Math.Sqrt(abRatio * abRatio + 4 * abRatio * (double)(BED / f2))) * 100;
                //double test = (-abRatio + Math.Sqrt(abRatio * abRatio + 4 * abRatio * (BED / f2)));
                return dose2;
            }
            else // abRatio=0 indicates no ab correction
            {
                return dose1;
            }
        }
    }
}

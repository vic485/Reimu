using System;
using System.Security.Cryptography;

namespace Reimu.Core
{
    public static class Rand
    {
        private static readonly RNGCryptoServiceProvider Rng = new RNGCryptoServiceProvider();

        public static int Range(int min, int max)
        {
            var randomNum = new byte[1];
            Rng.GetBytes(randomNum);
            var asciiVal = (float) Convert.ToDouble(randomNum[0]);

            // We are using EdoMath.Max, and subtracting 0.00000000001,
            // to ensure the "multiplier" will always be between 0.0 and .99999999999
            // Otherwise, it is possible for it to be "1", which causes problems with rounding.
            var multiplier = Math.Max(0, asciiVal / 255d - 0.00000000001d); // TODO: Precision?
            var valueInRange = Math.Floor(multiplier * (max - min));

            return (int) (min + valueInRange);
        }
    }
}
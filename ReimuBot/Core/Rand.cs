using System;
using System.Security.Cryptography;

namespace Reimu.Core
{
    public static class Rand
    {
        private static readonly RNGCryptoServiceProvider Generator = new RNGCryptoServiceProvider();
        public static int Range(int min, int max)
        {
            var randomNumber = new byte[1];
 
            Generator.GetBytes(randomNumber);
 
            var asciiValueOfRandomCharacter = Convert.ToDouble(randomNumber[0]);
 
            // We are using Math.Max, and subtracting 0.00000000001,
            // to ensure "multiplier" will always be between 0.0 and .99999999999
            // Otherwise, it's possible for it to be "1", which causes problems in our rounding.
            var multiplier = Math.Max(0, (asciiValueOfRandomCharacter / 255d) - 0.00000000001d);
 
            // We need to add one to the range, to allow for the rounding done with Math.Floor
            var range = max - min + 1;
 
            var randomValueInRange = Math.Floor(multiplier * range);
 
            return (int)(min + randomValueInRange);
        }
    }
}
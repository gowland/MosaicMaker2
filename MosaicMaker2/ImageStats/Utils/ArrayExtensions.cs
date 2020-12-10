using System;
using System.Linq;

namespace ImageStats.Utils
{
    public static class ArrayExtensions
    {
        public static int Difference(this int[] a, int[] b)
        {
            return (int)Math.Floor(a.Zip(b, (av, bv) => Abs(av - bv)).Average());
        }

        private static int Abs(int x)
        {
            return x > 0 ? x : -x;
        }
    }
}
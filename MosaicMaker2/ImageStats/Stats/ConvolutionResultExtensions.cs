using ImageStats.Utils;

namespace ImageStats.Stats
{
    public static class ConvolutionResultExtensions
    {
        public static int Difference(this ConvolutionResult a, ConvolutionResult b)
        {
            return a.Values.Difference(b.Values);
        }
    }
}
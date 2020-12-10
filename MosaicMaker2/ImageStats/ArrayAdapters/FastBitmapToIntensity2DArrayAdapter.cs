namespace ImageStats.ArrayAdapters
{
    public class FastBitmapToIntensity2DArrayAdapter : FastBitmapFilter2DArrayAdapter
    {
        public FastBitmapToIntensity2DArrayAdapter(FastBitmap.FastBitmap source) : base(source, c => (c.R + c.G + c.B)/3)
        {
        }
    }
}
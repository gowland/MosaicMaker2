using System.Drawing;

namespace ImageStats.ArrayAdapters
{
    public static class FlatArray2DArrayExtensions
    {
        public static Bitmap ToBitmap(this FlatArray2DArray<int> array)
        {
            FastBitmap.FastBitmap convolutedBitmap = new FastBitmap.FastBitmap(new Bitmap(array.Width, array.Height));
            convolutedBitmap.Lock();

            for (int y = 0; y < array.Height; y++)
            {
                for (int x = 0; x < array.Width; x++)
                {
                    // Get convolution value
                    int value = array[x, y];

                    value = value < 0 ? 0 : value;
                    value = value > 255 ? 255 : value;

                    // Set convolution value
                    convolutedBitmap.SetPixel(x, y, Color.FromArgb(value, value, value));
                }
            }

            convolutedBitmap.Unlock();

            return convolutedBitmap.ToBitmap();
        }
    }
}
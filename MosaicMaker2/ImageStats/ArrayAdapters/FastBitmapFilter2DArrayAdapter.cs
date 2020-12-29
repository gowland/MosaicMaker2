using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;

namespace ImageStats.ArrayAdapters
{
    public class FastBitmapFilter2DArrayAdapter : I2DArray<int>
    {
        private readonly FastBitmap.FastBitmap _source;
        private readonly Func<Color, int> _colorToIntFunc;

        public FastBitmapFilter2DArrayAdapter(FastBitmap.FastBitmap source, Func<Color, int> colorToIntFunc)
        {
            _source = source;
            _colorToIntFunc = colorToIntFunc;
        }

        public int this[int x, int y] => _colorToIntFunc(_source.GetPixel(x,y));

        public int Width => _source.Width;
        public int Height => _source.Height;

        public IEnumerator<int> GetEnumerator()
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    yield return this[x, y];
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
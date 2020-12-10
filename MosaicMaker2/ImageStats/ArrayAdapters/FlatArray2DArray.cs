using System.Collections.Generic;
using System.Linq;

namespace ImageStats.ArrayAdapters
{
    public class FlatArray2DArray<T> : I2DArray<T>
    {
        private readonly T[] _source;

        public FlatArray2DArray(IEnumerable<T> source, int width, int height)
        {
            _source = source.ToArray();
            Width = width;
            Height = height;
        }

        public T this[int x, int y]
        {
            get => _source[CoordsToIndex(x, y)];
            set => _source[CoordsToIndex(x, y)] = value;
        }

        public int Width { get; }
        public int Height { get; }

        private int CoordsToIndex(int x, int y)
        {
            return y * Width + x;
        }

        public T this[int i]
        {
            get => _source[i];
            set => _source[i] = value;
        }
    }
}
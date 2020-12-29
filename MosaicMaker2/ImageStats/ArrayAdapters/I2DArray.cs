using System.Collections;
using System.Collections.Generic;

namespace ImageStats.ArrayAdapters
{
    public interface I2DArray<T> : IEnumerable<T>
    {
        T this [int x, int y] { get; }
        int Width { get; }
        int Height { get; }
    }
}
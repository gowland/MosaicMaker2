namespace ImageStats.ArrayAdapters
{
    public interface I2DArray<T>
    {
        T this [int x, int y] { get; }
        int Width { get; }
        int Height { get; }
    }
}
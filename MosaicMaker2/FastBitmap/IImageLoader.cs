using System.Drawing;

namespace FastBitmap
{
    public interface IImageLoader
    {
        FastBitmap LoadImage(string imagePath);
        Bitmap LoadImageAsBitmap(string imagePath);
    }
}
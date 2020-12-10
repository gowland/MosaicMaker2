using System.Drawing;

namespace FastBitmap
{
    public class IncrediblyInefficientImageLoader : IImageLoader
    {
        public FastBitmap LoadImage(string imagePath)
        {
            var bitmap = LoadImageAsBitmap(imagePath);
            return new FastBitmap(bitmap);
        }

        public Bitmap LoadImageAsBitmap(string imagePath)
        {
            return (Bitmap)Image.FromFile(imagePath);
        }
    }
}
using System.Drawing;

namespace FastBitmap
{
    public class IncrediblyInefficientImageLoader : IImageLoader
    {
        public FastBitmap LoadImage(string imagePath)
        {
            var bitmap = (Bitmap)Image.FromFile(imagePath);
            return new FastBitmap(bitmap);
        }

        public Bitmap LoadImageAsBitmap(string imagePath)
        {
            return (Bitmap)Image.FromFile(imagePath);
        }
    }
}
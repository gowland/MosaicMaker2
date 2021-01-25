using System.Drawing;
using FastBitmap;
using ImageStats.Stats;

namespace ImageStats
{
    public class BitmapAdapter
    {
        private readonly Bitmap _bitmap;

        public BitmapAdapter(Bitmap bitmap)
        {
            _bitmap = bitmap;
        }

        public static BitmapAdapter FromPath(string path, IImageLoader loader)
        {
            var img = loader.LoadImageAsBitmap(path);
            return new BitmapAdapter(img);
        }

        public Bitmap GetSegment(ImageManipulationInfo manipulationInfo)
        {
            var targetBitmap = new Bitmap(manipulationInfo.Width, manipulationInfo.Height);
            FastBitmap.FastBitmap segment = new FastBitmap.FastBitmap(targetBitmap);
            segment.Lock();
            segment.CopyRegion(_bitmap,
                manipulationInfo.Rectangle,
                manipulationInfo.AsZeroBasedRectangleOfSameSize());
            segment.Unlock();
            return targetBitmap;
        }
    }
}
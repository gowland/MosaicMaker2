using System.Drawing;

namespace ImageStats.Stats
{
    public static class ImageManipulationInfoExtensions
    {
        public static Rectangle AsRectangle(this ImageManipulationInfo info)
        {
            return new Rectangle(info.StartX, info.StartY, info.Width, info.Height);
        }

        public static Rectangle AsZeroBasedRectangleOfSameSize(this ImageManipulationInfo info)
        {
            return new Rectangle(0, 0, info.Width, info.Height);
        }
    }
}
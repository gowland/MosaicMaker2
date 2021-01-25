using System.Drawing;
using ImageStats.Stats;

namespace ImageStats
{
    public class SourceAndMatch
    {
        public SourceAndMatch(ImageManipulationInfo sourceSegment, Bitmap replacementImage)
        {
            SourceSegment = sourceSegment;
            ReplacementImage = replacementImage;
        }

        public ImageManipulationInfo SourceSegment { get; }
        public Bitmap ReplacementImage { get; }
    }
}
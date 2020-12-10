using System;

namespace ImageStats.Stats
{
    [Serializable]
    public struct SegmentAndStats
    {
        public SegmentAndStats(ImageManipulationInfo manipulationInfo, ImageStats stats)
        {
            ManipulationInfo = manipulationInfo;
            Stats = stats;
        }

        public ImageManipulationInfo ManipulationInfo { get; set; }
        public ImageStats Stats { get; set; }
    }
}
using System;

namespace ImageStats.Stats
{
    [Serializable]
    public struct SegmentAndStats
    {
        public SegmentAndStats(ImageManipulationInfo manipulationInfo, BasicStats stats)
        {
            ManipulationInfo = manipulationInfo;
            Stats = stats;
        }

        public ImageManipulationInfo ManipulationInfo { get; set; }
        public BasicStats Stats { get; set; }
    }
}
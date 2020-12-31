using System;
using System.Collections.Generic;
using System.Linq;

namespace ImageStats.Stats
{
    [Serializable]
    public struct ImageAndStats
    {
        public ImageAndStats(PhysicalImage image, ImageManipulationInfo manipulationInfo, BasicStats stats)
            : this (image, new []{new SegmentAndStats(manipulationInfo, stats), })
        {
        }

        public ImageAndStats(PhysicalImage image, IEnumerable<SegmentAndStats> segments)
        {
            Image = image;
            Segments = segments.ToArray();
        }

        public PhysicalImage Image { get; set; }
        public SegmentAndStats[] Segments { get; set; }
    }

    public struct ImageSegments
    {
        public ImageSegments(PhysicalImage image, IEnumerable<ImageManipulationInfo> manipulationInfos)
        {
            Image = image;
            ManipulationInfos = manipulationInfos.ToArray();
        }

        public PhysicalImage Image { get; set; }
        public ImageManipulationInfo[] ManipulationInfos { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;

namespace ImageStats.Stats
{
    [Serializable]
    public struct ImageAndStats
    {
        public ImageAndStats(PhysicalImage image, ImageManipulationInfo manipulationInfo, ImageStats stats)
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
}
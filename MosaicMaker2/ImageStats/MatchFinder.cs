using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using FastBitmap;
using ImageStats.MatchFilters;
using ImageStats.Stats;
using ImageStats.Utils;

namespace ImageStats
{
    public class MatchFinder
    {
        private readonly IFilter _pointlessColorFilter;
        private readonly IFilter _laxColorFilter;
        private readonly IFilter _midColorFilter;
        private readonly IFilter _strictColorFilter;
        private readonly Alphabet _alphabet;

        public MatchFinder(Alphabet alphabet)
        {
            _alphabet = alphabet;

            _pointlessColorFilter = new CompoundFilterBuilder()
                .WithConvolutionResultFilter(diff => diff < 60, result => result.LowResR, "LowResRed")
                .WithConvolutionResultFilter(diff => diff < 60, result => result.LowResG, "LowResGreen")
                .WithConvolutionResultFilter(diff => diff < 60, result => result.LowResB, "LowResBlue")
                .WithConvolutionResultFilter(diff => diff < 75, result => result.LowResIntensity, "LowResIntensity")
                .Build();

            _laxColorFilter = new CompoundFilterBuilder()
                .WithConvolutionResultFilter(diff => diff < 40, result => result.LowResR, "LowResRed")
                .WithConvolutionResultFilter(diff => diff < 40, result => result.LowResG, "LowResGreen")
                .WithConvolutionResultFilter(diff => diff < 40, result => result.LowResB, "LowResBlue")
                .WithConvolutionResultFilter(diff => diff < 55, result => result.LowResIntensity, "LowResIntensity")
                .Build();

            _midColorFilter = new CompoundFilterBuilder()
                .WithConvolutionResultFilter(diff => diff < 30, result => result.LowResR, "LowResRed")
                .WithConvolutionResultFilter(diff => diff < 30, result => result.LowResG, "LowResGreen")
                .WithConvolutionResultFilter(diff => diff < 30, result => result.LowResB, "LowResBlue")
                .WithConvolutionResultFilter(diff => diff < 35, result => result.LowResIntensity, "LowResIntensity")
                .Build();

            _strictColorFilter = new CompoundFilterBuilder()
                .WithConvolutionResultFilter(diff => diff < 20, result => result.LowResR, "LowResRed")
                .WithConvolutionResultFilter(diff => diff < 20, result => result.LowResG, "LowResGreen")
                .WithConvolutionResultFilter(diff => diff < 20, result => result.LowResB, "LowResBlue")
                .WithConvolutionResultFilter(diff => diff < 25, result => result.LowResIntensity, "LowResIntensity")
                .Build();
        }

        public ImageSegments[] GetMatches(BasicStats origStats)
        {
            return _alphabet.ImagesAndStats
                .SelectMany(img => img.Segments.Select(seg => new {Image = img.Image, Segment = seg}))
                .OrderBy(seg => seg.Segment.Stats.LowResR.Difference(origStats.LowResR) +
                                1.1 * seg.Segment.Stats.LowResG.Difference(origStats.LowResG) +
                                seg.Segment.Stats.LowResB.Difference(origStats.LowResB) +
                                1.5 * seg.Segment.Stats.LowResIntensity.Difference(origStats.LowResIntensity))
                .Take(100)
/*
                .OrderBy(seg => seg.Segment.Stats.MidResHorizontal.Difference(origStats.MidResHorizontal)
                                + seg.Segment.Stats.MidResVertical.Difference(origStats.MidResVertical)
                                + 1.1 * seg.Segment.Stats.MidRes45.Difference(origStats.MidRes45)
                                // + 1.1 * seg.Segment.Stats.MidRes135.Difference(origStats.MidRes135)
                                // + 1.5 * seg.Segment.Stats.MidResEdge.Difference(origStats.MidResEdge)
                                )
*/
                .GroupBy(seg => seg.Image.ImagePath)
                .Select(group => new ImageSegments(group.First().Image, group.Select(i => i.Segment.ManipulationInfo)))
                .ToArray();
        }

        public BitmapAndSegments[] RefineMatches(AdvancedStats origStats, ImageSegments[] basicMatches, IImageLoader loader, StatsGenerator statsGenerator)
        {
            return basicMatches.Select(m => new
                {
                    Image = m.Image,
                    Bitmap = loader.LoadImage(m.Image.ImagePath),
                    ManipulationInfos = m.ManipulationInfos
                })
                .SelectMany(m => m.ManipulationInfos.Select(r => new
                {
                    ImagePath = m.Image,
                    Image = m.Bitmap,
                    Stats = statsGenerator.GetAdvancedStats(m.Bitmap, r.Rectangle),
                    ManipulationInfo = r,
                }))
                .OrderBy(m =>
                        m.Stats.MidResR.Difference(origStats.MidResR)
                         + 1.1 * m.Stats.MidResG.Difference(origStats.MidResG)
                         +       m.Stats.MidResB.Difference(origStats.MidResB)
                         + 1.5 * m.Stats.MidResIntensity.Difference(origStats.MidResIntensity)
                         )
                .Take(10)
                .GroupBy(m => m.ImagePath.ImagePath)
                .Select(group => new BitmapAndSegments(group.First().Image, group.Select(i => i.ManipulationInfo)))
                .ToArray();
        }

        private IEnumerable<ImageAndStats> Filter(Stats.BasicStats origStats, IEnumerable<ImageAndStats> images, IFilter filter)
        {
            foreach (var replacement in images)
            {
                var matchingSegments = FilterSegments(origStats, replacement, filter).ToArray();
                if (matchingSegments.Any())
                {
                    yield return new ImageAndStats(replacement.Image, matchingSegments);
                }
            }
        }

        private IEnumerable<SegmentAndStats> FilterSegments(Stats.BasicStats origStats, ImageAndStats imageAndStats, IFilter filter)
        {
            foreach (var replacement in imageAndStats.Segments)
            {
                if (filter.Compare(origStats, replacement.Stats).Passed)
                {
                    yield return replacement;
                }
            }
        }
    }
}
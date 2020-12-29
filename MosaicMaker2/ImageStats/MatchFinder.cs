using System.Collections.Generic;
using System.Linq;
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

        public ImageAndStats[] GetMatches(Stats.ImageStats origStats)
        {
            ImageAndStats[]
                matches = Filter(origStats, _alphabet.ImagesAndStats, _strictColorFilter)
                    .ToArray(); // get initial strict matches

            IFilter[] filters =
            {
                _strictColorFilter, _midColorFilter, _laxColorFilter, _pointlessColorFilter
            };

            foreach (var filter in filters) // apply progressively laxer filters until we get at least 2 matches
            {
                if (matches.Length < 2)
                {
                    var newMatches = Filter(origStats, matches, filter).ToArray();
                    if (newMatches.Length > matches.Length && newMatches.Length < 20)
                    {
                        matches = newMatches;
                    }
                }
            }

            matches = matches.Random().Take(100).ToArray(); // Max 100 matches
            return matches;
        }

        private IEnumerable<ImageAndStats> Filter(Stats.ImageStats origStats, IEnumerable<ImageAndStats> images, IFilter filter)
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

        private IEnumerable<SegmentAndStats> FilterSegments(Stats.ImageStats origStats, ImageAndStats imageAndStats, IFilter filter)
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
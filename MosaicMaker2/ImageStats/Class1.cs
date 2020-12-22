using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using FastBitmap;
using ImageStats.ArrayAdapters;
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
                .WithConvolutionResultFilter(diff => diff < 80, result => result.LowResR, "LowResRed")
                .WithConvolutionResultFilter(diff => diff < 80, result => result.LowResG, "LowResGreen")
                .WithConvolutionResultFilter(diff => diff < 80, result => result.LowResB, "LowResBlue")
                .WithConvolutionResultFilter(diff => diff < 100, result => result.LowResIntensity, "LowResIntensity")
                .Build();

            _laxColorFilter = new CompoundFilterBuilder()
                .WithConvolutionResultFilter(diff => diff < 40, result => result.LowResR, "LowResRed")
                .WithConvolutionResultFilter(diff => diff < 40, result => result.LowResG, "LowResGreen")
                .WithConvolutionResultFilter(diff => diff < 40, result => result.LowResB, "LowResBlue")
                .WithConvolutionResultFilter(diff => diff < 50, result => result.LowResIntensity, "LowResIntensity")
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

    public class Class1
    {
        private readonly IImageLoader Loader = new IncrediblyInefficientImageLoader();

        private readonly StatsGenerator _statsGenerator;
        private MatchFinder _matchFinder;

        public Class1(IImageLoader loader)
        {
            Loader = loader;
            _statsGenerator = new StatsGenerator(loader);
        }

        public void CreateIndex()
        {
            _statsGenerator.CreateIndex();
        }

        public void LoadIndex()
        {
            _statsGenerator.LoadIndex();
            _matchFinder = new MatchFinder(new Alphabet(_statsGenerator.ImagesAndStats));
        }

        public ImageAndStats GetRandom()
        {
            return _statsGenerator.ImagesAndStats.Random().First();
        }

        public Bitmap GetBitmap(PhysicalImage physicalImage, ImageManipulationInfo manipulationInfo)
        {
            return BitmapAdapter.FromPath(physicalImage.ImagePath, Loader)
                .GetSegment(manipulationInfo);
        }

        public IEnumerable<Bitmap> CompareImageToAlphabet(PhysicalImage image, ImageManipulationInfo manipulationInfo)
        {
            return CompareImageToAlphabet(_statsGenerator.GetStats(image, manipulationInfo));
        }

        public IEnumerable<Bitmap> CompareImageToAlphabet(Stats.ImageStats origStats)
        {
            return _matchFinder.GetMatches(origStats).SelectMany(r =>
            {
                var segmentsAsString = string.Join(",", r.Segments
                    .Select(s => s.ToString()));
                Console.WriteLine($"Returning match {r.Image.ImagePath} with segments {segmentsAsString}");
                var img = BitmapAdapter.FromPath(r.Image.ImagePath, Loader);
                return r.Segments.Select(s => img.GetSegment(s.ManipulationInfo));
            });
        }
    }

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
                manipulationInfo.AsRectangle(),
                manipulationInfo.AsZeroBasedRectangleOfSameSize());
            segment.Unlock();
            return targetBitmap;
        }
    }

    public class SourceAndMatch
    {
        public SourceAndMatch(ImageManipulationInfo sourceSegment, PhysicalImage replacementImage, ImageManipulationInfo replacementSegment)
        {
            SourceSegment = sourceSegment;
            ReplacementImage = replacementImage;
            ReplacementSegment = replacementSegment;
        }

        public ImageManipulationInfo SourceSegment { get; }
        public PhysicalImage ReplacementImage { get; }
        public ImageManipulationInfo ReplacementSegment { get; }
    }
}

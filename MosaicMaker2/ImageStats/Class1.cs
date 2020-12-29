using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using FastBitmap;
using ImageStats.ArrayAdapters;
using ImageStats.Stats;
using ImageStats.Utils;

namespace ImageStats
{
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

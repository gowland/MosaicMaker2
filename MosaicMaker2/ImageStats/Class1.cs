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
        private ImageAndStats _sourceImageStats;

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

        public void LoadSourceImage(string path)
        {
            var bmp = Loader.LoadImageAsBitmap(path);
            BitmapAdapter adapter = new BitmapAdapter(bmp);
            var rects = _statsGenerator
                .GetChunks(new Rectangle(0, 0, bmp.Width, bmp.Height))
                .Select(r => new ImageManipulationInfo(r.X, r.Y, r.Width, r.Height));


            _sourceImageStats = new ImageAndStats(new PhysicalImage(path),
                rects
                    .Select(r => new SegmentAndStats(r,  _statsGenerator.GetStats(adapter, r)))
                    .ToArray()
                );
        }

        public ImageAndStats GetSourceImage()
        {
            // return _statsGenerator.ImagesAndStats.Random().First();
            return new ImageAndStats(_sourceImageStats.Image, _sourceImageStats.Segments);
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

        public IEnumerable<Bitmap> CompareImageToAlphabet(BasicStats origStats)
        {
            return _matchFinder.GetMatches(origStats).SelectMany(r =>
            {
                var img = BitmapAdapter.FromPath(r.Image.ImagePath, Loader);
                return r.ManipulationInfos.Select(s => img.GetSegment(s));
            });
        }

        public IEnumerable<Bitmap> GetRefinedMatches(PhysicalImage image, ImageManipulationInfo manipulationInfo)
        {
            var bitmap = Loader.LoadImage(image.ImagePath);
            return GetRefinedMatches(bitmap, manipulationInfo);
        }

        public IEnumerable<Bitmap> GetRefinedMatches(FastBitmap.FastBitmap bitmap, ImageManipulationInfo manipulationInfo)
        {
            ImageSegments[] matches = _matchFinder.GetMatches(_statsGenerator.GetStats(bitmap, manipulationInfo));
            return _matchFinder.RefineMatches(_statsGenerator.GetAdvancedStats(bitmap, manipulationInfo.AsRectangle()), matches,
                    Loader, _statsGenerator)
                .SelectMany(m =>
                {
                    var img = BitmapAdapter.FromPath(m.Image.ImagePath, Loader);
                    return m.ManipulationInfos.Select(s => img.GetSegment(s));
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
        public SourceAndMatch(ImageManipulationInfo sourceSegment, Bitmap replacementImage)
        {
            SourceSegment = sourceSegment;
            ReplacementImage = replacementImage;
        }

        public ImageManipulationInfo SourceSegment { get; }
        public Bitmap ReplacementImage { get; }
    }


    public class ReconstructedImageBuilder
    {
        private readonly FastBitmap.FastBitmap _target;
        private readonly double _newImagePercentage;
        private readonly double _oldImagePercentage;
        private readonly Bitmap _newImage;

        public ReconstructedImageBuilder(FastBitmap.FastBitmap target, double newImagePercentage)
        {
            _target = target;
            _newImagePercentage = newImagePercentage;
            _oldImagePercentage = (1 - _newImagePercentage);
            _newImage = new Bitmap(target.Width, target.Height);
        }

        public void SaveAs(string path)
        {
            _newImage.Save(path);
        }

        public void ApplyMatches(IEnumerable<SourceAndMatch> matches)
        {
            _target.Lock();
            foreach (var imageMatch in matches)
            {
                WriteSourceAndFill(imageMatch);
            }
            _target.Unlock();
        }

        private void WriteSourceAndFill(SourceAndMatch match)
        {
            var fillImage = new FastBitmap.FastBitmap(match.ReplacementImage);

            fillImage.Lock();

            ImageManipulationInfo hole = match.SourceSegment;
            for (int xOffset = 0; xOffset < hole.Width; xOffset++)
            {
                for (int yOffset = 0; yOffset < hole.Height; yOffset++)
                {
                    Color holeColor = _target.GetPixel(hole.StartX + xOffset, hole.StartY + yOffset);
                    Color fillColor = fillImage.GetPixel(xOffset, yOffset);
                    _newImage.SetPixel(hole.StartX + xOffset, hole.StartY + yOffset, GetNewColor(holeColor, fillColor));
                }
            }

            fillImage.Unlock();
        }

        private Color GetNewColor(Color source, Color fill)
        {
            return Color.FromArgb(
                255,
                GetNewImageValue(source.R, fill.R),
                GetNewImageValue(source.G, fill.G),
                GetNewImageValue(source.B, fill.B));
        }

        private int GetNewImageValue(int source, int fill)
        {
            return (int)Math.Floor((source*_oldImagePercentage) + (fill*_newImagePercentage));
        }
    }
}

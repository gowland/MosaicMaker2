using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using FastBitmap;
using ImageStats.Stats;

namespace ImageStats
{
    public class Class1
    {
        private readonly IImageLoader _loader = new IncrediblyInefficientImageLoader();

        private readonly StatsGenerator _statsGenerator;
        private MatchFinder _matchFinder;
        private ImageAndStats _sourceImageStats;

        public Class1(IImageLoader loader)
        {
            _loader = loader;
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
            var bmp = _loader.LoadImageAsBitmap(path);
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
            return new ImageAndStats(_sourceImageStats.Image, _sourceImageStats.Segments);
        }

        public Bitmap GetBitmap(PhysicalImage physicalImage, ImageManipulationInfo manipulationInfo)
        {
            return BitmapAdapter.FromPath(physicalImage.ImagePath, _loader)
                .GetSegment(manipulationInfo);
        }

        public IEnumerable<Bitmap> GetRefinedMatches(PhysicalImage image, ImageManipulationInfo manipulationInfo)
        {
            var bitmap = _loader.LoadImage(image.ImagePath);
            return GetRefinedMatches(bitmap, manipulationInfo);
        }

        public IEnumerable<Bitmap> GetRefinedMatches(FastBitmap.FastBitmap bitmap, ImageManipulationInfo manipulationInfo)
        {
            ImageSegments[] matches = _matchFinder.GetMatches(_statsGenerator.GetStats(bitmap, manipulationInfo));
            return _matchFinder.RefineMatches(_statsGenerator.GetAdvancedStats(bitmap, manipulationInfo.Rectangle), matches,
                    _loader, _statsGenerator)
                .Select(m => new {Image = new BitmapAdapter(m.Image.ToBitmap()), Segments = m.ManipulationInfos})
                .SelectMany(m => m.Segments.Select(m.Image.GetSegment));
        }

        public IEnumerable<Bitmap> GetRefinedMatches2(PhysicalImage image, ImageManipulationInfo manipulationInfo)
        {
            var bitmap = _loader.LoadImage(image.ImagePath);
            return GetRefinedMatches2(bitmap, manipulationInfo);
        }

        public IEnumerable<Bitmap> GetRefinedMatches2(FastBitmap.FastBitmap bitmap, ImageManipulationInfo manipulationInfo)
        {
            ImageSegments[] matches = _matchFinder.GetMatches(_statsGenerator.GetStats(bitmap, manipulationInfo));
            return _matchFinder.RefineMatches2(_statsGenerator.GetAdvancedStats(bitmap, manipulationInfo.Rectangle), matches,
                    _loader, _statsGenerator)
                .Select(m => new {Image = new BitmapAdapter(m.Image.ToBitmap()), Segments = m.ManipulationInfos})
                .SelectMany(m => m.Segments.Select(m.Image.GetSegment));
        }
    }
}

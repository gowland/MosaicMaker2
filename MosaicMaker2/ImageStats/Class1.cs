using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using FastBitmap;
using ImageStats.ArrayAdapters;
using ImageStats.MatchFilters;
using ImageStats.RegionCreation;
using ImageStats.Stats;
using ImageStats.Utils;

namespace ImageStats
{
    public class StatsGenerator
    {
        private readonly IImageLoader _loader;
        public static readonly double[] LowResSingleIntFilter = new double[] {0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,};
        public static readonly double[] MidResEdgeFilter = {
            -1.0, -1.0, -1.0, 
            -1.0,  8, -1.0, 
            -1.0, -1.0, -1.0, 
        };
        public static readonly double[] MidResHorizontalFilter = {
            1,  0, -1, 
            2,  0, -2, 
            1,  0, -1, 
        };
        public static readonly double[] MidResVerticalFilter = {
            1,  2,  1, 
            0,  0,  0, 
            -1, -2, -1, 
        };
        public static readonly double[] MidRes45Filter = {
            -1, -1,  2, 
            -1,  2, -1, 
            2, -1, -1, 
        };
        public static readonly double[] MidRes135Filter = {
            2, -1, -1, 
            -1,  2, -1, 
            -1, -1,  2, 
        };

        public StatsGenerator(IImageLoader loader)
        {
            _loader = loader;
        }

        public ImageAndStats[] _imagesAndStats { get; set; }

        public IEnumerable<Rectangle> GetSegmentRectangles(Rectangle source)
        {
            var regionCreationStrategy = new FixedSizeRegionCreationStrategy(40,30, 5, 5);
            return regionCreationStrategy.GetRegions(source);
        }

        public IEnumerable<Rectangle> GetMidResRectangles(Rectangle source)
        {
            FixedSizeRegionCreationStrategy regionCreationStrategy = new FixedSizeRegionCreationStrategy(3, 3, 1, 1);
            return regionCreationStrategy.GetRegions(source);
        }

        public IEnumerable<Rectangle> GetLowResRectangles(Rectangle source)
        {
            NonOverlappingRegionCreationStrategy regionCreationStrategy = new NonOverlappingRegionCreationStrategy(10,10);
            return regionCreationStrategy.GetRegions(source);
        }

        public Stats.ImageStats GetStats(PhysicalImage image, ImageManipulationInfo manipulationInfo)
        {
            global::FastBitmap.FastBitmap bitmap = _loader.LoadImage(image.ImagePath);
            var sourceRectangle = manipulationInfo.AsRectangle(); 

            List<int> lowResRPoints = new List<int>(12);
            List<int> lowResGPoints = new List<int>(12);
            List<int> lowResBPoints = new List<int>(12);
            List<int> lowResIntensityPoints = new List<int>(12);

            List<int> midResVerticalPoints = new List<int>(48);

            foreach (var lowResRect in this.GetLowResRectangles(sourceRectangle))
            {
                var segment = bitmap.GetColors(lowResRect).ToArray();

                lowResRPoints.Add(ApplyFilter(segment, c => c.R, StatsGenerator.LowResSingleIntFilter));
                lowResGPoints.Add(ApplyFilter(segment, c => c.G, StatsGenerator.LowResSingleIntFilter));
                lowResBPoints.Add(ApplyFilter(segment, c => c.B, StatsGenerator.LowResSingleIntFilter));
                lowResIntensityPoints.Add(ApplyFilter(segment, c => (int)((c.R + c.G + c.B)/3.0), StatsGenerator.LowResSingleIntFilter));
            }

            foreach (var midResRect in this.GetMidResRectangles(sourceRectangle))
            {
                var segment = bitmap.GetColors(midResRect).ToArray();

//                lowResRPoints.Add(ApplyFilter(segment, c => c.R, lowResSingleIntFilter));
//                lowResGPoints.Add(ApplyFilter(segment, c => c.G, lowResSingleIntFilter));
//                lowResBPoints.Add(ApplyFilter(segment, c => c.B, lowResSingleIntFilter));
//                lowResIntensityPoints.Add(ApplyFilter(segment, c => (int)((c.R + c.G + c.B)/3.0), lowResSingleIntFilter));
            }

            return new Stats.ImageStats()
            {
                LowResR = new ConvolutionResult(lowResRPoints),
                LowResG = new ConvolutionResult(lowResGPoints),
                LowResB = new ConvolutionResult(lowResBPoints),
                LowResIntensity = new ConvolutionResult(lowResIntensityPoints),
            };
        }

        public int ApplyFilter(Color[] colors, Func<Color, int> colorToIntFunc, double[] filter)
        {
            return (int) colors
                .Select(colorToIntFunc)
                .Zip(filter, (v, f) => v * f)
                .Sum();
        }
    }

    public class Class1
    {
        private const string PathToAlphabet = @"c:\src\MosaicMaker2\Alphabet";
        private const string IndexFileName = @"segment_stats.json";
        private static readonly IncrediblyInefficientImageLoader Loader = new IncrediblyInefficientImageLoader();

        private readonly Random _random = new Random();
        private readonly IFilter _pointlessColorFilter;
        private readonly IFilter _laxColorFilter;
        private readonly IFilter _midColorFilter;
        private readonly IFilter _strictColorFilter;

        private StatsGenerator _statsGenerator;

        public Class1()
        {
            _statsGenerator = new StatsGenerator(Loader);

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

        public StatsGenerator StatsGenerator
        {
            set { _statsGenerator = value; }
            get { return _statsGenerator; }
        }

        public void CreateIndex()
        {
            _statsGenerator._imagesAndStats = GetFiles(PathToAlphabet)
                .Select(GetMatchableSegments)
                .ToArray();
            Serializer.WriteToJsonFile(Path.Combine(PathToAlphabet, IndexFileName), new Alphabet(_statsGenerator._imagesAndStats));
        }

        public void LoadIndex()
        {
            Alphabet alphabet = Serializer.ReadFromJsonFile<Alphabet>(Path.Combine(PathToAlphabet, IndexFileName));
            _statsGenerator._imagesAndStats = alphabet.ImagesAndStats;
        }

        private ImageAndStats GetMatchableSegments(PhysicalImage physicalImage)
        {
            var img = Loader.LoadImage(physicalImage.ImagePath);
            IEnumerable<Rectangle> rects = _statsGenerator.GetSegmentRectangles(new Rectangle(0, 0, img.Width, img.Height));
            var segmentStats = new List<SegmentAndStats>();
            foreach (var rectangle in rects)
            {
                var imageManipulationInfo = new ImageManipulationInfo(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
                Stats.ImageStats imageStats = _statsGenerator.GetStats(physicalImage, imageManipulationInfo);
                segmentStats.Add(new SegmentAndStats(imageManipulationInfo, imageStats));
            }

            return new ImageAndStats(physicalImage, segmentStats);
        }

        public ImageAndStats GetRandom()
        {
            return _statsGenerator._imagesAndStats.Random().First();
        }


        public Bitmap GetBitmap(PhysicalImage physicalImage, ImageManipulationInfo manipulationInfo)
        {
            return BitmapAdapter.FromPath(physicalImage.ImagePath, Loader)
                .GetSegment(manipulationInfo);
        }

        private static Bitmap GetBitmap(ImageManipulationInfo manipulationInfo, Bitmap img)
        {
            var bmp = new Bitmap(manipulationInfo.Width, manipulationInfo.Height);
            FastBitmap.FastBitmap segment = new FastBitmap.FastBitmap(bmp);
            segment.Lock();
            segment.CopyRegion(img,
                manipulationInfo.AsRectangle(),
                manipulationInfo.AsZeroBasedRectangleOfSameSize());
            segment.Unlock();
            return bmp;
        }


        public IEnumerable<Bitmap> GetMidResConvolution(PhysicalImage physicalImage)
        {
            int GetConvolution(IEnumerable<int> convolutionPixels, double[] filter)
            {
                int value = (int)convolutionPixels
                    .Zip(filter, (a, b) => a * b)
                    .Sum();

                value = value < 0 ? 0 : value;
                value = value > 255 ? 255 : value;

                return value;
            }

            FastBitmap.FastBitmap bitmap = Loader.LoadImage(physicalImage.ImagePath);
            var bitmapAsIntensity = new FastBitmapToIntensity2DArrayAdapter(bitmap);

            var convolution135Result = new FlatArray2DArray<int>(new int[bitmap.Width * bitmap.Height], bitmap.Width, bitmap.Height);
            var convolution45Result = new FlatArray2DArray<int>(new int[bitmap.Width * bitmap.Height], bitmap.Width, bitmap.Height);
            var convolutionHorizontalResult = new FlatArray2DArray<int>(new int[bitmap.Width * bitmap.Height], bitmap.Width, bitmap.Height);
            var convolutionVerticalResult = new FlatArray2DArray<int>(new int[bitmap.Width * bitmap.Height], bitmap.Width, bitmap.Height);
            var convolutionEdgeResult = new FlatArray2DArray<int>(new int[bitmap.Width * bitmap.Height], bitmap.Width, bitmap.Height);

            bitmap.Lock();
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    var convolutionPixels = GetConvolutionPixels(bitmapAsIntensity, new Rectangle(x - 1, y - 1, 3, 3)).ToArray();

                    convolution135Result[x,y] = GetConvolution(convolutionPixels, StatsGenerator.MidRes135Filter);
                    convolution45Result[x,y] = GetConvolution(convolutionPixels, StatsGenerator.MidRes45Filter);
                    convolutionHorizontalResult[x,y] = GetConvolution(convolutionPixels, StatsGenerator.MidResHorizontalFilter);
                    convolutionVerticalResult[x,y] = GetConvolution(convolutionPixels, StatsGenerator.MidResVerticalFilter);
                    convolutionEdgeResult[x,y] = GetConvolution(convolutionPixels, StatsGenerator.MidResEdgeFilter);
                }
            }
            bitmap.Unlock();

            return new []{
                convolution135Result.ToBitmap(),
                convolution45Result.ToBitmap(),
                convolutionHorizontalResult.ToBitmap(),
                convolutionVerticalResult.ToBitmap(),
                convolutionEdgeResult.ToBitmap(),
            };
        }

        private IEnumerable<int> GetConvolutionPixels(I2DArray<int> bitmap, Rectangle rectangle)
        {
            for (int y = rectangle.Y; y < rectangle.Y + rectangle.Width; y++)
            {
                for (int x = rectangle.X; x < rectangle.X + rectangle.Height; x++)
                {
                    if (x >= 0 && x < bitmap.Width && y >= 0 && y < bitmap.Height)
                    {
                        yield return bitmap[x, y];
                    }
                    else
                    {
                        yield return 0;
                    }
                }
            }
        }

        public IEnumerable<Bitmap> CompareImageToAlphabet(PhysicalImage image, ImageManipulationInfo manipulationInfo)
        {
            var origStats = _statsGenerator.GetStats(image, manipulationInfo);

            ImageAndStats[] matches = Filter(origStats, _statsGenerator._imagesAndStats, _strictColorFilter).ToArray();  // get initial strict matches

            IFilter[] filters = {/*_strictColorFilter,*/ _midColorFilter, _laxColorFilter, _pointlessColorFilter};

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

            return matches.SelectMany(r =>
            {
                var segmentsAsString = string.Join(",", r.Segments
                    .Select(s => s.ToString()));
                Console.WriteLine($"Returning match {r.Image.ImagePath} with segments {segmentsAsString}");
                var img = BitmapAdapter.FromPath(r.Image.ImagePath, Loader);
                return r.Segments.Select(s => img.GetSegment(s.ManipulationInfo));
            });
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

        private IEnumerable<PhysicalImage> GetFiles(string path)
        {
            foreach (string file in Directory.EnumerateFiles(path, "*.bmp", SearchOption.AllDirectories)) // TODO: Handle other image types
            {
                yield return new PhysicalImage(file);
            }
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
}

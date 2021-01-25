using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using FastBitmap;
using ImageStats.ArrayAdapters;
using ImageStats.RegionCreation;
using ImageStats.Stats;
using ImageStats.Utils;

namespace ImageStats
{
    public class StatsGenerator
    {
        private readonly IImageLoader _loader;
        private static readonly double[] LowResSingleIntFilter = {0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,};
        private static readonly double[] ReduceIdentityFilter = { 1/25.0, 1/25.0, 1/25.0, 1/25.0, 1/25.0, 1/25.0, 1/25.0, 1/25.0, 1/25.0, 1/25.0, 1/25.0, 1/25.0, 1/25.0, 1/25.0, 1/25.0, 1/25.0, 1/25.0, 1/25.0, 1/25.0, 1/25.0, 1/25.0, 1/25.0, 1/25.0, 1/25.0, 1/25.0, 1/25.0 };
        private static readonly double[] MidResEdgeFilter = {
            -1.0, -1.0, -1.0,
            -1.0,  8, -1.0,
            -1.0, -1.0, -1.0,
        };
        private static readonly double[] MidResHorizontalFilter = {
            1,  0, -1,
            2,  0, -2,
            1,  0, -1,
        };
        private static readonly double[] MidResVerticalFilter = {
            1,  2,  1,
            0,  0,  0,
            -1, -2, -1,
        };
        private static readonly double[] MidRes45Filter = {
            -1, -1,  2,
            -1,  2, -1,
            2, -1, -1,
        };
        private static readonly double[] MidRes135Filter = {
            2, -1, -1,
            -1,  2, -1,
            -1, -1,  2,
        };

        public StatsGenerator(IImageLoader loader)
        {
            _loader = loader;
        }

        public ImageAndStats[] ImagesAndStats { get; private set; }

        public IEnumerable<Rectangle> GetSegmentRectangles(Rectangle source)
        {
            var regionCreationStrategy = new FixedSizeRegionCreationStrategy(40,30, 5, 5);
            return regionCreationStrategy.GetRegions(source);
        }

        public IEnumerable<Rectangle> GetChunks(Rectangle source)
        {
            var regionCreationStrategy = new FixedSizeRegionCreationStrategy(40,30, 40, 30);
            return regionCreationStrategy.GetRegions(source);
        }

        private IEnumerable<Rectangle> GetMidResRectangles(Rectangle source)
        {
            FixedSizeRegionCreationStrategy regionCreationStrategy = new FixedSizeRegionCreationStrategy(3, 3, 1, 1);
            return regionCreationStrategy.GetRegions(source);
        }

        private IEnumerable<Rectangle> GetLowResRectangles(Rectangle source)
        {
            NonOverlappingRegionCreationStrategy regionCreationStrategy = new NonOverlappingRegionCreationStrategy(10,10);
            return regionCreationStrategy.GetRegions(source);
        }

        private IEnumerable<Rectangle> GetReductionIdentityRectangles(Rectangle source)
        {
            var regionCreationStrategy = new NonOverlappingRegionCreationStrategy(5, 5);
            return regionCreationStrategy.GetRegions(source);
        }

        public BasicStats GetStats(BitmapAdapter bitmap, ImageManipulationInfo manipulationInfo)
        {
            Console.WriteLine(manipulationInfo.ToString());
            var fast = new FastBitmap.FastBitmap(bitmap.GetSegment(manipulationInfo));
            return GetBasicStats(fast, new Rectangle(0,0,fast.Width, fast.Height));
        }

        public BasicStats GetStats(PhysicalImage image, ImageManipulationInfo manipulationInfo)
        {
            global::FastBitmap.FastBitmap bitmap = _loader.LoadImage(image.ImagePath);
            return GetStats(bitmap, manipulationInfo);
        }

        public BasicStats GetStats(FastBitmap.FastBitmap bitmap, ImageManipulationInfo manipulationInfo)
        {
            return GetBasicStats(bitmap, manipulationInfo.Rectangle);
        }

        private BasicStats GetBasicStats(FastBitmap.FastBitmap bitmap, Rectangle sourceRectangle)
        {
            List<int> lowResRPoints = new List<int>(12);
            List<int> lowResGPoints = new List<int>(12);
            List<int> lowResBPoints = new List<int>(12);
            List<int> lowResIntensityPoints = new List<int>(12);

            foreach (var lowResRect in GetLowResRectangles(sourceRectangle))
            {
                var segment = bitmap.GetColors(lowResRect).ToArray();

                lowResRPoints.Add(ApplyFilter(segment, c => c.R, LowResSingleIntFilter));
                lowResGPoints.Add(ApplyFilter(segment, c => c.G, LowResSingleIntFilter));
                lowResBPoints.Add(ApplyFilter(segment, c => c.B, LowResSingleIntFilter));
                lowResIntensityPoints.Add(ApplyFilter(segment, c => (int) ((c.R + c.G + c.B) / 3.0), LowResSingleIntFilter));
            }

            return new BasicStats()
            {
                LowResR = new ConvolutionResult(lowResRPoints),
                LowResG = new ConvolutionResult(lowResGPoints),
                LowResB = new ConvolutionResult(lowResBPoints),
                LowResIntensity = new ConvolutionResult(lowResIntensityPoints),
            };
        }

        public AdvancedStats GetAdvancedStats(FastBitmap.FastBitmap bitmap, Rectangle sourceRectangle)
        {

            /*
            List<int> midResAngle45Points = new List<int>(48);
            List<int> midResAngle135Points = new List<int>(48);
            List<int> midResVerticalPoints = new List<int>(48);
            List<int> midResHorizontalPoints = new List<int>(48);
            List<int> midResEdgePoints = new List<int>(48);
            */

            List<int> midResRPoints = new List<int>(48);
            List<int> midResGPoints = new List<int>(48);
            List<int> midResBPoints = new List<int>(48);
            List<int> midResIntensityPoints = new List<int>(48);

            foreach (var midResRect in GetMidResRectangles(sourceRectangle))
            {
                Color[] segment = bitmap.GetColors(midResRect).ToArray();

                int[] greyScaleSegment = segment.Select(c => (int) ((c.R + c.G + c.B) / 3.0)).ToArray();

                /*
                midResAngle45Points.Add(ApplyFilter(greyScaleSegment, MidRes45Filter));
                midResAngle135Points.Add(ApplyFilter(greyScaleSegment, MidRes135Filter));
                midResVerticalPoints.Add(ApplyFilter(greyScaleSegment, MidResVerticalFilter));
                midResHorizontalPoints.Add(ApplyFilter(greyScaleSegment, MidResHorizontalFilter));
                midResEdgePoints.Add(ApplyFilter(greyScaleSegment, MidResEdgeFilter));
                */

                midResRPoints.Add(ApplyFilter(segment, c => c.R, ReduceIdentityFilter));
                midResGPoints.Add(ApplyFilter(segment, c => c.G, ReduceIdentityFilter));
                midResBPoints.Add(ApplyFilter(segment, c => c.B, ReduceIdentityFilter));
                // midResIntensityPoints.Add(ApplyFilter(segment, c => (int) ((c.R + c.G + c.B) / 3.0), ReduceIdentityFilter));
                midResIntensityPoints.Add(ApplyFilter(greyScaleSegment, ReduceIdentityFilter));
            }

            return new AdvancedStats()
            {
                /*
                MidRes45 = new ConvolutionResult(midResAngle45Points),
                MidRes135 = new ConvolutionResult(midResAngle135Points),
                MidResVertical = new ConvolutionResult(midResVerticalPoints),
                MidResHorizontal = new ConvolutionResult(midResHorizontalPoints),
                MidResEdge = new ConvolutionResult(midResEdgePoints),
                */

                MidResR = new ConvolutionResult(midResRPoints),
                MidResG = new ConvolutionResult(midResGPoints),
                MidResB = new ConvolutionResult(midResBPoints),
                MidResIntensity = new ConvolutionResult(midResIntensityPoints),
            };
        }

        private FlatArray2DArray<int> Reduce(FlatArray2DArray<int> source)
        {
            var rect = new Rectangle(0,0,source.Width,source.Height);
            var subRects = GetReductionIdentityRectangles(rect);
            var reduced = subRects
                .Select(r => ApplyFilter(GetConvolutionPixels(source, r).ToArray(), ReduceIdentityFilter));
            return new FlatArray2DArray<int>(reduced, source.Width/5, source.Height/5);
        }

        private int ApplyFilter<T>(T[] colors, Func<T, int> colorToIntFunc, double[] filter)
        {
            return (int) colors
                .Select(colorToIntFunc)
                .Zip(filter, (v, f) => v * f)
                .Sum();
        }

        private int ApplyFilter(int[] colors, double[] filter)
        {
            return (int) colors
                .Zip(filter, (v, f) => v * f)
                .Sum();
        }

        private ImageAndStats GetMatchableSegments(PhysicalImage physicalImage)
        {
            var img = _loader.LoadImage(physicalImage.ImagePath);
            IEnumerable<Rectangle> rects = GetSegmentRectangles(new Rectangle(0, 0, img.Width, img.Height));
            var segmentStats = new List<SegmentAndStats>();
            foreach (var rectangle in rects)
            {
                var imageManipulationInfo = new ImageManipulationInfo(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
                Stats.BasicStats basicStats = GetStats(physicalImage,
                    imageManipulationInfo);
                segmentStats.Add(new SegmentAndStats(imageManipulationInfo, basicStats));
            }

            return new ImageAndStats(physicalImage, segmentStats);
        }

        public void CreateIndex()
        {
            var path = Path.Combine(PathToAlphabet, IndexFileName);
            File.Delete(path);

            /*
            foreach (var page in GetFiles(PathToAlphabet).AsPages(1000))
            {
                var stats = page.Select(GetMatchableSegments).ToArray();
                Serializer.WriteToJsonFile(path, new Alphabet(stats), append:true);
            }
            */

            Alphabet alphabet = new Alphabet(GetFiles(PathToAlphabet).Select(GetMatchableSegments).ToArray());
            Serializer.WriteToJsonFile(path, alphabet);
            this.ImagesAndStats = alphabet.ImagesAndStats;
        }

        public void LoadIndex()
        {
            Alphabet alphabet = Serializer.ReadFromJsonFile<Alphabet>(Path.Combine(PathToAlphabet, IndexFileName));
            ImagesAndStats = alphabet.ImagesAndStats;
        }

        private const string PathToAlphabet = @"c:\src\MosaicMaker2\Alphabet";
        private const string IndexFileName = @"segment_stats.json";

        private IEnumerable<PhysicalImage> GetFiles(string path)
        {
            foreach (string file in Directory.EnumerateFiles(path, "*.bmp", SearchOption.AllDirectories)) // TODO: Handle other image types
            {
                yield return new PhysicalImage(file);
            }
        }

        public IEnumerable<Bitmap> GetMidResConvolutionAsBitmap(PhysicalImage physicalImage) // Only for demo
        {
            var convolutionResults = GetMidResConvolution2(physicalImage);

            return convolutionResults.Select(r => r.ToBitmap());
        }

        public IEnumerable<Bitmap> GetMidResConvolutionReducedAsBitmap(PhysicalImage physicalImage) // Only for demo
        {
            var convolutionResults = GetMidResConvolution2(physicalImage);

            return convolutionResults
                .Select(Reduce)
                .Select(r => r.ToBitmap());
        }

        private FlatArray2DArray<int>[] GetMidResConvolution2(PhysicalImage physicalImage)
        {
            int GetConvolution(IEnumerable<int> convolutionPixels, double[] filter)
            {
                int value = (int) convolutionPixels
                    .Zip(filter, (a, b) => a * b)
                    .Sum();

                value = value < 0 ? 0 : value;
                value = value > 255 ? 255 : value;

                return value;
            }

            FastBitmap.FastBitmap bitmap = _loader.LoadImage(physicalImage.ImagePath);
            var bitmapAsIntensity = new FastBitmapToIntensity2DArrayAdapter(bitmap);

            var convolution135Result =
                new FlatArray2DArray<int>(new int[bitmap.Width * bitmap.Height], bitmap.Width, bitmap.Height);
            var convolution45Result =
                new FlatArray2DArray<int>(new int[bitmap.Width * bitmap.Height], bitmap.Width, bitmap.Height);
            var convolutionHorizontalResult =
                new FlatArray2DArray<int>(new int[bitmap.Width * bitmap.Height], bitmap.Width, bitmap.Height);
            var convolutionVerticalResult =
                new FlatArray2DArray<int>(new int[bitmap.Width * bitmap.Height], bitmap.Width, bitmap.Height);
            var convolutionEdgeResult =
                new FlatArray2DArray<int>(new int[bitmap.Width * bitmap.Height], bitmap.Width, bitmap.Height);

            bitmap.Lock();
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    var convolutionPixels =
                        GetConvolutionPixels(bitmapAsIntensity, new Rectangle(x - 1, y - 1, 3, 3)).ToArray();

                    convolution135Result[x, y] = GetConvolution(convolutionPixels, StatsGenerator.MidRes135Filter);
                    convolution45Result[x, y] = GetConvolution(convolutionPixels, StatsGenerator.MidRes45Filter);
                    convolutionHorizontalResult[x, y] =
                        GetConvolution(convolutionPixels, StatsGenerator.MidResHorizontalFilter);
                    convolutionVerticalResult[x, y] = GetConvolution(convolutionPixels, StatsGenerator.MidResVerticalFilter);
                    convolutionEdgeResult[x, y] = GetConvolution(convolutionPixels, StatsGenerator.MidResEdgeFilter);
                }
            }

            bitmap.Unlock();

            var convolutionResults = new[]
            {
                convolution135Result,
                convolution45Result,
                convolutionHorizontalResult,
                convolutionVerticalResult,
                convolutionEdgeResult,
            };
            return convolutionResults;
        }

        /*
        private FlatArray2DArray<int>[] GetMidResConvolutionsReduced(PhysicalImage physicalImage)
        {
            int GetConvolution(IEnumerable<int> convolutionPixels, double[] filter)
            {
                int value = (int) convolutionPixels
                    .Zip(filter, (a, b) => a * b)
                    .Sum();

                value = value < 0 ? 0 : value;
                value = value > 255 ? 255 : value;

                return value;
            }

            FastBitmap.FastBitmap bitmap = _loader.LoadImage(physicalImage.ImagePath);
            var bitmapAsIntensity = new FastBitmapToIntensity2DArrayAdapter(bitmap);

            var convolution135Result =
                new FlatArray2DArray<int>(new int[bitmap.Width * bitmap.Height], bitmap.Width, bitmap.Height);
            var convolution45Result =
                new FlatArray2DArray<int>(new int[bitmap.Width * bitmap.Height], bitmap.Width, bitmap.Height);
            var convolutionHorizontalResult =
                new FlatArray2DArray<int>(new int[bitmap.Width * bitmap.Height], bitmap.Width, bitmap.Height);
            var convolutionVerticalResult =
                new FlatArray2DArray<int>(new int[bitmap.Width * bitmap.Height], bitmap.Width, bitmap.Height);
            var convolutionEdgeResult =
                new FlatArray2DArray<int>(new int[bitmap.Width * bitmap.Height], bitmap.Width, bitmap.Height);

            bitmap.Lock();
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    var convolutionPixels =
                        GetConvolutionPixels(bitmapAsIntensity, new Rectangle(x - 1, y - 1, 3, 3)).ToArray();

                    convolution135Result[x, y] = GetConvolution(convolutionPixels, StatsGenerator.MidRes135Filter);
                    convolution45Result[x, y] = GetConvolution(convolutionPixels, StatsGenerator.MidRes45Filter);
                    convolutionHorizontalResult[x, y] =
                        GetConvolution(convolutionPixels, StatsGenerator.MidResHorizontalFilter);
                    convolutionVerticalResult[x, y] = GetConvolution(convolutionPixels, StatsGenerator.MidResVerticalFilter);
                    convolutionEdgeResult[x, y] = GetConvolution(convolutionPixels, StatsGenerator.MidResEdgeFilter);
                }
            }

            bitmap.Unlock();

            var convolutionResults = new[]
            {
                convolution135Result,
                convolution45Result,
                convolutionHorizontalResult,
                convolutionVerticalResult,
                convolutionEdgeResult,
            };

            return convolutionResults.Select();
        }
        */

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
    }
}
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using FastBitmap;
using Newtonsoft.Json;

namespace ImageStats
{
    public class Class1
    {
        private const string _pathToAlphabet = @"c:\src\MosaicMaker2\Alphabet\2004_Photos";
        private const string _indexFileName = @"segment_stats.json";
        private static readonly IncrediblyInefficientImageLoader Loader = new IncrediblyInefficientImageLoader();
        private static readonly double[] lowResSingleIntFilter = new double[] {0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,0.01,};

        private static readonly double[] midResEdgeFilter = new double[] {
            -1.0, -1.0, -1.0, 
            -1.0,  8, -1.0, 
            -1.0, -1.0, -1.0, 
        };
        private static readonly double[] midResHorizontalFilter = new double[] {
            1,  0, -1, 
            2,  0, -2, 
            1,  0, -1, 
        };
        private static readonly double[] midResVerticalFilter = new double[] {
            1,  2,  1, 
            0,  0,  0, 
           -1, -2, -1, 
        };
        private static readonly double[] midRes45Filter = new double[] {
           -1, -1,  2, 
           -1,  2, -1, 
            2, -1, -1, 
        };
        private static readonly double[] midRes135Filter = new double[] {
            2, -1, -1, 
           -1,  2, -1, 
           -1, -1,  2, 
        };

        private ImageAndStats[] _imagesAndStats;
        private readonly Random _random = new Random();
        private readonly IFilter _laxColorFilter;
        private readonly IFilter _midColorFilter;
        private readonly IFilter _strictColorFilter;

        public Class1()
        {
            _imagesAndStats = GetFiles(@"c:\src\MosaicMaker2\Alphabet\")
                .Select(GetMatchableSegments)
                .ToArray();
            Serializer.WriteToJsonFile(@"c:\src\MosaicMaker2\Alphabet\segment_stats.json", new Alphabet(_imagesAndStats));

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

        public void CreateIndex()
        {
            _imagesAndStats = GetFiles(_pathToAlphabet)
                .Select(GetMatchableSegments)
                .ToArray();
            Serializer.WriteToJsonFile<Alphabet>(Path.Combine(_pathToAlphabet, _indexFileName), new Alphabet(_imagesAndStats));
        }

        public void LoadIndex()
        {
            Alphabet alphabet = Serializer.ReadFromJsonFile<Alphabet>(Path.Combine(_pathToAlphabet, _indexFileName));
            _imagesAndStats = alphabet.ImagesAndStats;
        }

        private ImageAndStats GetMatchableSegments(PhysicalImage physicalImage)
        {
            var img = Loader.LoadImage(physicalImage.ImagePath);
            IEnumerable<Rectangle> rects = GetSegmentRectangles(new Rectangle(0, 0, img.Width, img.Height));
            var segmentStats = new List<SegmentAndStats>();
            foreach (var rectangle in rects)
            {
                var imageManipulationInfo = new ImageManipulationInfo(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
                ImageStats imageStats = GetStats(physicalImage,
                    imageManipulationInfo);
                segmentStats.Add(new SegmentAndStats(imageManipulationInfo, imageStats));
            }

            return new ImageAndStats(physicalImage, segmentStats);
        }

        public ImageAndStats GetRandom()
        {
            return _imagesAndStats.OrderBy(_ => _random.Next()).First();
        }

        public Bitmap GetBitmap(PhysicalImage physicalImage)
        {
            return Loader.LoadImage(physicalImage.ImagePath).ToBitmap();
        }

        public Bitmap GetBitmap(PhysicalImage physicalImage, ImageManipulationInfo manipulationInfo)
        {
            var img = Loader.LoadImageAsBitmap(physicalImage.ImagePath);
            var bmp = new Bitmap(manipulationInfo.Width, manipulationInfo.Height);
            FastBitmap.FastBitmap segment = new FastBitmap.FastBitmap(bmp);
            segment.Lock();
            segment.CopyRegion(img, 
                new Rectangle(manipulationInfo.StartX, manipulationInfo.StartY, manipulationInfo.Width, manipulationInfo.Height),
                new Rectangle(0, 0, manipulationInfo.Width, manipulationInfo.Height));
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

                    convolution135Result[x,y] = GetConvolution(convolutionPixels, midRes135Filter);
                    convolution45Result[x,y] = GetConvolution(convolutionPixels, midRes45Filter);
                    convolutionHorizontalResult[x,y] = GetConvolution(convolutionPixels, midResHorizontalFilter);
                    convolutionVerticalResult[x,y] = GetConvolution(convolutionPixels, midResVerticalFilter);
                    convolutionEdgeResult[x,y] = GetConvolution(convolutionPixels, midResEdgeFilter);
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

        private IEnumerable<int> GetConvolutionPixels(FastBitmap.FastBitmap bitmap, Func<Color,int> colorToIntFunc, Rectangle rectangle)
        {
            for (int y = rectangle.Y; y < rectangle.Y + rectangle.Width; y++)
            {
                for (int x = rectangle.X; x < rectangle.X + rectangle.Height; x++)
                {
                    if (x >= 0 && x < bitmap.Width && y >= 0 && y < bitmap.Height)
                    {
                        yield return colorToIntFunc(bitmap.GetPixel(x, y));
                    }
                    else
                    {
                        yield return 0;
                    }
                }
            }
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
            var origStats = GetStats(image, manipulationInfo);

            ImageAndStats[] matches = _imagesAndStats.ToArray();

            IFilter[] filters = {_laxColorFilter, _midColorFilter, _strictColorFilter};

            foreach (var filter in filters)
            {
                if (matches.Length > 10)
                {
                    var newMatches = Filter(origStats, matches, filter).ToArray();
                    if (newMatches.Length > 1)
                    {
                        matches = newMatches;
                    }
                }
            }

            return matches.SelectMany(r =>
            {
                return r.Segments.Select(s => GetBitmap(r.Image, s.ManipulationInfo));
/*
                Bitmap img = Loader.LoadImageAsBitmap(r.Image.ImagePath);
                var bmp = new Bitmap(r.ManipulationInfo.Width, r.ManipulationInfo.Height);
                FastBitmap.FastBitmap segment = new FastBitmap.FastBitmap(bmp);
                segment.Lock();
                segment.CopyRegion(img, 
                    new Rectangle(r.ManipulationInfo.StartX, r.ManipulationInfo.StartY, r.ManipulationInfo.Width,
                        r.ManipulationInfo.Height),
                    new Rectangle(0, 0, r.ManipulationInfo.Width, r.ManipulationInfo.Height));
                segment.Unlock();
                return bmp;
*/
            });
        }

        private IEnumerable<ImageAndStats> Filter(ImageStats origStats, IEnumerable<ImageAndStats> images, IFilter filter)
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

        private IEnumerable<SegmentAndStats> FilterSegments(ImageStats origStats, ImageAndStats imageAndStats, IFilter filter)
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

        public ImageStats GetStats(PhysicalImage image, ImageManipulationInfo manipulationInfo)
        {
            global::FastBitmap.FastBitmap bitmap = Loader.LoadImage(image.ImagePath);
            var sourceRectangle = new Rectangle(manipulationInfo.StartX, manipulationInfo.StartY, manipulationInfo.Width, manipulationInfo.Height);

            List<int> lowResRPoints = new List<int>(12);
            List<int> lowResGPoints = new List<int>(12);
            List<int> lowResBPoints = new List<int>(12);
            List<int> lowResIntensityPoints = new List<int>(12);

            List<int> midResVerticalPoints = new List<int>(48);

            foreach (var lowResRect in GetLowResRectangles(sourceRectangle))
            {
                var segment = bitmap.GetColors(lowResRect).ToArray();

                lowResRPoints.Add(ApplyFilter(segment, c => c.R, lowResSingleIntFilter));
                lowResGPoints.Add(ApplyFilter(segment, c => c.G, lowResSingleIntFilter));
                lowResBPoints.Add(ApplyFilter(segment, c => c.B, lowResSingleIntFilter));
                lowResIntensityPoints.Add(ApplyFilter(segment, c => (int)((c.R + c.G + c.B)/3.0), lowResSingleIntFilter));
            }

            foreach (var midResRect in GetMidResRectangles(sourceRectangle))
            {
                var segment = bitmap.GetColors(midResRect).ToArray();

//                lowResRPoints.Add(ApplyFilter(segment, c => c.R, lowResSingleIntFilter));
//                lowResGPoints.Add(ApplyFilter(segment, c => c.G, lowResSingleIntFilter));
//                lowResBPoints.Add(ApplyFilter(segment, c => c.B, lowResSingleIntFilter));
//                lowResIntensityPoints.Add(ApplyFilter(segment, c => (int)((c.R + c.G + c.B)/3.0), lowResSingleIntFilter));
            }

            return new ImageStats()
            {
                LowResR = new ConvolutionResult(lowResRPoints),
                LowResG = new ConvolutionResult(lowResGPoints),
                LowResB = new ConvolutionResult(lowResBPoints),
                LowResIntensity = new ConvolutionResult(lowResIntensityPoints),
            };
        }

        private int ApplyFilter(Color[] colors, Func<Color, int> colorToIntFunc, double[] filter)
        {
            return (int) colors
                .Select(colorToIntFunc)
                .Zip(filter, (v, f) => v * f)
                .Sum();
        }

        private IEnumerable<Rectangle> GetSegmentRectangles(Rectangle source)
        {
            var regionCreationStrategy = new FixedSizeRegionCreationStrategy(40,30, 5, 5);
            return regionCreationStrategy.GetRegions(source);
        }

        private IEnumerable<Rectangle> GetLowResRectangles(Rectangle source)
        {
            NonOverlappingRegionCreationStrategy regionCreationStrategy = new NonOverlappingRegionCreationStrategy(10,10);
            return regionCreationStrategy.GetRegions(source);
        }

        private IEnumerable<Rectangle> GetMidResRectangles(Rectangle source)
        {
            FixedSizeRegionCreationStrategy regionCreationStrategy = new FixedSizeRegionCreationStrategy(3, 3, 1, 1);
            return regionCreationStrategy.GetRegions(source);
        }
    }

    public interface I2DArray<T>
    {
        T this [int x, int y] { get; }
        int Width { get; }
        int Height { get; }
    }

    public class FastBitmap2DArray : I2DArray<Color>
    {
        private readonly FastBitmap.FastBitmap _source;

        public FastBitmap2DArray(FastBitmap.FastBitmap source)
        {
            _source = source;
        }

        public Color this[int x, int y]
        {
            get => _source.GetPixel(x,y);
        }

        public int Width => _source.Width;
        public int Height => _source.Height;
    }

    public class FastBitmapFilter2DArrayAdapter : I2DArray<int>
    {
        private readonly FastBitmap.FastBitmap _source;
        private readonly Func<Color, int> _colorToIntFunc;

        public FastBitmapFilter2DArrayAdapter(FastBitmap.FastBitmap source, Func<Color, int> colorToIntFunc)
        {
            _source = source;
            _colorToIntFunc = colorToIntFunc;
        }

        public int this[int x, int y] => _colorToIntFunc(_source.GetPixel(x,y));

        public int Width => _source.Width;
        public int Height => _source.Height;
    }

    public class FastBitmapToIntensity2DArrayAdapter : FastBitmapFilter2DArrayAdapter
    {
        public FastBitmapToIntensity2DArrayAdapter(FastBitmap.FastBitmap source) : base(source, c => (c.R + c.G + c.B)/3)
        {
        }
    }

    public class FlatArray2DArray<T> : I2DArray<T>
    {
        private readonly T[] _source;

        public FlatArray2DArray(IEnumerable<T> source, int width, int height)
        {
            _source = source.ToArray();
            Width = width;
            Height = height;
        }

        public T this[int x, int y]
        {
            get => _source[CoordsToIndex(x, y)];
            set => _source[CoordsToIndex(x, y)] = value;
        }

        public int Width { get; }
        public int Height { get; }

        private int CoordsToIndex(int x, int y)
        {
            return y * Width + x;
        }

        public T this[int i]
        {
            get => _source[i];
            set => _source[i] = value;
        }
    }

    public static class FlatArray2DArrayExtensions
    {
        public static Bitmap ToBitmap(this FlatArray2DArray<int> array)
        {
            FastBitmap.FastBitmap convolutedBitmap = new FastBitmap.FastBitmap(new Bitmap(array.Width, array.Height));
            convolutedBitmap.Lock();

            for (int y = 0; y < array.Height; y++)
            {
                for (int x = 0; x < array.Width; x++)
                {
                    // Get convolution value
                    int value = array[x, y];

                    value = value < 0 ? 0 : value;
                    value = value > 255 ? 255 : value;

                    // Set convolution value
                    convolutedBitmap.SetPixel(x, y, Color.FromArgb(value, value, value));
                }
            }

            convolutedBitmap.Unlock();

            return convolutedBitmap.ToBitmap();
        }
    }

    public interface IRegionCreationStrategy
    {
        IEnumerable<Rectangle> GetRegions(Rectangle sourceRegion);
    }

    public class NonOverlappingRegionCreationStrategy : FixedSizeRegionCreationStrategy
    {
        public NonOverlappingRegionCreationStrategy(int holeWidth, int holeHeight)
            : base(holeWidth, holeHeight, holeWidth, holeHeight)
        {

        }
    }

    public class FixedSizeRegionCreationStrategy : IRegionCreationStrategy
    {
        private readonly int _regionHeight;
        private readonly int _regionWidth;
        private readonly int _horizontalStep;
        private readonly int _verticalStep;

        public FixedSizeRegionCreationStrategy(int regionWidth, int regionHeight, int horizontalStep, int verticalStep)
        {
            _regionHeight = regionHeight;
            _regionWidth = regionWidth;
            _horizontalStep = horizontalStep;
            _verticalStep = verticalStep;
        }

        public IEnumerable<Rectangle> GetRegions(Rectangle sourceRegion)
        {
            if (sourceRegion.Width < _regionWidth || sourceRegion.Height < _regionHeight)
                throw new Exception($"{sourceRegion.Width}x{sourceRegion.Height} < {_regionWidth}x{_regionHeight}");

            for (int xOffset = 0; xOffset + _regionWidth <= sourceRegion.Width; xOffset += _horizontalStep)
            {
                for (int yOffset = 0; yOffset + _regionHeight <= sourceRegion.Height; yOffset += _verticalStep)
                {
                    yield return new Rectangle(sourceRegion.X + xOffset, sourceRegion.Y + yOffset, _regionWidth, _regionHeight);
                }
            }
        }
    }

    public interface IFilter
    {
        FilterResult Compare(ImageStats a, ImageStats b);
    }

    public class ConvolutionResultFilter : IFilter
    {
        private readonly Func<int, bool> _isWithinThresholdFunc;
        private readonly Func<ImageStats, ConvolutionResult> _getConvolutionResult;
        private readonly string _name;

        public ConvolutionResultFilter(Func<int, bool> isWithinThresholdFunc, Func<ImageStats, ConvolutionResult> getConvolutionResult, string name)
        {
            _isWithinThresholdFunc = isWithinThresholdFunc;
            _getConvolutionResult = getConvolutionResult;
            _name = name;
        }

        public FilterResult Compare(ImageStats a, ImageStats b)
        {
            return new FilterResult(_isWithinThresholdFunc(_getConvolutionResult(a).Difference(_getConvolutionResult(b))), _name);
        }
    }

    public class CompoundFilter : IFilter
    {
        readonly IEnumerable<IFilter> _filters;
        private readonly string _name;

        public CompoundFilter(IEnumerable<IFilter> filters, string name)
        {
            _filters = filters;
            _name = name;
        }
        public FilterResult Compare(ImageStats a, ImageStats b)
        {
            var failedFilters = _filters
                       .Select(filter => filter.Compare(a, b))
                       .Where(result => !result.Passed);
            return failedFilters.Any() ? failedFilters.First() : new FilterResult(true, _name);
        }
    }

    public class CompoundFilterBuilder
    {
        private readonly string _name;
        readonly IList<IFilter> _filters = new List<IFilter>();

        public CompoundFilterBuilder(string name = null)
        {
            _name = name ?? Guid.NewGuid().ToString();
        }

        public CompoundFilterBuilder WithConvolutionResultFilter(Func<int, bool> isWithinThresholdFunc, Func<ImageStats, ConvolutionResult> getConvolutionResult, string name)
        {
            _filters.Add(new ConvolutionResultFilter(isWithinThresholdFunc, getConvolutionResult, name));
            return this;
        }

        public CompoundFilter Build()
        {
            return new CompoundFilter(_filters, _name);
        }
    }

    public struct FilterResult
    {
        public FilterResult(bool passed, string filterName)
        {
            Passed = passed;
            FilterName = filterName;
        }
        public bool Passed { get; }
        public string FilterName { get; }
    }

    [Serializable]
    public struct PhysicalImage
    {
        public PhysicalImage(string imagePath)
        {
            ImagePath = imagePath;
        }

        public string ImagePath { get; set; }
    }

    [Serializable]
    public struct ImageManipulationInfo
    {
        public ImageManipulationInfo(int startX, int startY, int width, int height)
        {
            StartX = startX;
            StartY = startY;
            Width = width;
            Height = height;
        }

        public int StartX { get; set; }
        public int StartY { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

    [Serializable]
    public struct ImageStats
    {
        public ConvolutionResult LowResIntensity { get; set; }
        public ConvolutionResult LowResR { get; set; }
        public ConvolutionResult LowResG { get; set; }
        public ConvolutionResult LowResB { get; set; }
        public ConvolutionResult MidResHorizontal { get; set; }
        public ConvolutionResult MidResVertical { get; set; }
        public ConvolutionResult MidRes45 { get; set; }
        public ConvolutionResult MidRes135 { get; set; }
    }

    [Serializable]
    public struct ConvolutionResult
    {
        public ConvolutionResult(IEnumerable<int> values)
        {
            Values = values.ToArray();
        }
        public int[] Values { get; set; }
    }

    public static class ConvolutionResultExtensions
    {
        public static int Difference(this ConvolutionResult a, ConvolutionResult b)
        {
            return a.Values.Difference(b.Values);
        }
    }

    public static class ArrayExtensions
    {
        public static int Difference(this int[] a, int[] b)
        {
            return (int)Math.Floor(a.Zip(b, (av, bv) => Abs(av - bv)).Average());
        }

        private static int Abs(int x)
        {
            return x > 0 ? x : -x;
        }
    }

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

    [Serializable]
    public struct SegmentAndStats
    {
        public SegmentAndStats(ImageManipulationInfo manipulationInfo, ImageStats stats)
        {
            ManipulationInfo = manipulationInfo;
            Stats = stats;
        }

        public ImageManipulationInfo ManipulationInfo { get; set; }
        public ImageStats Stats { get; set; }
    }

    public static class Serializer
    {
        /// <summary>
        /// Writes the given object instance to a binary file.
        /// <para>Object type (and all child types) must be decorated with the [Serializable] attribute.</para>
        /// <para>To prevent a variable from being serialized, decorate it with the [NonSerialized] attribute; cannot be applied to properties.</para>
        /// </summary>
        /// <typeparam name="T">The type of object being written to the binary file.</typeparam>
        /// <param name="filePath">The file path to write the object instance to.</param>
        /// <param name="objectToWrite">The object instance to write to the binary file.</param>
        /// <param name="append">If false the file will be overwritten if it already exists. If true the contents will be appended to the file.</param>
        public static void WriteToBinaryFile<T>(string filePath, T objectToWrite, bool append = false)
        {
            using (Stream stream = File.Open(filePath, append ? FileMode.Append : FileMode.Create))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                binaryFormatter.Serialize(stream, objectToWrite);
            }
        }

        /// <summary>
        /// Reads an object instance from a binary file.
        /// </summary>
        /// <typeparam name="T">The type of object to read from the binary file.</typeparam>
        /// <param name="filePath">The file path to read the object instance from.</param>
        /// <returns>Returns a new instance of the object read from the binary file.</returns>
        public static T ReadFromBinaryFile<T>(string filePath)
        {
            using (Stream stream = File.Open(filePath, FileMode.Open))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                return (T)binaryFormatter.Deserialize(stream);
            }
        }

        /// <summary>
        /// Writes the given object instance to an XML file.
        /// <para>Only Public properties and variables will be written to the file. These can be any type though, even other classes.</para>
        /// <para>If there are public properties/variables that you do not want written to the file, decorate them with the [XmlIgnore] attribute.</para>
        /// <para>Object type must have a parameterless constructor.</para>
        /// </summary>
        /// <typeparam name="T">The type of object being written to the file.</typeparam>
        /// <param name="filePath">The file path to write the object instance to.</param>
        /// <param name="objectToWrite">The object instance to write to the file.</param>
        /// <param name="append">If false the file will be overwritten if it already exists. If true the contents will be appended to the file.</param>
        public static void WriteToXmlFile<T>(string filePath, T objectToWrite, bool append = false) where T : new()
        {
            TextWriter writer = null;
            try
            {
                var serializer = new XmlSerializer(typeof(T));
                writer = new StreamWriter(filePath, append);
                serializer.Serialize(writer, objectToWrite);
            }
            finally
            {
                if (writer != null)
                    writer.Close();
            }
        }

        /// <summary>
        /// Reads an object instance from an XML file.
        /// <para>Object type must have a parameterless constructor.</para>
        /// </summary>
        /// <typeparam name="T">The type of object to read from the file.</typeparam>
        /// <param name="filePath">The file path to read the object instance from.</param>
        /// <returns>Returns a new instance of the object read from the XML file.</returns>
        public static T ReadFromXmlFile<T>(string filePath) where T : new()
        {
            TextReader reader = null;
            try
            {
                var serializer = new XmlSerializer(typeof(T));
                reader = new StreamReader(filePath);
                return (T)serializer.Deserialize(reader);
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
        }

        /// <summary>
        /// Writes the given object instance to a Json file.
        /// <para>Object type must have a parameterless constructor.</para>
        /// <para>Only Public properties and variables will be written to the file. These can be any type though, even other classes.</para>
        /// <para>If there are public properties/variables that you do not want written to the file, decorate them with the [JsonIgnore] attribute.</para>
        /// </summary>
        /// <typeparam name="T">The type of object being written to the file.</typeparam>
        /// <param name="filePath">The file path to write the object instance to.</param>
        /// <param name="objectToWrite">The object instance to write to the file.</param>
        /// <param name="append">If false the file will be overwritten if it already exists. If true the contents will be appended to the file.</param>
        public static void WriteToJsonFile<T>(string filePath, T objectToWrite, bool append = false) where T : new()
        {
            TextWriter writer = null;
            try
            {
                var contentsToWriteToFile = JsonConvert.SerializeObject(objectToWrite);
                writer = new StreamWriter(filePath, append);
                writer.Write(contentsToWriteToFile);
            }
            finally
            {
                if (writer != null)
                    writer.Close();
            }
        }

        /// <summary>
        /// Reads an object instance from an Json file.
        /// <para>Object type must have a parameterless constructor.</para>
        /// </summary>
        /// <typeparam name="T">The type of object to read from the file.</typeparam>
        /// <param name="filePath">The file path to read the object instance from.</param>
        /// <returns>Returns a new instance of the object read from the Json file.</returns>
        public static T ReadFromJsonFile<T>(string filePath) where T : new()
        {
            TextReader reader = null;
            try
            {
                reader = new StreamReader(filePath);
                var fileContents = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<T>(fileContents);
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
        }
    }

    [Serializable]
    public struct Alphabet
    {
        public Alphabet(ImageAndStats[] imagesAndStats)
        {
            ImagesAndStats = imagesAndStats;
        }

        public ImageAndStats[] ImagesAndStats { get; set; }
    }
}

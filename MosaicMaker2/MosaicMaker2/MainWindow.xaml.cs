using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FastBitmap;
using ImageStats;
using ImageStats.Stats;
using ImageStats.Utils;

namespace MosaicMaker2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel _mainWindowViewModel;

        public MainWindow()
        {
            InitializeComponent();
            _mainWindowViewModel = new MainWindowViewModel();
            DataContext = _mainWindowViewModel;
            next.Click += (sender, args) => _mainWindowViewModel.DumpStats();
            load.Click += (sender, args) => _mainWindowViewModel.LoadIndex();
            create.Click += (sender, args) => _mainWindowViewModel.CreateIndex();
            build.Click += (sender, args) => _mainWindowViewModel.ProduceImage();
        }
    }

    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private static readonly ObservableCollection<BitmapImage> ConvolutionObservableCollection = new ObservableCollection<BitmapImage>(new List<BitmapImage>());
        private static readonly ObservableCollection<BitmapImage> ConvolutionReducedObservableCollection = new ObservableCollection<BitmapImage>(new List<BitmapImage>());
        private static readonly ObservableCollection<BitmapImage> MatchesObservableCollection = new ObservableCollection<BitmapImage>(new List<BitmapImage>());
        private static readonly ObservableCollection<BitmapImage> RefinedMatchesObservableCollection = new ObservableCollection<BitmapImage>(new List<BitmapImage>());
        private static readonly ObservableCollection<BitmapImage> RefinedMatches2ObservableCollection = new ObservableCollection<BitmapImage>(new List<BitmapImage>());
        private readonly Class1 class1;
        private readonly IImageLoader Loader = new IncrediblyInefficientImageLoader();

        public MainWindowViewModel()
        {
            class1 = new Class1(Loader);
            class1.LoadSourceImage(@"C:\src\MosaicMaker2\20201224_214836_tiny.bmp");
            ConvolutionImages = new ReadOnlyObservableCollection<BitmapImage>(ConvolutionObservableCollection);
            ConvolutionReducedImages = new ReadOnlyObservableCollection<BitmapImage>(ConvolutionReducedObservableCollection);
            MatchingImages = new ReadOnlyObservableCollection<BitmapImage>(MatchesObservableCollection);
            RefinedMatchingImages = new ReadOnlyObservableCollection<BitmapImage>(RefinedMatchesObservableCollection);
            RefinedMatchingImages2 = new ReadOnlyObservableCollection<BitmapImage>(RefinedMatches2ObservableCollection);
        }

        public void LoadIndex() => class1.LoadIndex();
        public void CreateIndex() => class1.CreateIndex();

        public void DumpStats()
        {
            var img = class1.GetSourceImage();

            var randomSourceSegment = img.Segments.Random().First().ManipulationInfo;
            SourceImage = class1.GetBitmap(img.Image, randomSourceSegment).ToBitmapImage();
            OnPropertyChanged(nameof(SourceImage));

            // StatsGenerator statsGenerator = new StatsGenerator(Loader);
            // var convolutionImages = statsGenerator.GetMidResConvolutionAsBitmap(img.Image).Select(bm => bm.ToBitmapImage());
            // var convolutionReducedImages = statsGenerator.GetMidResConvolutionReducedAsBitmap(img.Image).Select(bm => bm.ToBitmapImage());

            ConvolutionObservableCollection.Clear();
            ConvolutionReducedObservableCollection.Clear();

            /*
            foreach (var convolutionImage in convolutionImages)
            {
                ConvolutionObservableCollection.Add(convolutionImage);
            }

            foreach (var convolutionImage in convolutionReducedImages)
            {
                ConvolutionReducedObservableCollection.Add(convolutionImage);
            }
            */

            /*
            var matchedImages = class1.CompareImageToAlphabet(img.Image, firstSegment)
                .Select(i => i.ToBitmapImage());
            MatchesObservableCollection.Clear();
            foreach (var matchedImage in matchedImages)
            {
                MatchesObservableCollection.Add(matchedImage);
            }
            */


            var refinedMatchedImages = class1.GetRefinedMatches(img.Image, randomSourceSegment)
                .Select(i => i.ToBitmapImage());
            RefinedMatchesObservableCollection.Clear();
            foreach (var matchedImage in refinedMatchedImages)
            {
                RefinedMatchesObservableCollection.Add(matchedImage);
            }

            var refinedMatchedImages2 = class1.GetRefinedMatches2(img.Image, randomSourceSegment)
                .Select(i => i.ToBitmapImage());
            RefinedMatches2ObservableCollection.Clear();
            foreach (var matchedImage in refinedMatchedImages2)
            {
                RefinedMatches2ObservableCollection.Add(matchedImage);
            }

            OnPropertyChanged(nameof(MatchingImages));
            OnPropertyChanged(nameof(RefinedMatchingImages));
            OnPropertyChanged(nameof(RefinedMatchingImages2));
        }

        public void ProduceImage()
        {
            var img = class1.GetSourceImage();
            FastBitmap.FastBitmap originalImage = Loader.LoadImage(img.Image.ImagePath);

            ConvolutionObservableCollection.Clear();
            ConvolutionReducedObservableCollection.Clear();


            var matches = new List<SourceAndMatch>();
            foreach (var segmentAndStatse in img.Segments)
            {
                // Get matches
                var refinedMatchedImages = class1.GetRefinedMatches2(originalImage, segmentAndStatse.ManipulationInfo);

                // Select a match
                // TODO: Do better
                // var bestMatch = refinedMatchedImages.SelectRandom();
                var bestMatch = refinedMatchedImages.First();

                var match = new SourceAndMatch(segmentAndStatse.ManipulationInfo, bestMatch);
                matches.Add(match);
            }

            var newImageBuilder = new ReconstructedImageBuilder(originalImage, 0.75);
            newImageBuilder.ApplyMatches(matches);
            newImageBuilder.SaveAs(@"c:\src\MosaicMaker2\result.bmp");
        }

        private void DumpArr(int[] arr)
        {
            Console.WriteLine(string.Join(", ", arr));
        }

        public ReadOnlyObservableCollection<BitmapImage> ConvolutionImages { get; set; }
        public ReadOnlyObservableCollection<BitmapImage> ConvolutionReducedImages { get; set; }
        public ReadOnlyObservableCollection<BitmapImage> MatchingImages { get; set; }
        public ReadOnlyObservableCollection<BitmapImage> RefinedMatchingImages { get; set; }
        public ReadOnlyObservableCollection<BitmapImage> RefinedMatchingImages2 { get; set; }
        public BitmapImage SourceImage { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public static class BitmapExtensions
    {
        public static BitmapImage ToBitmapImage(this Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }
    }


}

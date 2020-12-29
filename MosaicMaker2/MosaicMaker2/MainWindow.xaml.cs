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
        }
    }

    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private static readonly ObservableCollection<BitmapImage> ConvolutionObservableCollection = new ObservableCollection<BitmapImage>(new List<BitmapImage>());
        private static readonly ObservableCollection<BitmapImage> ConvolutionReducedObservableCollection = new ObservableCollection<BitmapImage>(new List<BitmapImage>());
        private static readonly ObservableCollection<BitmapImage> MatchesObservableCollection = new ObservableCollection<BitmapImage>(new List<BitmapImage>());
        private readonly Class1 class1;
        private readonly IImageLoader Loader = new IncrediblyInefficientImageLoader();

        public MainWindowViewModel()
        {
            class1 = new Class1(Loader);
            ConvolutionImages = new ReadOnlyObservableCollection<BitmapImage>(ConvolutionObservableCollection);
            ConvolutionReducedImages = new ReadOnlyObservableCollection<BitmapImage>(ConvolutionReducedObservableCollection);
            MatchingImages = new ReadOnlyObservableCollection<BitmapImage>(MatchesObservableCollection);
        }

        public void LoadIndex() => class1.LoadIndex();
        public void CreateIndex() => class1.CreateIndex();

        public void DumpStats()
        {
            var img = class1.GetRandom();

            var firstSegment = new ImageManipulationInfo(0, 0, 40, 30);
            SourceImage = class1.GetBitmap(img.Image, firstSegment).ToBitmapImage();
            OnPropertyChanged(nameof(SourceImage));

            StatsGenerator statsGenerator = new StatsGenerator(Loader);
            var convolutionImages = statsGenerator.GetMidResConvolutionAsBitmap(img.Image).Select(bm => bm.ToBitmapImage());
            var convolutionReducedImages = statsGenerator.GetMidResConvolutionReducedAsBitmap(img.Image).Select(bm => bm.ToBitmapImage());

            ConvolutionObservableCollection.Clear();
            ConvolutionReducedObservableCollection.Clear();

            foreach (var convolutionImage in convolutionImages)
            {
                ConvolutionObservableCollection.Add(convolutionImage);
            }

            foreach (var convolutionImage in convolutionReducedImages)
            {
                ConvolutionReducedObservableCollection.Add(convolutionImage);
            }

            var matchedImages = class1.CompareImageToAlphabet(img.Image, firstSegment)
                .Select(i => i.ToBitmapImage());
            MatchesObservableCollection.Clear();
            foreach (var matchedImage in matchedImages)
            {
                MatchesObservableCollection.Add(matchedImage);
            }
            OnPropertyChanged(nameof(MatchingImages));
        }

        private void DumpArr(int[] arr)
        {
            Console.WriteLine(string.Join(", ", arr));
        }

        public ReadOnlyObservableCollection<BitmapImage> ConvolutionImages { get; set; }
        public ReadOnlyObservableCollection<BitmapImage> ConvolutionReducedImages { get; set; }
        public ReadOnlyObservableCollection<BitmapImage> MatchingImages { get; set; }
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

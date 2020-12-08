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
using ImageStats;

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
        }
    }

    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private static readonly ObservableCollection<BitmapImage> ConvolutionObservableCollection = new ObservableCollection<BitmapImage>(new List<BitmapImage>());
        private static readonly ObservableCollection<BitmapImage> MatchesObservableCollection = new ObservableCollection<BitmapImage>(new List<BitmapImage>());
        private static readonly Class1 class1 = new Class1();

        public MainWindowViewModel()
        {
            ConvolutionImages = new ReadOnlyObservableCollection<BitmapImage>(ConvolutionObservableCollection);
            MatchingImages = new ReadOnlyObservableCollection<BitmapImage>(MatchesObservableCollection);
        }

        public void DumpStats()
        {
            var img = class1.GetRandom();
            
            DumpArr(img.Stats.LowResR.Values);
            DumpArr(img.Stats.LowResG.Values);
            DumpArr(img.Stats.LowResB.Values);
            DumpArr(img.Stats.LowResIntensity.Values);

            SourceImage = class1.GetBitmap(img.Image).ToBitmapImage();
            OnPropertyChanged(nameof(SourceImage));

            var convolutionImages = class1.GetMidResConvolution(img.Image).Select(bm => bm.ToBitmapImage());
            ConvolutionObservableCollection.Clear();
            foreach (var convolutionImage in convolutionImages)
            {
                ConvolutionObservableCollection.Add(convolutionImage);
            }

            var matchedImages = class1.CompareImageToAlphabet(img.Image, new ImageManipulationInfo(0, 0, 40, 30))
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

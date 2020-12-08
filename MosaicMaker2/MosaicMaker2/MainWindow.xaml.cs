using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
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
        public MainWindow()
        {
            InitializeComponent();
            var vm = new MainWindowViewModel();
            DataContext = vm;
            vm.DumpStats();
        }
    }

    public class MainWindowViewModel
    {
        public void DumpStats()
        {
            var sourceImg = @"C:\src\MosaicMaker2\Alphabet\2004_Photos\100_0052.bmp";
            Class1 class1 = new Class1();
            var physicalImage = new PhysicalImage(sourceImg);

            var imageStats = class1.GetStats(physicalImage, new ImageManipulationInfo(0, 0, 40, 30));
            DumpArr(imageStats.LowResR.Values);
            DumpArr(imageStats.LowResG.Values);
            DumpArr(imageStats.LowResB.Values);
            DumpArr(imageStats.LowResIntensity.Values);

//            class1.CompareImageToAlphabet(physicalImage, new ImageManipulationInfo(0, 0, 40, 30));

            var edge = class1.GetMidResConvolution(physicalImage);
//            edge.Save(@"C:\src\MosaicMaker2.png");
            EdgeDetectedBitmap = edge.ToBitmapImage();

        }

        private void DumpArr(int[] arr)
        {
            Console.WriteLine(string.Join(", ", arr));
        }

        public BitmapImage EdgeDetectedBitmap { get; set; }


        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        private BitmapImage Bitmap2BitmapImage(Bitmap bitmap)
        {
            IntPtr hBitmap = bitmap.GetHbitmap();
            BitmapImage retval;

            try
            {
                retval = (BitmapImage)Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                DeleteObject(hBitmap);
            }

            return retval;
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

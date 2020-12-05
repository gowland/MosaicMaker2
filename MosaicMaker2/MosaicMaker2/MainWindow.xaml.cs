using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
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
            var imageStats = class1.GetStats(new PhysicalImage(sourceImg), new ImageManipulationInfo(0, 0, 40, 30));
            DumpArr(imageStats.LowResR.Values);
            DumpArr(imageStats.LowResG.Values);
            DumpArr(imageStats.LowResB.Values);
            DumpArr(imageStats.LowResIntensity.Values);
            class1.CompareImageToAlphabet(new PhysicalImage(sourceImg), new ImageManipulationInfo(0, 0, 40, 30));
        }

        private void DumpArr(int[] arr)
        {
            Console.WriteLine(string.Join(", ", arr));
        }
    }
}

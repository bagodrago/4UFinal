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
using System.IO;

namespace _4UFinal
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // <RO Variables> - Read-only variables
        List<BitmapImage> assets = new List<BitmapImage>() { }; // Static assets (do not change during the game)
        List<BitmapImage> portraits = new List<BitmapImage>() { }; // Character portraits for text box
        List<BitmapImage> items = new List<BitmapImage>() { }; // Item portraits for text box
        // </RO Variables>

        // <RW Variables> - Read and write variables
        public bool isFullscreen = false; // Is F11 mode is on?
        public bool changingFullscreen = false; // Is F11 currently changing?
        ImageBrush bk = new ImageBrush();
        // </RW Variables>

        public MainWindow()
        {
            InitializeComponent();
            // <loadFiles>
            assets = LoadImages(@".\img\assets");
            portraits = LoadImages(@".\img\portraits");
            items = LoadImages(@".\img\items");
            // </loadFiles>
            // <tiling> - Creates tiling background
            bk = new ImageBrush(assets[0]);
            bk.TileMode = TileMode.Tile;
            bk.Stretch = Stretch.None;
            bk.Viewport = new Rect(0, 0, 32, 32);
            bk.ViewportUnits = BrushMappingMode.Absolute;
            OuterScreen.Background = bk;
            // </tiling>
            // <loadAssets>
            TextCanvas.Background = new ImageBrush(assets[1]);
            TextPortrait.Source = items[0];
            // </loadAssets>
        }

        private List<BitmapImage> LoadImages(string path) // Loads images from a specified folder and returns a list of ImageBrushs
        {
            string[] temp = Directory.GetFiles(path, "*.png");
            Array.Sort(temp);
            List<BitmapImage> container = new List<BitmapImage>();
            foreach (string file in temp) { container.Add(new BitmapImage(new Uri(System.IO.Path.Combine(Environment.CurrentDirectory, file)))); }
            return container;
        }

        private void OuterScreen_KeyDown(object sender, KeyEventArgs e) // Activates and deactivates F11 Mode
        {
            if (e.Key == Key.F11 && !changingFullscreen)
            {
                changingFullscreen = true;
                switch (isFullscreen)
                {
                    case true:
                        WindowStyle = WindowStyle.SingleBorderWindow;
                        WindowState = WindowState.Maximized;
                        ResizeMode = ResizeMode.CanResize;
                        isFullscreen = false;
                        break;
                    default:
                        WindowStyle = WindowStyle.None;
                        WindowState = WindowState.Normal;
                        WindowState = WindowState.Maximized;
                        ResizeMode = ResizeMode.NoResize;
                        isFullscreen = true;
                        break;
                }
                changingFullscreen = false;
            }
        }
    }
}

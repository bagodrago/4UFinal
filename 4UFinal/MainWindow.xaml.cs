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

namespace _4UFinal
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ImageBrush bk = new ImageBrush(new BitmapImage(new Uri(System.IO.Path.Combine(Environment.CurrentDirectory, @".\img\dump\thin.png")))); //background scroll

        public MainWindow()
        {
            InitializeComponent();
            bk.TileMode = TileMode.Tile;
            bk.Stretch = Stretch.None;
            bk.Viewport = new Rect(0, 0, 32, 32);
            bk.ViewportUnits = BrushMappingMode.Absolute;
            OuterScreen.Background = bk;
        }
    }
}

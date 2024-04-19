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

namespace Rasterization2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public class Shape
    {
        public double strokeThickness;
        public Color strokeColor;
        public bool isSelected;

        public virtual void Draw(WriteableBitmap writeableBitmap) { }
        public virtual List<Point> Points() { return new List<Point>(); }
    }
    public class Line : Shape
    {
        public Point start;
        public Point end;

        public Line(Point start, Point end, double strokeThickness, Color strokeColor)
        {
            this.start = start;
            this.end = end;
            this.strokeThickness = strokeThickness;
            this.strokeColor = strokeColor;
        }

        public override void Draw(WriteableBitmap writeableBitmap)
        {
            DrawLine(writeableBitmap, start, end);
        }

        public List<Point> DrawLine(WriteableBitmap? writeableBitmap, Point start, Point end)
        {
            List<Point> points = new List<Point>();
            return points;

        }
        public override List<Point> Points()
        {
            return DrawLine(null, start, end);
        }
    }

    public partial class MainWindow : Window
    {
        private WriteableBitmap bitmap;
        public MainWindow()
        {
            InitializeComponent();
        }
        private void window_ContentRendered(object sender, EventArgs e)
        {
            int heigth = (int)imageControlGrid.ActualHeight;
            int width = (int)imageControlGrid.ActualWidth;

            //96 is the default DPI for WPF
            bitmap = new WriteableBitmap(width, heigth, 96, 96, PixelFormats.Bgra32, null);
            byte[] colorWhite = { 255, 255, 255, 255 };
            paintBackground(bitmap,colorWhite);
            imageControl.Source = bitmap;
        }

        public void paintBackground(WriteableBitmap bitmap, byte[] color)
        {
            /*            int stride = bitmap.PixelWidth * (bitmap.Format.BitsPerPixel / 8);
                        byte[] pixels = new byte[bitmap.PixelHeight * stride];

                        // Set all pixels to white (255, 255, 255, 255)
                        for (int i = 0; i < pixels.Length; i += 4)
                        {
                            pixels[i] = color[0];     // Blue component
                            pixels[i + 1] = color[1]; // Green component
                            pixels[i + 2] = color[2]; // Red component
                            pixels[i + 3] = color[3]; // Alpha component
                        }
                        bitmap.WritePixels(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight), pixels, stride, 0, 0);*/
            int stride = bitmap.PixelWidth * (bitmap.Format.BitsPerPixel / 8);
            for (int i = 0; i < bitmap.PixelHeight; i++)
            {
                for (int j = 0; j < bitmap.PixelWidth; j++)
                {
                    bitmap.WritePixels(new Int32Rect(j, i, 1, 1), color, 4, 0);
                }
            }
        }

        private void buttonClear_Click(object sender, RoutedEventArgs e)
        {
            paintBackground(bitmap, new byte[] { 0, 255, 255, 255 });
        }
    }
}

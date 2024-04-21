using System;
using System.Collections.Generic;
using System.Drawing;
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
using Point = System.Windows.Point;
using Color = System.Drawing.Color;

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
        public virtual List<Point> GetPoints() { return new List<Point>(); }
        public void putPixel(WriteableBitmap writeableBitmap, int x, int y, Color color)
        {
            writeableBitmap.WritePixels(new Int32Rect(x, y, 1, 1), new byte[] { color.R, color.G, color.B, color.A }, 4, 0);
        }
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
            if(start.X == end.X && start.Y == end.Y)
            {
                putPixel(writeableBitmap, (int)start.X, (int)start.Y, strokeColor);
                return new List<Point>() { start };
            }
            
            List<Point> points = new List<Point>();
            putPixel(writeableBitmap, (int)start.X, (int)start.Y, strokeColor);
            putPixel(writeableBitmap, (int)end.X, (int)end.Y, strokeColor);
            int xf = start.X <= end.X ? (int)start.X : (int)end.X;
            int yf = start.X <= end.X ? (int)start.Y : (int)end.Y;

            int xb = start.X <= end.X ? (int)end.X : (int)start.X;
            int yb = start.X <= end.X ? (int)end.Y : (int)start.Y;


            int dx = (int)(xb - xf);
            int dy = (int)(yb - yf);



            if (xf <= xb && yf <= yb && Math.Abs(dy) <= Math.Abs(dx)) // E - NE
            {
                int d = 2 * dy - dx;
                int dE = 2 * dy;
                int dNE = 2 * (dy - dx);
                while (xf < xb)
                {
                    ++xf; --xb;
                    if (d < 0)
                        d += dE;
                    else
                    {
                        d += dNE;
                        ++yf; --yb;
                    }
                    putPixel(writeableBitmap, xf, yf, strokeColor); // Plot pixel in the forward direction
                    putPixel(writeableBitmap, xb, yb, strokeColor); // Plot pixel in the backward direction
                }
            }
            else if (xf <= xb && yf <= yb && Math.Abs(dy) > Math.Abs(dx)) // N - NE
            {
                int d = 2 * dx - dy;
                int dN = 2 * dx;
                int dNE = 2 * (dx - dy);
                while (yf < yb)
                {
                    ++yf; --yb;
                    if (d < 0)
                        d += dN;
                    else
                    {
                        d += dNE;
                        ++xf; --xb;
                    }
                    putPixel(writeableBitmap, xf, yf, strokeColor); // Plot pixel in the N direction
                    putPixel(writeableBitmap, xb, yb, strokeColor); // Plot pixel in the NE direction
                }
            }
            else if (xf <= xb && yf >= yb && Math.Abs(dy) <= Math.Abs(dx)) // E - ES
            {
                int d = 2 * Math.Abs(dy) - dx;
                int dE = 2 * Math.Abs(dy);
                int dNE = 2 * (Math.Abs(dy) - dx);
                while (xf < xb)
                {
                    ++xf; --xb;
                    if (d < 0)
                        d += dE;
                    else
                    {
                        d += dNE;
                        --yf; ++yb;
                    }
                    putPixel(writeableBitmap, xf, yf, strokeColor); // Plot pixel in the forward direction
                    putPixel(writeableBitmap, xb, yb, strokeColor); // Plot pixel in the backward direction
                }
            }
            else if (xf <= xb && yf >= yb && Math.Abs(dy) > Math.Abs(dx)) // ES - S
            {
                int d = 2 * dx - Math.Abs(dy);
                int dE = 2 * Math.Abs(dx);
                int dNE = 2 * (Math.Abs(dx) - Math.Abs(dy));
                while (yf > yb)
                {
                    --yf; ++yb;
                    if (d < 0)
                        d += dE;
                    else
                    {
                        d += dNE;
                        ++xf; --xb;
                    }
                    putPixel(writeableBitmap, xf, yf, strokeColor); // Plot pixel in the forward direction
                    putPixel(writeableBitmap, xb, yb, strokeColor); // Plot pixel in the backward direction
                }
            }


            return points;

        }
        public override List<Point> GetPoints()
        {
            return DrawLine(null, start, end);
        }
    }

    public partial class MainWindow : Window
    {
        private WriteableBitmap bitmap;
        private enum DrawMode { Line, Circle, Rectangle, Polygon, None };
        private enum DrawState { Start, Drawing, End };
        private DrawMode drawMode;
        private DrawState drawState;
        private Dictionary<int, Shape> shapes;
        private Dictionary<Point, int> points;

        public MainWindow()
        {
            InitializeComponent();
            drawMode = DrawMode.None;
            drawState = DrawState.Start;
            shapes = new Dictionary<int, Shape>();
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
            int stride = bitmap.PixelWidth * (bitmap.Format.BitsPerPixel / 8);
            byte[] pixels = new byte[bitmap.PixelHeight * stride];

            // Set all pixels to white (255, 255, 255, 255)
            for (int i = 0; i < pixels.Length; i += 4)
            {
                pixels[i] = color[0];     // Blue component
                pixels[i + 1] = color[1]; // Green component
                pixels[i + 2] = color[2]; // Red component
                pixels[i + 3] = color[3]; // Alpha component
            }
            bitmap.WritePixels(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight), pixels, stride, 0, 0);
/*            int stride = bitmap.PixelWidth * (bitmap.Format.BitsPerPixel / 8);
            for (int i = 0; i < bitmap.PixelHeight; i++)
            {
                for (int j = 0; j < bitmap.PixelWidth; j++)
                {
                    bitmap.WritePixels(new Int32Rect(j, i, 1, 1), color, 4, 0);
                }
            }*/
        }

        private void buttonClear_Click(object sender, RoutedEventArgs e)
        {
            paintBackground(bitmap, new byte[] { 255, 255, 255, 255 });
        }

        private void buttonLine_Click(object sender, RoutedEventArgs e)
        {
            if (drawMode != DrawMode.Line)
            {
                drawMode = DrawMode.Line;
            }
            else
            {
                drawMode = DrawMode.None;
            }

        }

        private void imageControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (drawMode == DrawMode.Line)
            {
                if (drawState == DrawState.Start)
                {
                    Point p = e.GetPosition(imageControl);
                    Line line = new Line(p, p, 1, Color.Black);
                    shapes.Add(shapes.Count, line);
                    drawState = DrawState.Drawing;
                }
                else if (drawState == DrawState.Drawing)
                {
                    Point p = e.GetPosition(imageControl);
                    Line line = (Line)shapes[shapes.Count - 1];
                    line.end = p;
                    shapes[shapes.Count - 1] = line;
                    line.Draw(bitmap);
                    drawState = DrawState.Start;
                }
                else if (drawState == DrawState.End)
                {
                    drawState = DrawState.Start;
                }
            }
        }

        private void imageControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void imageControl_MouseMove(object sender, MouseEventArgs e)
        {

        }
    }
}

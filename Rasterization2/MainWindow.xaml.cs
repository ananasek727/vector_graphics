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
using System.Collections;
using System.Windows.Controls.Primitives;
using System.Diagnostics.Eventing.Reader;

namespace Rasterization2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public abstract class Shape
    {
        public double strokeThickness;
        public Color strokeColor;
        public bool isSelected;

        public abstract void Draw(WriteableBitmap writeableBitmap);
        public abstract List<Point> GetPoints();
        public void putPixel(WriteableBitmap writeableBitmap, int x, int y, Color color)
        {
            try
            {
                writeableBitmap.WritePixels(new Int32Rect(x, y, 1, 1), new byte[] { color.R, color.G, color.B, color.A }, 4, 0);
            }
            catch (Exception e)
            {

            }
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
            if(writeableBitmap != null)
            {
                putPixel(writeableBitmap, (int)start.X, (int)start.Y, strokeColor);
                putPixel(writeableBitmap, (int)end.X, (int)end.Y, strokeColor);
            }
            points.Add(start);
            points.Add(end);

            //algorithm below only works for lines where start.X <= end.X so we need to check and swap the points if needed
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
                    if (writeableBitmap != null)
                    {
                        putPixel(writeableBitmap, xf, yf, strokeColor);
                        putPixel(writeableBitmap, xb, yb, strokeColor);
                    }
                    points.Add(new Point(xf, yf));
                    points.Add(new Point(xb, yb));
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
                    if (writeableBitmap != null)
                    {
                        putPixel(writeableBitmap, xf, yf, strokeColor);
                        putPixel(writeableBitmap, xb, yb, strokeColor);
                    }
                    points.Add(new Point(xf, yf));
                    points.Add(new Point(xb, yb));
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
                    if (writeableBitmap != null)
                    {
                        putPixel(writeableBitmap, xf, yf, strokeColor);
                        putPixel(writeableBitmap, xb, yb, strokeColor);
                    }
                    points.Add(new Point(xf, yf));
                    points.Add(new Point(xb, yb));
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
                    if (writeableBitmap != null)
                    {
                        putPixel(writeableBitmap, xf, yf, strokeColor);
                        putPixel(writeableBitmap, xb, yb, strokeColor);
                    }
                    points.Add(new Point(xf, yf));
                    points.Add(new Point(xb, yb));
                }
            }
            else
            {
                throw new Exception("Invalid line coordinates");
            }

            return points;

        }
        public override List<Point> GetPoints()
        {
            return DrawLine(null, start, end);
        }
    }
    public class Circle : Shape
    {
        public Point center;
        public double radius;

        public Circle(Point center, double radius, double strokeThickness, Color strokeColor)
        {
            this.center = center;
            this.radius = radius;
            this.strokeThickness = strokeThickness;
            this.strokeColor = strokeColor;
        }

        public override void Draw(WriteableBitmap writeableBitmap)
        {
            DrawCircle(writeableBitmap, center, radius);
        }

        public List<Point> DrawCircle(WriteableBitmap? writeableBitmap, Point center, double radius)
        {
            List<Point> points = new List<Point>();
            if (radius == 0)
            {
                putPixel(writeableBitmap, (int)center.X, (int)center.Y, strokeColor);
                return new List<Point>() { center };
            }
            int dE = 3;
            int dSE = (int)(5 - 2 * radius);
            int d = (int)(1 - radius);
            int x = 0;
            int y = (int)radius;
            if (writeableBitmap != null)
            {
                putPixel(writeableBitmap, (int)center.X + x, (int)center.Y + y, strokeColor);
                putPixel(writeableBitmap, (int)center.X + x, (int)center.Y - y, strokeColor);
                putPixel(writeableBitmap, (int)center.X - x, (int)center.Y + y, strokeColor);
                putPixel(writeableBitmap, (int)center.X - x, (int)center.Y - y, strokeColor);
                putPixel(writeableBitmap, (int)center.X + y, (int)center.Y + x, strokeColor);
                putPixel(writeableBitmap, (int)center.X + y, (int)center.Y - x, strokeColor);
                putPixel(writeableBitmap, (int)center.X - y, (int)center.Y + x, strokeColor);
                putPixel(writeableBitmap, (int)center.X - y, (int)center.Y - x, strokeColor);
            }
            points.Add(new Point((int)center.X + x, (int)center.Y + y));
            points.Add(new Point((int)center.X + x, (int)center.Y - y));
            points.Add(new Point((int)center.X - x, (int)center.Y + y));
            points.Add(new Point((int)center.X - x, (int)center.Y - y));
            points.Add(new Point((int)center.X + y, (int)center.Y + x));
            points.Add(new Point((int)center.X + y, (int)center.Y - x));
            points.Add(new Point((int)center.X - y, (int)center.Y + x));
            points.Add(new Point((int)center.X - y, (int)center.Y - x));

            while (y > x)
            {
                if (d < 0) //move to E
                {
                    d += dE;
                    dE += 2;
                    dSE += 2;
                }
                else //move to SE
                {
                    d += dSE;
                    dE += 2;
                    dSE += 4;
                    --y;
                }
                ++x;
                if (writeableBitmap != null)
                {
                    putPixel(writeableBitmap, (int)center.X + x, (int)center.Y + y, strokeColor);
                    putPixel(writeableBitmap, (int)center.X + x, (int)center.Y - y, strokeColor);
                    putPixel(writeableBitmap, (int)center.X - x, (int)center.Y + y, strokeColor);
                    putPixel(writeableBitmap, (int)center.X - x, (int)center.Y - y, strokeColor);
                    putPixel(writeableBitmap, (int)center.X + y, (int)center.Y + x, strokeColor);
                    putPixel(writeableBitmap, (int)center.X + y, (int)center.Y - x, strokeColor);
                    putPixel(writeableBitmap, (int)center.X - y, (int)center.Y + x, strokeColor);
                    putPixel(writeableBitmap, (int)center.X - y, (int)center.Y - x, strokeColor);
                }
                points.Add(new Point((int)center.X + x, (int)center.Y + y));
                points.Add(new Point((int)center.X + x, (int)center.Y - y));
                points.Add(new Point((int)center.X - x, (int)center.Y + y));
                points.Add(new Point((int)center.X - x, (int)center.Y - y));
                points.Add(new Point((int)center.X + y, (int)center.Y + x));
                points.Add(new Point((int)center.X + y, (int)center.Y - x));
                points.Add(new Point((int)center.X - y, (int)center.Y + x));
                points.Add(new Point((int)center.X - y, (int)center.Y - x));
            }
            return points;
        }

        public override List<Point> GetPoints()
        {
            return DrawCircle(null, center, radius);
        }
    }
    public class Polygons : Shape
    {
        public Point start;
        public List<Point> vertives;

        public Polygons(Point start, List<Point> vertives, double strokeThickness, Color strokeColor)
        {
            this.start = start;
            this.vertives = vertives;
            this.strokeThickness = strokeThickness;
            this.strokeColor = strokeColor;
        }
        public override void Draw(WriteableBitmap writeableBitmap)
        {
            DrawPolygon(writeableBitmap, start, vertives);
        }

        private void DrawPolygon(WriteableBitmap? writeableBitmap, Point start, List<Point> vertives)
        {
            List<Point> points = new List<Point>();
            if (vertives.Count == 1)
            {
                if(writeableBitmap != null)
                {
                    putPixel(writeableBitmap, (int)start.X, (int)start.Y, strokeColor);
                }
                return;
            }
            Line tmp;
            for (int i = 0; i < vertives.Count - 1; i++)
            {
                tmp = new Line(vertives[i], vertives[i+1], strokeThickness, strokeColor);
                if (writeableBitmap != null)
                {
                    tmp.Draw(writeableBitmap);
                }
                List<Point> points2 = tmp.GetPoints();
            }
/*            tmp = new Line(start, vertives.Last<Point>(), strokeThickness, strokeColor);
            if (writeableBitmap != null)
            {
                tmp.Draw(writeableBitmap);
            }*/
        }

        public override List<Point> GetPoints()
        {
            return vertives;
        }

        //return true if the polygon is closed
        public bool DrawPolygonLineByLine(WriteableBitmap writeableBitmap)
        {
            if(Math.Sqrt(Math.Pow(vertives.Last().X - start.X,2) + Math.Pow(vertives.Last().Y - start.Y, 2)) < 10)
            {
                vertives.Remove(vertives.Last());
                Line tmp = new Line(vertives.Last(), start, strokeThickness, strokeColor);
                tmp.Draw(writeableBitmap);
                return true;
            }
            else
            {
                Line tmp = new Line(vertives[vertives.Count-2], vertives[vertives.Count-1], strokeThickness, strokeColor);
                tmp.Draw(writeableBitmap);
                return false;
            }

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
        private Color currentColor;

        public MainWindow()
        {
            InitializeComponent();
            drawMode = DrawMode.None;
            drawState = DrawState.Start;
            shapes = new Dictionary<int, Shape>();
            points = new Dictionary<Point, int>();
            currentColor = Color.Black;
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
                uncheckedAllOtherButtons(sender);
            }
            else
            {
                drawMode = DrawMode.None;
                drawState = DrawState.Start;
            }
        }
        private void buttonCircle_Click(object sender, RoutedEventArgs e)
        {
            if (drawMode != DrawMode.Circle)
            {
                drawMode = DrawMode.Circle;
                uncheckedAllOtherButtons(sender);
            }
            else
            {
                drawMode = DrawMode.None;
                drawState = DrawState.Start;
            }
        }
        private void buttonPolygon_Click(object sender, RoutedEventArgs e)
        {
            if (drawMode != DrawMode.Polygon)
            {
                drawMode = DrawMode.Polygon;
                uncheckedAllOtherButtons(sender);
            }
            else
            {
                drawMode = DrawMode.None;
                drawState = DrawState.Start;
            }
        }

        private void imageControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (drawMode == DrawMode.Line)
            {
                if (drawState == DrawState.Start)
                {
                    Point p = e.GetPosition(imageControl);
                    Line line = new Line(p, p, 1, currentColor);
                    shapes.Add(shapes.Count, line);
                    drawState = DrawState.Drawing;
                }
                else if (drawState == DrawState.Drawing)
                {
                    Point p = e.GetPosition(imageControl);
                    Line line = (Line)shapes[shapes.Count - 1];
                    line.end = p;
                    shapes[shapes.Count - 1] = line;

                    List<Point> points = line.GetPoints();
                    foreach (Point point in points)
                    {
                        this.points.TryAdd(point, shapes.Count - 1);
                    }
                    line.Draw(bitmap);
                    drawState = DrawState.Start;

                }
                else if (drawState == DrawState.End)
                {
                    drawState = DrawState.Start;
                }
            }
            else if (drawMode == DrawMode.Circle)
            {
                if (drawState == DrawState.Start)
                {
                    Point p = e.GetPosition(imageControl);
                    Circle circle = new Circle(p, 0, 1, currentColor);
                    shapes.Add(shapes.Count, circle);
                    drawState = DrawState.Drawing;
                }
                else if (drawState == DrawState.Drawing)
                {
                    Point p = e.GetPosition(imageControl);
                    Circle circle = (Circle)shapes[shapes.Count - 1];
                    circle.radius = Math.Sqrt(Math.Pow(circle.center.X - p.X, 2) + Math.Pow(circle.center.Y - p.Y, 2));
                    shapes[shapes.Count - 1] = circle;

                    List<Point> points = circle.GetPoints();
                    foreach (Point point in points)
                    {
                        this.points.TryAdd(point, shapes.Count - 1);
                    }
                    circle.Draw(bitmap);
                    drawState = DrawState.Start;
                }
                else if (drawState == DrawState.End)
                {
                    drawState = DrawState.Start;
                }
            }
            else if (drawMode == DrawMode.Polygon)
            {
                if (drawState == DrawState.Start)
                {
                    Point p = e.GetPosition(imageControl);
                    Polygons polygon = new Polygons(p, new List<Point>() { p }, 1, currentColor);
                    shapes.Add(shapes.Count, polygon);
                    drawState = DrawState.Drawing;
                }
                else if (drawState == DrawState.Drawing)
                {
                    Point p = e.GetPosition(imageControl);
                    Polygons polygon = (Polygons)shapes[shapes.Count - 1];
                    polygon.vertives.Add(p);
                    shapes[shapes.Count - 1] = polygon;

                    List<Point> points = polygon.GetPoints();
                    foreach (Point point in points)
                    {
                        this.points.TryAdd(point, shapes.Count - 1);
                    }
                    bool isClosed = polygon.DrawPolygonLineByLine(bitmap);
                    if (isClosed)
                    {
                        drawState = DrawState.Start;
                    }
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
        private void uncheckedAllOtherButtons(object button)
        {
            ToggleButton? toggleButton = button as ToggleButton;
            if (toggleButton != null)
            {
                buttonLine.IsChecked = false;
                buttonCircle.IsChecked = false;
                toggleButton.IsChecked = true;
            }
        }
    }
}

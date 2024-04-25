﻿using System;
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
using System.Windows.Ink;
using System.Data;

namespace Rasterization2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    ///
    static class Constants
    {
        public static Color backgroundColor = Color.FromArgb(255, 255, 255, 255);
    }



    public abstract class Shape
    {
        public double strokeThickness;
        public Color strokeColor;
        public bool isSelected;
        public bool Xiaolin_Wu;

        public abstract void Draw(WriteableBitmap writeableBitmap);
        public abstract List<Point> GetPoints();
        public abstract void Move(Vector vector);
        public void putPixel(WriteableBitmap writeableBitmap, int x, int y, Color color)
        {
            try
            {
                if(x < 0 || x >= writeableBitmap.PixelWidth || y < 0 || y >= writeableBitmap.PixelHeight)
                {
                    return;
                }
                writeableBitmap.WritePixels(new Int32Rect(x, y, 1, 1), new byte[] {color.B, color.G, color.R, color.A}, 4, 0);
            }
            catch (Exception e)
            {

            }
        }
        public int Clamp(int value, int min, int max)
        {
            return Math.Max(min, Math.Min(max, value));
        }
    }
    public class Line : Shape
    {
        public Point start;
        public Point end;

        public Line(Point start, Point end, double strokeThickness, Color strokeColor, bool Xiaolin_Wu)
        {
            this.start = start;
            this.end = end;
            this.strokeColor = strokeColor;
            this.Xiaolin_Wu = Xiaolin_Wu;
            if (Xiaolin_Wu)
            {
                this.strokeThickness = 1;
            }
            else
            {
                this.strokeThickness = strokeThickness;
            }
        }

        public override void Draw(WriteableBitmap writeableBitmap)
        {
            if (Xiaolin_Wu)
            {
                XiaolinWu(writeableBitmap, start, end);
            }
            else
            {
                DrawLine(writeableBitmap, start, end);
            }
        }

        public List<Point> DrawLine(WriteableBitmap? writeableBitmap, Point start, Point end)
        {
            if(start.X == end.X && start.Y == end.Y)
            {
                if (writeableBitmap != null)
                {
                    putPixel(writeableBitmap, (int)start.X, (int)start.Y, strokeColor);
                    return new List<Point>() { start };
                }
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
            if (strokeThickness > 1)
            {
                dx = (int)(end.X - start.X);
                dy = (int)(end.Y - start.Y);
                if (Math.Abs(dx) <= Math.Abs(dy))
                {
                    new List<Point>(points).ForEach(point =>
                    {
                        for (int i = 1; i < strokeThickness / 2; i++)
                        {
                            if (writeableBitmap != null)
                            {
                                putPixel(writeableBitmap, Clamp((int)point.X + i, 0, writeableBitmap.PixelWidth), (int)point.Y, strokeColor);
                                putPixel(writeableBitmap, Clamp((int)point.X - i, 0, writeableBitmap.PixelWidth), (int)point.Y, strokeColor);
                            }
                            points.Add(new Point((int)point.X + i, (int)point.Y));
                            points.Add(new Point((int)point.X - i, (int)point.Y));
                        }
                    });
                }
                else
                {
                    new List<Point>(points).ForEach(point =>
                    {
                        for (int i = 1; i < strokeThickness / 2; i++)
                        {
                            if (writeableBitmap != null)
                            {
                                putPixel(writeableBitmap, (int)point.X, Clamp((int)point.Y + i, 0, writeableBitmap.PixelHeight), strokeColor);
                                putPixel(writeableBitmap, (int)point.X, Clamp((int)point.Y - i, 0, writeableBitmap.PixelHeight), strokeColor);
                            }
                            points.Add(new Point((int)point.X, (int)point.Y + i));
                            points.Add(new Point((int)point.X, (int)point.Y - i));
                        }
                    });
                }
            }

            return points;

        }
        public override List<Point> GetPoints()
        {
            if (Xiaolin_Wu)
            {
                return XiaolinWu(null, start, end);
            }
            else
            {
                return DrawLine(null, start, end);
            }
        }

        private List<Point> XiaolinWu(WriteableBitmap? writeableBitmap, Point start, Point end)
        {
            List<Point> points = new List<Point>();
            Color L = strokeColor;
            Color B = Constants.backgroundColor; //background-color



            bool steep = Math.Abs(end.Y - start.Y) > Math.Abs(end.X - start.X);
            if (steep)
            {
                (start.X, start.Y) = (start.Y, start.X);
                (end.X, end.Y) = (end.Y, end.X);
            }
            if (start.X > end.X)
            {
                (start.X, end.X) = (end.X, start.X);
                (start.Y, end.Y) = (end.Y, start.Y);
            }

            float dx = (float)(end.X - start.X);
            float dy = (float)(end.Y - start.Y);
            float m = dy / dx;

            float y = (float)start.Y;
            

            
            if (steep)
            {
                for (int x = (int)start.X; x <= end.X; ++x)
                {
                    float brightness1 = Math.Max(0, Math.Min(1, (1 - (y % 1))));
                    float brightness2 = Math.Max(0, Math.Min(1, (y % 1)));
                    Color c1 = Color.FromArgb(
                        Clamp((int)(L.A * brightness1 + B.A * brightness2), 0, 255),
                        Clamp((int)(L.R * brightness1 + B.R * brightness2), 0, 255),
                        Clamp((int)(L.G * brightness1 + B.G * brightness2), 0, 255),
                        Clamp((int)(L.B * brightness1 + B.B * brightness2), 0, 255)
                    );
                    Color c2 = Color.FromArgb(
                        Clamp((int)(L.A * brightness2 + B.A * brightness1), 0, 255),
                        Clamp((int)(L.R * brightness2 + B.R * brightness1), 0, 255),
                        Clamp((int)(L.G * brightness2 + B.G * brightness1), 0, 255),
                        Clamp((int)(L.B * brightness2 + B.B * brightness1), 0, 255)
                    );

                    if (writeableBitmap != null)
                    {
                        putPixel(writeableBitmap, (int)Math.Floor(y), x, c1);
                        putPixel(writeableBitmap, (int)Math.Floor(y) + 1, x, c2);
                    }
                    points.Add(new Point((int)Math.Floor(y), x));
                    points.Add(new Point((int)Math.Floor(y) + 1, x));
                    y += m;
                }

            }
            else
            {
                for (int x = (int)start.X; x <= end.X; ++x)
                {
                    float brightness1 = Math.Max(0, Math.Min(1, (1 - (y % 1))));
                    float brightness2 = Math.Max(0, Math.Min(1, (y % 1)));
                    Color c1 = Color.FromArgb(
                        Clamp((int)(L.A * brightness1 + B.A * brightness2), 0, 255),
                        Clamp((int)(L.R * brightness1 + B.R * brightness2), 0, 255),
                        Clamp((int)(L.G * brightness1 + B.G * brightness2), 0, 255),
                        Clamp((int)(L.B * brightness1 + B.B * brightness2), 0, 255)
                    );
                    Color c2 = Color.FromArgb(
                        Clamp((int)(L.A * brightness2 + B.A * brightness1), 0, 255),
                        Clamp((int)(L.R * brightness2 + B.R * brightness1), 0, 255),
                        Clamp((int)(L.G * brightness2 + B.G * brightness1), 0, 255),
                        Clamp((int)(L.B * brightness2 + B.B * brightness1), 0, 255)
                    );

                    if (writeableBitmap != null)
                    {
                        putPixel(writeableBitmap, x, (int)Math.Floor(y), c1);
                        putPixel(writeableBitmap, x, (int)Math.Floor(y) + 1, c2);
                    }
                    points.Add(new Point(x, (int)Math.Floor(y)));
                    points.Add(new Point(x, (int)Math.Floor(y) + 1));
                    y += m;
                }
            }
            return points;
        }

        public override void Move(Vector vector)
        {
            start = new Point(start.X + vector.X, start.Y + vector.Y);
            end = new Point(end.X + vector.X, end.Y + vector.Y);
        }
    }
    public class Circle : Shape
    {
        public Point center;
        public double radius;

        public Circle(Point center, double radius, double strokeThickness, Color strokeColor, bool Xiaolin_Wu)
        {
            this.center = center;
            this.radius = radius;
            this.strokeColor = strokeColor;
            this.Xiaolin_Wu = Xiaolin_Wu;
            if (Xiaolin_Wu)
            {
                this.strokeThickness = 1;
            }
            else
            {
                this.strokeThickness = strokeThickness;
            }
        }

        public override void Draw(WriteableBitmap writeableBitmap)
        {
            if (Xiaolin_Wu)
            {
                XiaolinWu(writeableBitmap, center, radius);
            }
            else
            {
                DrawCircle(writeableBitmap, center, radius);
            }
        }

        public List<Point> DrawCircle(WriteableBitmap? writeableBitmap, Point center, double radius)
        {
            List<Point> points = new List<Point>();
            if (radius == 0)
            {
                putPixel(writeableBitmap, (int)center.X, (int)center.Y, strokeColor);
                return new List<Point>() { center };
            }

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
                    d += 2 * x + 3;
                }
                else //move to SE
                {
                    d += 2 * x - 2 * y + 5;
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

        private List<Point> XiaolinWu(WriteableBitmap? writeableBitmap, Point center, double radius)
        {
            List<Point> points = new List<Point>();
            if (radius == 0)
            {
                putPixel(writeableBitmap, (int)center.X, (int)center.Y, strokeColor);
                return new List<Point>() { center };
            }
            Color L = strokeColor;
            Color B = Constants.backgroundColor; //background-color

            int x = (int)radius;
            int y = 0;
            if (writeableBitmap != null)
            {
                putPixel(writeableBitmap, (int)center.X + x, (int)center.Y + y, L);
                putPixel(writeableBitmap, (int)center.X + x, (int)center.Y - y, L);
                putPixel(writeableBitmap, (int)center.X - x, (int)center.Y + y, L);
                putPixel(writeableBitmap, (int)center.X - x, (int)center.Y - y, L);
                putPixel(writeableBitmap, (int)center.X + y, (int)center.Y + x, L);
                putPixel(writeableBitmap, (int)center.X + y, (int)center.Y - x, L);
                putPixel(writeableBitmap, (int)center.X - y, (int)center.Y + x, L);
                putPixel(writeableBitmap, (int)center.X - y, (int)center.Y - x, L);
            }
            points.Add(new Point((int)center.X + x, (int)center.Y + y));
            points.Add(new Point((int)center.X + x, (int)center.Y - y));
            points.Add(new Point((int)center.X - x, (int)center.Y + y));
            points.Add(new Point((int)center.X - x, (int)center.Y - y));
            points.Add(new Point((int)center.X + y, (int)center.Y + x));
            points.Add(new Point((int)center.X + y, (int)center.Y - x));
            points.Add(new Point((int)center.X - y, (int)center.Y + x));
            points.Add(new Point((int)center.X - y, (int)center.Y - x));

            float D(int R, int y)
            {
                return (float)Math.Min(1.0f, Math.Ceiling(Math.Sqrt(radius * radius - y * y)) - Math.Sqrt(radius * radius - y * y));
            }
            while (x>y)
            {
                ++y;
                x = (int)Math.Ceiling(Math.Sqrt(radius * radius - y * y));
                float brightness1 = Math.Max(0, Math.Min(1, 1-D((int)radius, y)));
                float brightness2 = Math.Max(0, Math.Min(1, D((int)radius, y)));
                Color c2 = Color.FromArgb(
                    Clamp((int)(L.A * brightness1 + B.A * brightness2), 0, 255),
                    Clamp((int)(L.R * brightness1 + B.R * brightness2), 0, 255),
                    Clamp((int)(L.G * brightness1 + B.G * brightness2), 0, 255),
                    Clamp((int)(L.B * brightness1 + B.B * brightness2), 0, 255)
                );
                Color c1 = Color.FromArgb(
                    Clamp((int)(L.A * brightness2 + B.A * brightness1), 0, 255),
                    Clamp((int)(L.R * brightness2 + B.R * brightness1), 0, 255),
                    Clamp((int)(L.G * brightness2 + B.G * brightness1), 0, 255),
                    Clamp((int)(L.B * brightness2 + B.B * brightness1), 0, 255)
                );

                if (writeableBitmap != null)
                {
                    putPixel(writeableBitmap, (int)center.X + x, (int)center.Y + y, c2);
                    putPixel(writeableBitmap, (int)center.X + y, (int)center.Y + x, c2);
                    putPixel(writeableBitmap, (int)center.X - x, (int)center.Y + y, c2);
                    putPixel(writeableBitmap, (int)center.X - x, (int)center.Y - y, c2);
                    putPixel(writeableBitmap, (int)center.X + y, (int)center.Y - x, c2);
                    putPixel(writeableBitmap, (int)center.X - y, (int)center.Y + x, c2);
                    putPixel(writeableBitmap, (int)center.X - y, (int)center.Y - x, c2);
                    putPixel(writeableBitmap, (int)center.X + x, (int)center.Y - y, c2);

                    putPixel(writeableBitmap, (int)center.X + x - 1, (int)center.Y + y, c1);
                    putPixel(writeableBitmap, (int)center.X + y, (int)center.Y + x - 1, c1);
                    putPixel(writeableBitmap, (int)center.X - x + 1, (int)center.Y + y, c1);
                    putPixel(writeableBitmap, (int)center.X - x + 1, (int)center.Y - y, c1);
                    putPixel(writeableBitmap, (int)center.X + y, (int)center.Y - x + 1, c1);
                    putPixel(writeableBitmap, (int)center.X - y, (int)center.Y + x - 1, c1);
                    putPixel(writeableBitmap, (int)center.X - y, (int)center.Y - x + 1, c1);
                    putPixel(writeableBitmap, (int)center.X + x - 1, (int)center.Y - y, c1);
                }
                points.Add(new Point((int)center.X + x, (int)center.Y + y));
                points.Add(new Point((int)center.X + y, (int)center.Y + x));
                points.Add(new Point((int)center.X - x, (int)center.Y + y));
                points.Add(new Point((int)center.X - x, (int)center.Y - y));
                points.Add(new Point((int)center.X + y, (int)center.Y - x));
                points.Add(new Point((int)center.X - y, (int)center.Y + x));
                points.Add(new Point((int)center.X - y, (int)center.Y - x));
                points.Add(new Point((int)center.X + x, (int)center.Y - y));

                points.Add(new Point((int)center.X + x - 1, (int)center.Y + y));
                points.Add(new Point((int)center.X + y, (int)center.Y + x - 1));
                points.Add(new Point((int)center.X - x + 1, (int)center.Y + y));
                points.Add(new Point((int)center.X - x + 1, (int)center.Y - y));
                points.Add(new Point((int)center.X + y, (int)center.Y - x + 1));
                points.Add(new Point((int)center.X - y, (int)center.Y + x - 1));
                points.Add(new Point((int)center.X - y, (int)center.Y - x + 1));
                points.Add(new Point((int)center.X + x - 1, (int)center.Y - y));

            }
            return points;
        }

        public override List<Point> GetPoints()
        {
            if (Xiaolin_Wu)
            {
                return XiaolinWu(null, center, radius);
            }
            else
            {
                return DrawCircle(null, center, radius);
            }
        }

        public override void Move(Vector vector)
        {
            center = new Point(center.X + vector.X, center.Y + vector.Y);
        }
    }
    public class Polygons : Shape
    {
        public Point start;
        public List<Point> vertives;

        public Polygons(Point start, List<Point> vertives, double strokeThickness, Color strokeColor, bool XiaolinWu)
        {
            this.start = start;
            this.vertives = vertives;
            this.strokeThickness = strokeThickness;
            this.strokeColor = strokeColor;
            this.Xiaolin_Wu = XiaolinWu;
        }
        public override void Draw(WriteableBitmap writeableBitmap)
        {
            DrawPolygon(writeableBitmap, start, vertives);
        }

        private List<Point> DrawPolygon(WriteableBitmap? writeableBitmap, Point start, List<Point> vertives)
        {
            List<Point> points = new List<Point>();
            if (vertives.Count == 1)
            {
                if(writeableBitmap != null)
                {
                    putPixel(writeableBitmap, (int)start.X, (int)start.Y, strokeColor);
                }
                points.Add(new Point((int)start.X, (int)start.Y));
                return points;
            }
            Line tmp;
            for (int i = 0; i < vertives.Count - 1; i++)
            {
                tmp = new Line(vertives[i], vertives[i+1], strokeThickness, strokeColor, Xiaolin_Wu);
                if (writeableBitmap != null)
                {
                    tmp.Draw(writeableBitmap);
                }
                foreach (Point point in tmp.GetPoints())
                {
                    points.Add(point);
                }
            }
            tmp = new Line(start, vertives.Last<Point>(), strokeThickness, strokeColor, Xiaolin_Wu);
            if (writeableBitmap != null)
            {
                tmp.Draw(writeableBitmap);
            }
            foreach (Point point in tmp.GetPoints())
            {
                points.Add(point);
            }
            return points;
        }

        public override List<Point> GetPoints()
        {
            return DrawPolygon(null, start, vertives);
        }

        //return true if the polygon is closed
        public bool DrawPolygonLineByLine(WriteableBitmap writeableBitmap)
        {
            if(Math.Sqrt(Math.Pow(vertives.Last().X - start.X,2) + Math.Pow(vertives.Last().Y - start.Y, 2)) < 10)
            {
                vertives.Remove(vertives.Last());
                Line tmp = new Line(vertives.Last(), start, strokeThickness, strokeColor, Xiaolin_Wu);
                tmp.Draw(writeableBitmap);
                return true;
            }
            else
            {
                Line tmp = new Line(vertives[vertives.Count-2], vertives[vertives.Count-1], strokeThickness, strokeColor, Xiaolin_Wu);
                tmp.Draw(writeableBitmap);
                return false;
            }

        }

        public override void Move(Vector vector)
        {
            start = new Point(start.X + vector.X, start.Y + vector.Y);
            for (int i = 0; i < vertives.Count; i++)
            {
                vertives[i] = new Point(vertives[i].X + vector.X, vertives[i].Y + vector.Y);
            }
        }
    }

    public partial class MainWindow : Window
    {
        private WriteableBitmap bitmap;
        private enum DrawMode { Line, Circle, Rectangle, Polygon, None };
        private enum DrawState { Start, Drawing, End };
        private enum EditorialMode { Move, Delete, Edit, None };
        private enum EditState { Start, Moving, Deleting, Editing, End };
        private EditorialMode editorialMode;
        private EditState editState;
        private DrawMode drawMode;
        private DrawState drawState;
        private Dictionary<int, Shape> shapes;
        private Dictionary<Point, int> points;
        private Color currentColor;
        private bool XiaolinWuFlag;
        private int thickness;
        private int index;
        private int selectedShapeIndex;
        private List<Point> selectedShapeOldPoints;
        private Point lastMousePosition;
        private List<Point> circlePointAroundTheVertex;
        private bool isCirclePointAroundTheVertex;
        public MainWindow()
        {
            InitializeComponent();
            drawMode = DrawMode.None;
            drawState = DrawState.Start;
            shapes = new Dictionary<int, Shape>();
            points = new Dictionary<Point, int>();
            currentColor = Color.Black;
            XiaolinWuFlag = false;
            thickness = 1;
            editorialMode = EditorialMode.None;
            editState = EditState.Start;
            index = 0;
            selectedShapeIndex = -1;
            selectedShapeOldPoints = new List<Point>();
            circlePointAroundTheVertex = new List<Point>();
            isCirclePointAroundTheVertex = false;
            
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

        private static List<Point> clickCircle(Point center, double radius)
        {
            return new Circle(center, radius, 1, Color.Black, false).GetPoints();
        }

                private void buttonClear_Click(object sender, RoutedEventArgs e)
        {
            paintBackground(bitmap, new byte[] { 255, 255, 255, 255 });
            uncheckedAllOtherButtons(null);
            shapes.Clear();
            points.Clear();
            drawMode = DrawMode.None;
            drawState = DrawState.Start;
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

        private void buttonEdit_Click(object sender, RoutedEventArgs e)
        {



        }

        private void buttonMove_Click(object sender, RoutedEventArgs e)
        {
            uncheckedAllOtherButtons(sender);
            drawMode = DrawMode.None;
            drawState = DrawState.Start;
            editorialMode = EditorialMode.Move;
            editState = EditState.Start;
        }

        private void buttonDelete_Click(object sender, RoutedEventArgs e)
        {
            uncheckedAllOtherButtons(sender);
            drawMode = DrawMode.None;
            drawState = DrawState.Start;
            editorialMode = EditorialMode.Delete;
            editState = EditState.Start;
        }

        private void uncheckedAllOtherButtons(object? button)
        {
            ToggleButton? toggleButton = button as ToggleButton;
            buttonLine.IsChecked = false;
            buttonCircle.IsChecked = false;
            buttonPolygon.IsChecked = false;
            buttonEdit.IsChecked = false;
            buttonMove.IsChecked = false;
            buttonDelete.IsChecked = false;
            if (toggleButton != null)
            {
                toggleButton.IsChecked = true;
            }
        }

        private void imageControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (drawMode == DrawMode.Line)
            {
                if (drawState == DrawState.Start)
                {
                    Point p = e.GetPosition(imageControl);
                    Line line = new Line(p, p, thickness, currentColor, XiaolinWuFlag);
                    shapes.Add(index, line);
                    drawState = DrawState.Drawing;
                    ++index;
                }
                else if (drawState == DrawState.Drawing)
                {
                    Point p = e.GetPosition(imageControl);
                    Line line = (Line)shapes[index - 1];
                    line.end = p;
                    shapes[index - 1] = line;

                    List<Point> points = line.GetPoints();
                    foreach (Point point in points)
                    {
                        this.points.TryAdd(point, index - 1);
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
                    Circle circle = new Circle(p, 0, thickness, currentColor, XiaolinWuFlag);
                    shapes.Add(index, circle);
                    drawState = DrawState.Drawing;
                    index++;
                }
                else if (drawState == DrawState.Drawing)
                {
                    Point p = e.GetPosition(imageControl);
                    Circle circle = (Circle)shapes[index - 1];
                    circle.radius = Math.Sqrt(Math.Pow(circle.center.X - p.X, 2) + Math.Pow(circle.center.Y - p.Y, 2));
                    shapes[index - 1] = circle;

                    List<Point> points = circle.GetPoints();
                    foreach (Point point in points)
                    {
                        this.points.TryAdd(point, index - 1);
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
                    Polygons polygon = new Polygons(p, new List<Point>() { p }, thickness, currentColor, XiaolinWuFlag);
                    shapes.Add(index, polygon);
                    drawState = DrawState.Drawing;
                    index++;
                }
                else if (drawState == DrawState.Drawing)
                {
                    Point p = e.GetPosition(imageControl);
                    Polygons polygon = (Polygons)shapes[index - 1];
                    polygon.vertives.Add(p);
                    shapes[index - 1] = polygon;

                    List<Point> points = polygon.GetPoints();
                    foreach (Point point in points)
                    {
                        this.points.TryAdd(point, index - 1);
                    }
                    bool isClosed = polygon.DrawPolygonLineByLine(bitmap);
                    if (isClosed)
                    {
                        drawState = DrawState.Start;
                        foreach (Point point in polygon.GetPoints())
                        {
                            this.points.TryAdd(point, index - 1);
                        }
                    }
                    
                }
                else if (drawState == DrawState.End)
                {
                    drawState = DrawState.Start;
                }
            }
            else if (drawMode == DrawMode.None)
            {
                if (editorialMode == EditorialMode.Delete)
                {
                    Point p = e.GetPosition(imageControl);
                    List<Point> clickPoints;
                    clickPoints = clickCircle(p, 10);
                    foreach (Point point1 in clickPoints)
                    {
                        if (points.ContainsKey(point1))
                        {
                            int shapeIndex = this.points[point1];
                            Shape shape = shapes[shapeIndex];
                            shapes.Remove(shapeIndex);
                            List<Point> points = shape.GetPoints();
                            foreach (Point point in points)
                            {
                                this.points.Remove(point);
                                if (point.X < 0 || point.X >= bitmap.PixelWidth || point.Y < 0 || point.Y >= bitmap.PixelHeight)
                                {
                                    continue;
                                }
                                bitmap.WritePixels(new Int32Rect((int)point.X, (int)point.Y, 1, 1), new byte[] { 255, 255, 255, 255 }, 4, 0);
                            }
                            this.points.Remove(point1);
                            foreach (Shape s in shapes.Values)
                            {
                                s.Draw(bitmap);
                            }
                            break;
                        }
                    }
                }
                else if(editorialMode == EditorialMode.Move)
                {
                    if (editState == EditState.Start)
                    {
                        Point p = e.GetPosition(imageControl);
                        List<Point> clickPoints;
                        clickPoints = clickCircle(p, 10);
                        foreach (Point point1 in clickPoints)
                        {
                            if (points.ContainsKey(point1))
                            {
                                selectedShapeIndex = this.points[point1];
                                editState = EditState.Moving;
                                List<Point> points = shapes[selectedShapeIndex].GetPoints();
                                foreach (Point point in points)
                                {
                                    this.points.Remove(point);
                                    if (point.X < 0 || point.X >= bitmap.PixelWidth || point.Y < 0 || point.Y >= bitmap.PixelHeight)
                                    {
                                        continue;
                                    }
                                    bitmap.WritePixels(new Int32Rect((int)point.X, (int)point.Y, 1, 1), new byte[] { 0, 0, 255, 255 }, 4, 0);
                                }

                                editState = EditState.Moving;
                                lastMousePosition = e.GetPosition(imageControl);
                                selectedShapeOldPoints = shapes[selectedShapeIndex].GetPoints();
                                break;
                            }
                        }

                    }
                }
                else if(editorialMode == EditorialMode.Edit)
                {
                    if (editState == EditState.Start)
                    {
                            Point p = e.GetPosition(imageControl);
                            List<Point> clickPoints;
                            clickPoints = clickCircle(p, 10);
                            foreach (Point point1 in clickPoints)
                            {
                                if (points.ContainsKey(point1))
                                {
                                    selectedShapeIndex = this.points[point1];
                                    Shape shape = shapes[selectedShapeIndex];
                                    if (shape is Line)
                                    {
                                        Circle circle = new Circle(((Line)shape).start, 10, 1, Color.Blue, false);
                                        circle.Draw(bitmap);
                                        foreach(Point point in circle.GetPoints())
                                        {
                                            circlePointAroundTheVertex.Add(point);
                                        }
                                        Circle circle2 = new Circle(((Line)shape).end, 10, 1, Color.Blue, false);
                                        circle2.Draw(bitmap);
                                        foreach (Point point in circle2.GetPoints())
                                        {
                                            circlePointAroundTheVertex.Add(point);
                                        }
                                    }
                                    if (shape is Polygons)
                                    {
                                        Polygons polygon = (Polygons)shape;
                                        Circle circle = new Circle(polygon.start, 10, 1, Color.Blue, false);
                                        circle.Draw(bitmap);
                                        foreach (Point point in circle.GetPoints())
                                        {
                                            circlePointAroundTheVertex.Add(point);
                                        }
                                        foreach (Point point in polygon.vertives)
                                        {
                                            Circle circle2 = new Circle(point, 10, 1, Color.Blue, false);
                                            circle2.Draw(bitmap);
                                            foreach (Point point2 in circle2.GetPoints())
                                            {
                                                circlePointAroundTheVertex.Add(point2);
                                            }
                                        }
                                        //put circle in the middle of edges of the polygon
                                        for (int i = 0; i < polygon.vertives.Count - 1; i++)
                                    {
                                            Circle circle2 = new Circle(new Point((polygon.vertives[i].X + polygon.vertives[i + 1].X) / 2, (polygon.vertives[i].Y + polygon.vertives[i + 1].Y) / 2), 10, 1, Color.Blue, false);
                                            circle2.Draw(bitmap);
                                            foreach (Point point in circle2.GetPoints())
                                        {
                                                circlePointAroundTheVertex.Add(point);
                                            }
                                        }
                                    }
                                    if (shape is Circle)
                                {
                                        Circle circle = (Circle)shape;
                                        Circle circle3 = new Circle(new Point(circle.center.X + circle.radius,      circle.center.Y), 10, 1, Color.Blue, false);
                                        circle3.Draw(bitmap);
                                        foreach (Point point in circle3.GetPoints())
                                        {
                                            circlePointAroundTheVertex.Add(point);
                                        }
                                    }
                                    editState = EditState.Editing;
                                    lastMousePosition = e.GetPosition(imageControl);
                                    selectedShapeOldPoints = shapes[selectedShapeIndex].GetPoints();

                                break;
                                }
                            }
                    }

                }
            }
            
        }
        

        private void imageControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (editorialMode == EditorialMode.Move)
            {
                if (editState == EditState.Moving)
                {
                    editState = EditState.Start;
                    foreach (Shape s in shapes.Values)
                    {
                        s.Draw(bitmap);
                    }

                    foreach (Point point in selectedShapeOldPoints)
                    {
                        this.points.Remove(point);
                        if (point.X < 0 || point.X >= bitmap.PixelWidth || point.Y < 0 || point.Y >= bitmap.PixelHeight)
                        {
                            continue;
                        }
                        bitmap.WritePixels(new Int32Rect((int)point.X, (int)point.Y, 1, 1), new byte[] { 255, 255, 255, 255 }, 4, 0);
                    }
                    Shape shape = shapes[selectedShapeIndex];
                    List<Point> newPoints = shape.GetPoints();
                    foreach (Point point in newPoints)
                    {
                        this.points.TryAdd(point, selectedShapeIndex);
                    }
                    foreach (Shape s in shapes.Values)
                    {
                        s.Draw(bitmap);
                    }
                    selectedShapeIndex = -1;
                }
            }
            else if(editorialMode == EditorialMode.Edit)
            {
                if (editState == EditState.Editing)
                {
                    editState = EditState.Start;
                    foreach (Point point in selectedShapeOldPoints)
                    {
                        this.points.Remove(point);
                        if (point.X < 0 || point.X >= bitmap.PixelWidth || point.Y < 0 || point.Y >= bitmap.PixelHeight)
                        {
                            continue;
                        }
                        bitmap.WritePixels(new Int32Rect((int)point.X, (int)point.Y, 1, 1), new byte[] { 255, 255, 255, 255 }, 4, 0);
                    }
                    Shape shape = shapes[selectedShapeIndex];
                    List<Point> newPoints = shape.GetPoints();
                    foreach (Point point in newPoints)
                    {
                        this.points.TryAdd(point, selectedShapeIndex);
                    }
                    if (isCirclePointAroundTheVertex)
                    {
                        foreach (Point point in circlePointAroundTheVertex)
                        {
                            if (point.X < 0 || point.X >= bitmap.PixelWidth || point.Y < 0 || point.Y >= bitmap.PixelHeight)
                            {
                                continue;
                            }
                            bitmap.WritePixels(new Int32Rect((int)point.X, (int)point.Y, 1, 1), new byte[] { 255, 255, 255, 255 }, 4, 0);
                        }
                        isCirclePointAroundTheVertex = false;
                    }

                    foreach (Shape s in shapes.Values)
                    {
                        s.Draw(bitmap);
                    }
                    selectedShapeIndex = -1;
                }
            }
        }

        private void imageControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (selectedShapeIndex != -1 && e.LeftButton == MouseButtonState.Pressed && editorialMode == EditorialMode.Move)
            {
                Point currentMousePosition = e.GetPosition(imageControl);
                Vector displacement = currentMousePosition - lastMousePosition;
                Shape selectedShape = shapes[selectedShapeIndex];
                selectedShape.Move(displacement);
                lastMousePosition = currentMousePosition;

            }
            if (selectedShapeIndex != -1 && e.LeftButton == MouseButtonState.Pressed && editorialMode == EditorialMode.Edit)
            {
                Point currentMousePosition = e.GetPosition(imageControl);
                Shape selectedShape = shapes[selectedShapeIndex];
                
                if (selectedShape is Line)
                {
                    Line line = (Line)selectedShape;
                    if((currentMousePosition - line.start).Length < 15)
                    {
                        isCirclePointAroundTheVertex = true;

                        line.start = currentMousePosition;
                    }
                    else if((currentMousePosition - line.end).Length < 15)
                    {
                        isCirclePointAroundTheVertex = true;

                        line.end = currentMousePosition;
                    }
                    
                }
                if (selectedShape is Polygons)
                {
                    Polygons polygon = (Polygons)selectedShape;
                    if ((currentMousePosition - polygon.start).Length < 15)
                    {
                        isCirclePointAroundTheVertex = true;

                        polygon.start = currentMousePosition;
                    }
                    List<Point> vertives = new List<Point>(polygon.vertives);
                    foreach (Point point in vertives)
                    {
                        if ((currentMousePosition - point).Length < 15)
                        {
                            isCirclePointAroundTheVertex = true;

                            polygon.vertives[polygon.vertives.IndexOf(point)] = currentMousePosition;
                        }
                    }
                    //move circle to the middle of edges of the polygon
                    for (int i = 0; i < polygon.vertives.Count - 1; i++)
                    {
                        if ((currentMousePosition - new Point((polygon.vertives[i].X + polygon.vertives[i + 1].X) / 2, (polygon.vertives[i].Y + polygon.vertives[i + 1].Y) / 2)).Length < 15)
                        {
                            Vector displacement = currentMousePosition - lastMousePosition;
                            isCirclePointAroundTheVertex = true;
                            polygon.vertives[i] = polygon.vertives[i] + displacement;
                            polygon.vertives[i + 1] = polygon.vertives[i+1] + displacement;
                        }
                    }
                }
                if (selectedShape is Circle)
                {
                    Circle circle = (Circle)selectedShape;
                    if ((currentMousePosition - new Point(circle.center.X + circle.radius, circle.center.Y)).Length < 15)
                    {
                        isCirclePointAroundTheVertex = true;

                        circle.radius = (currentMousePosition - circle.center).Length;
                    }
                }
                lastMousePosition = currentMousePosition;
            }
        }

        private void checkboxXiaoliWu_Checked(object sender, RoutedEventArgs e)
        {
            XiaolinWuFlag = true;
        }

        private void checkboxXiaoliWu_Unchecked(object sender, RoutedEventArgs e)
        {
            XiaolinWuFlag = false;
        }
        private void sliderThickness_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            thickness = (int)e.NewValue;
        }

        private void colorpickerStroke_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<System.Windows.Media.Color?> e)
        {
            if(colorpickerStroke.SelectedColor.HasValue)
                currentColor = Color.FromArgb(colorpickerStroke.SelectedColor.Value.A, colorpickerStroke.SelectedColor.Value.R, colorpickerStroke.SelectedColor.Value.G, colorpickerStroke.SelectedColor.Value.B);
              
        }

        private void buttonEdit_Unchecked(object sender, RoutedEventArgs e)
        {
            editorialMode = EditorialMode.None;
            editState = EditState.Start;
            isCirclePointAroundTheVertex = false;
            foreach (Point point in circlePointAroundTheVertex)
            {
                if (point.X < 0 || point.X >= bitmap.PixelWidth || point.Y < 0 || point.Y >= bitmap.PixelHeight)
                {
                    continue;
                }
                bitmap.WritePixels(new Int32Rect((int)point.X, (int)point.Y, 1, 1), new byte[] { 255, 255, 255, 255 }, 4, 0);
            }
        }

        private void buttonEdit_Checked(object sender, RoutedEventArgs e)
        {
            buttonLine.IsChecked = false;
            buttonCircle.IsChecked = false;
            buttonPolygon.IsChecked = false;
            buttonMove.IsChecked = false;
            buttonDelete.IsChecked = false;

            drawMode = DrawMode.None;
            drawState = DrawState.Start;
            editorialMode = EditorialMode.Edit;
            editState = EditState.Start;
        }
    }
}

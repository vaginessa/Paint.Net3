namespace PaintDotNet
{
    using PaintDotNet.Rendering;
    using System;
    using System.Runtime.CompilerServices;
    using System.Windows;

    internal static class ScaleFactorExtensions
    {
        public static Point Scale(this ScaleFactor sf, Int32Point p) => 
            new Point(sf.Scale((double) p.X), sf.Scale((double) p.Y));

        public static Size Scale(this ScaleFactor sf, Int32Size size) => 
            new Size(sf.Scale((double) size.Width), sf.Scale((double) size.Height));

        public static double Scale(this ScaleFactor sf, double x) => 
            ((x * sf.Numerator) / ((double) sf.Denominator));

        public static Rect Scale(this ScaleFactor sf, Int32Rect rect) => 
            new Rect(sf.Scale((double) rect.X), sf.Scale((double) rect.Y), sf.Scale((double) rect.Width), sf.Scale((double) rect.Height));

        public static Point Scale(this ScaleFactor sf, Point p) => 
            new Point(sf.Scale(p.X), sf.Scale(p.Y));

        public static Rect Scale(this ScaleFactor sf, Rect rect) => 
            new Rect(sf.Scale(rect.X), sf.Scale(rect.Y), sf.Scale(rect.Width), sf.Scale(rect.Height));

        public static Size Scale(this ScaleFactor sf, Size size) => 
            new Size(sf.Scale(size.Width), sf.Scale(size.Height));

        public static Vector Scale(this ScaleFactor sf, Vector vec) => 
            new Vector(sf.Scale(vec.X), sf.Scale(vec.Y));

        public static Point Unscale(this ScaleFactor sf, Int32Point p) => 
            new Point(sf.Unscale((double) p.X), sf.Unscale((double) p.Y));

        public static Size Unscale(this ScaleFactor sf, Int32Size size) => 
            new Size(sf.Unscale((double) size.Width), sf.Unscale((double) size.Height));

        public static double Unscale(this ScaleFactor sf, double x) => 
            ((x * sf.Denominator) / ((double) sf.Numerator));

        public static Rect Unscale(this ScaleFactor sf, Int32Rect rect) => 
            new Rect(sf.Unscale((double) rect.X), sf.Unscale((double) rect.Y), sf.Unscale((double) rect.Width), sf.Unscale((double) rect.Height));

        public static Point Unscale(this ScaleFactor sf, Point p) => 
            new Point(sf.Unscale(p.X), sf.Unscale(p.Y));

        public static Rect Unscale(this ScaleFactor sf, Rect rect) => 
            new Rect(sf.Unscale(rect.X), sf.Unscale(rect.Y), sf.Unscale(rect.Width), sf.Unscale(rect.Height));

        public static Size Unscale(this ScaleFactor sf, Size size) => 
            new Size(sf.Unscale(size.Width), sf.Unscale(size.Height));

        public static Vector Unscale(this ScaleFactor sf, Vector vec) => 
            new Vector(sf.Unscale(vec.X), sf.Unscale(vec.Y));
    }
}


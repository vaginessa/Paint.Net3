namespace PaintDotNet
{
    using PaintDotNet.Rendering;
    using System;
    using System.ComponentModel;
    using System.Drawing.Drawing2D;
    using System.Runtime.CompilerServices;

    public static class SelectionCombineModeExtensions
    {
        public static CombineMode ToGdipCombineMode(this SelectionCombineMode scm)
        {
            switch (scm)
            {
                case SelectionCombineMode.Replace:
                    return CombineMode.Replace;

                case SelectionCombineMode.Union:
                    return CombineMode.Union;

                case SelectionCombineMode.Exclude:
                    return CombineMode.Exclude;

                case SelectionCombineMode.Intersect:
                    return CombineMode.Intersect;

                case SelectionCombineMode.Xor:
                    return CombineMode.Xor;
            }
            throw new InvalidEnumArgumentException();
        }

        public static GeometryCombineMode ToGeometryCombineMode(this SelectionCombineMode scm)
        {
            switch (scm)
            {
                case SelectionCombineMode.Replace:
                    throw new InvalidEnumArgumentException();

                case SelectionCombineMode.Union:
                    return GeometryCombineMode.Union;

                case SelectionCombineMode.Exclude:
                    return GeometryCombineMode.Exclude;

                case SelectionCombineMode.Intersect:
                    return GeometryCombineMode.Intersect;

                case SelectionCombineMode.Xor:
                    return GeometryCombineMode.Xor;
            }
            throw new InvalidEnumArgumentException();
        }
    }
}


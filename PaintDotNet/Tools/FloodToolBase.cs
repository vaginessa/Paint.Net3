namespace PaintDotNet.Tools
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.Rendering;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Forms;

    internal abstract class FloodToolBase : PaintDotNet.Tools.Tool
    {
        private bool clipToSelection;
        private bool contiguous;

        public FloodToolBase(DocumentWorkspace documentWorkspace, ImageResource toolBarImage, string name, string helpText, char hotKey, bool skipIfActiveOnHotKey, ToolBarConfigItems toolBarConfigItems) : base(documentWorkspace, toolBarImage, name, helpText, hotKey, skipIfActiveOnHotKey, (ToolBarConfigItems.FloodMode | ToolBarConfigItems.Tolerance) | toolBarConfigItems)
        {
            this.clipToSelection = true;
        }

        private static bool CheckColor(ColorBgra a, ColorBgra b, int tolerance)
        {
            int num = 0;
            int num2 = a.R - b.R;
            num += ((1 + (num2 * num2)) * a.A) / 0x100;
            num2 = a.G - b.G;
            num += ((1 + (num2 * num2)) * a.A) / 0x100;
            num2 = a.B - b.B;
            num += ((1 + (num2 * num2)) * a.A) / 0x100;
            num2 = a.A - b.A;
            num += num2 * num2;
            return (num <= ((tolerance * tolerance) * 4));
        }

        protected static unsafe void FillStencilByColor(Surface surface, IBitVector2D stencil, ColorBgra cmp, int tolerance, out Int32Rect boundingBox, GeometryList limitRegion, bool limitToSelection)
        {
            Int32Rect[] interiorScans;
            int top = 0x7fffffff;
            int num2 = -2147483648;
            int left = 0x7fffffff;
            int num4 = -2147483648;
            Int32Rect rect = new Int32Rect(0, 0, stencil.Width, stencil.Height);
            if (limitToSelection)
            {
                stencil.Clear(true);
                interiorScans = limitRegion.GetInteriorScans();
                for (int k = 0; k < interiorScans.Length; k++)
                {
                    Int32Rect rect2 = interiorScans[k].IntersectCopy(rect);
                    if (rect2.HasPositiveArea())
                    {
                        stencil.Set(rect2, false);
                    }
                }
            }
            else
            {
                stencil.Clear(false);
                interiorScans = new Int32Rect[] { rect };
            }
            for (int i = 0; i < surface.Height; i++)
            {
                bool flag = false;
                ColorBgra* rowAddressUnchecked = surface.GetRowAddressUnchecked(i);
                for (int m = 0; m < surface.Width; m++)
                {
                    if (CheckColor(cmp, rowAddressUnchecked[0], tolerance))
                    {
                        stencil.SetUnchecked(m, i, true);
                        if (m < left)
                        {
                            left = m;
                        }
                        if (m > num4)
                        {
                            num4 = m;
                        }
                        flag = true;
                    }
                    rowAddressUnchecked++;
                }
                if (flag)
                {
                    if (i < top)
                    {
                        top = i;
                    }
                    if (i >= num2)
                    {
                        num2 = i;
                    }
                }
            }
            stencil.Invert(rect);
            for (int j = 0; j < interiorScans.Length; j++)
            {
                Int32Rect rect3 = interiorScans[j].IntersectCopy(rect);
                if (rect3.HasPositiveArea())
                {
                    stencil.Invert(rect3);
                }
            }
            boundingBox = Int32RectUtil.FromEdges(left, top, num4 + 1, num2 + 1);
        }

        protected static unsafe void FillStencilFromPoint(Surface surface, IBitVector2D stencil, System.Drawing.Point start, int tolerance, out Int32Rect boundingBox, GeometryList limitRegion, bool limitToSelection)
        {
            Int32Rect[] interiorScans;
            ColorBgra a = surface[start];
            int top = 0x7fffffff;
            int y = -2147483648;
            int left = 0x7fffffff;
            int num4 = -2147483648;
            Int32Rect rect = new Int32Rect(0, 0, stencil.Width, stencil.Height);
            if (limitToSelection)
            {
                stencil.Clear(true);
                interiorScans = limitRegion.GetInteriorScans();
                for (int j = 0; j < interiorScans.Length; j++)
                {
                    Int32Rect rect2 = interiorScans[j].IntersectCopy(rect);
                    if (rect2.HasPositiveArea())
                    {
                        stencil.Set(rect2, false);
                    }
                }
            }
            else
            {
                stencil.Clear(false);
                interiorScans = new Int32Rect[] { rect };
            }
            Queue<System.Drawing.Point> queue = new Queue<System.Drawing.Point>(0x10);
            queue.Enqueue(start);
            while (queue.Count > 0)
            {
                System.Drawing.Point point = queue.Dequeue();
                ColorBgra* rowAddressUnchecked = surface.GetRowAddressUnchecked(point.Y);
                int x = point.X - 1;
                int num7 = point.X;
                while (((x >= 0) && !stencil.GetUnchecked(x, point.Y)) && CheckColor(a, rowAddressUnchecked[x], tolerance))
                {
                    stencil.SetUnchecked(x, point.Y, true);
                    x--;
                }
                while (((num7 < surface.Width) && !stencil.GetUnchecked(num7, point.Y)) && CheckColor(a, rowAddressUnchecked[num7], tolerance))
                {
                    stencil.SetUnchecked(num7, point.Y, true);
                    num7++;
                }
                x++;
                num7--;
                if (point.Y > 0)
                {
                    int num8 = x;
                    int num9 = x;
                    ColorBgra* bgraPtr2 = surface.GetRowAddressUnchecked(point.Y - 1);
                    for (int k = x; k <= num7; k++)
                    {
                        if (!stencil.GetUnchecked(k, point.Y - 1) && CheckColor(a, bgraPtr2[k], tolerance))
                        {
                            num9++;
                        }
                        else
                        {
                            if ((num9 - num8) > 0)
                            {
                                queue.Enqueue(new System.Drawing.Point(num8, point.Y - 1));
                            }
                            num9++;
                            num8 = num9;
                        }
                    }
                    if ((num9 - num8) > 0)
                    {
                        queue.Enqueue(new System.Drawing.Point(num8, point.Y - 1));
                    }
                }
                if (point.Y < (surface.Height - 1))
                {
                    int num11 = x;
                    int num12 = x;
                    ColorBgra* bgraPtr3 = surface.GetRowAddressUnchecked(point.Y + 1);
                    for (int m = x; m <= num7; m++)
                    {
                        if (!stencil.GetUnchecked(m, point.Y + 1) && CheckColor(a, bgraPtr3[m], tolerance))
                        {
                            num12++;
                        }
                        else
                        {
                            if ((num12 - num11) > 0)
                            {
                                queue.Enqueue(new System.Drawing.Point(num11, point.Y + 1));
                            }
                            num12++;
                            num11 = num12;
                        }
                    }
                    if ((num12 - num11) > 0)
                    {
                        queue.Enqueue(new System.Drawing.Point(num11, point.Y + 1));
                    }
                }
                if (x < left)
                {
                    left = x;
                }
                if (num7 > num4)
                {
                    num4 = num7;
                }
                if (point.Y < top)
                {
                    top = point.Y;
                }
                if (point.Y > y)
                {
                    y = point.Y;
                }
            }
            stencil.Invert(rect);
            for (int i = 0; i < interiorScans.Length; i++)
            {
                Int32Rect rect3 = interiorScans[i].IntersectCopy(rect);
                if (rect3.HasPositiveArea())
                {
                    stencil.Invert(rect3);
                }
            }
            boundingBox = Int32RectUtil.FromEdges(left, top, num4 + 1, y + 1);
        }

        protected abstract void OnFillRegionComputed(GeometryList geometry);
        protected override void OnMouseDown(MouseEventArgsF e)
        {
            System.Drawing.Point pt = new System.Drawing.Point(e.X, e.Y);
            switch (base.AppEnvironment.FloodMode)
            {
                case FloodMode.Local:
                    this.contiguous = true;
                    break;

                case FloodMode.Global:
                    this.contiguous = false;
                    break;

                default:
                    throw new InvalidEnumArgumentException();
            }
            if ((base.ModifierKeys & Keys.Shift) != Keys.None)
            {
                this.contiguous = !this.contiguous;
            }
            if (base.Document.Bounds.Contains(pt))
            {
                Int32Rect rect;
                base.OnMouseDown(e);
                GeometryList limitRegion = base.Selection.CreateGeometryListClippingMask();
                Surface surface = ((BitmapLayer) base.ActiveLayer).Surface;
                BitVector2DSurfaceAdapter stencil = new BitVector2DSurfaceAdapter(base.ScratchSurface);
                int tolerance = (int) ((base.AppEnvironment.Tolerance * base.AppEnvironment.Tolerance) * 256f);
                if (this.contiguous)
                {
                    FillStencilFromPoint(surface, stencil, pt, tolerance, out rect, limitRegion, this.clipToSelection);
                }
                else
                {
                    FillStencilByColor(surface, stencil, surface[pt], tolerance, out rect, limitRegion, this.clipToSelection);
                }
                GeometryList geometry = GeometryList.FromStencil<BitVector2DSurfaceAdapter>(stencil);
                this.OnFillRegionComputed(geometry);
            }
            base.OnMouseDown(e);
        }

        protected bool ClipToSelection
        {
            get => 
                this.clipToSelection;
            set
            {
                this.clipToSelection = value;
            }
        }
    }
}


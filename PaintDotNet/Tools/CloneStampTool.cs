namespace PaintDotNet.Tools
{
    using PaintDotNet;
    using PaintDotNet.Canvas;
    using PaintDotNet.Collections;
    using PaintDotNet.Controls;
    using PaintDotNet.HistoryMementos;
    using PaintDotNet.Rendering;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Windows;
    using System.Windows.Forms;

    internal sealed class CloneStampTool : PaintDotNet.Tools.Tool
    {
        private bool antialiasing;
        private GeometryList clipRegion;
        private Cursor cursorMouseDown;
        private Cursor cursorMouseDownSetSource;
        private Cursor cursorMouseUp;
        private SegmentedList<Int32Rect> historyRects;
        private bool mouseDownSettingCloneSource;
        private bool mouseUp;
        private RenderArgs ra;
        private BrushPreviewRenderer rendererDst;
        private BrushPreviewRenderer rendererSrc;
        private GeometryList savedRegion;
        private bool switchedTo;
        private BitmapLayer takeFromLayer;
        private Int32Rect undoRegion;

        public CloneStampTool(DocumentWorkspace documentWorkspace) : base(documentWorkspace, PdnResources.GetImageResource2("Icons.CloneStampToolIcon.png"), PdnResources.GetString2("CloneStampTool.Name"), PdnResources.GetString2("CloneStampTool.HelpText"), 'l', false, ToolBarConfigItems.Antialiasing | ToolBarConfigItems.Pen)
        {
            this.undoRegion = Int32RectUtil.Zero;
            this.mouseUp = true;
        }

        private unsafe void DrawACircle(System.Windows.Point pt, Surface srfSrc, Surface srfDst, Int32Point difference, Int32Rect rect)
        {
            double num = base.AppEnvironment.PenInfo.Width / 2f;
            double num2 = ((float) base.AppEnvironment.PrimaryColor.A) / 255f;
            rect = rect.IntersectCopy(Int32RectUtil.From(difference, srfSrc.Size<ColorBgra>()));
            rect = rect.IntersectCopy(srfDst.Bounds<ColorBgra>());
            if ((rect.Width != 0) && (rect.Height != 0))
            {
                num2 *= num2;
                num2 *= num2;
                for (int i = rect.Y; i < (rect.Y + rect.Height); i++)
                {
                    ColorBgra* rowAddressUnchecked = srfSrc.GetRowAddressUnchecked(i - difference.Y);
                    ColorBgra* bgraPtr2 = srfDst.GetRowAddressUnchecked(i);
                    for (int j = rect.X; j < (rect.X + rect.Width); j++)
                    {
                        ColorBgra* bgraPtr3 = (rowAddressUnchecked + j) - difference.X;
                        ColorBgra* bgraPtr4 = bgraPtr2 + j;
                        double num5 = (0.5 + num) - pt.DistanceTo(new System.Windows.Point((double) j, (double) i));
                        if (num5 > 0.0)
                        {
                            double num6 = this.antialiasing ? DoubleUtil.Clamp(num5 * num2, 0.0, 1.0) : 1.0;
                            num6 *= ((double) bgraPtr3->A) / 255.0;
                            bgraPtr4->A = (byte) (255.0 - ((0xff - bgraPtr4->A) * (1.0 - num6)));
                            if (0.0 == (num6 + (((1.0 - num6) * bgraPtr4->A) / 255.0)))
                            {
                                bgraPtr4->Bgra = 0;
                            }
                            else
                            {
                                bgraPtr4->R = (byte) (((bgraPtr3->R * num6) + (((bgraPtr4->R * (1.0 - num6)) * bgraPtr4->A) / 255.0)) / (num6 + (((1.0 - num6) * bgraPtr4->A) / 255.0)));
                                bgraPtr4->G = (byte) (((bgraPtr3->G * num6) + (((bgraPtr4->G * (1.0 - num6)) * bgraPtr4->A) / 255.0)) / (num6 + (((1.0 - num6) * bgraPtr4->A) / 255.0)));
                                bgraPtr4->B = (byte) (((bgraPtr3->B * num6) + (((bgraPtr4->B * (1.0 - num6)) * bgraPtr4->A) / 255.0)) / (num6 + (((1.0 - num6) * bgraPtr4->A) / 255.0)));
                            }
                        }
                    }
                }
                Int32Rect roi = rect.InflateCopy(1, 1);
                base.Document.Invalidate(roi);
            }
        }

        private void DrawCloneLine(Int32Point currentMouse, Int32Point lastMoved, Int32Point lastTakeFrom, Surface surfaceSource, Surface surfaceDest)
        {
            int width = (int) base.AppEnvironment.PenInfo.Width;
            Math.Ceiling((double) width);
            if (this.mouseUp || this.switchedTo)
            {
                lastMoved = currentMouse;
                lastTakeFrom = this.GetStaticData().takeFrom;
                this.mouseUp = false;
                this.switchedTo = false;
            }
            Int32Point location = new Int32Point(currentMouse.X - this.GetStaticData().takeFrom.X, currentMouse.Y - this.GetStaticData().takeFrom.Y);
            Int32Point point2 = new Int32Point(currentMouse.X - lastMoved.X, currentMouse.Y - lastMoved.Y);
            double length = ((System.Windows.Point) point2).ToVector().Length;
            double d = 1f + (base.AppEnvironment.PenInfo.Width / 2f);
            UnsafeList<Int32Rect> interiorScansUnsafeList = this.clipRegion.GetInteriorScansUnsafeList();
            Int32Rect rect = Int32RectUtil.FromPixelPoints(lastMoved, currentMouse).InflateCopy(((width / 2) + 1), ((width / 2) + 1)).IntersectCopy(Int32RectUtil.From(location, surfaceSource.Size<ColorBgra>())).IntersectCopy(surfaceDest.Bounds<ColorBgra>());
            if (!rect.HasZeroArea())
            {
                double num4;
                base.SaveRegion(null, rect);
                this.historyRects.Add(rect);
                try
                {
                    num4 = Math.Sqrt(d) / length;
                }
                catch (DivideByZeroException)
                {
                    return;
                }
                for (double i = 0.0; i < 1.0; i += num4)
                {
                    foreach (Int32Rect rect2 in interiorScansUnsafeList)
                    {
                        System.Windows.Point pt = new System.Windows.Point((currentMouse.X * (1.0 - i)) + (i * lastMoved.X), (currentMouse.Y * (1.0 - i)) + (i * lastMoved.Y));
                        Int32Rect rect3 = new Int32Rect((int) (pt.X - d), (int) (pt.Y - d), (int) ((d * 2.0) + 1.0), (int) ((d * 2.0) + 1.0));
                        Int32Rect saveMeBounds = new Int32Rect(rect3.X - location.X, rect3.Y - location.Y, rect3.Width, rect3.Height);
                        if (rect3.IntersectsWith(rect2))
                        {
                            rect3 = rect3.IntersectCopy(rect2);
                            base.SaveRegion(null, rect3);
                            base.SaveRegion(null, saveMeBounds);
                            this.DrawACircle(pt, surfaceSource, surfaceDest, location, rect3);
                        }
                    }
                }
            }
        }

        private void Environment_PenInfoChanged(object sender, EventArgs e)
        {
            this.rendererSrc.BrushSize = base.AppEnvironment.PenInfo.Width / 2f;
            this.rendererDst.BrushSize = base.AppEnvironment.PenInfo.Width / 2f;
        }

        private StaticData GetStaticData()
        {
            object staticData = base.GetStaticData();
            if (staticData == null)
            {
                staticData = new StaticData();
                base.SetStaticData(staticData);
            }
            return (StaticData) staticData;
        }

        private bool IsCtrlDown() => 
            (base.ModifierKeys == Keys.Control);

        private bool IsMouseLeftDown(MouseEventArgs e) => 
            (e.Button == MouseButtons.Left);

        private bool IsMouseRightDown(MouseEventArgs e) => 
            (e.Button == MouseButtons.Right);

        private bool IsShiftDown() => 
            (base.ModifierKeys == Keys.Shift);

        protected override void OnActivate()
        {
            base.OnActivate();
            this.cursorMouseDown = PdnResources.GetCursor2("Cursors.GenericToolCursorMouseDown.cur");
            this.cursorMouseDownSetSource = PdnResources.GetCursor2("Cursors.CloneStampToolCursorSetSource.cur");
            this.cursorMouseUp = PdnResources.GetCursor2("Cursors.CloneStampToolCursor.cur");
            base.Cursor = this.cursorMouseUp;
            this.rendererDst = new BrushPreviewRenderer(base.CanvasRenderer);
            base.CanvasRenderer.Add(this.rendererDst, false);
            this.rendererSrc = new BrushPreviewRenderer(base.CanvasRenderer);
            this.rendererSrc.BrushLocation = (System.Windows.Point) this.GetStaticData().takeFrom;
            this.rendererSrc.BrushSize = base.AppEnvironment.PenInfo.Width / 2f;
            this.rendererSrc.Visible = this.GetStaticData().takeFrom != new Int32Point(0, 0);
            base.CanvasRenderer.Add(this.rendererSrc, false);
            if (base.ActiveLayer != null)
            {
                this.switchedTo = true;
                this.historyRects = new SegmentedList<Int32Rect>();
                if ((this.GetStaticData().wr != null) && this.GetStaticData().wr.IsAlive)
                {
                    this.takeFromLayer = (BitmapLayer) this.GetStaticData().wr.Target;
                }
                else
                {
                    this.takeFromLayer = null;
                }
            }
            base.AppEnvironment.PenInfoChanged += new EventHandler(this.Environment_PenInfoChanged);
        }

        protected override void OnDeactivate()
        {
            if (!this.mouseUp)
            {
                StaticData staticData = this.GetStaticData();
                System.Drawing.Point empty = System.Drawing.Point.Empty;
                if (staticData != null)
                {
                    empty = (System.Drawing.Point) staticData.lastMoved;
                }
                this.OnMouseUp(new MouseEventArgsF(MouseButtons.Left, 0, (double) empty.X, (double) empty.Y, 0));
            }
            base.AppEnvironment.PenInfoChanged -= new EventHandler(this.Environment_PenInfoChanged);
            base.CanvasRenderer.Remove(this.rendererDst);
            this.rendererDst.Dispose();
            this.rendererDst = null;
            base.CanvasRenderer.Remove(this.rendererSrc);
            this.rendererSrc.Dispose();
            this.rendererSrc = null;
            if (this.cursorMouseDown != null)
            {
                this.cursorMouseDown.Dispose();
                this.cursorMouseDown = null;
            }
            if (this.cursorMouseUp != null)
            {
                this.cursorMouseUp.Dispose();
                this.cursorMouseUp = null;
            }
            if (this.cursorMouseDownSetSource != null)
            {
                this.cursorMouseDownSetSource.Dispose();
                this.cursorMouseDownSetSource = null;
            }
            base.OnDeactivate();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (this.IsCtrlDown() && this.mouseUp)
            {
                base.Cursor = this.cursorMouseDownSetSource;
                this.mouseDownSettingCloneSource = true;
            }
            base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (!this.IsCtrlDown() && this.mouseDownSettingCloneSource)
            {
                base.Cursor = this.cursorMouseUp;
                this.mouseDownSettingCloneSource = false;
            }
            base.OnKeyUp(e);
        }

        protected override void OnMouseDown(MouseEventArgsF e)
        {
            base.OnMouseDown(e);
            if (base.ActiveLayer is BitmapLayer)
            {
                base.Cursor = this.cursorMouseDown;
                if (this.IsMouseLeftDown(e))
                {
                    this.rendererDst.Visible = false;
                    if (this.IsCtrlDown())
                    {
                        this.GetStaticData().takeFrom = new Int32Point(e.X, e.Y);
                        this.rendererSrc.BrushLocation = new System.Windows.Point((double) e.X, (double) e.Y);
                        this.rendererSrc.BrushSize = base.AppEnvironment.PenInfo.Width / 2f;
                        this.rendererSrc.Visible = true;
                        this.GetStaticData().updateSrcPreview = false;
                        this.GetStaticData().wr = new WeakReference((BitmapLayer) base.ActiveLayer);
                        this.takeFromLayer = (BitmapLayer) this.GetStaticData().wr.Target;
                        this.GetStaticData().lastMoved = System.Drawing.Point.Empty;
                        this.ra = new RenderArgs(((BitmapLayer) base.ActiveLayer).Surface);
                    }
                    else
                    {
                        this.GetStaticData().updateSrcPreview = true;
                        if (this.GetStaticData().takeFrom != new Int32Point(0, 0))
                        {
                            if (!this.GetStaticData().wr.IsAlive || (this.takeFromLayer == null))
                            {
                                this.GetStaticData().takeFrom = System.Drawing.Point.Empty;
                                this.GetStaticData().lastMoved = System.Drawing.Point.Empty;
                            }
                            else if ((this.takeFromLayer != null) && !base.Document.Layers.Contains(this.takeFromLayer))
                            {
                                this.GetStaticData().takeFrom = System.Drawing.Point.Empty;
                                this.GetStaticData().lastMoved = System.Drawing.Point.Empty;
                            }
                            else
                            {
                                this.antialiasing = base.AppEnvironment.AntiAliasing;
                                this.ra = new RenderArgs(((BitmapLayer) base.ActiveLayer).Surface);
                                this.ra.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                                this.OnMouseMove(e);
                            }
                        }
                    }
                }
            }
        }

        protected override void OnMouseEnter()
        {
            this.rendererDst.Visible = true;
            base.OnMouseEnter();
        }

        protected override void OnMouseLeave()
        {
            this.rendererDst.Visible = false;
            base.OnMouseLeave();
        }

        protected override void OnMouseMove(MouseEventArgsF e)
        {
            base.OnMouseMove(e);
            this.rendererDst.BrushLocation = new System.Windows.Point((double) e.X, (double) e.Y);
            this.rendererDst.BrushSize = ((double) base.AppEnvironment.PenInfo.Width) / 2.0;
            if ((base.ActiveLayer is BitmapLayer) && (this.takeFromLayer != null))
            {
                if (this.GetStaticData().updateSrcPreview)
                {
                    Int32Point point = new Int32Point(e.X, e.Y);
                    Int32Point point2 = new Int32Point(point.X - this.GetStaticData().lastMoved.X, point.Y - this.GetStaticData().lastMoved.Y);
                    this.rendererSrc.BrushLocation = new System.Windows.Point((double) (this.GetStaticData().takeFrom.X + point2.X), (double) (this.GetStaticData().takeFrom.Y + point2.Y));
                    this.rendererSrc.BrushSize = base.AppEnvironment.PenInfo.Width / 2f;
                }
                if ((this.IsMouseLeftDown(e) && (this.GetStaticData().takeFrom != new Int32Point(0, 0))) && !this.IsCtrlDown())
                {
                    Int32Rect rect;
                    Int32Point currentMouse = new Int32Point(e.X, e.Y);
                    Int32Point zero = Int32Point.Zero;
                    zero = this.GetStaticData().takeFrom;
                    if (this.GetStaticData().lastMoved != new Int32Point(0, 0))
                    {
                        Int32Point point5 = new Int32Point(currentMouse.X - this.GetStaticData().lastMoved.X, currentMouse.Y - this.GetStaticData().lastMoved.Y);
                        this.GetStaticData().takeFrom = new Int32Point(this.GetStaticData().takeFrom.X + point5.X, this.GetStaticData().takeFrom.Y + point5.Y);
                    }
                    else
                    {
                        this.GetStaticData().lastMoved = currentMouse;
                    }
                    int width = (int) base.AppEnvironment.PenInfo.Width;
                    if (width != 1)
                    {
                        rect = Int32RectUtil.From(new Int32Point(this.GetStaticData().takeFrom.X - (width / 2), this.GetStaticData().takeFrom.Y - (width / 2)), new Int32Size(width + 1, width + 1));
                    }
                    else
                    {
                        rect = Int32RectUtil.From(new Int32Point(this.GetStaticData().takeFrom.X - width, this.GetStaticData().takeFrom.Y - width), new Int32Size(1 + (2 * width), 1 + (2 * width)));
                    }
                    Int32Rect subRect = Int32RectUtil.From(this.GetStaticData().takeFrom, new Int32Size(1, 1));
                    if (!base.ActiveLayer.Bounds().Contains(subRect))
                    {
                        this.GetStaticData().lastMoved = currentMouse;
                        zero = this.GetStaticData().takeFrom;
                    }
                    if (this.savedRegion != null)
                    {
                        base.ActiveLayer.Invalidate(this.savedRegion);
                        this.savedRegion.Dispose();
                        this.savedRegion = null;
                    }
                    rect = rect.IntersectCopy(this.takeFromLayer.Surface.Bounds<ColorBgra>());
                    if (!rect.HasZeroArea())
                    {
                        Surface scratchSurface;
                        this.savedRegion = new GeometryList();
                        this.savedRegion.AddRect(rect);
                        base.SaveRegion(this.savedRegion, rect);
                        if (object.ReferenceEquals(this.takeFromLayer, base.ActiveLayer))
                        {
                            scratchSurface = base.ScratchSurface;
                        }
                        else
                        {
                            scratchSurface = this.takeFromLayer.Surface;
                        }
                        if (this.clipRegion == null)
                        {
                            this.clipRegion = base.Selection.CreateGeometryListClippingMask();
                        }
                        this.DrawCloneLine(currentMouse, this.GetStaticData().lastMoved, zero, scratchSurface, ((BitmapLayer) base.ActiveLayer).Surface);
                        this.rendererSrc.BrushLocation = (System.Windows.Point) this.GetStaticData().takeFrom;
                        base.ActiveLayer.Invalidate(rect);
                        base.Update();
                        this.GetStaticData().lastMoved = currentMouse;
                    }
                }
            }
        }

        protected override void OnMouseUp(MouseEventArgsF e)
        {
            this.mouseUp = true;
            if (!this.mouseDownSettingCloneSource)
            {
                base.Cursor = this.cursorMouseUp;
            }
            if (this.IsMouseLeftDown(e))
            {
                this.rendererDst.Visible = true;
                if (this.savedRegion != null)
                {
                    base.ActiveLayer.Invalidate(this.savedRegion);
                    this.savedRegion.Dispose();
                    this.savedRegion = null;
                    base.Update();
                }
                if (((this.GetStaticData().takeFrom != new Int32Point(0, 0)) && (this.GetStaticData().lastMoved != new Int32Point(0, 0))) && (this.historyRects.Count > 0))
                {
                    GeometryList saveMeGeometry = GeometryList.FromScans(Int32RectUtil.SimplifyRegion(this.historyRects));
                    base.SaveRegion(saveMeGeometry, saveMeGeometry.Bounds.Int32Bound());
                    this.historyRects.Clear();
                    HistoryMemento memento = new BitmapHistoryMemento(base.Name, base.Image, base.DocumentWorkspace, base.ActiveLayerIndex, saveMeGeometry, base.ScratchSurface);
                    base.HistoryStack.PushNewMemento(memento);
                    base.ClearSavedMemory();
                }
            }
        }

        protected override void OnPulse()
        {
            int num3 = (int) Math.Ceiling((double) ((((Math.Sin(Timing.Global.GetTickCountDouble() / 300.0) + 1.0) / 2.0) * 224.0) + 31.0));
            this.rendererSrc.BrushAlpha = num3;
            base.OnPulse();
        }

        protected override void OnSelectionChanged()
        {
            if (this.clipRegion != null)
            {
                this.clipRegion.Dispose();
                this.clipRegion = null;
            }
            base.OnSelectionChanged();
        }

        private class StaticData
        {
            public Int32Point lastMoved;
            public Int32Point takeFrom;
            public bool updateSrcPreview;
            public WeakReference wr;
        }
    }
}


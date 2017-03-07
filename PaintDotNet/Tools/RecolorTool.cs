namespace PaintDotNet.Tools
{
    using PaintDotNet;
    using PaintDotNet.Canvas;
    using PaintDotNet.Collections;
    using PaintDotNet.Controls;
    using PaintDotNet.HistoryMementos;
    using PaintDotNet.Rendering;
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Windows;
    using System.Windows.Forms;

    internal sealed class RecolorTool : PaintDotNet.Tools.Tool
    {
        private Surface aaPoints;
        private BitmapLayer bitmapLayer;
        private UserBlendOps.NormalBlendOp blendOp;
        private RenderArgs brushRenderArgs;
        private int ceilingPenWidth;
        private PdnRegion clipRegion;
        private ColorBgra colorReplacing;
        private static ColorBgra colorToleranceBasis = ColorBgra.FromBgra(0x20, 0x20, 0x20, 0);
        private ColorBgra colorToReplace;
        private Cursor cursorMouseDown;
        private Cursor cursorMouseDownAdjustColor;
        private Cursor cursorMouseDownPickColor;
        private Cursor cursorMouseUp;
        private int halfPenWidth;
        private bool hasDrawn;
        private BitVector2D isPointAlreadyAA;
        private System.Drawing.Point lastMouseXY;
        private Keys modifierDown;
        private MouseButtons mouseButton;
        private bool mouseDown;
        private int myTolerance;
        private float penWidth;
        private BrushPreviewRenderer previewRenderer;
        private RenderArgs renderArgs;
        private SegmentedList<PlacedSurface> savedSurfaces;

        public RecolorTool(DocumentWorkspace documentWorkspace) : base(documentWorkspace, PdnResources.GetImageResource2("Icons.RecoloringToolIcon.png"), PdnResources.GetString2("RecolorTool.Name"), PdnResources.GetString2("RecolorTool.HelpText"), 'r', false, ToolBarConfigItems.Antialiasing | ToolBarConfigItems.Pen | ToolBarConfigItems.Tolerance)
        {
            this.blendOp = UserBlendOps.NormalBlendOp.Static;
        }

        public ColorBgra AAPoints(int x, int y) => 
            this.aaPoints[x, y];

        public void AAPointsAdd(int x, int y, ColorBgra color)
        {
            this.aaPoints[x, y] = color;
            this.isPointAlreadyAA[x, y] = true;
        }

        public void AAPointsRemove(int x, int y)
        {
            this.isPointAlreadyAA[x, y] = false;
        }

        private byte AdjustColorByte(byte oldByte, byte newByte, byte basisByte)
        {
            if (oldByte > newByte)
            {
                return Int32Util.ClampToByte(basisByte - (oldByte - newByte));
            }
            return Int32Util.ClampToByte(basisByte + (newByte - oldByte));
        }

        private ColorBgra AdjustColorDifference(ColorBgra oldColor, ColorBgra newColor, ColorBgra basisColor)
        {
            ColorBgra bgra = basisColor;
            bgra.B = this.AdjustColorByte(oldColor.B, newColor.B, basisColor.B);
            bgra.G = this.AdjustColorByte(oldColor.G, newColor.G, basisColor.G);
            bgra.R = this.AdjustColorByte(oldColor.R, newColor.R, basisColor.R);
            return bgra;
        }

        private void AdjustDrawingColor(MouseEventArgs e)
        {
            ColorBgra primaryColor;
            if (this.BtnDownMouseLeft(e))
            {
                primaryColor = base.AppEnvironment.PrimaryColor;
                this.PickColor(e);
                base.AppEnvironment.SecondaryColor = this.AdjustColorDifference(primaryColor, base.AppEnvironment.PrimaryColor, base.AppEnvironment.SecondaryColor);
            }
            if (this.BtnDownMouseRight(e))
            {
                primaryColor = base.AppEnvironment.SecondaryColor;
                this.PickColor(e);
                base.AppEnvironment.PrimaryColor = this.AdjustColorDifference(primaryColor, base.AppEnvironment.SecondaryColor, base.AppEnvironment.PrimaryColor);
            }
        }

        private bool BtnDownMouseLeft(MouseEventArgs e) => 
            (e.Button == MouseButtons.Left);

        private bool BtnDownMouseRight(MouseEventArgs e) => 
            (e.Button == MouseButtons.Right);

        private unsafe void DrawOverPoints(System.Drawing.Point start, System.Drawing.Point finish, ColorBgra colorToReplaceWith, ColorBgra colorBeingReplaced)
        {
            Rectangle[] regionScansReadOnlyInt;
            float positiveInfinity;
            ColorBgra rhs = ColorBgra.FromColor(Color.Empty);
            Rectangle rectangle2 = new Rectangle(0, 0, 0, 0);
            if (base.Selection.IsEmpty)
            {
                regionScansReadOnlyInt = new Rectangle[] { base.DocumentWorkspace.Document.Bounds };
            }
            else
            {
                regionScansReadOnlyInt = this.clipRegion.GetRegionScansReadOnlyInt();
            }
            System.Drawing.Point point = new System.Drawing.Point(finish.X - start.X, finish.Y - start.Y);
            float num2 = ((PointF) point).Length();
            float num3 = base.AppEnvironment.PenInfo.Width / 2f;
            if (num2 == 0f)
            {
                positiveInfinity = float.PositiveInfinity;
            }
            else
            {
                positiveInfinity = ((float) Math.Sqrt((double) num3)) / num2;
            }
            for (float i = 0f; i < 1f; i += positiveInfinity)
            {
                PointF tf = new PointF((finish.X * (1f - i)) + (i * start.X), (finish.Y * (1f - i)) + (i * start.Y));
                System.Drawing.Point point2 = System.Drawing.Point.Round(tf);
                foreach (Rectangle rectangle3 in regionScansReadOnlyInt)
                {
                    Rectangle rectangle = new Rectangle(point2.X - this.halfPenWidth, point2.Y - this.halfPenWidth, this.ceilingPenWidth, this.ceilingPenWidth);
                    if (rectangle.IntersectsWith(rectangle3))
                    {
                        rectangle.Intersect(rectangle3);
                        for (int j = rectangle.Top; j < rectangle.Bottom; j++)
                        {
                            ColorBgra* pointAddress;
                            ColorBgra* bgraPtr2;
                            rectangle2.X = Math.Max(rectangle3.X - (point2.X - this.halfPenWidth), 0);
                            rectangle2.Y = Math.Max(rectangle3.Y - (point2.Y - this.halfPenWidth), 0);
                            rectangle2.Size = rectangle.Size;
                            try
                            {
                                pointAddress = this.brushRenderArgs.Surface.GetPointAddress(rectangle2.Left, rectangle2.Y + (j - rectangle.Y));
                                bgraPtr2 = this.renderArgs.Surface.GetPointAddress(rectangle.Left, j);
                            }
                            catch
                            {
                                break;
                            }
                            for (int k = rectangle.Left; k < rectangle.Right; k++)
                            {
                                if (pointAddress->A != 0)
                                {
                                    ColorBgra colorA = bgraPtr2[0];
                                    bool flag = this.IsColorInTolerance(colorA, colorBeingReplaced);
                                    bool flag2 = false;
                                    if (base.AppEnvironment.AntiAliasing)
                                    {
                                        flag2 = this.IsPointAlreadyAntiAliased(k, j);
                                    }
                                    if (flag || flag2)
                                    {
                                        if (flag2)
                                        {
                                            rhs = this.AAPoints(k, j);
                                            if (this.penWidth < 2f)
                                            {
                                                rhs.B = Int32Util.ClampToByte(colorToReplaceWith.B + (rhs.B - colorBeingReplaced.B));
                                                rhs.G = Int32Util.ClampToByte(colorToReplaceWith.G + (rhs.G - colorBeingReplaced.G));
                                                rhs.R = Int32Util.ClampToByte(colorToReplaceWith.R + (rhs.R - colorBeingReplaced.R));
                                                rhs.A = Int32Util.ClampToByte(colorToReplaceWith.A + (rhs.A - colorBeingReplaced.A));
                                            }
                                        }
                                        else
                                        {
                                            rhs.B = Int32Util.ClampToByte(colorA.B + (colorToReplaceWith.B - colorBeingReplaced.B));
                                            rhs.G = Int32Util.ClampToByte(colorA.G + (colorToReplaceWith.G - colorBeingReplaced.G));
                                            rhs.R = Int32Util.ClampToByte(colorA.R + (colorToReplaceWith.R - colorBeingReplaced.R));
                                            rhs.A = Int32Util.ClampToByte(colorA.A + (colorToReplaceWith.A - colorBeingReplaced.A));
                                        }
                                        if ((pointAddress->A != 0xff) && base.AppEnvironment.AntiAliasing)
                                        {
                                            rhs.A = pointAddress->A;
                                            byte a = bgraPtr2->A;
                                            bgraPtr2[0] = this.blendOp.Apply(bgraPtr2[0], rhs);
                                            bgraPtr2->A = a;
                                            if (!this.IsPointAlreadyAntiAliased(k, j))
                                            {
                                                this.AAPointsAdd(k, j, rhs);
                                            }
                                        }
                                        else
                                        {
                                            rhs.A = bgraPtr2->A;
                                            bgraPtr2[0] = rhs;
                                            if (flag2)
                                            {
                                                this.AAPointsRemove(k, j);
                                            }
                                        }
                                        this.hasDrawn = true;
                                    }
                                }
                                pointAddress++;
                                bgraPtr2++;
                            }
                        }
                    }
                }
            }
        }

        private bool IsColorInTolerance(ColorBgra colorA, ColorBgra colorB) => 
            (Utility.ColorDifference(colorA, colorB) <= this.myTolerance);

        private bool IsPointAlreadyAntiAliased(System.Drawing.Point pt) => 
            this.IsPointAlreadyAntiAliased(pt.X, pt.Y);

        private bool IsPointAlreadyAntiAliased(int x, int y) => 
            this.isPointAlreadyAA[x, y];

        private bool KeyDownControlOnly() => 
            (base.ModifierKeys == Keys.Control);

        private bool KeyDownShiftOnly() => 
            (base.ModifierKeys == Keys.Shift);

        private ColorBgra LiftColor(int x, int y) => 
            ((BitmapLayer) base.ActiveLayer).Surface[x, y];

        protected override void OnActivate()
        {
            base.OnActivate();
            this.cursorMouseUp = PdnResources.GetCursor2("Cursors.RecoloringToolCursor.cur");
            this.cursorMouseDown = PdnResources.GetCursor2("Cursors.GenericToolCursorMouseDown.cur");
            this.cursorMouseDownPickColor = PdnResources.GetCursor2("Cursors.RecoloringToolCursorPickColor.cur");
            this.cursorMouseDownAdjustColor = PdnResources.GetCursor2("Cursors.RecoloringToolCursorAdjustColor.cur");
            this.previewRenderer = new BrushPreviewRenderer(base.CanvasRenderer);
            base.CanvasRenderer.Add(this.previewRenderer, false);
            base.Cursor = this.cursorMouseUp;
            this.mouseDown = false;
            this.colorToReplace = base.AppEnvironment.PrimaryColor;
            this.colorReplacing = base.AppEnvironment.SecondaryColor;
            this.aaPoints = base.ScratchSurface;
            this.isPointAlreadyAA = new BitVector2D(this.aaPoints.Width, this.aaPoints.Height);
            if (this.savedSurfaces != null)
            {
                foreach (PlacedSurface surface in this.savedSurfaces)
                {
                    surface.Dispose();
                }
            }
            this.savedSurfaces = new SegmentedList<PlacedSurface>();
            if (base.ActiveLayer != null)
            {
                this.bitmapLayer = (BitmapLayer) base.ActiveLayer;
                this.renderArgs = new RenderArgs(this.bitmapLayer.Surface);
            }
            else
            {
                this.bitmapLayer = null;
                this.renderArgs = null;
            }
        }

        protected override void OnDeactivate()
        {
            base.OnDeactivate();
            if (this.mouseDown)
            {
                this.OnMouseUp(new MouseEventArgsF(this.mouseButton, 0, (double) this.lastMouseXY.X, (double) this.lastMouseXY.Y, 0));
            }
            base.CanvasRenderer.Remove(this.previewRenderer);
            this.previewRenderer.Dispose();
            this.previewRenderer = null;
            if (this.savedSurfaces != null)
            {
                foreach (PlacedSurface surface in this.savedSurfaces)
                {
                    surface.Dispose();
                }
                this.savedSurfaces.Clear();
                this.savedSurfaces = null;
            }
            this.renderArgs.Dispose();
            this.renderArgs = null;
            this.aaPoints = null;
            this.renderArgs = null;
            this.bitmapLayer = null;
            if (this.clipRegion != null)
            {
                this.clipRegion.Dispose();
                this.clipRegion = null;
            }
            if (this.cursorMouseUp != null)
            {
                this.cursorMouseUp.Dispose();
                this.cursorMouseUp = null;
            }
            if (this.cursorMouseDown != null)
            {
                this.cursorMouseDown.Dispose();
                this.cursorMouseDown = null;
            }
            if (this.cursorMouseDownPickColor != null)
            {
                this.cursorMouseDownPickColor.Dispose();
                this.cursorMouseDownPickColor = null;
            }
            if (this.cursorMouseDownAdjustColor != null)
            {
                this.cursorMouseDownAdjustColor.Dispose();
                this.cursorMouseDownAdjustColor = null;
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (((this.modifierDown != Keys.Control) && (this.modifierDown != Keys.Shift)) && !this.mouseDown)
            {
                if (this.KeyDownControlOnly())
                {
                    base.Cursor = this.cursorMouseDownPickColor;
                }
                else if (this.KeyDownShiftOnly())
                {
                    base.Cursor = this.cursorMouseDownAdjustColor;
                }
                else
                {
                    base.OnKeyDown(e);
                }
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if ((!this.KeyDownControlOnly() && !this.KeyDownShiftOnly()) && !this.mouseDown)
            {
                this.modifierDown = Keys.None;
                base.Cursor = this.cursorMouseUp;
            }
            base.OnKeyUp(e);
        }

        protected override void OnMouseDown(MouseEventArgsF e)
        {
            base.OnMouseDown(e);
            if (!this.mouseDown && (this.BtnDownMouseLeft(e) || this.BtnDownMouseRight(e)))
            {
                this.previewRenderer.Visible = false;
                this.mouseDown = true;
                base.Cursor = this.cursorMouseDown;
                if (!this.KeyDownControlOnly() && !this.KeyDownShiftOnly())
                {
                    this.mouseButton = e.Button;
                    this.lastMouseXY.X = e.X;
                    this.lastMouseXY.Y = e.Y;
                    if (this.clipRegion != null)
                    {
                        this.clipRegion.Dispose();
                        this.clipRegion = null;
                    }
                    this.clipRegion = base.Selection.CreateRegion();
                    this.renderArgs.Graphics.SetClip(this.clipRegion.GetRegionReadOnly(), CombineMode.Replace);
                    this.colorReplacing = base.AppEnvironment.PrimaryColor;
                    this.colorToReplace = base.AppEnvironment.SecondaryColor;
                    this.penWidth = base.AppEnvironment.PenInfo.Width;
                    this.ceilingPenWidth = (int) Math.Max(Math.Ceiling((double) this.penWidth), 3.0);
                    this.halfPenWidth = (int) Math.Ceiling((double) (this.penWidth / 2f));
                    this.hasDrawn = false;
                    this.brushRenderArgs = this.RenderCircleBrush();
                    this.myTolerance = (int) (base.AppEnvironment.Tolerance * 256f);
                    this.RestrictTolerance();
                    this.OnMouseMove(e);
                }
                else
                {
                    this.modifierDown = base.ModifierKeys;
                    this.OnMouseMove(e);
                }
            }
        }

        protected override void OnMouseEnter()
        {
            this.previewRenderer.Visible = true;
            base.OnMouseEnter();
        }

        protected override void OnMouseLeave()
        {
            this.previewRenderer.Visible = false;
            base.OnMouseLeave();
        }

        protected override void OnMouseMove(MouseEventArgsF e)
        {
            base.OnMouseMove(e);
            this.previewRenderer.BrushLocation = new System.Windows.Point((double) e.X, (double) e.Y);
            this.previewRenderer.BrushSize = ((double) base.AppEnvironment.PenInfo.Width) / 2.0;
            if (this.mouseDown && (this.BtnDownMouseLeft(e) || this.BtnDownMouseRight(e)))
            {
                if (this.modifierDown == Keys.None)
                {
                    if (this.colorReplacing != this.colorToReplace)
                    {
                        System.Drawing.Point lastMouseXY = this.lastMouseXY;
                        System.Drawing.Point b = new System.Drawing.Point(e.X, e.Y);
                        Int32Rect rect = Int32RectUtil.FromPixelPoints(lastMouseXY, b).InflateCopy((1 + (this.ceilingPenWidth / 2)), (1 + (this.ceilingPenWidth / 2))).IntersectCopy(base.ActiveLayer.Bounds());
                        bool flag = rect.Width > 0;
                        bool flag2 = rect.Height > 0;
                        bool flag3 = this.renderArgs.Graphics.IsVisible(rect.ToGdipRectangle());
                        if ((flag && flag2) && flag3)
                        {
                            PlacedSurface item = new PlacedSurface(this.renderArgs.Surface, rect);
                            this.savedSurfaces.Add(item);
                            this.renderArgs.Graphics.CompositingMode = CompositingMode.SourceOver;
                            if (this.BtnDownMouseLeft(e))
                            {
                                this.DrawOverPoints(lastMouseXY, b, this.colorReplacing, this.colorToReplace);
                            }
                            else
                            {
                                this.DrawOverPoints(lastMouseXY, b, this.colorToReplace, this.colorReplacing);
                            }
                            this.bitmapLayer.Invalidate(rect);
                            base.Update();
                        }
                        this.lastMouseXY = b;
                    }
                }
                else
                {
                    switch ((this.modifierDown & (Keys.Control | Keys.Shift)))
                    {
                        case Keys.Shift:
                            this.AdjustDrawingColor(e);
                            break;

                        case Keys.Control:
                            this.PickColor(e);
                            break;
                    }
                }
            }
        }

        protected override void OnMouseUp(MouseEventArgsF e)
        {
            base.OnMouseUp(e);
            if (!this.KeyDownShiftOnly() && !this.KeyDownControlOnly())
            {
                base.Cursor = this.cursorMouseUp;
            }
            if (this.mouseDown)
            {
                this.previewRenderer.Visible = true;
                this.OnMouseMove(e);
                if (this.savedSurfaces.Count > 0)
                {
                    GeometryList roi = GeometryList.FromScans((from ps in this.savedSurfaces select ps.Bounds).ToArrayEx<Int32Rect>());
                    using (IrregularSurface surface = new IrregularSurface(this.renderArgs.Surface, roi))
                    {
                        for (int i = this.savedSurfaces.Count - 1; i >= 0; i--)
                        {
                            PlacedSurface surface2 = this.savedSurfaces[i];
                            surface2.Draw(this.renderArgs.Surface);
                            surface2.Dispose();
                        }
                        this.savedSurfaces.Clear();
                        if (this.hasDrawn)
                        {
                            HistoryMemento memento = new BitmapHistoryMemento(base.Name, base.Image, base.DocumentWorkspace, base.ActiveLayerIndex, roi);
                            surface.Draw(this.bitmapLayer.Surface);
                            base.HistoryStack.PushNewMemento(memento);
                        }
                    }
                }
                this.mouseDown = false;
                this.modifierDown = Keys.None;
            }
            if ((this.brushRenderArgs != null) && (this.brushRenderArgs.Surface != null))
            {
                this.brushRenderArgs.Surface.Dispose();
            }
        }

        private void PickColor(MouseEventArgs e)
        {
            if (base.DocumentWorkspace.Document.Bounds.Contains(e.X, e.Y) && (this.BtnDownMouseLeft(e) || this.BtnDownMouseRight(e)))
            {
                if (this.BtnDownMouseLeft(e))
                {
                    this.colorReplacing = this.LiftColor(e.X, e.Y);
                    this.colorReplacing.A = base.AppEnvironment.PrimaryColor.A;
                    base.AppEnvironment.PrimaryColor = this.colorReplacing;
                }
                else
                {
                    this.colorToReplace = this.LiftColor(e.X, e.Y);
                    this.colorToReplace.A = base.AppEnvironment.SecondaryColor.A;
                    base.AppEnvironment.SecondaryColor = this.colorToReplace;
                }
            }
        }

        private RenderArgs RenderCircleBrush()
        {
            Surface surface = new Surface(this.ceilingPenWidth, this.ceilingPenWidth);
            surface.Clear((ColorBgra) 0);
            RenderArgs args = new RenderArgs(surface);
            if (base.AppEnvironment.AntiAliasing)
            {
                args.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            }
            else
            {
                args.Graphics.SmoothingMode = SmoothingMode.None;
            }
            if (base.AppEnvironment.AntiAliasing)
            {
                if (this.penWidth > 2f)
                {
                    this.penWidth--;
                }
                else
                {
                    this.penWidth /= 2f;
                }
            }
            else if (this.penWidth <= 1f)
            {
                args.Surface[1, 1] = ColorBgra.Black;
            }
            else
            {
                this.penWidth = (float) Math.Round((double) (this.penWidth + 1f));
            }
            using (Brush brush = new SolidBrush(Color.Black))
            {
                args.Graphics.FillEllipse(brush, 0f, 0f, this.penWidth, this.penWidth);
            }
            return args;
        }

        private void RestrictTolerance()
        {
            int num = Utility.ColorDifference(this.colorReplacing, this.colorToReplace);
            if (this.myTolerance > num)
            {
                this.myTolerance = num;
            }
        }
    }
}


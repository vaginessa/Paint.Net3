namespace PaintDotNet.Actions
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.HistoryMementos;
    using PaintDotNet.Rendering;
    using PaintDotNet.SystemLayer;
    using PaintDotNet.Tools;
    using PaintDotNet.VisualStyling;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Windows;
    using System.Windows.Forms;
    using System.Windows.Media;

    internal sealed class PasteAction
    {
        private IDataObject clipData;
        private DocumentWorkspace documentWorkspace;
        private MaskedSurface maskedSurface;

        public PasteAction(DocumentWorkspace documentWorkspace) : this(documentWorkspace, null, null)
        {
        }

        public PasteAction(DocumentWorkspace documentWorkspace, IDataObject clipData, MaskedSurface maskedSurface)
        {
            this.documentWorkspace = documentWorkspace;
            this.clipData = clipData;
            this.maskedSurface = maskedSurface;
        }

        private static Surface CreateThumbnail(MaskedSurface maskedSurface)
        {
            int thumbSideLength = UI.ScaleWidth(120);
            Surface surfaceReadOnly = maskedSurface.SurfaceReadOnly;
            GeometryList geometryMaskCopy = maskedSurface.GetGeometryMaskCopy();
            Int32Rect maskBounds = maskedSurface.GeometryMaskBounds.Int32Bound();
            Surface surface2 = CreateThumbnail(surfaceReadOnly, geometryMaskCopy, maskBounds, thumbSideLength);
            geometryMaskCopy.Dispose();
            return surface2;
        }

        public static Surface CreateThumbnail(Surface sourceSurface, GeometryList maskGeometry, Int32Rect maskBounds, int thumbSideLength)
        {
            Surface dst = new Surface(Utility.ComputeThumbnailSize(sourceSurface.Size<ColorBgra>(), thumbSideLength));
            dst.Clear(ColorBgra.Transparent);
            sourceSurface.ResizeSuperSampling(dst.Size<ColorBgra>()).Parallelize(5).Render<ColorBgra>(dst);
            Surface surface = new Surface(dst.Size<ColorBgra>());
            surface.Clear(ColorBgra.Black);
            using (PdnGraphicsPath path = new PdnGraphicsPath())
            {
                path.AddGeometryList(maskGeometry);
                double scaleX = (maskBounds.Width == 0) ? 0.0 : (((double) dst.Width) / ((double) maskBounds.Width));
                double scaleY = (maskBounds.Height == 0) ? 0.0 : (((double) dst.Height) / ((double) maskBounds.Height));
                System.Windows.Media.Matrix m = new System.Windows.Media.Matrix();
                m.Translate((double) -maskBounds.X, (double) -maskBounds.Y);
                m.Scale(scaleX, scaleY);
                using (RenderArgs args = new RenderArgs(surface))
                {
                    args.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    using (System.Drawing.Drawing2D.Matrix matrix2 = m.ToGdipMatrix())
                    {
                        args.Graphics.Transform = matrix2;
                    }
                    args.Graphics.FillPath(Brushes.White, (GraphicsPath) path);
                    args.Graphics.DrawPath(Pens.White, (GraphicsPath) path);
                }
            }
            new IntensityMaskOp().Apply(surface, dst, surface);
            RendererBgra.Checkers(dst.Size<ColorBgra>()).Render<ColorBgra>(dst);
            UserBlendOps.NormalBlendOp.Static.Apply(dst, dst, surface);
            surface.Dispose();
            surface = null;
            int recommendedExtent = DropShadow.GetRecommendedExtent(dst.Size<ColorBgra>());
            ShadowDecorationRenderer renderer = new ShadowDecorationRenderer(dst, recommendedExtent);
            Surface surface3 = new Surface(renderer.Size<ColorBgra>());
            renderer.Render<ColorBgra>(surface3);
            return surface3;
        }

        public bool PerformAction()
        {
            bool flag;
            try
            {
                flag = this.PerformActionImpl();
            }
            finally
            {
                this.clipData = null;
                this.maskedSurface = null;
            }
            return flag;
        }

        private bool PerformActionImpl()
        {
            if (this.clipData == null)
            {
                try
                {
                    using (new WaitCursorChanger(this.documentWorkspace))
                    {
                        Utility.GCFullCollect();
                        this.clipData = System.Windows.Forms.Clipboard.GetDataObject();
                    }
                }
                catch (OutOfMemoryException)
                {
                    Utility.ErrorBox(this.documentWorkspace, PdnResources.GetString2("PasteAction.Error.OutOfMemory"));
                    return false;
                }
                catch (Exception)
                {
                    Utility.ErrorBox(this.documentWorkspace, PdnResources.GetString2("PasteAction.Error.TransferFromClipboard"));
                    return false;
                }
            }
            bool handled = false;
            if (this.documentWorkspace.Tool != null)
            {
                this.documentWorkspace.Tool.PerformPaste(this.clipData, out handled);
            }
            if (!handled)
            {
                System.Drawing.Point point;
                if (this.maskedSurface == null)
                {
                    try
                    {
                        using (new WaitCursorChanger(this.documentWorkspace))
                        {
                            this.maskedSurface = ClipboardUtil.GetClipboardImage(this.documentWorkspace, this.clipData);
                        }
                    }
                    catch (OutOfMemoryException)
                    {
                        Utility.ErrorBox(this.documentWorkspace, PdnResources.GetString2("PasteAction.Error.OutOfMemory"));
                        return false;
                    }
                    catch (Exception)
                    {
                        Utility.ErrorBox(this.documentWorkspace, PdnResources.GetString2("PasteAction.Error.TransferFromClipboard"));
                        return false;
                    }
                }
                if (this.maskedSurface == null)
                {
                    Utility.ErrorBox(this.documentWorkspace, PdnResources.GetString2("PasteAction.Error.NoImage"));
                    return false;
                }
                Int32Rect rect = this.maskedSurface.GetGeometryMaskScans().Bounds();
                if ((rect.Width > this.documentWorkspace.Document.Width) || (rect.Height > this.documentWorkspace.Document.Height))
                {
                    Surface surface;
                    try
                    {
                        using (new WaitCursorChanger(this.documentWorkspace))
                        {
                            surface = CreateThumbnail(this.maskedSurface);
                        }
                    }
                    catch (OutOfMemoryException)
                    {
                        surface = null;
                    }
                    DialogResult result = ShowExpandCanvasTaskDialog(this.documentWorkspace, surface);
                    int activeLayerIndex = this.documentWorkspace.ActiveLayerIndex;
                    ColorBgra secondaryColor = this.documentWorkspace.AppWorkspace.AppEnvironment.SecondaryColor;
                    switch (result)
                    {
                        case DialogResult.Yes:
                        {
                            int width = Math.Max(rect.Width, this.documentWorkspace.Document.Width);
                            System.Drawing.Size newSize = new System.Drawing.Size(width, Math.Max(rect.Height, this.documentWorkspace.Document.Height));
                            Document document = CanvasSizeAction.ResizeDocument(this.documentWorkspace.Document, newSize, AnchorEdge.TopLeft, secondaryColor);
                            if (document != null)
                            {
                                HistoryMemento memento = new ReplaceDocumentHistoryMemento(CanvasSizeAction.StaticName, CanvasSizeAction.StaticImage, this.documentWorkspace);
                                this.documentWorkspace.Document = document;
                                this.documentWorkspace.History.PushNewMemento(memento);
                                this.documentWorkspace.ActiveLayer = (Layer) this.documentWorkspace.Document.Layers[activeLayerIndex];
                                break;
                            }
                            return false;
                        }
                        case DialogResult.No:
                            break;

                        case DialogResult.Cancel:
                            return false;

                        default:
                            throw new InvalidEnumArgumentException("Internal error: DialogResult was neither Yes, No, nor Cancel");
                    }
                }
                Int32Rect rect2 = this.documentWorkspace.Document.Bounds();
                Rect visibleDocumentRect = this.documentWorkspace.VisibleDocumentRect;
                Int32Rect? nullable = visibleDocumentRect.Int32Inset();
                Rect rect4 = nullable.HasValue ? nullable.Value.ToRect() : visibleDocumentRect;
                Int32Rect rect5 = rect4.Int32Bound();
                if (rect4.Contains(rect.ToRect()))
                {
                    point = new System.Drawing.Point(0, 0);
                }
                else
                {
                    int num2;
                    int num3;
                    int num4;
                    int num5;
                    if (rect.X < rect4.Left)
                    {
                        num2 = -rect.X + rect5.X;
                    }
                    else if (rect.Right() > rect5.Right())
                    {
                        num2 = (-rect.X + rect5.Right()) - rect.Width;
                    }
                    else
                    {
                        num2 = 0;
                    }
                    if (rect.Y < rect4.Top)
                    {
                        num3 = -rect.Y + rect5.Y;
                    }
                    else if (rect.Bottom() > rect5.Bottom())
                    {
                        num3 = (-rect.Y + rect5.Bottom()) - rect.Height;
                    }
                    else
                    {
                        num3 = 0;
                    }
                    Int32Point point2 = new System.Drawing.Point(num2, num3);
                    Int32Rect rect6 = new Int32Rect(rect.X + point2.X, rect.Y + point2.Y, rect.Width, rect.Height);
                    if (rect6.X < 0)
                    {
                        num4 = num2 - rect6.X;
                    }
                    else
                    {
                        num4 = num2;
                    }
                    if (rect6.Y < 0)
                    {
                        num5 = num3 - rect6.Y;
                    }
                    else
                    {
                        num5 = num3;
                    }
                    Int32Point point3 = new Int32Point(num4, num5);
                    Int32Rect subRect = new Int32Rect(rect.X + point3.X, rect.Y + point3.Y, rect.Width, rect.Height);
                    if (rect2.Contains(subRect))
                    {
                        point = (System.Drawing.Point) point3;
                    }
                    else
                    {
                        Int32Point point4 = point3;
                        if (subRect.Right() > rect2.Right())
                        {
                            int num6 = subRect.Right() - rect2.Right();
                            int num7 = Math.Min(num6, subRect.Left());
                            point4.X -= num7;
                        }
                        if (subRect.Bottom() > rect2.Bottom())
                        {
                            int num8 = subRect.Bottom() - rect2.Bottom();
                            int num9 = Math.Min(num8, subRect.Top());
                            point4.Y -= num9;
                        }
                        point = (System.Drawing.Point) point4;
                    }
                }
                Int32Rect rect9 = this.documentWorkspace.VisibleDocumentRect.Int32Bound();
                Int32Rect rect10 = new Int32Rect(rect.X + point.X, rect.Y + point.Y, rect.Width, rect.Height);
                bool isEmpty = Int32RectUtil.Intersect(rect10, rect9).IsEmpty;
                this.documentWorkspace.SetTool(null);
                this.documentWorkspace.SetToolFromType(typeof(MoveTool));
                ((MoveTool) this.documentWorkspace.Tool).PasteMouseDown(this.maskedSurface, point);
                if (isEmpty)
                {
                    int introduced49 = rect9.Left();
                    int introduced50 = rect9.Top();
                    Int32Point point5 = new Int32Point(introduced49 + (rect9.Width / 2), introduced50 + (rect9.Height / 2));
                    int introduced51 = rect10.Left();
                    int introduced52 = rect10.Top();
                    Int32Point point6 = new Int32Point(introduced51 + (rect10.Width / 2), introduced52 + (rect10.Height / 2));
                    Int32Size size2 = new Int32Size(point6.X - point5.X, point6.Y - point5.Y);
                    System.Windows.Point documentScrollPosition = this.documentWorkspace.DocumentScrollPosition;
                    System.Windows.Point point8 = new System.Windows.Point(documentScrollPosition.X + size2.Width, documentScrollPosition.Y + size2.Height);
                    this.documentWorkspace.DocumentScrollPosition = point8;
                }
            }
            return true;
        }

        private static DialogResult ShowExpandCanvasTaskDialog(IWin32Window owner, Surface thumbnail)
        {
            DialogResult yes;
            Icon icon = Utility.ImageToIcon(PdnResources.GetImageResource2("Icons.MenuEditPasteIcon.png").Reference);
            string str = PdnResources.GetString2("ExpandCanvasQuestion.Title");
            RenderArgs args = new RenderArgs(thumbnail);
            Image bitmap = args.Bitmap;
            string str2 = PdnResources.GetString2("ExpandCanvasQuestion.IntroText");
            TaskButton button = new TaskButton(PdnResources.GetImageResource2("Icons.ExpandCanvasQuestion.YesTB.Image.png").Reference, PdnResources.GetString2("ExpandCanvasQuestion.YesTB.ActionText"), PdnResources.GetString2("ExpandCanvasQuestion.YesTB.ExplanationText"));
            TaskButton button2 = new TaskButton(PdnResources.GetImageResource2("Icons.ExpandCanvasQuestion.NoTB.Image.png").Reference, PdnResources.GetString2("ExpandCanvasQuestion.NoTB.ActionText"), PdnResources.GetString2("ExpandCanvasQuestion.NoTB.ExplanationText"));
            TaskButton button3 = new TaskButton(PdnResources.GetImageResource2("Icons.CancelIcon.png").Reference, PdnResources.GetString2("ExpandCanvasQuestion.CancelTB.ActionText"), PdnResources.GetString2("ExpandCanvasQuestion.CancelTB.ExplanationText"));
            int num = (TaskDialog.DefaultPixelWidth96Dpi * 3) / 2;
            TaskDialog dialog2 = new TaskDialog {
                Icon = icon,
                Title = str,
                TaskImage = bitmap,
                ScaleTaskImageWithDpi = true,
                IntroText = str2,
                TaskButtons = new TaskButton[] { 
                    button,
                    button2,
                    button3
                },
                AcceptButton = button,
                CancelButton = button3,
                PixelWidth96Dpi = num
            };
            TaskButton button4 = dialog2.Show(owner);
            if (button4 == button)
            {
                yes = DialogResult.Yes;
            }
            else if (button4 == button2)
            {
                yes = DialogResult.No;
            }
            else
            {
                yes = DialogResult.Cancel;
            }
            args.Dispose();
            args = null;
            return yes;
        }

        private sealed class IntensityMaskOp : BinaryPixelOp
        {
            public override ColorBgra Apply(ColorBgra lhs, ColorBgra rhs)
            {
                byte intensityByte = rhs.GetIntensityByte();
                return ColorBgra.FromBgra(lhs.B, lhs.G, lhs.R, ByteUtil.FastScale(intensityByte, lhs.A));
            }
        }
    }
}


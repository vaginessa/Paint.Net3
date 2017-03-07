namespace PaintDotNet.Actions
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.Functional;
    using PaintDotNet.Rendering;
    using PaintDotNet.SystemLayer;
    using PaintDotNet.VisualStyling;
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.Threading;
    using System.Windows.Forms;

    internal sealed class CloseWorkspaceAction : AppWorkspaceAction
    {
        private bool cancelled;
        private DocumentWorkspace closeMe;

        public CloseWorkspaceAction() : this(null)
        {
        }

        public CloseWorkspaceAction(DocumentWorkspace closeMe)
        {
            this.closeMe = closeMe;
            this.cancelled = false;
        }

        public override void PerformAction(AppWorkspace appWorkspace)
        {
            DocumentWorkspace dw;
            if (appWorkspace == null)
            {
                throw new ArgumentNullException("appWorkspace");
            }
            if (this.closeMe == null)
            {
                dw = appWorkspace.ActiveDocumentWorkspace;
            }
            else
            {
                dw = this.closeMe;
            }
            if (dw != null)
            {
                if (dw.Document == null)
                {
                    appWorkspace.RemoveDocumentWorkspace(dw);
                }
                else if (!dw.Document.Dirty)
                {
                    appWorkspace.RemoveDocumentWorkspace(dw);
                }
                else
                {
                    WaitCallback callBack = null;
                    appWorkspace.ActiveDocumentWorkspace = dw;
                    TaskButton button = new TaskButton(PdnResources.GetImageResource2("Icons.MenuFileSaveIcon.png").Reference, PdnResources.GetString2("CloseWorkspaceAction.SaveButton.ActionText"), PdnResources.GetString2("CloseWorkspaceAction.SaveButton.ExplanationText"));
                    TaskButton button2 = new TaskButton(PdnResources.GetImageResource2("Icons.MenuFileCloseIcon.png").Reference, PdnResources.GetString2("CloseWorkspaceAction.DontSaveButton.ActionText"), PdnResources.GetString2("CloseWorkspaceAction.DontSaveButton.ExplanationText"));
                    TaskButton button3 = new TaskButton(PdnResources.GetImageResource2("Icons.CancelIcon.png").Reference, PdnResources.GetString2("CloseWorkspaceAction.CancelButton.ActionText"), PdnResources.GetString2("CloseWorkspaceAction.CancelButton.ExplanationText"));
                    string str = PdnResources.GetString2("CloseWorkspaceAction.Title");
                    string str3 = string.Format(PdnResources.GetString2("CloseWorkspaceAction.IntroText.Format"), dw.GetFriendlyName());
                    int thumbEdgeLength = UI.ScaleWidth(80);
                    Int32Size size = Utility.ComputeThumbnailSize(dw.Document.Size(), thumbEdgeLength);
                    Int32Size fullThumbSize = new Int32Size(size.Width + 4, size.Height + 4);
                    bool animating = true;
                    Image finalThumb = null;
                    Action<Image> animationEvent = null;
                    Image[] busyAnimationFrames = AnimationResources.Working;
                    Image[] busyAnimationThumbs = new Image[busyAnimationFrames.Length];
                    int animationHz = 50;
                    Timing timing = new Timing();
                    timing.GetTickCount();
                    EventHandler handler2 = null;
                    using (System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer())
                    {
                        Icon icon;
                        timer.Interval = animationHz / 2;
                        timer.Enabled = true;
                        if (handler2 == null)
                        {
                            handler2 = delegate {
                                if (!animating)
                                {
                                    timer.Enabled = false;
                                    animationEvent(finalThumb);
                                    timer.Dispose();
                                    for (int j = 0; j < busyAnimationThumbs.Length; j++)
                                    {
                                        if (busyAnimationThumbs[j] != null)
                                        {
                                            busyAnimationThumbs[j].Dispose();
                                        }
                                    }
                                    busyAnimationThumbs = null;
                                }
                                else
                                {
                                    int num2 = (int) (timing.GetTickCount() / ((long) animationHz));
                                    int index = num2 % busyAnimationFrames.Length;
                                    Image image = busyAnimationFrames[index];
                                    if (busyAnimationThumbs[index] == null)
                                    {
                                        Bitmap bitmap = new Bitmap(fullThumbSize.Width, fullThumbSize.Height, PixelFormat.Format32bppArgb);
                                        using (Graphics graphics = Graphics.FromImage(bitmap))
                                        {
                                            graphics.CompositingMode = CompositingMode.SourceCopy;
                                            graphics.Clear(Color.Transparent);
                                            graphics.DrawImage(image, new Rectangle((bitmap.Width - image.Width) / 2, (bitmap.Height - image.Height) / 2, image.Width, image.Height), new Rectangle(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);
                                        }
                                        busyAnimationThumbs[index] = bitmap;
                                    }
                                    animationEvent(busyAnimationThumbs[index]);
                                }
                            };
                        }
                        EventHandler handler = handler2;
                        timer.Tick += handler;
                        if (callBack == null)
                        {
                            callBack =  => delegate {
                                using (new ThreadBackground(ThreadBackgroundFlags.All))
                                {
                                    IRenderer<ColorBgra> source = dw.CreateThumbnailRenderer(thumbEdgeLength);
                                    ShadowDecorationRenderer renderer = new ShadowDecorationRenderer(source, DropShadow.GetRecommendedExtent(source.Size<ColorBgra>()));
                                    ISurface<ColorBgra> surface = renderer.Parallelize(4).ToSurface();
                                    Bitmap original = surface.CreateAliasedGdipBitmap();
                                    finalThumb = new Bitmap(original);
                                    original.Dispose();
                                    surface.Dispose();
                                    animating = false;
                                }
                            }.Try().Observe();
                        }
                        ThreadPool.QueueUserWorkItem(callBack);
                        Bitmap bitmap = new Bitmap(fullThumbSize.Width, fullThumbSize.Height, PixelFormat.Format32bppArgb);
                        Form form = appWorkspace.FindForm();
                        if (form != null)
                        {
                            PdnBaseForm form2 = form as PdnBaseForm;
                            if (form2 != null)
                            {
                                form2.RestoreWindow();
                            }
                        }
                        ImageResource resource = PdnResources.GetImageResource2("Icons.WarningIcon.png");
                        if (resource != null)
                        {
                            icon = Utility.ImageToIcon(resource.Reference, false);
                        }
                        else
                        {
                            icon = null;
                        }
                        TaskDialog taskDialog = new TaskDialog {
                            Icon = icon,
                            Title = str,
                            TaskImage = bitmap,
                            ScaleTaskImageWithDpi = false,
                            IntroText = str3,
                            TaskButtons = new TaskButton[] { 
                                button,
                                button2,
                                button3
                            },
                            AcceptButton = button,
                            CancelButton = button3,
                            PixelWidth96Dpi = 340
                        };
                        animationEvent = (Action<Image>) Delegate.Combine(animationEvent, image => taskDialog.TaskImage = image);
                        TaskButton button4 = taskDialog.Show(appWorkspace);
                        timer.Enabled = false;
                        timer.Tick -= handler;
                        if (button4 == button)
                        {
                            if (dw.DoSave())
                            {
                                this.cancelled = false;
                                appWorkspace.RemoveDocumentWorkspace(dw);
                            }
                            else
                            {
                                this.cancelled = true;
                            }
                        }
                        else if (button4 == button2)
                        {
                            this.cancelled = false;
                            appWorkspace.RemoveDocumentWorkspace(dw);
                        }
                        else
                        {
                            this.cancelled = true;
                        }
                        if (finalThumb != null)
                        {
                            finalThumb.Dispose();
                            finalThumb = null;
                        }
                        bitmap.Dispose();
                        bitmap = null;
                    }
                }
            }
            Utility.GCFullCollect();
        }

        public bool Cancelled =>
            this.cancelled;
    }
}


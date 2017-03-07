namespace PaintDotNet.Tasks
{
    using PaintDotNet;
    using PaintDotNet.Concurrency;
    using PaintDotNet.Controls;
    using PaintDotNet.Dialogs;
    using PaintDotNet.Functional;
    using PaintDotNet.HistoryMementos;
    using PaintDotNet.Rendering;
    using PaintDotNet.SystemLayer;
    using PaintDotNet.Threading;
    using PaintDotNet.Threading.IterativeTaskDirectives;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Forms;

    internal sealed class ResizeTask : DocWorkspaceTask<Unit>
    {
        public ResizeTask(DocumentWorkspace dw) : base(dw)
        {
        }

        protected override IEnumerator<Directive> OnExecute()
        {
            MeasurementUnit units;
            double resolution;
            int imageHeight;
            int imageWidth;
            BitmapLayer layer;
            IRenderer<ColorBgra> renderer;
            ObservableRenderer<ColorBgra> renderer2;
            EventHandler<EventArgs<Int32Rect>> iteratorVariable13 = null;
            Directive iteratorVariable0 = Directive.DispatchTo(this.DocWorkspace.BackgroundThread);
            Directive iteratorVariable1 = Directive.DispatchTo(this.UIThread);
            yield return iteratorVariable1;
            string resamplingAlgorithm = Settings.CurrentUser.GetString("LastResamplingMethod", ResamplingAlgorithm.SuperSampling.ToString());
            ResamplingAlgorithm iteratorVariable6 = () => ((ResamplingAlgorithm) Enum.Parse(typeof(ResamplingAlgorithm), resamplingAlgorithm, true)).Eval<ResamplingAlgorithm>().Repair<ResamplingAlgorithm>(ex => ResamplingAlgorithm.SuperSampling).Value;
            bool boolean = Settings.CurrentUser.GetBoolean("LastMaintainAspectRatio", true);
            using (ResizeDialog dialog = new ResizeDialog())
            {
                dialog.OriginalSize = this.DocWorkspace.Document.Size;
                dialog.OriginalDpuUnit = this.DocWorkspace.Document.DpuUnit;
                dialog.OriginalDpu = this.DocWorkspace.Document.DpuX;
                dialog.ImageHeight = this.DocWorkspace.Document.Height;
                dialog.ImageWidth = this.DocWorkspace.Document.Width;
                dialog.ResamplingAlgorithm = iteratorVariable6;
                dialog.LayerCount = this.DocWorkspace.Document.Layers.Count;
                dialog.Units = dialog.OriginalDpuUnit;
                dialog.Resolution = this.DocWorkspace.Document.DpuX;
                dialog.Units = PaintDotNet.SettingNames.GetLastNonPixelUnits();
                dialog.ConstrainToAspect = boolean;
                if (dialog.ShowDialog(this.UIOwner) == DialogResult.Cancel)
                {
                    goto Label_097A;
                }
                Settings.CurrentUser.SetString("LastResamplingMethod", dialog.ResamplingAlgorithm.ToString());
                Settings.CurrentUser.SetBoolean("LastMaintainAspectRatio", dialog.ConstrainToAspect);
                units = dialog.Units;
                imageWidth = dialog.ImageWidth;
                imageHeight = dialog.ImageHeight;
                resolution = dialog.Resolution;
                iteratorVariable6 = dialog.ResamplingAlgorithm;
                if (units != MeasurementUnit.Pixel)
                {
                    Settings.CurrentUser.SetString("LastNonPixelUnits", units.ToString());
                    if (this.DocWorkspace.AppWorkspace.Units != MeasurementUnit.Pixel)
                    {
                        this.DocWorkspace.AppWorkspace.Units = units;
                    }
                }
                if (((this.DocWorkspace.Document.Size == new System.Drawing.Size(dialog.ImageWidth, dialog.ImageHeight)) && (this.DocWorkspace.Document.DpuX == resolution)) && (this.DocWorkspace.Document.DpuUnit == units))
                {
                    goto Label_097A;
                }
            }
            if ((imageWidth == this.DocWorkspace.Document.Width) && (imageHeight == this.DocWorkspace.Document.Height))
            {
                MetaDataHistoryMemento memento = new MetaDataHistoryMemento(StaticName, PdnResources.GetImageResource2(StaticImageName), this.DocWorkspace);
                this.DocWorkspace.Document.DpuUnit = units;
                this.DocWorkspace.Document.DpuX = resolution;
                this.DocWorkspace.Document.DpuY = resolution;
                this.DocWorkspace.History.PushNewMemento(memento);
                goto Label_097A;
            }
            VirtualTask<Unit> resizeTask = this.TaskManager.CreateVirtualTask();
            resizeTask.SetState(TaskState.Running);
            resizeTask.Progress = null;
            yield return iteratorVariable1;
            TaskProgressDialog progressDialog = new TaskProgressDialog {
                Task = resizeTask,
                CloseOnFinished = true
            };
            string iteratorVariable8 = PdnResources.GetString2("TaskProgressDialog.Initializing.Text");
            string renderingText = PdnResources.GetString2("ResizeProgressDialog.Resizing.Text");
            string renderingWithPercentTextFormat = PdnResources.GetString2("ResizeProgressDialog.ResizingWithPercent.Text.Format");
            string cancelingText = PdnResources.GetString2("TaskProgressDialog.Canceling.Text");
            progressDialog.HeaderText = iteratorVariable8;
            progressDialog.Text = StaticName;
            progressDialog.Icon = Utility.ImageToIcon(PdnResources.GetImageResource2(StaticImageName).Reference, false);
            this.UIThread.BeginTry(delegate {
                progressDialog.ShowDialog(this.UIOwner);
            });
            yield return iteratorVariable0;
            yield return Directive.Wait(progressDialog.ShownAsync());
            resizeTask.CancelRequested += delegate (object s, EventArgs e) {
                this.UIThread.BeginTry((Action) (() => (progressDialog.HeaderText = cancelingText))).Observe();
            };
            resizeTask.ProgressChanged += delegate (object s, NewValueEventArgs<double?> e) {
                this.UIThread.BeginTry(delegate {
                    if ((resizeTask != null) && (progressDialog != null))
                    {
                        string text1;
                        if (resizeTask.IsCancelRequested)
                        {
                            text1 = cancelingText;
                        }
                        else if (!e.NewValue.HasValue)
                        {
                            text1 = renderingText;
                        }
                        else
                        {
                            text1 = string.Format(renderingWithPercentTextFormat, (100.0 * e.NewValue.Value).ToString("N0"));
                        }
                        if (progressDialog != null)
                        {
                            progressDialog.HeaderText = text1;
                        }
                    }
                }).Observe();
            };
            ReplaceDocumentHistoryMemento iteratorVariable9 = new ReplaceDocumentHistoryMemento(StaticName, PdnResources.GetImageResource2(StaticImageName), this.DocWorkspace);
            Document document = this.DocWorkspace.Document;
            Document iteratorVariable11 = new Document(imageWidth, imageHeight);
            iteratorVariable11.ReplaceMetaDataFrom(document);
            iteratorVariable11.DpuUnit = units;
            iteratorVariable11.DpuX = resolution;
            iteratorVariable11.DpuY = resolution;
            Exception error = null;
            long pixelsSoFar = 0L;
            int progressSoFar = 0;
            long totalPixels = (imageWidth * imageHeight) * document.Layers.Count;
            int num = 0;
            goto Label_08BB;
        Label_080E:
            renderer2 = renderer.ToObservable<ColorBgra>();
            IRenderer<ColorBgra> renderer3 = renderer2.Parallelize(7);
            if (iteratorVariable13 == null)
            {
                iteratorVariable13 = delegate (object sender, EventArgs<Int32Rect> e) {
                    if (resizeTask.IsCancelRequested)
                    {
                        throw new OperationCanceledException();
                    }
                    this.UIThread.BeginTry(delegate {
                        Int32Rect data = e.Data;
                        pixelsSoFar += data.Width * data.Height;
                        double num = ((double) pixelsSoFar) / ((double) totalPixels);
                        int num2 = (int) (100.0 * num);
                        if (progressSoFar < num2)
                        {
                            resizeTask.Progress = new double?(DoubleUtil.Clamp(num, 0.0, 1.0));
                            progressSoFar = num2;
                        }
                    });
                };
            }
            EventHandler<EventArgs<Int32Rect>> handler = iteratorVariable13;
            renderer2.Rendered += handler;
            Surface dst = new Surface(imageWidth, imageHeight);
            try
            {
                renderer3.Render<ColorBgra>(dst);
            }
            catch (Exception exception)
            {
                error = exception;
            }
            if (error != null)
            {
                dst.Dispose();
                goto Label_08D1;
            }
            BitmapLayer layer2 = new BitmapLayer(dst, true);
            layer2.LoadProperties(layer.SaveProperties());
            iteratorVariable11.Layers.Add(layer2);
            num++;
        Label_08BB:
            if (num < document.Layers.Count)
            {
                layer = (BitmapLayer) document.Layers[num];
                Surface surface = layer.Surface;
                switch (iteratorVariable6)
                {
                    case ResamplingAlgorithm.NearestNeighbor:
                        renderer = surface.ResizeNearestNeighbor(imageWidth, imageHeight);
                        goto Label_080E;

                    case ResamplingAlgorithm.Bilinear:
                        renderer = surface.ResizeBilinear(imageWidth, imageHeight);
                        goto Label_080E;

                    case ResamplingAlgorithm.Bicubic:
                        renderer = surface.ResizeBicubic(imageWidth, imageHeight);
                        goto Label_080E;

                    case ResamplingAlgorithm.SuperSampling:
                        if ((imageWidth >= document.Width) || (imageHeight >= document.Height))
                        {
                            renderer = surface.ResizeBicubic(imageWidth, imageHeight);
                        }
                        else
                        {
                            renderer = surface.ResizeSuperSampling(imageWidth, imageHeight);
                        }
                        goto Label_080E;
                }
                throw new InvalidEnumArgumentException();
            }
        Label_08D1:
            if (error != null)
            {
                iteratorVariable11.Dispose();
                iteratorVariable11 = null;
            }
            yield return iteratorVariable1;
            if (error != null)
            {
                this.TaskResult = Result.NewError(error, false);
            }
            else
            {
                this.DocWorkspace.Document = iteratorVariable11;
                this.DocWorkspace.History.PushNewMemento(iteratorVariable9);
            }
            resizeTask.SetState(TaskState.Finished);
            DisposableUtil.Free<TaskProgressDialog>(ref progressDialog);
        Label_097A:;
        }

        protected override void OnFinished()
        {
            base.OnFinished();
        }

        public static string StaticImageName =>
            "Icons.MenuImageResizeIcon.png";

        public static string StaticName =>
            PdnResources.GetString2("ResizeAction.Name");

    }
}


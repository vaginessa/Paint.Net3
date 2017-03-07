namespace PaintDotNet.Actions
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.HistoryMementos;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Drawing.Imaging;
    using System.IO;

    internal sealed class PrintAction : DocumentWorkspaceAction
    {
        public PrintAction() : base(ActionFlags.KeepToolActive)
        {
        }

        public override HistoryMemento PerformAction(DocumentWorkspace documentWorkspace)
        {
            if (!ScanningAndPrinting.CanPrint)
            {
                Utility.ShowWiaError(documentWorkspace);
                return null;
            }
            using (new PushNullToolMode(documentWorkspace))
            {
                Surface surface = documentWorkspace.BorrowScratchSurface(base.GetType().Name + ".PerformAction()");
                try
                {
                    surface.Clear();
                    RenderArgs args = new RenderArgs(surface);
                    documentWorkspace.Update();
                    using (new WaitCursorChanger(documentWorkspace))
                    {
                        args.Surface.Clear(ColorBgra.White);
                        documentWorkspace.Document.Render(args, false);
                    }
                    string filename = Path.GetTempFileName() + ".bmp";
                    args.Bitmap.Save(filename, ImageFormat.Bmp);
                    try
                    {
                        ScanningAndPrinting.Print(documentWorkspace, filename);
                    }
                    catch (Exception)
                    {
                        Utility.ShowWiaError(documentWorkspace);
                    }
                    FileSystem.TryDeleteFile(filename);
                }
                finally
                {
                    documentWorkspace.ReturnScratchSurface(surface);
                }
            }
            return null;
        }
    }
}


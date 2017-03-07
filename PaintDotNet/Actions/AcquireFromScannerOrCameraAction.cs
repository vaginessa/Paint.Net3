namespace PaintDotNet.Actions
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.HistoryMementos;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Drawing;
    using System.IO;

    internal sealed class AcquireFromScannerOrCameraAction : AppWorkspaceAction
    {
        public override void PerformAction(AppWorkspace appWorkspace)
        {
            if (!ScanningAndPrinting.CanScan)
            {
                Utility.ShowWiaError(appWorkspace);
            }
            else
            {
                ScanResult userCancelled;
                string fileName = Path.ChangeExtension(FileSystem.GetTempFileName(), ".bmp");
                try
                {
                    userCancelled = ScanningAndPrinting.Scan(appWorkspace, fileName);
                }
                catch (Exception)
                {
                    userCancelled = ScanResult.UserCancelled;
                }
                if (userCancelled == ScanResult.Success)
                {
                    string message = null;
                    try
                    {
                        Image image;
                        Document document;
                        try
                        {
                            using (FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                            {
                                image = Image.FromStream(stream, false, true);
                            }
                        }
                        catch (FileNotFoundException)
                        {
                            message = PdnResources.GetString2("LoadImage.Error.FileNotFoundException");
                            throw;
                        }
                        catch (OutOfMemoryException)
                        {
                            message = PdnResources.GetString2("LoadImage.Error.OutOfMemoryException");
                            throw;
                        }
                        try
                        {
                            document = Document.FromGdipImage(image, false);
                        }
                        catch (OutOfMemoryException)
                        {
                            message = PdnResources.GetString2("LoadImage.Error.OutOfMemoryException");
                            throw;
                        }
                        finally
                        {
                            image.Dispose();
                            image = null;
                        }
                        DocumentWorkspace workspace = appWorkspace.AddNewDocumentWorkspace();
                        try
                        {
                            workspace.Document = document;
                        }
                        catch (OutOfMemoryException)
                        {
                            message = PdnResources.GetString2("LoadImage.Error.OutOfMemoryException");
                            throw;
                        }
                        document = null;
                        workspace.SetDocumentSaveOptions(null, null, null);
                        workspace.History.ClearAll();
                        HistoryMemento memento = new NullHistoryMemento(PdnResources.GetString2("AcquireImageAction.Name"), PdnResources.GetImageResource2("Icons.MenuLayersAddNewLayerIcon.png"));
                        workspace.History.PushNewMemento(memento);
                        appWorkspace.ActiveDocumentWorkspace = workspace;
                        try
                        {
                            File.Delete(fileName);
                        }
                        catch
                        {
                        }
                    }
                    catch (Exception)
                    {
                        if (message == null)
                        {
                            throw;
                        }
                        Utility.ErrorBox(appWorkspace, message);
                    }
                }
            }
        }
    }
}


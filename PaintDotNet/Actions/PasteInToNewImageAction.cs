namespace PaintDotNet.Actions
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.HistoryMementos;
    using PaintDotNet.Rendering;
    using System;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows.Forms;

    internal sealed class PasteInToNewImageAction : AppWorkspaceAction
    {
        public override void PerformAction(AppWorkspace appWorkspace)
        {
            try
            {
                Int32Size? nullable;
                IDataObject dataObject;
                MaskedSurface clipboardImage;
                try
                {
                    using (new WaitCursorChanger(appWorkspace))
                    {
                        Utility.GCFullCollect();
                        dataObject = Clipboard.GetDataObject();
                        if (ClipboardUtil.IsClipboardImageMaybeAvailable(appWorkspace, dataObject))
                        {
                            clipboardImage = ClipboardUtil.GetClipboardImage(appWorkspace, dataObject);
                            nullable = new Int32Size?(clipboardImage.GetGeometryMaskScans().Bounds().Size());
                        }
                        else
                        {
                            clipboardImage = null;
                            nullable = null;
                        }
                    }
                }
                catch (OutOfMemoryException)
                {
                    Utility.ErrorBox(appWorkspace, PdnResources.GetString2("PasteAction.Error.OutOfMemory"));
                    return;
                }
                catch (Exception)
                {
                    Utility.ErrorBox(appWorkspace, PdnResources.GetString2("PasteAction.Error.TransferFromClipboard"));
                    return;
                }
                if (!nullable.HasValue)
                {
                    Utility.ErrorBox(appWorkspace, PdnResources.GetString2("PasteInToNewImageAction.Error.NoClipboardImage"));
                }
                else
                {
                    Int32Size size = nullable.Value;
                    Document document = null;
                    using (new WaitCursorChanger(appWorkspace))
                    {
                        document = new Document(size);
                        DocumentWorkspace documentWorkspace = appWorkspace.AddNewDocumentWorkspace();
                        documentWorkspace.Document = document;
                        documentWorkspace.History.PushNewMemento(new NullHistoryMemento(string.Empty, null));
                        PasteInToNewLayerAction action = new PasteInToNewLayerAction(documentWorkspace, dataObject, clipboardImage);
                        if (action.PerformAction())
                        {
                            documentWorkspace.Selection.Reset();
                            documentWorkspace.SetDocumentSaveOptions(null, null, null);
                            documentWorkspace.History.ClearAll();
                            documentWorkspace.History.PushNewMemento(new NullHistoryMemento(PdnResources.GetString2("NewImageAction.Name"), PdnResources.GetImageResource2("Icons.MenuLayersAddNewLayerIcon.png")));
                            appWorkspace.ActiveDocumentWorkspace = documentWorkspace;
                        }
                        else
                        {
                            appWorkspace.RemoveDocumentWorkspace(documentWorkspace);
                            document.Dispose();
                        }
                    }
                }
            }
            catch (ExternalException)
            {
                Utility.ErrorBox(appWorkspace, PdnResources.GetString2("AcquireImageAction.Error.Clipboard.TransferError"));
            }
            catch (OutOfMemoryException)
            {
                Utility.ErrorBox(appWorkspace, PdnResources.GetString2("AcquireImageAction.Error.Clipboard.OutOfMemory"));
            }
            catch (ThreadStateException)
            {
            }
        }
    }
}


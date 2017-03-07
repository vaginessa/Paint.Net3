namespace PaintDotNet.Actions
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.Controls;
    using PaintDotNet.Dialogs;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Windows.Forms;

    internal sealed class CloseAllWorkspacesAction : AppWorkspaceAction
    {
        private bool cancelled = false;

        public override void PerformAction(AppWorkspace appWorkspace)
        {
            EventHandler<EventArgs<DocumentWorkspace>> handler = null;
            DocumentWorkspace activeDocumentWorkspace = appWorkspace.ActiveDocumentWorkspace;
            int? nullable = null;
            try
            {
                nullable = new int?(appWorkspace.Widgets.DocumentStrip.ThumbnailUpdateLatency);
                appWorkspace.Widgets.DocumentStrip.ThumbnailUpdateLatency = 0;
            }
            catch (NullReferenceException)
            {
            }
            List<DocumentWorkspace> items = new List<DocumentWorkspace>();
            foreach (DocumentWorkspace workspace2 in appWorkspace.DocumentWorkspaces)
            {
                if ((workspace2.Document != null) && workspace2.Document.Dirty)
                {
                    items.Add(workspace2);
                }
            }
            if (items.Count == 1)
            {
                CloseWorkspaceAction action = new CloseWorkspaceAction(items[0]);
                action.PerformAction(appWorkspace);
                this.cancelled = action.Cancelled;
            }
            else if (items.Count > 1)
            {
                using (UnsavedChangesDialog dialog = new UnsavedChangesDialog())
                {
                    if (handler == null)
                    {
                        handler = (EventHandler<EventArgs<DocumentWorkspace>>) ((s, e2) => (appWorkspace.ActiveDocumentWorkspace = e2.Data));
                    }
                    dialog.DocumentClicked += handler;
                    dialog.Documents = items.ToArrayEx<DocumentWorkspace>();
                    if (appWorkspace.ActiveDocumentWorkspace.Document.Dirty)
                    {
                        dialog.SelectedDocument = appWorkspace.ActiveDocumentWorkspace;
                    }
                    Form form = appWorkspace.FindForm();
                    if (form != null)
                    {
                        PdnBaseForm form2 = form as PdnBaseForm;
                        if (form2 != null)
                        {
                            form2.RestoreWindow();
                        }
                    }
                    switch (dialog.ShowDialog(appWorkspace))
                    {
                        case DialogResult.Yes:
                            foreach (DocumentWorkspace workspace3 in items)
                            {
                                appWorkspace.ActiveDocumentWorkspace = workspace3;
                                if (workspace3.DoSave())
                                {
                                    appWorkspace.RemoveDocumentWorkspace(workspace3);
                                }
                                else
                                {
                                    this.cancelled = true;
                                    break;
                                }
                            }
                            goto Label_021E;

                        case DialogResult.No:
                            this.cancelled = false;
                            goto Label_021E;

                        case DialogResult.Cancel:
                            break;

                        default:
                            throw new InvalidEnumArgumentException();
                    }
                    this.cancelled = true;
                }
            }
        Label_021E:
            try
            {
                if (nullable.HasValue)
                {
                    appWorkspace.Widgets.DocumentStrip.ThumbnailUpdateLatency = nullable.Value;
                }
            }
            catch (NullReferenceException)
            {
            }
            if (this.cancelled)
            {
                if ((appWorkspace.ActiveDocumentWorkspace != activeDocumentWorkspace) && !activeDocumentWorkspace.IsDisposed)
                {
                    appWorkspace.ActiveDocumentWorkspace = activeDocumentWorkspace;
                }
            }
            else
            {
                UI.SuspendControlPainting(appWorkspace);
                foreach (DocumentWorkspace workspace4 in appWorkspace.DocumentWorkspaces)
                {
                    appWorkspace.RemoveDocumentWorkspace(workspace4);
                }
                UI.ResumeControlPainting(appWorkspace);
                appWorkspace.Invalidate(true);
            }
        }

        public bool Cancelled =>
            this.cancelled;
    }
}


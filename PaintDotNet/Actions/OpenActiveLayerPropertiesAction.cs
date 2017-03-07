namespace PaintDotNet.Actions
{
    using PaintDotNet.Controls;
    using PaintDotNet.HistoryMementos;
    using System;
    using System.Windows.Forms;

    internal sealed class OpenActiveLayerPropertiesAction : DocumentWorkspaceAction
    {
        public OpenActiveLayerPropertiesAction() : base(ActionFlags.KeepToolActive)
        {
        }

        public override HistoryMemento PerformAction(DocumentWorkspace documentWorkspace)
        {
            bool dirty = documentWorkspace.Document.Dirty;
            using (Form form = documentWorkspace.ActiveLayer.CreateConfigDialog())
            {
                if (form.ShowDialog(documentWorkspace.AppWorkspace) == DialogResult.Cancel)
                {
                    documentWorkspace.Document.Dirty = dirty;
                }
            }
            return null;
        }
    }
}


namespace PaintDotNet.Actions
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.HistoryFunctions;
    using System;
    using System.Windows.Forms;

    internal sealed class PasteInToNewLayerAction
    {
        private IDataObject clipData;
        private DocumentWorkspace documentWorkspace;
        private MaskedSurface maskedSurface;

        public PasteInToNewLayerAction(DocumentWorkspace documentWorkspace) : this(documentWorkspace, null, null)
        {
        }

        public PasteInToNewLayerAction(DocumentWorkspace documentWorkspace, IDataObject clipData, MaskedSurface maskedSurface)
        {
            this.documentWorkspace = documentWorkspace;
            this.clipData = clipData;
            this.maskedSurface = maskedSurface;
        }

        public bool PerformAction()
        {
            bool flag2;
            try
            {
                if (this.documentWorkspace.ExecuteFunction(new AddNewBlankLayerFunction()) == HistoryFunctionResult.Success)
                {
                    PasteAction action = new PasteAction(this.documentWorkspace, this.clipData, this.maskedSurface);
                    if (!action.PerformAction())
                    {
                        using (new WaitCursorChanger(this.documentWorkspace))
                        {
                            this.documentWorkspace.History.StepBackward(this.documentWorkspace.AppWorkspace);
                            goto Label_0070;
                        }
                    }
                    return true;
                }
            Label_0070:
                flag2 = false;
            }
            finally
            {
                this.clipData = null;
                this.maskedSurface = null;
            }
            return flag2;
        }
    }
}


namespace PaintDotNet.Actions
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.HistoryFunctions;
    using PaintDotNet.HistoryMementos;
    using System;

    internal sealed class MoveActiveLayerUpAction : DocumentWorkspaceAction
    {
        public MoveActiveLayerUpAction() : base(ActionFlags.None)
        {
        }

        public override HistoryMemento PerformAction(DocumentWorkspace documentWorkspace)
        {
            HistoryMemento memento = null;
            int activeLayerIndex = documentWorkspace.ActiveLayerIndex;
            if (activeLayerIndex != (documentWorkspace.Document.Layers.Count - 1))
            {
                HistoryMemento memento2 = new SwapLayerFunction(activeLayerIndex, activeLayerIndex + 1).Execute(documentWorkspace);
                memento = new CompoundHistoryMemento(StaticName, StaticImage, new HistoryMemento[] { memento2 });
                documentWorkspace.ActiveLayer = (Layer) documentWorkspace.Document.Layers[activeLayerIndex + 1];
            }
            return memento;
        }

        public static ImageResource StaticImage =>
            PdnResources.GetImageResource2("Icons.MenuLayersMoveLayerUpIcon.png");

        public static string StaticName =>
            PdnResources.GetString2("MoveLayerUp.HistoryMementoName");
    }
}


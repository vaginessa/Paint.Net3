namespace PaintDotNet
{
    using PaintDotNet.Controls;
    using System;

    internal interface IDocumentList
    {
        event EventHandler<EventArgs<Pair<DocumentWorkspace, DocumentClickAction>>> DocumentClicked;

        event EventHandler DocumentListChanged;

        void AddDocumentWorkspace(DocumentWorkspace addMe);
        void RemoveDocumentWorkspace(DocumentWorkspace removeMe);
        void SelectDocumentWorkspace(DocumentWorkspace selectMe);

        int DocumentCount { get; }

        DocumentWorkspace[] DocumentList { get; }
    }
}


namespace PaintDotNet.HistoryMementos
{
    using PaintDotNet;
    using System;
    using System.Collections.Generic;

    internal class CompoundHistoryMemento : HistoryMemento
    {
        private List<HistoryMemento> actions;

        public CompoundHistoryMemento(string name, ImageResource image) : this(name, image, ArrayUtil.Empty<HistoryMemento>())
        {
        }

        public CompoundHistoryMemento(string name, ImageResource image, List<HistoryMemento> actions) : base(name, image)
        {
            this.actions = new List<HistoryMemento>(actions);
        }

        public CompoundHistoryMemento(string name, ImageResource image, HistoryMemento[] actions) : base(name, image)
        {
            this.actions = new List<HistoryMemento>(actions);
        }

        protected override void OnFlush()
        {
            for (int i = 0; i < this.actions.Count; i++)
            {
                if (this.actions[i] != null)
                {
                    this.actions[i].Flush();
                }
            }
        }

        protected override HistoryMemento OnUndo()
        {
            List<HistoryMemento> actions = new List<HistoryMemento>(this.actions.Count);
            for (int i = 0; i < this.actions.Count; i++)
            {
                HistoryMemento memento = this.actions[(this.actions.Count - i) - 1];
                HistoryMemento item = null;
                if (memento != null)
                {
                    item = memento.PerformUndo();
                }
                actions.Add(item);
            }
            return new CompoundHistoryMemento(base.Name, base.Image, actions);
        }

        public void PushNewAction(HistoryMemento newHA)
        {
            this.actions.Add(newHA);
        }

        public int ActionCount =>
            this.actions.Count;
    }
}


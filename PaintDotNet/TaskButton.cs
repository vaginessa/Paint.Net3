namespace PaintDotNet
{
    using System;
    using System.Drawing;

    internal sealed class TaskButton
    {
        private string actionText;
        private string explanationText;
        private System.Drawing.Image image;

        public TaskButton(System.Drawing.Image image, string actionText, string explanationText)
        {
            this.image = image;
            this.actionText = actionText;
            this.explanationText = explanationText;
        }

        public string ActionText =>
            this.actionText;

        public string ExplanationText =>
            this.explanationText;

        public System.Drawing.Image Image =>
            this.image;
    }
}


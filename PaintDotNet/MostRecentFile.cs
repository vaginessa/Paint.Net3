namespace PaintDotNet
{
    using System;
    using System.Drawing;

    internal class MostRecentFile
    {
        private string fileName;
        private Image thumb;

        public MostRecentFile(string fileName, Image thumb)
        {
            this.fileName = fileName;
            this.thumb = thumb;
        }

        public string FileName =>
            this.fileName;

        public Image Thumb =>
            this.thumb;
    }
}


namespace PaintDotNet
{
    using System;
    using System.Drawing;
    using System.Threading;
    using System.Windows.Forms;

    internal sealed class TaskAuxLabel : TaskAuxControl
    {
        private string text;
        private Font textFont;

        public event EventHandler<NewValueEventArgs<string>> TextChanged;

        public event EventHandler<NewValueEventArgs<Font>> TextFontChanged;

        public override Control CreateControl()
        {
            Label label = new Label {
                FlatStyle = FlatStyle.System,
                AutoSize = true,
                Text = this.text
            };
            if (this.textFont != null)
            {
                label.Font = this.textFont;
            }
            EventHandler<NewValueEventArgs<string>> textChangedHandler = delegate (object s, NewValueEventArgs<string> e) {
                label.Text = e.NewValue;
            };
            this.TextChanged += textChangedHandler;
            label.Disposed += delegate (object s, EventArgs e) {
                this.TextChanged -= textChangedHandler;
            };
            EventHandler<NewValueEventArgs<Font>> textFontChangedHandler = delegate (object s, NewValueEventArgs<Font> e) {
                label.Font = e.NewValue;
            };
            this.TextFontChanged += textFontChangedHandler;
            label.Disposed += delegate (object s, EventArgs e) {
                this.TextFontChanged -= textFontChangedHandler;
            };
            return label;
        }

        private void OnTextChanged(string newText)
        {
            if (this.TextChanged != null)
            {
                this.TextChanged(this, new NewValueEventArgs<string>(newText));
            }
        }

        private void OnTextFontChanged(Font newFont)
        {
            if (this.TextFontChanged != null)
            {
                this.TextFontChanged(this, new NewValueEventArgs<Font>(newFont));
            }
        }

        public string Text
        {
            get => 
                this.text;
            set
            {
                if (this.text != value)
                {
                    this.text = value;
                    this.OnTextChanged(value);
                }
            }
        }

        public Font TextFont
        {
            get => 
                this.textFont;
            set
            {
                this.textFont = value;
                this.OnTextFontChanged(value);
            }
        }
    }
}


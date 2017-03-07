namespace PaintDotNet
{
    using System;
    using System.Threading;
    using System.Windows.Forms;

    internal sealed class TaskAuxButton : TaskAuxControl
    {
        private string text;

        public event EventHandler Clicked;

        public event EventHandler<NewValueEventArgs<string>> TextChanged;

        public override Control CreateControl()
        {
            Button button = new Button {
                FlatStyle = FlatStyle.System,
                AutoSize = true
            };
            button.Click += delegate (object s, EventArgs e) {
                this.OnClicked();
            };
            button.Text = this.text;
            EventHandler<NewValueEventArgs<string>> textChangedHandler = delegate (object s, NewValueEventArgs<string> e) {
                button.Text = e.NewValue;
            };
            this.TextChanged += textChangedHandler;
            button.Disposed += delegate (object s, EventArgs e) {
                this.TextChanged -= textChangedHandler;
            };
            return button;
        }

        private void OnClicked()
        {
            if (this.Clicked != null)
            {
                this.Clicked(this, EventArgs.Empty);
            }
        }

        private void OnTextChanged(string newText)
        {
            if (this.TextChanged != null)
            {
                this.TextChanged(this, new NewValueEventArgs<string>(newText));
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
    }
}


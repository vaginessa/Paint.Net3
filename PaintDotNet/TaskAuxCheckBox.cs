namespace PaintDotNet
{
    using System;
    using System.Threading;
    using System.Windows.Forms;

    internal sealed class TaskAuxCheckBox : TaskAuxControl
    {
        private bool isChecked;
        private string text;

        public event EventHandler Clicked;

        public event EventHandler<NewValueEventArgs<bool>> IsCheckedChanged;

        public event EventHandler<NewValueEventArgs<string>> TextChanged;

        public override Control CreateControl()
        {
            CheckBox checkBox = new CheckBox {
                FlatStyle = FlatStyle.System,
                AutoSize = true,
                Text = this.text,
                Checked = this.isChecked
            };
            checkBox.CheckedChanged += delegate (object s, EventArgs e) {
                this.IsChecked = checkBox.Checked;
            };
            EventHandler<NewValueEventArgs<string>> textChangedHandler = delegate (object s, NewValueEventArgs<string> e) {
                checkBox.Text = e.NewValue;
            };
            this.TextChanged += textChangedHandler;
            checkBox.Disposed += delegate (object s, EventArgs e) {
                this.TextChanged -= textChangedHandler;
            };
            EventHandler<NewValueEventArgs<bool>> isCheckedChangedHandler = delegate (object s, NewValueEventArgs<bool> e) {
                checkBox.Checked = e.NewValue;
            };
            this.IsCheckedChanged += isCheckedChangedHandler;
            checkBox.Disposed += delegate (object s, EventArgs e) {
                this.IsCheckedChanged -= isCheckedChangedHandler;
            };
            return checkBox;
        }

        private void OnClicked()
        {
            if (this.Clicked != null)
            {
                this.Clicked(this, EventArgs.Empty);
            }
        }

        private void OnIsCheckedChanged(bool newValue)
        {
            if (this.IsCheckedChanged != null)
            {
                this.IsCheckedChanged(this, new NewValueEventArgs<bool>(newValue));
            }
        }

        private void OnTextChanged(string newText)
        {
            if (this.TextChanged != null)
            {
                this.TextChanged(this, new NewValueEventArgs<string>(newText));
            }
        }

        public bool IsChecked
        {
            get => 
                this.isChecked;
            set
            {
                if (value != this.isChecked)
                {
                    this.isChecked = value;
                    this.OnIsCheckedChanged(value);
                }
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


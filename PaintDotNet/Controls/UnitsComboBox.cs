namespace PaintDotNet.Controls
{
    using PaintDotNet;
    using System;
    using System.Windows.Forms;

    internal sealed class UnitsComboBox : UserControl, IUnitsComboBox
    {
        private ComboBox comboBox;
        private UnitsComboBoxHandler comboBoxHandler;

        public event EventHandler UnitsChanged
        {
            add
            {
                this.comboBoxHandler.UnitsChanged += value;
            }
            remove
            {
                this.comboBoxHandler.UnitsChanged -= value;
            }
        }

        public UnitsComboBox()
        {
            this.InitializeComponent();
            this.comboBoxHandler = new UnitsComboBoxHandler(this.comboBox);
        }

        public void AddUnit(MeasurementUnit addMe)
        {
            this.comboBoxHandler.AddUnit(addMe);
        }

        private void InitializeComponent()
        {
            this.comboBox = new ComboBox();
            this.comboBox.Dock = DockStyle.Fill;
            this.comboBox.FlatStyle = FlatStyle.System;
            base.Controls.Add(this.comboBox);
        }

        public void RemoveUnit(MeasurementUnit removeMe)
        {
            this.comboBoxHandler.AddUnit(removeMe);
        }

        public bool CentimetersAvailable =>
            this.comboBoxHandler.CentimetersAvailable;

        public bool InchesAvailable =>
            this.comboBoxHandler.InchesAvailable;

        public bool LowercaseStrings
        {
            get => 
                this.comboBoxHandler.LowercaseStrings;
            set
            {
                this.comboBoxHandler.LowercaseStrings = value;
            }
        }

        public bool PixelsAvailable
        {
            get => 
                this.comboBoxHandler.PixelsAvailable;
            set
            {
                this.comboBoxHandler.PixelsAvailable = value;
            }
        }

        public MeasurementUnit Units
        {
            get => 
                this.comboBoxHandler.Units;
            set
            {
                this.comboBoxHandler.Units = value;
            }
        }

        public PaintDotNet.Controls.UnitsDisplayType UnitsDisplayType
        {
            get => 
                this.comboBoxHandler.UnitsDisplayType;
            set
            {
                this.comboBoxHandler.UnitsDisplayType = value;
            }
        }

        public string UnitsText =>
            this.comboBoxHandler.UnitsText;
    }
}


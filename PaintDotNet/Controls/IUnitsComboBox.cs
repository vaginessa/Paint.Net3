namespace PaintDotNet.Controls
{
    using PaintDotNet;
    using System;

    internal interface IUnitsComboBox
    {
        event EventHandler UnitsChanged;

        void AddUnit(MeasurementUnit addMe);
        void RemoveUnit(MeasurementUnit removeMe);

        bool CentimetersAvailable { get; }

        bool InchesAvailable { get; }

        bool LowercaseStrings { get; set; }

        bool PixelsAvailable { get; set; }

        MeasurementUnit Units { get; set; }

        PaintDotNet.Controls.UnitsDisplayType UnitsDisplayType { get; set; }

        string UnitsText { get; }
    }
}


namespace PaintDotNet
{
    using PaintDotNet.SystemLayer;
    using System;

    internal sealed class SettingNames
    {
        public const string AlsoCheckForBetas = "CHECKFORBETAS";
        public const string AutoCheckForUpdates = "CHECKFORUPDATES";
        public const string ColorsFormVisible = "ColorsForm.Visible";
        public const string CurrentPalette = "CurrentPalette";
        public const string DefaultAppEnvironment = "DefaultAppEnvironment";
        public const string DefaultToolTypeName = "DefaultToolTypeName";
        public const string DrawGrid = "DrawGrid";
        public const string FontSmoothing = "FontSmoothing";
        public const string GlassDialogButtons = "GlassDialogButtons";
        public const string Height = "Height";
        public const string HistoryFormVisible = "HistoryForm.Visible";
        public const string InstallDirectory = "TARGETDIR";
        public const string LanguageName = "LanguageName";
        public const string LastCanvasSizeAnchorEdge = "LastCanvasSizeAnchorEdge";
        public const string LastFileDialogDirectory = "LastFileDialogDirectory";
        public const string LastMaintainAspectRatio = "LastMaintainAspectRatio";
        public const string LastMaintainAspectRatioCS = "LastMaintainAspectRatioCS";
        public const string LastMaintainAspectRatioNF = "LastMaintainAspectRatioNF";
        public const string LastNonPixelUnits = "LastNonPixelUnits";
        public const string LastResamplingMethod = "LastResamplingMethod";
        public const string LastUpdateCheckTimeTicks = "LastUpdateCheckTimeTicks";
        public const string LayersFormVisible = "LayersForm.Visible";
        public const string Left = "Left";
        public const string MruMax = "MRUMax";
        public const string Rulers = "Rulers";
        public const string ToolsFormVisible = "ToolsForm.Visible";
        public const string Top = "Top";
        public const string TranslucentWindows = "TranslucentWindows";
        public const string Units = "Units";
        public const string UpdateMsiFileName = "UpdateMsiFileName";
        public const string Width = "Width";
        public const string WindowState = "WindowState";

        private SettingNames()
        {
        }

        public static AnchorEdge GetLastCanvasSizeAnchorEdge()
        {
            string str = Settings.CurrentUser.GetString("LastCanvasSizeAnchorEdge", AnchorEdge.TopLeft.ToString());
            try
            {
                return (AnchorEdge) Enum.Parse(typeof(AnchorEdge), str, true);
            }
            catch
            {
                return AnchorEdge.TopLeft;
            }
        }

        public static MeasurementUnit GetLastNonPixelUnits()
        {
            string str = Settings.CurrentUser.GetString("LastNonPixelUnits", MeasurementUnit.Inch.ToString());
            try
            {
                return (MeasurementUnit) Enum.Parse(typeof(MeasurementUnit), str, true);
            }
            catch
            {
                return MeasurementUnit.Inch;
            }
        }
    }
}


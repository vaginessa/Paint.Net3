namespace PaintDotNet.Actions
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.Dialogs;
    using PaintDotNet.Rendering;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    internal sealed class NewImageAction : AppWorkspaceAction
    {
        public override void PerformAction(AppWorkspace appWorkspace)
        {
            using (NewFileDialog dialog = new NewFileDialog())
            {
                Int32Size? clipboardImageSize;
                Int32Size newDocumentSize = appWorkspace.GetNewDocumentSize();
                using (new WaitCursorChanger(appWorkspace))
                {
                    Utility.GCFullCollect();
                    try
                    {
                        IDataObject dataObject = System.Windows.Forms.Clipboard.GetDataObject();
                        clipboardImageSize = ClipboardUtil.GetClipboardImageSize(appWorkspace, dataObject);
                        dataObject = null;
                    }
                    catch (Exception)
                    {
                        clipboardImageSize = null;
                    }
                    Utility.GCFullCollect();
                }
                if (clipboardImageSize.HasValue)
                {
                    newDocumentSize = clipboardImageSize.Value;
                }
                dialog.OriginalSize = new Size(newDocumentSize.Width, newDocumentSize.Height);
                dialog.OriginalDpuUnit = PaintDotNet.SettingNames.GetLastNonPixelUnits();
                dialog.OriginalDpu = Document.GetDefaultDpu(dialog.OriginalDpuUnit);
                dialog.Units = dialog.OriginalDpuUnit;
                dialog.Resolution = dialog.OriginalDpu;
                dialog.ConstrainToAspect = Settings.CurrentUser.GetBoolean("LastMaintainAspectRatioNF", false);
                if ((((dialog.ShowDialog(appWorkspace) == DialogResult.OK) && (dialog.ImageWidth > 0)) && ((dialog.ImageHeight > 0) && dialog.Resolution.IsFinite())) && (dialog.Resolution > 0.0))
                {
                    Int32Size size = new Int32Size(dialog.ImageWidth, dialog.ImageHeight);
                    if (appWorkspace.CreateBlankDocumentInNewWorkspace(size, dialog.Units, dialog.Resolution, false))
                    {
                        appWorkspace.ActiveDocumentWorkspace.ZoomBasis = ZoomBasis.FitToWindow;
                        Settings.CurrentUser.SetBoolean("LastMaintainAspectRatioNF", dialog.ConstrainToAspect);
                        if (dialog.Units != MeasurementUnit.Pixel)
                        {
                            Settings.CurrentUser.SetString("LastNonPixelUnits", dialog.Units.ToString());
                        }
                        if (appWorkspace.Units != MeasurementUnit.Pixel)
                        {
                            appWorkspace.Units = dialog.Units;
                        }
                    }
                }
            }
        }
    }
}


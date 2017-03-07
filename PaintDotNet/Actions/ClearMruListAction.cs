namespace PaintDotNet.Actions
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using System;
    using System.Windows.Forms;

    internal sealed class ClearMruListAction : AppWorkspaceAction
    {
        public override void PerformAction(AppWorkspace appWorkspace)
        {
            string question = PdnResources.GetString2("ClearOpenRecentList.Dialog.Text");
            if (Utility.AskYesNo(appWorkspace, question) == DialogResult.Yes)
            {
                appWorkspace.MostRecentFiles.Clear();
                appWorkspace.MostRecentFiles.SaveMruList();
            }
        }
    }
}


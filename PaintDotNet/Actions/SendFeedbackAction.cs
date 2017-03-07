namespace PaintDotNet.Actions
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using System;
    using System.Diagnostics;

    internal sealed class SendFeedbackAction : AppWorkspaceAction
    {
        private string GetEmailLaunchString(string email, string subject, string body)
        {
            string str = body.Replace("\r\n", "%0D%0A");
            return $"mailto:{email}?subject={subject}&body={str}";
        }

        public override void PerformAction(AppWorkspace appWorkspace)
        {
            string email = "feedback@getpaint.net";
            string subject = string.Format(PdnResources.GetString2("SendFeedback.Email.Subject.Format"), PdnInfo.FullAppName);
            string body = PdnResources.GetString2("SendFeedback.Email.Body");
            string fileName = this.GetEmailLaunchString(email, subject, body);
            fileName = fileName.Substring(0, Math.Min(0x400, fileName.Length));
            try
            {
                Process.Start(fileName);
            }
            catch (Exception)
            {
                Utility.ErrorBox(appWorkspace, PdnResources.GetString2("SendFeedbackAction.Error"));
            }
        }
    }
}


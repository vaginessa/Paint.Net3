namespace PaintDotNet
{
    using System;

    internal interface IStatusBarProgress
    {
        void EraseProgressStatusBar();
        void EraseProgressStatusBarAsync();
        double GetProgressStatusBarValue();
        void ResetProgressStatusBar();
        void ResetProgressStatusBarAsync();
        void SetProgressStatusBar(double percent);
    }
}


namespace PaintDotNet.Updates
{
    using PaintDotNet;
    using System;

    internal abstract class UpdatesState : PaintDotNet.State
    {
        private bool continueButtonVisible;
        private PaintDotNet.Updates.MarqueeStyle marqueeStyle;

        public UpdatesState(bool isFinalState, bool continueButtonVisible, PaintDotNet.Updates.MarqueeStyle marqueeStyle) : base(isFinalState)
        {
            this.continueButtonVisible = continueButtonVisible;
            this.marqueeStyle = marqueeStyle;
        }

        public string ContinueButtonText =>
            PdnResources.GetString2("UpdatesDialog.ContinueButton.Text." + base.GetType().Name);

        public bool ContinueButtonVisible =>
            this.continueButtonVisible;

        public virtual string InfoText =>
            PdnResources.GetString2("UpdatesDialog.InfoText.Text." + base.GetType().Name);

        public PaintDotNet.Updates.MarqueeStyle MarqueeStyle =>
            this.marqueeStyle;

        public UpdatesStateMachine StateMachine =>
            ((UpdatesStateMachine) base.StateMachine);
    }
}


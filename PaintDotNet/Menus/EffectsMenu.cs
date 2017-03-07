namespace PaintDotNet.Menus
{
    using PaintDotNet;
    using PaintDotNet.Effects;
    using System;

    internal sealed class EffectsMenu : EffectMenuBase
    {
        public EffectsMenu()
        {
            this.InitializeComponent();
        }

        protected override bool FilterEffects(Effect effect) => 
            (effect.Category == EffectCategory.Effect);

        private void InitializeComponent()
        {
            base.Name = "Menu.Effects";
            this.Text = PdnResources.GetString2("Menu.Effects.Text");
        }

        protected override bool EnableEffectShortcuts =>
            false;

        protected override bool EnableRepeatEffectMenuItem =>
            true;
    }
}


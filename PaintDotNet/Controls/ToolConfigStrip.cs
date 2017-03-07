namespace PaintDotNet.Controls
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.Concurrency;
    using PaintDotNet.Dialogs;
    using PaintDotNet.Functional;
    using PaintDotNet.Rendering;
    using PaintDotNet.SystemLayer;
    using PaintDotNet.Threading;
    using PaintDotNet.Typography;
    using PaintDotNet.Typography.Drivers;
    using PaintDotNet.VisualStyling;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.Drawing.Text;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows;
    using System.Windows.Forms;

    internal class ToolConfigStrip : ToolStripEx, IBrushConfig, IShapeTypeConfig, IPenConfig, IAntiAliasingConfig, IAlphaBlendingConfig, ITextConfig, IToleranceConfig, IColorPickerConfig, IGradientConfig, IResamplingConfig, ISelectionCombineModeConfig, IFloodModeConfig, ISelectionDrawModeConfig
    {
        private TextAlignment alignment;
        private bool alphaBlendingEnabled = true;
        private ImageResource alphaBlendingEnabledImage;
        private ImageResource alphaBlendingOverwriteImage;
        private PdnToolStripSplitButton alphaBlendingSplitButton;
        private ImageResource antiAliasingDisabledImage;
        private bool antiAliasingEnabled = true;
        private ImageResource antiAliasingEnabledImage;
        private PdnToolStripSplitButton antiAliasingSplitButton;
        private ToolStripSeparator blendingSeparator;
        private Dictionary<Pair<ColorBgra, ColorBgra>, ColorBgra[]> blendLookupTables;
        private ToolStripSeparator brushSeparator;
        private int[] brushSizes;
        private PdnToolStripComboBox brushStyleComboBox;
        private ToolStripLabel brushStyleLabel;
        private EnumLocalizer colorPickerBehaviorNames = EnumLocalizer.Create(typeof(PaintDotNet.ColorPickerClickBehavior));
        private PdnToolStripComboBox colorPickerComboBox;
        private ToolStripLabel colorPickerLabel;
        private ToolStripSeparator colorPickerSeparator;
        private EnumLocalizer dashStyleLocalizer = EnumLocalizer.Create(typeof(DashStyle));
        private DashStyle[] dashStyles;
        private PaintDotNet.Typography.FontFamily defaultFontFamily;
        private const string defaultFontName = "Arial";
        private int[] defaultFontSizes;
        private ToolStripLabel floodModeLabel;
        private ToolStripSeparator floodModeSeparator;
        private PdnToolStripSplitButton floodModeSplitButton;
        private ToolStripButton fontAlignCenterButton;
        private ToolStripButton fontAlignLeftButton;
        private ToolStripButton fontAlignRightButton;
        private ToolStripSeparator fontAlignSeparator;
        private ToolStripButton fontBoldButton;
        private FontFamilyCollection fontFamilyCollection;
        private PdnToolStripComboBox fontFamilyComboBox;
        private IAsyncWorkDeque fontFamilyComboBoxBackgroundThread;
        private const int fontFamilyComboBoxFontSize = 12;
        private Dictionary<string, FontPreviewMask> fontFamilyPreviewMaskCache;
        private ToolStripButton fontItalicsButton;
        private ToolStripLabel fontLabel;
        private bool fontsComboBoxPopulated;
        private bool fontsComboBoxShown;
        private ToolStripSeparator fontSeparator;
        private PdnToolStripComboBox fontSizeComboBox;
        private PdnToolStripComboBox fontSmoothingComboBox;
        private EnumLocalizer fontSmoothingLocalizer;
        private ToolStripButton fontStrikeoutButton;
        private System.Drawing.FontStyle fontStyle;
        private ToolStripSeparator fontStyleSeparator;
        private ToolStripButton fontUnderlineButton;
        private ImageResource gradientAllColorChannelsImage;
        private ImageResource gradientAlphaChannelOnlyImage;
        private PdnToolStripSplitButton gradientChannelsSplitButton;
        private ToolStripButton gradientConicalButton;
        private PaintDotNet.GradientInfo gradientInfo;
        private ToolStripButton gradientLinearClampedButton;
        private ToolStripButton gradientLinearDiamondButton;
        private ToolStripButton gradientLinearReflectedButton;
        private ToolStripButton gradientRadialButton;
        private ToolStripSeparator gradientSeparator1;
        private ToolStripSeparator gradientSeparator2;
        private EnumLocalizer gradientTypeNames;
        private EnumLocalizer hatchStyleNames = EnumLocalizer.Create(typeof(HatchStyle));
        private const int initialFontSize = 12;
        private EnumLocalizer lineCapLocalizer = EnumLocalizer.Create(typeof(LineCap2));
        private LineCap2[] lineCaps;
        private const int maxFontSize = 0x7d0;
        private const float maxPenSize = 500f;
        private const int minFontSize = 1;
        private const float minPenSize = 1f;
        private float oldSizeValue;
        private PenBrushCache penBrushCache = PenBrushCache.ThreadInstance;
        private PdnToolStripSplitButton penDashStyleSplitButton;
        private PdnToolStripSplitButton penEndCapSplitButton;
        private ToolStripSeparator penSeparator;
        private PdnToolStripComboBox penSizeComboBox;
        private ToolStripButton penSizeDecButton;
        private ToolStripButton penSizeIncButton;
        private ToolStripLabel penSizeLabel;
        private PdnToolStripSplitButton penStartCapSplitButton;
        private ToolStripLabel penStyleLabel;
        private EnumLocalizer resamplingAlgorithmNames = EnumLocalizer.Create(typeof(PaintDotNet.ResamplingAlgorithm));
        private PdnToolStripComboBox resamplingComboBox;
        private ToolStripLabel resamplingLabel;
        private ToolStripSeparator resamplingSeparator;
        private ToolStripLabel selectionCombineModeLabel;
        private ToolStripSeparator selectionCombineModeSeparator;
        private PdnToolStripSplitButton selectionCombineModeSplitButton;
        private ToolStripLabel selectionDrawModeHeightLabel;
        private PdnToolStripTextBox selectionDrawModeHeightTextBox;
        private PaintDotNet.SelectionDrawModeInfo selectionDrawModeInfo;
        private ToolStripLabel selectionDrawModeModeLabel;
        private ToolStripSeparator selectionDrawModeSeparator;
        private PdnToolStripSplitButton selectionDrawModeSplitButton;
        private ToolStripButton selectionDrawModeSwapButton;
        private UnitsComboBoxStrip selectionDrawModeUnits;
        private ToolStripLabel selectionDrawModeWidthLabel;
        private PdnToolStripTextBox selectionDrawModeWidthTextBox;
        private ImageResource shapeBothImage = PdnResources.GetImageResource2("Icons.ShapeBothIcon.png");
        private PdnToolStripSplitButton shapeButton;
        private PaintDotNet.ShapeDrawType shapeDrawType;
        private ImageResource shapeInteriorImage = PdnResources.GetImageResource2("Icons.ShapeInteriorIcon.png");
        private ImageResource shapeOutlineImage = PdnResources.GetImageResource2("Icons.ShapeOutlineIcon.png");
        private ToolStripSeparator shapeSeparator;
        private string solidBrushText;
        private ToolStripLabel toleranceLabel;
        private ToolStripSeparator toleranceSeparator;
        private ToleranceSliderControl toleranceSlider;
        private ToolStripControlHost toleranceSliderStrip;
        private PaintDotNet.ToolBarConfigItems toolBarConfigItems;
        private ITypographyDriver typographyDriver;
        private ITypographyService typographyService;

        public event EventHandler AlphaBlendingChanged;

        public event EventHandler AntiAliasingChanged;

        public event EventHandler BrushInfoChanged;

        public event EventHandler ColorPickerClickBehaviorChanged;

        public event EventHandler FloodModeChanged;

        public event EventHandler FontAlignmentChanged;

        public event EventHandler FontInfoChanged;

        public event EventHandler FontSmoothingChanged;

        public event EventHandler GradientInfoChanged;

        public event EventHandler PenInfoChanged;

        public event EventHandler ResamplingAlgorithmChanged;

        public event EventHandler SelectionCombineModeChanged;

        public event EventHandler SelectionDrawModeInfoChanged;

        public event EventHandler SelectionDrawModeUnitsChanged;

        public event EventHandler SelectionDrawModeUnitsChanging;

        public event EventHandler ShapeDrawTypeChanged;

        public event EventHandler ToleranceChanged;

        public event EventHandler ToolBarConfigItemsChanged;

        public ToolConfigStrip()
        {
            LineCap2[] capArray = new LineCap2[4];
            capArray[1] = LineCap2.Arrow;
            capArray[2] = LineCap2.ArrowFilled;
            capArray[3] = LineCap2.Rounded;
            this.lineCaps = capArray;
            DashStyle[] styleArray = new DashStyle[5];
            styleArray[1] = DashStyle.Dash;
            styleArray[2] = DashStyle.DashDot;
            styleArray[3] = DashStyle.DashDotDot;
            styleArray[4] = DashStyle.Dot;
            this.dashStyles = styleArray;
            this.brushSizes = new int[] { 
                1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 20,
                0x19, 30, 0x23, 40, 0x2d, 50, 0x37, 60, 0x41, 70, 0x4b, 80, 0x55, 90, 0x5f, 100,
                0x7d, 150, 0xaf, 200, 0xe1, 250, 0x113, 300, 0x145, 350, 0x177, 400, 0x1a9, 450, 0x1db, 500
            };
            this.gradientTypeNames = EnumLocalizer.Create(typeof(GradientType));
            this.gradientInfo = new PaintDotNet.GradientInfo(GradientType.LinearClamped, false);
            this.fontSmoothingLocalizer = EnumLocalizer.Create(typeof(PaintDotNet.SystemLayer.FontSmoothing));
            this.defaultFontSizes = new int[] { 
                8, 9, 10, 11, 12, 14, 0x10, 0x12, 20, 0x16, 0x18, 0x1a, 0x1c, 0x24, 0x30, 0x48,
                0x54, 0x60, 0x6c, 0x90, 0xc0, 0xd8, 0x120
            };
            this.fontFamilyPreviewMaskCache = new Dictionary<string, FontPreviewMask>();
            this.blendLookupTables = new Dictionary<Pair<ColorBgra, ColorBgra>, ColorBgra[]>();
            this.typographyService = new TypographyService();
            this.typographyDriver = this.typographyService.DefaultDriver;
            this.fontFamilyCollection = this.typographyService.SystemFontFamilies;
            this.fontFamilyCollection.CollectionChanged += new EventHandler(this.FontFamilyCollection_CollectionChanged);
            base.SuspendLayout();
            this.InitializeComponent();
            this.solidBrushText = PdnResources.GetString2("BrushConfigWidget.SolidBrush.Text");
            this.brushStyleComboBox.Items.Add(this.solidBrushText);
            LocalizedEnumValue[] items = (from lev in this.hatchStyleNames.GetLocalizedEnumValues()
                orderby lev.EnumValue
                select lev).ToArrayEx<LocalizedEnumValue>();
            this.brushStyleComboBox.Items.AddRange(items);
            this.brushStyleComboBox.SelectedIndex = 0;
            this.brushStyleLabel.Text = PdnResources.GetString2("BrushConfigWidget.FillStyleLabel.Text");
            this.shapeDrawType = PaintDotNet.ShapeDrawType.Outline;
            this.shapeButton.Image = this.shapeOutlineImage.Reference;
            this.penSizeLabel.Text = PdnResources.GetString2("PenConfigWidget.BrushWidthLabel");
            this.penSizeComboBox.ComboBox.SuspendLayout();
            for (int i = 0; i < this.brushSizes.Length; i++)
            {
                this.penSizeComboBox.Items.Add(this.brushSizes[i].ToString());
            }
            this.penSizeComboBox.DropDownWidth = (this.penSizeComboBox.DropDownWidth * 3) / 2;
            this.penSizeComboBox.ComboBox.ResumeLayout(false);
            this.penSizeComboBox.SelectedIndex = 1;
            this.penSizeDecButton.ToolTipText = PdnResources.GetString2("ToolConfigStrip.PenSizeDecButton.ToolTipText");
            this.penSizeDecButton.Image = PdnResources.GetImageResource2("Icons.MinusButtonIcon.png").Reference;
            this.penSizeIncButton.ToolTipText = PdnResources.GetString2("ToolConfigStrip.PenSizeIncButton.ToolTipText");
            this.penSizeIncButton.Image = PdnResources.GetImageResource2("Icons.PlusButtonIcon.png").Reference;
            this.penStyleLabel.Text = PdnResources.GetString2("ToolConfigStrip.PenStyleLabel.Text");
            this.penStartCapSplitButton.Tag = LineCap2.Flat;
            this.penStartCapSplitButton.Image = this.GetLineCapImage(LineCap2.Flat, true).Reference;
            this.penStartCapSplitButton.ToolTipText = PdnResources.GetString2("ToolConfigStrip.PenStartCapSplitButton.ToolTipText");
            this.penDashStyleSplitButton.Tag = DashStyle.Solid;
            this.penDashStyleSplitButton.Image = this.GetDashStyleImage(DashStyle.Solid);
            this.penDashStyleSplitButton.ToolTipText = PdnResources.GetString2("ToolConfigStrip.PenDashStyleSplitButton.ToolTipText");
            this.penEndCapSplitButton.Tag = LineCap2.Flat;
            this.penEndCapSplitButton.Image = this.GetLineCapImage(LineCap2.Flat, false).Reference;
            this.penEndCapSplitButton.ToolTipText = PdnResources.GetString2("ToolConfigStrip.PenEndCapSplitButton.ToolTipText");
            this.gradientLinearClampedButton.ToolTipText = this.gradientTypeNames.GetLocalizedEnumValue(GradientType.LinearClamped).LocalizedName;
            this.gradientLinearClampedButton.Image = PdnResources.GetImageResource2("Icons.LinearClampedGradientIcon.png").Reference;
            this.gradientLinearReflectedButton.ToolTipText = this.gradientTypeNames.GetLocalizedEnumValue(GradientType.LinearReflected).LocalizedName;
            this.gradientLinearReflectedButton.Image = PdnResources.GetImageResource2("Icons.LinearReflectedGradientIcon.png").Reference;
            this.gradientLinearDiamondButton.ToolTipText = this.gradientTypeNames.GetLocalizedEnumValue(GradientType.LinearDiamond).LocalizedName;
            this.gradientLinearDiamondButton.Image = PdnResources.GetImageResource2("Icons.LinearDiamondGradientIcon.png").Reference;
            this.gradientRadialButton.ToolTipText = this.gradientTypeNames.GetLocalizedEnumValue(GradientType.Radial).LocalizedName;
            this.gradientRadialButton.Image = PdnResources.GetImageResource2("Icons.RadialGradientIcon.png").Reference;
            this.gradientConicalButton.ToolTipText = this.gradientTypeNames.GetLocalizedEnumValue(GradientType.Conical).LocalizedName;
            this.gradientConicalButton.Image = PdnResources.GetImageResource2("Icons.ConicalGradientIcon.png").Reference;
            this.gradientAllColorChannelsImage = PdnResources.GetImageResource2("Icons.AllColorChannelsIcon.png");
            this.gradientAlphaChannelOnlyImage = PdnResources.GetImageResource2("Icons.AlphaChannelOnlyIcon.png");
            this.gradientChannelsSplitButton.Image = this.gradientAllColorChannelsImage.Reference;
            this.antiAliasingEnabledImage = PdnResources.GetImageResource2("Icons.AntiAliasingEnabledIcon.png");
            this.antiAliasingDisabledImage = PdnResources.GetImageResource2("Icons.AntiAliasingDisabledIcon.png");
            this.antiAliasingSplitButton.Image = this.antiAliasingEnabledImage.Reference;
            this.alphaBlendingEnabledImage = PdnResources.GetImageResource2("Icons.BlendingEnabledIcon.png");
            this.alphaBlendingOverwriteImage = PdnResources.GetImageResource2("Icons.BlendingOverwriteIcon.png");
            this.alphaBlendingSplitButton.Image = this.alphaBlendingEnabledImage.Reference;
            this.penSizeComboBox.Size = new System.Drawing.Size(UI.ScaleWidth(this.penSizeComboBox.Width), this.penSizeComboBox.Height);
            this.brushStyleComboBox.Size = new System.Drawing.Size(UI.ScaleWidth(this.brushStyleComboBox.Width), this.brushStyleComboBox.Height);
            this.brushStyleComboBox.DropDownWidth = UI.ScaleWidth(this.brushStyleComboBox.DropDownWidth);
            this.brushStyleComboBox.DropDownHeight = UI.ScaleHeight(this.brushStyleComboBox.DropDownHeight);
            this.toleranceLabel.Text = PdnResources.GetString2("ToleranceConfig.ToleranceLabel.Text");
            this.toleranceSlider.Tolerance = 0.5f;
            this.fontSizeComboBox.ComboBox.SuspendLayout();
            for (int j = 0; j < this.defaultFontSizes.Length; j++)
            {
                this.fontSizeComboBox.Items.Add(this.defaultFontSizes[j].ToString());
            }
            this.fontSizeComboBox.ComboBox.ResumeLayout(false);
            this.fontSmoothingComboBox.Items.AddRange(new object[] { this.fontSmoothingLocalizer.GetLocalizedEnumValue(PaintDotNet.SystemLayer.FontSmoothing.Smooth), this.fontSmoothingLocalizer.GetLocalizedEnumValue(PaintDotNet.SystemLayer.FontSmoothing.Sharp) });
            this.fontSmoothingComboBox.SelectedIndex = 0;
            this.fontLabel.Text = PdnResources.GetString2("TextConfigWidget.FontLabel.Text");
            this.defaultFontFamily = this.typographyService.SystemFontMetrics.MessageFont.FontFamily;
            this.alignment = TextAlignment.Left;
            this.fontAlignLeftButton.Checked = true;
            this.oldSizeValue = 12f;
            this.fontBoldButton.Image = PdnResources.GetImageResource2("Icons.FontBoldIcon.png").Reference;
            this.fontItalicsButton.Image = PdnResources.GetImageResource2("Icons.FontItalicIcon.png").Reference;
            this.fontUnderlineButton.Image = PdnResources.GetImageResource2("Icons.FontUnderlineIcon.png").Reference;
            this.fontStrikeoutButton.Image = PdnResources.GetImageResource2("Icons.FontStrikeoutIcon.png").Reference;
            this.fontAlignLeftButton.Image = PdnResources.GetImageResource2("Icons.TextAlignLeftIcon.png").Reference;
            this.fontAlignCenterButton.Image = PdnResources.GetImageResource2("Icons.TextAlignCenterIcon.png").Reference;
            this.fontAlignRightButton.Image = PdnResources.GetImageResource2("Icons.TextAlignRightIcon.png").Reference;
            this.fontBoldButton.ToolTipText = PdnResources.GetString2("TextConfigWidget.BoldButton.ToolTipText");
            this.fontItalicsButton.ToolTipText = PdnResources.GetString2("TextConfigWidget.ItalicButton.ToolTipText");
            this.fontUnderlineButton.ToolTipText = PdnResources.GetString2("TextConfigWidget.UnderlineButton.ToolTipText");
            this.fontStrikeoutButton.ToolTipText = PdnResources.GetString2("TextConfigWidget.StrikeoutButton.ToolTipText");
            this.fontAlignLeftButton.ToolTipText = PdnResources.GetString2("TextConfigWidget.AlignLeftButton.ToolTipText");
            this.fontAlignCenterButton.ToolTipText = PdnResources.GetString2("TextConfigWidget.AlignCenterButton.ToolTipText");
            this.fontAlignRightButton.ToolTipText = PdnResources.GetString2("TextConfigWidget.AlignRightButton.ToolTipText");
            this.fontFamilyComboBox.Size = new System.Drawing.Size(UI.ScaleWidth(this.fontFamilyComboBox.Width), this.fontFamilyComboBox.Height);
            this.fontFamilyComboBox.ComboBox.DropDownHeight = UI.ScaleHeight(this.fontFamilyComboBox.ComboBox.DropDownHeight);
            this.fontFamilyComboBox.ComboBox.DropDownWidth = UI.ScaleWidth(this.fontFamilyComboBox.DropDownWidth);
            this.fontSizeComboBox.Size = new System.Drawing.Size(UI.ScaleWidth(this.fontSizeComboBox.Width), this.fontSizeComboBox.Height);
            this.fontSmoothingComboBox.Size = new System.Drawing.Size(UI.ScaleWidth(this.fontSmoothingComboBox.Width), this.fontSmoothingComboBox.Height);
            this.fontSmoothingComboBox.DropDownWidth = UI.ScaleWidth(this.fontSmoothingComboBox.DropDownWidth);
            this.resamplingLabel.Text = PdnResources.GetString2("ToolConfigStrip.ResamplingLabel.Text");
            this.resamplingComboBox.BeginUpdate();
            this.resamplingComboBox.Items.Add(this.resamplingAlgorithmNames.GetLocalizedEnumValue(PaintDotNet.ResamplingAlgorithm.Bilinear));
            this.resamplingComboBox.Items.Add(this.resamplingAlgorithmNames.GetLocalizedEnumValue(PaintDotNet.ResamplingAlgorithm.NearestNeighbor));
            this.resamplingComboBox.EndUpdate();
            this.resamplingComboBox.SelectedIndex = 0;
            this.resamplingComboBox.Size = new System.Drawing.Size(UI.ScaleWidth(this.resamplingComboBox.Width), this.resamplingComboBox.Height);
            this.resamplingComboBox.DropDownWidth = UI.ScaleWidth(this.resamplingComboBox.DropDownWidth);
            this.colorPickerLabel.Text = PdnResources.GetString2("ToolConfigStrip.ColorPickerLabel.Text");
            LocalizedEnumValue[] valueArray2 = (from lev in this.colorPickerBehaviorNames.GetLocalizedEnumValues()
                orderby lev.EnumValue
                select lev).ToArrayEx<LocalizedEnumValue>();
            this.colorPickerComboBox.Items.AddRange(valueArray2);
            this.colorPickerComboBox.SelectedIndex = 0;
            this.colorPickerComboBox.Size = new System.Drawing.Size(UI.ScaleWidth(this.colorPickerComboBox.Width), this.colorPickerComboBox.Height);
            this.colorPickerComboBox.DropDownWidth = (this.colorPickerComboBox.DropDownWidth * 5) / 4;
            this.colorPickerComboBox.DropDownWidth = UI.ScaleWidth(this.colorPickerComboBox.DropDownWidth);
            this.toleranceSlider.Size = UI.ScaleSize(this.toleranceSlider.Size);
            this.selectionCombineModeLabel.Text = PdnResources.GetString2("ToolConfigStrip.SelectionCombineModeLabel.Text");
            this.floodModeLabel.Text = PdnResources.GetString2("ToolConfigStrip.FloodModeLabel.Text");
            this.selectionDrawModeModeLabel.Text = PdnResources.GetString2("ToolConfigStrip.SelectionDrawModeLabel.Text");
            this.selectionDrawModeWidthLabel.Text = PdnResources.GetString2("ToolConfigStrip.SelectionDrawModeWidthLabel.Text");
            this.selectionDrawModeHeightLabel.Text = PdnResources.GetString2("ToolConfigStrip.SelectionDrawModeHeightLabel.Text");
            this.selectionDrawModeSwapButton.Image = PdnResources.GetImageResource2("Icons.ToolConfigStrip.SelectionDrawModeSwapButton.png").Reference;
            this.selectionDrawModeWidthTextBox.Size = new System.Drawing.Size(UI.ScaleWidth(this.selectionDrawModeWidthTextBox.Width), this.selectionDrawModeWidthTextBox.Height);
            this.selectionDrawModeHeightTextBox.Size = new System.Drawing.Size(UI.ScaleWidth(this.selectionDrawModeHeightTextBox.Width), this.selectionDrawModeHeightTextBox.Height);
            this.selectionDrawModeUnits.Size = new System.Drawing.Size(UI.ScaleWidth(this.selectionDrawModeUnits.Width), this.selectionDrawModeUnits.Height);
            this.ToolBarConfigItems = PaintDotNet.ToolBarConfigItems.None;
            base.ResumeLayout(false);
        }

        public void AddToPenSize(float delta)
        {
            if ((this.toolBarConfigItems & (PaintDotNet.ToolBarConfigItems.None | PaintDotNet.ToolBarConfigItems.Pen)) == (PaintDotNet.ToolBarConfigItems.None | PaintDotNet.ToolBarConfigItems.Pen))
            {
                (this.PenInfo.Width + delta).Clamp(1f, 500f);
                PaintDotNet.PenInfo info = this.PenInfo.Clone();
                info.Width += delta;
                info.Width = info.Width.Clamp(1f, 500f);
                this.PenInfo = info;
            }
        }

        private void AlphaBlendingComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.OnAlphaBlendingChanged();
        }

        private void AlphaBlendingSplitButton_DropDownOpening(object sender, EventArgs e)
        {
            ToolStripMenuItem item = new ToolStripMenuItem(PdnResources.GetString2("AlphaBlendingSplitButton.BlendingEnabled.Text"), this.alphaBlendingEnabledImage.Reference, (EventHandler) ((sender2, e2) => (this.AlphaBlending = true)));
            ToolStripMenuItem item2 = new ToolStripMenuItem(PdnResources.GetString2("AlphaBlendingSplitButton.BlendingOverwrite.Text"), this.alphaBlendingOverwriteImage.Reference, (EventHandler) ((sender3, e3) => (this.AlphaBlending = false)));
            item.Checked = this.AlphaBlending;
            item2.Checked = !this.AlphaBlending;
            this.alphaBlendingSplitButton.DropDownItems.Clear();
            this.alphaBlendingSplitButton.DropDownItems.AddRange(new ToolStripItem[] { item, item2 });
        }

        private void AntiAliasingSplitButton_DropDownOpening(object sender, EventArgs e)
        {
            ToolStripMenuItem item = new ToolStripMenuItem(PdnResources.GetString2("AntiAliasingSplitButton.Enabled.Text"), this.antiAliasingEnabledImage.Reference, (EventHandler) ((sender2, e2) => (this.AntiAliasing = true)));
            ToolStripMenuItem item2 = new ToolStripMenuItem(PdnResources.GetString2("AntiAliasingSplitButton.Disabled.Text"), this.antiAliasingDisabledImage.Reference, (EventHandler) ((sender3, e3) => (this.AntiAliasing = false)));
            item.Checked = this.AntiAliasing;
            item2.Checked = !this.AntiAliasing;
            this.antiAliasingSplitButton.DropDownItems.Clear();
            this.antiAliasingSplitButton.DropDownItems.AddRange(new ToolStripItem[] { item, item2 });
        }

        private void AsyncPrefetchFontNames()
        {
            if (!base.IsHandleCreated)
            {
                base.CreateControl();
            }
            if (!this.fontFamilyComboBox.ComboBox.IsHandleCreated)
            {
                this.fontFamilyComboBox.ComboBox.CreateControl();
            }
            FontFamilyCollection fontFamilyCollectionP = this.fontFamilyCollection;
            ThreadPool.QueueUserWorkItem(delegate {
                try
                {
                    fontFamilyCollectionP.ToArrayEx<PaintDotNet.Typography.FontFamily>();
                }
                catch (Exception)
                {
                }
            });
        }

        private void BrushSizeComboBox_Validating(object sender, CancelEventArgs e)
        {
            float num;
            if (!float.TryParse(this.penSizeComboBox.Text, out num))
            {
                this.penSizeComboBox.BackColor = Color.Red;
                this.penSizeComboBox.ToolTipText = PdnResources.GetString2("PenConfigWidget.Error.InvalidNumber");
            }
            else if (num < 1f)
            {
                this.penSizeComboBox.BackColor = Color.Red;
                string str2 = string.Format(PdnResources.GetString2("PenConfigWidget.Error.TooSmall.Format"), 1f);
                this.penSizeComboBox.ToolTipText = str2;
            }
            else if (num > 500f)
            {
                this.penSizeComboBox.BackColor = Color.Red;
                string str4 = string.Format(PdnResources.GetString2("PenConfigWidget.Error.TooLarge.Format"), 500f);
                this.penSizeComboBox.ToolTipText = str4;
            }
            else
            {
                this.penSizeComboBox.BackColor = SystemColors.Window;
                this.penSizeComboBox.ToolTipText = string.Empty;
                this.OnPenChanged();
            }
        }

        private void ColorPickerComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.OnColorPickerClickBehaviorChanged();
        }

        private void ComboBoxStyle_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index != -1)
            {
                using (Bitmap bitmap = new Bitmap(e.Bounds.Width, e.Bounds.Height, PixelFormat.Format24bppRgb))
                {
                    Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                    using (Graphics graphics = Graphics.FromImage(bitmap))
                    {
                        Brush brush2;
                        graphics.Clear(SystemColors.Window);
                        bool flag = (e.State & DrawItemState.Selected) != DrawItemState.None;
                        HighlightState state = ((e.Bounds.Width >= (this.brushStyleComboBox.DropDownWidth / 2)) && flag) ? HighlightState.Hover : HighlightState.Default;
                        Rectangle rectangle2 = rect;
                        object objA = this.brushStyleComboBox.Items[e.Index];
                        LocalizedEnumValue value2 = objA as LocalizedEnumValue;
                        if (object.ReferenceEquals(objA, this.solidBrushText))
                        {
                            rectangle2.Width = 0;
                        }
                        else
                        {
                            rectangle2.Width = UI.ScaleWidth(20);
                        }
                        Rectangle layoutRectangle = rect;
                        layoutRectangle.X = rectangle2.Right + UI.ScaleWidth(2);
                        layoutRectangle.Width = rect.Right - layoutRectangle.Left;
                        string s = this.brushStyleComboBox.Items[e.Index].ToString();
                        Color selectionForeColor = SelectionHighlight.GetSelectionForeColor(state);
                        Color selectionBackColor = SelectionHighlight.GetSelectionBackColor(state);
                        using (Brush brush = new SolidBrush(selectionBackColor))
                        {
                            graphics.FillRectangle(brush, e.Bounds);
                        }
                        if (SelectionHighlight.ShouldDrawFirst)
                        {
                            SelectionHighlight.DrawBackground(graphics, this.penBrushCache, rect, state);
                        }
                        if (object.ReferenceEquals(objA, this.solidBrushText))
                        {
                            brush2 = new SolidBrush(selectionForeColor);
                        }
                        else
                        {
                            HatchStyle enumValue = (HatchStyle) value2.EnumValue;
                            brush2 = new HatchBrush(enumValue, selectionForeColor, selectionBackColor);
                        }
                        graphics.FillRectangle(brush2, rectangle2);
                        brush2.Dispose();
                        using (Brush brush3 = new SolidBrush(selectionForeColor))
                        {
                            using (StringFormat format = StringFormat.GenericDefault.CloneT<StringFormat>())
                            {
                                format.LineAlignment = StringAlignment.Center;
                                format.FormatFlags |= StringFormatFlags.NoWrap;
                                format.HotkeyPrefix = HotkeyPrefix.None;
                                format.Trimming = StringTrimming.EllipsisCharacter;
                                graphics.DrawString(s, this.brushStyleComboBox.Font, brush3, layoutRectangle, format);
                            }
                        }
                        if (!SelectionHighlight.ShouldDrawFirst)
                        {
                            SelectionHighlight.DrawBackground(graphics, this.penBrushCache, rect, state);
                        }
                        if (this.ShowFocusCues && ((e.State & DrawItemState.Focus) == DrawItemState.Focus))
                        {
                            ControlPaint.DrawFocusRectangle(graphics, new Rectangle(0, 0, bitmap.Width, bitmap.Height));
                        }
                        graphics.Flush();
                    }
                    CompositingMode compositingMode = e.Graphics.CompositingMode;
                    e.Graphics.CompositingMode = CompositingMode.SourceCopy;
                    e.Graphics.DrawImage(bitmap, e.Bounds, rect, GraphicsUnit.Pixel);
                    e.Graphics.CompositingMode = compositingMode;
                }
            }
        }

        private void ComboBoxStyle_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            string text = this.brushStyleComboBox.Items[e.Index].ToString();
            SizeF ef = e.Graphics.MeasureString(text, this.Font);
            e.ItemHeight = (int) ef.Height;
            e.ItemWidth = (int) ef.Width;
        }

        private void ComboBoxStyle_SelectedValueChanged(object sender, EventArgs e)
        {
            this.OnBrushChanged();
        }

        private PaintDotNet.FloodMode CycleFloodMode(PaintDotNet.FloodMode mode)
        {
            switch (mode)
            {
                case PaintDotNet.FloodMode.Local:
                    return PaintDotNet.FloodMode.Global;

                case PaintDotNet.FloodMode.Global:
                    return PaintDotNet.FloodMode.Local;
            }
            throw new InvalidEnumArgumentException();
        }

        public void CyclePenDashStyle()
        {
            PaintDotNet.PenInfo info = this.PenInfo.Clone();
            info.DashStyle = this.NextDashStyle(info.DashStyle);
            this.PenInfo = info;
        }

        public void CyclePenEndCap()
        {
            PaintDotNet.PenInfo info = this.PenInfo.Clone();
            info.EndCap = this.NextLineCap(info.EndCap);
            this.PenInfo = info;
        }

        public void CyclePenStartCap()
        {
            PaintDotNet.PenInfo info = this.PenInfo.Clone();
            info.StartCap = this.NextLineCap(info.StartCap);
            this.PenInfo = info;
        }

        private PaintDotNet.SelectionCombineMode CycleSelectionCombineMode(PaintDotNet.SelectionCombineMode mode)
        {
            switch (mode)
            {
                case PaintDotNet.SelectionCombineMode.Replace:
                    return PaintDotNet.SelectionCombineMode.Union;

                case PaintDotNet.SelectionCombineMode.Union:
                    return PaintDotNet.SelectionCombineMode.Exclude;

                case PaintDotNet.SelectionCombineMode.Intersect:
                    return PaintDotNet.SelectionCombineMode.Xor;

                case PaintDotNet.SelectionCombineMode.Xor:
                    return PaintDotNet.SelectionCombineMode.Replace;
            }
            return PaintDotNet.SelectionCombineMode.Intersect;
        }

        private SelectionDrawMode CycleSelectionDrawMode(SelectionDrawMode drawMode)
        {
            switch (drawMode)
            {
                case SelectionDrawMode.Normal:
                    return SelectionDrawMode.FixedRatio;

                case SelectionDrawMode.FixedRatio:
                    return SelectionDrawMode.FixedSize;

                case SelectionDrawMode.FixedSize:
                    return SelectionDrawMode.Normal;
            }
            throw new InvalidEnumArgumentException();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.fontFamilyCollection.CollectionChanged -= new EventHandler(this.FontFamilyCollection_CollectionChanged);
                if ((this.fontFamilyComboBoxBackgroundThread != null) && (this.fontFamilyComboBoxBackgroundThread is IDisposable))
                {
                    ((IDisposable) this.fontFamilyComboBoxBackgroundThread).Dispose();
                }
                this.fontFamilyComboBoxBackgroundThread = null;
            }
            base.Dispose(disposing);
        }

        private unsafe FontPreviewMask DrawFontFamilyPreviewMask(string familyName, double emSize)
        {
            PaintDotNet.Typography.Font font = this.fontFamilyCollection.CreateFontFamily(familyName).CreateFont(emSize, FontStyleFlags.Regular);
            IFontRendererSettings settings = this.typographyDriver.CreateFontRendererSettings(TextDisplayIntent.Other, TextRenderMode.Default);
            Surface surface = null;
            string text = PdnResources.GetString2("ToolConfigStrip.FontFamilyComboBox.FontSampleText");
            Int32Point zero = Int32Point.Zero;
            using (IFontRenderer renderer = this.typographyDriver.CreateFontRenderer(font, settings))
            {
                TextSize size = renderer.MeasureText(text);
                Rect blackBoxBounds = size.BlackBoxBounds;
                Rect safeClipBounds = size.SafeClipBounds;
                Int32Rect rect = safeClipBounds.Int32Bound();
                if ((rect.Width > 0) && (rect.Height > 0))
                {
                    surface = new Surface(rect.Size());
                    surface.Clear(ColorBgra.White);
                    System.Windows.Point offset = new System.Windows.Point(safeClipBounds.X - rect.X, safeClipBounds.Y - rect.Y);
                    using (IBitmapTextRenderTarget target = this.typographyDriver.CreateBitmapRenderTarget(surface))
                    {
                        Int32Rect clipBounds = surface.Bounds<ColorBgra>();
                        renderer.DrawText(target, text, offset, clipBounds);
                    }
                    System.Windows.Point pt = new System.Windows.Point((blackBoxBounds.X - safeClipBounds.X) + offset.X, (blackBoxBounds.Y - safeClipBounds.Y) + offset.Y);
                    zero = Int32Point.Ceiling(pt);
                }
            }
            FontPreviewMask mask = null;
            if (surface != null)
            {
                mask = new FontPreviewMask(surface.Width, surface.Height, zero);
                for (int i = 0; i < surface.Height; i++)
                {
                    ColorBgra* rowAddress = surface.GetRowAddress(i);
                    byte* numPtr = mask.GetRowAddress(i);
                    byte* numPtr2 = numPtr + mask.Width;
                    while (numPtr < numPtr2)
                    {
                        numPtr[0] = (byte) (0xff - rowAddress.GetIntensityByte());
                        rowAddress++;
                        numPtr++;
                    }
                }
                surface.Dispose();
            }
            return mask;
        }

        private FontPreviewMask DrawFontFamilyPreviewMaskCached(string familyName, double targetEmSize, int maxPixelHeight)
        {
            FontPreviewMask mask;
            bool flag;
            lock (this.fontFamilyPreviewMaskCache)
            {
                flag = this.fontFamilyPreviewMaskCache.TryGetValue(familyName, out mask);
            }
            if (!flag)
            {
                int num = 0;
                double emSize = targetEmSize;
                PaintDotNet.Typography.Font font = this.fontFamilyCollection.CreateFontFamily(familyName).CreateFont(emSize, FontStyleFlags.Regular);
                IFontRendererSettings settings = this.typographyDriver.CreateFontRendererSettings(TextDisplayIntent.Other, TextRenderMode.Default);
                string text = PdnResources.GetString2("ToolConfigStrip.FontFamilyComboBox.FontSampleText");
                while ((mask == null) && (emSize >= 1.0))
                {
                    num++;
                    using (IFontRenderer renderer = this.typographyDriver.CreateFontRenderer(font, settings))
                    {
                        if ((renderer.Height <= maxPixelHeight) && (renderer.MeasureText(text).BlackBoxBounds.Int32Bound().Height <= maxPixelHeight))
                        {
                            mask = this.DrawFontFamilyPreviewMask(familyName, emSize);
                        }
                        emSize--;
                        if ((emSize < 1.0) && (mask == null))
                        {
                            mask = this.DrawFontFamilyPreviewMask(familyName, targetEmSize);
                        }
                        continue;
                    }
                }
                lock (this.fontFamilyPreviewMaskCache)
                {
                    if (this.fontFamilyPreviewMaskCache.ContainsKey(familyName))
                    {
                        if (mask != null)
                        {
                            mask.Dispose();
                        }
                        return this.fontFamilyPreviewMaskCache[familyName];
                    }
                    this.fontFamilyPreviewMaskCache.Add(familyName, mask);
                }
            }
            return mask;
        }

        private void FloodModeSplitButton_DropDownOpening(object sender, EventArgs e)
        {
            EventHandler onClick = null;
            this.floodModeSplitButton.DropDownItems.Clear();
            PaintDotNet.FloodMode[] modeArray = new PaintDotNet.FloodMode[2];
            modeArray[1] = PaintDotNet.FloodMode.Global;
            foreach (PaintDotNet.FloodMode mode in modeArray)
            {
                if (onClick == null)
                {
                    onClick = delegate (object sender2, EventArgs e2) {
                        ToolStripMenuItem item = (ToolStripMenuItem) sender2;
                        PaintDotNet.FloodMode tag = (PaintDotNet.FloodMode) item.Tag;
                        this.FloodMode = tag;
                    };
                }
                ToolStripMenuItem item = new ToolStripMenuItem(PdnResources.GetString2("ToolConfigStrip.FloodModeSplitButton." + mode.ToString() + ".Text"), this.GetFloodModeImage(mode).Reference, onClick) {
                    Tag = mode,
                    Checked = mode == this.FloodMode
                };
                this.floodModeSplitButton.DropDownItems.Add(item);
            }
        }

        private void FontFamilyCollection_CollectionChanged(object sender, EventArgs e)
        {
            base.BeginInvoke(() => this.fontsComboBoxPopulated = false);
        }

        private unsafe void FontFamilyComboBox_DrawItem(DrawItemEventArgs e)
        {
            HighlightState hover;
            int num = UI.ScaleWidth(2);
            int num2 = num;
            int num3 = UI.ScaleHeight(0);
            int num4 = num3;
            int num5 = UI.ScaleWidth(8);
            string familyName = (string) this.fontFamilyComboBox.Items[e.Index];
            bool flag = (e.State & DrawItemState.Selected) != DrawItemState.None;
            bool flag2 = !this.fontsComboBoxShown || (e.Bounds.Width < (this.fontFamilyComboBox.DropDownWidth / 2));
            if (flag)
            {
                hover = HighlightState.Hover;
            }
            else
            {
                hover = HighlightState.Default;
            }
            if (flag2)
            {
                hover = HighlightState.Default;
            }
            Color selectionForeColor = SelectionHighlight.GetSelectionForeColor(hover);
            ColorBgra color = ColorBgra.FromColor(SelectionHighlight.GetSelectionBackColor(hover));
            ColorBgra cb = ColorBgra.FromColor(selectionForeColor);
            Int32Rect clipBounds = new Int32Rect(e.Bounds.X + num, e.Bounds.Y + num3, (e.Bounds.Width - num) - num2, (e.Bounds.Height - num3) - num4);
            FontPreviewMask mask = null;
            if (flag2)
            {
                mask = null;
            }
            else
            {
                lock (this.fontFamilyPreviewMaskCache)
                {
                    if (this.fontFamilyPreviewMaskCache.ContainsKey(familyName))
                    {
                        mask = this.fontFamilyPreviewMaskCache[familyName];
                    }
                }
                if (mask == null)
                {
                    FontPreviewMask previewMaskP = null;
                    int index = e.Index;
                    IntPtr listHwnd = UI.GetListBoxHwnd(this.fontFamilyComboBox.ComboBox);
                    Action f = delegate {
                        Action method = null;
                        try
                        {
                            if (this.typographyDriver is IGdiTypographyDriver)
                            {
                                Thread.Sleep(10);
                            }
                            ThreadBackground background = null;
                            try
                            {
                                if (this.typographyDriver is IDirectWriteTypographyDriver)
                                {
                                    background = new ThreadBackground(ThreadBackgroundFlags.Cpu);
                                }
                                previewMaskP = this.DrawFontFamilyPreviewMaskCached(familyName, 12.0, clipBounds.Height);
                            }
                            finally
                            {
                                if (background != null)
                                {
                                    background.Dispose();
                                    background = null;
                                }
                            }
                        }
                        finally
                        {
                            try
                            {
                                if (listHwnd != IntPtr.Zero)
                                {
                                    if (method == null)
                                    {
                                        method = () => UI.InvalidateHwnd(listHwnd);
                                    }
                                    this.BeginInvoke(method);
                                }
                            }
                            catch (Exception)
                            {
                            }
                        }
                    };
                    if (this.fontFamilyComboBoxBackgroundThread == null)
                    {
                        this.fontFamilyComboBoxBackgroundThread = new ThreadDispatcher(ApartmentState.MTA);
                    }
                    this.fontFamilyComboBoxBackgroundThread.Enqueue(true, f).Observe();
                }
            }
            Surface surface = new Surface(e.Bounds.Width, e.Bounds.Height);
            RenderArgs args = new RenderArgs(surface);
            surface.Clear(color);
            SelectionHighlight.DrawBackground(args.Graphics, this.penBrushCache, new Rectangle(System.Drawing.Point.Empty, args.Size), hover);
            args.Graphics.Flush();
            Int32Rect rect = Int32RectUtil.From(0, 0, 0, surface.Height);
            PaintDotNet.Typography.Font menuFont = this.typographyService.SystemFontMetrics.MenuFont;
            using (System.Drawing.Font font2 = FontUtil.CreateGdipFont(menuFont.FontFamily.Name, (float) menuFont.EmSize, System.Drawing.FontStyle.Regular, GraphicsUnit.Point))
            {
                StringFormat stringFormat = StringFormat.GenericDefault.CloneT<StringFormat>();
                stringFormat.LineAlignment = StringAlignment.Center;
                stringFormat.FormatFlags |= StringFormatFlags.NoWrap;
                stringFormat.HotkeyPrefix = HotkeyPrefix.None;
                stringFormat.Trimming = StringTrimming.EllipsisCharacter;
                System.Drawing.Size size = System.Drawing.Size.Ceiling(args.Graphics.MeasureString(familyName, font2, new SizeF((float) clipBounds.Width, (float) clipBounds.Height), stringFormat));
                Rectangle rectangle = new Rectangle(clipBounds.X - e.Bounds.X, (clipBounds.Y - e.Bounds.Y) + ((surface.Height - size.Height) / 2), size.Width, size.Height);
                rectangle.Intersect(surface.Bounds);
                using (Brush brush = new SolidBrush(selectionForeColor))
                {
                    args.Graphics.DrawString(familyName, font2, brush, new RectangleF((float) (clipBounds.X - e.Bounds.X), (float) (clipBounds.Y - e.Bounds.Y), (float) clipBounds.Width, (float) clipBounds.Height), stringFormat);
                }
                stringFormat.Dispose();
                rect = rectangle.ToInt32Rect();
            }
            if (mask != null)
            {
                int num13;
                int x = Int32Util.ClampSafe(rect.Right() + num5, (surface.Width - num2) - mask.Width, surface.Width - num2);
                int num7 = (surface.Height - mask.Height) / 2;
                int num9 = Math.Min((int) (surface.Width - num2), (int) (x + mask.Width)) - x;
                int num10 = Math.Max(0, num7);
                int num11 = Math.Min(surface.Height, num7 + mask.Height);
                int num12 = UI.ScaleWidth(0x10);
                if (num9 < mask.Width)
                {
                    num13 = num12;
                }
                else
                {
                    num13 = 0;
                }
                for (int i = num10; i < num11; i++)
                {
                    ColorBgra* pointAddress = surface.GetPointAddress(x, i);
                    ColorBgra* bgraPtr2 = (pointAddress + num9) - num13;
                    ColorBgra* bgraPtr3 = bgraPtr2 + num13;
                    byte* rowAddress = mask.GetRowAddress(i - num7);
                    while (pointAddress < bgraPtr2)
                    {
                        byte cbAlpha = rowAddress[0];
                        pointAddress[0] = ColorBgra.Blend(pointAddress[0], cb, cbAlpha);
                        pointAddress++;
                        rowAddress++;
                    }
                    for (int j = 0; pointAddress < bgraPtr3; j++)
                    {
                        byte num17 = rowAddress[0];
                        byte frac = (byte) (0xff - ((j * 0xff) / num13));
                        byte num19 = ByteUtil.FastScale(num17, frac);
                        pointAddress[0] = ColorBgra.Blend(pointAddress[0], cb, num19);
                        pointAddress++;
                        rowAddress++;
                    }
                }
            }
            if (this.ShowFocusCues && ((e.State & DrawItemState.Focus) == DrawItemState.Focus))
            {
                ControlPaint.DrawFocusRectangle(args.Graphics, new Rectangle(0, 0, surface.Width, surface.Height));
            }
            args.Graphics.Flush();
            CompositingMode compositingMode = e.Graphics.CompositingMode;
            e.Graphics.CompositingMode = CompositingMode.SourceCopy;
            e.Graphics.DrawImage(args.Bitmap, e.Bounds, new Rectangle(0, 0, surface.Width, surface.Height), GraphicsUnit.Pixel);
            e.Graphics.CompositingMode = compositingMode;
            args.Dispose();
            surface.Dispose();
        }

        private void FontFamilyComboBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index != -1)
            {
                this.FontFamilyComboBox_DrawItem(e);
            }
        }

        private void FontFamilyComboBox_GotFocus(object sender, EventArgs e)
        {
            if (!this.fontsComboBoxPopulated)
            {
                this.fontsComboBoxPopulated = true;
                Timing.Global.GetTickCountDouble();
                using (new WaitCursorChanger(this))
                {
                    string selectedItem = (string) this.fontFamilyComboBox.ComboBox.SelectedItem;
                    string name = null;
                    ManualResetEvent gotFamilies = new ManualResetEvent(false);
                    PaintDotNet.Typography.FontFamily[] families = null;
                    TaskManager taskManager = new TaskManager();
                    ThreadTask<Unit> task = taskManager.CreateThreadTask<Unit>(ApartmentState.STA, delegate {
                        Unit unit;
                        try
                        {
                            families = (from ff in this.fontFamilyCollection
                                where (ff != null) && !string.IsNullOrEmpty(ff.Name)
                                select ff).ToArrayEx<PaintDotNet.Typography.FontFamily>();
                            unit = new Unit();
                        }
                        finally
                        {
                            gotFamilies.Set();
                        }
                        return unit;
                    });
                    task.Start();
                    if (!gotFamilies.WaitOne(0x3e8, false))
                    {
                        new TaskProgressDialog { 
                            Task = task,
                            CloseOnFinished = true,
                            ShowCancelButton = false,
                            Text = PdnInfo.BareProductName,
                            Icon = PdnInfo.AppIcon,
                            HeaderText = PdnResources.GetString2("TextConfigWidget.LoadingFontsList.Text")
                        }.ShowDialog(this);
                    }
                    gotFamilies.WaitOne();
                    this.fontFamilyComboBox.ComboBox.BeginUpdate();
                    this.fontFamilyComboBox.ComboBox.Items.Clear();
                    foreach (PaintDotNet.Typography.FontFamily family in families)
                    {
                        this.fontFamilyComboBox.Items.Add(family.Name);
                        if ((selectedItem != null) && (selectedItem == family.Name))
                        {
                            name = family.Name;
                        }
                    }
                    this.fontFamilyComboBox.ComboBox.EndUpdate();
                    if (name != null)
                    {
                        this.fontFamilyComboBox.SelectedItem = name;
                    }
                    else
                    {
                        this.fontFamilyComboBox.SelectedItem = "Arial";
                    }
                    taskManager.BeginShutdown();
                    taskManager.Dispose();
                }
                Timing.Global.GetTickCountDouble();
            }
        }

        private void FontFamilyComboBox_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            e.ItemHeight = (e.ItemHeight * 5) / 4;
        }

        private void FontFamilyComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.OnFontInfoChanged();
        }

        private void FontSizeComboBox_TextChanged(object sender, EventArgs e)
        {
            this.FontSizeComboBox_Validating(sender, new CancelEventArgs());
        }

        private void FontSizeComboBox_Validating(object sender, CancelEventArgs e)
        {
            try
            {
                float num;
                if (!float.TryParse(this.fontSizeComboBox.Text, out num))
                {
                    this.fontSizeComboBox.BackColor = Color.Red;
                    this.fontSizeComboBox.ToolTipText = PdnResources.GetString2("TextConfigWidget.Error.InvalidNumber");
                }
                else if (float.Parse(this.fontSizeComboBox.Text) < 1f)
                {
                    this.fontSizeComboBox.BackColor = Color.Red;
                    string str2 = string.Format(PdnResources.GetString2("TextConfigWidget.Error.TooSmall.Format"), 1);
                    this.fontSizeComboBox.ToolTipText = str2;
                }
                else if (float.Parse(this.fontSizeComboBox.Text) > 2000f)
                {
                    this.fontSizeComboBox.BackColor = Color.Red;
                    string str4 = string.Format(PdnResources.GetString2("TextConfigWidget.Error.TooLarge.Format"), 0x7d0);
                    this.fontSizeComboBox.ToolTipText = str4;
                }
                else
                {
                    this.fontSizeComboBox.ToolTipText = string.Empty;
                    this.fontSizeComboBox.BackColor = SystemColors.Window;
                    this.OnFontInfoChanged();
                }
            }
            catch (FormatException)
            {
                e.Cancel = true;
            }
        }

        private ColorBgra[] GetBlendLookupTable(ColorBgra from, ColorBgra to)
        {
            ColorBgra[] bgraArray;
            bool flag;
            Pair<ColorBgra, ColorBgra> key = Pair.Create<ColorBgra, ColorBgra>(from, to);
            lock (this.blendLookupTables)
            {
                flag = this.blendLookupTables.TryGetValue(key, out bgraArray);
            }
            if (!flag)
            {
                bgraArray = new ColorBgra[0x100];
                for (int i = 0; i <= 0xff; i++)
                {
                    bgraArray[i] = ColorBgra.Blend(from, to, (byte) i);
                }
                lock (this.blendLookupTables)
                {
                    if (!this.blendLookupTables.ContainsKey(key))
                    {
                        this.blendLookupTables.Add(key, bgraArray);
                    }
                }
            }
            return bgraArray;
        }

        private Image GetDashStyleImage(DashStyle dashStyle)
        {
            string format = "Images.DashStyleButton.{0}.png";
            ImageResource resource = PdnResources.GetImageResource2(string.Format(format, dashStyle.ToString()));
            if ((UI.GetXScaleFactor() == 1f) && (UI.GetYScaleFactor() == 1f))
            {
                return resource.Reference;
            }
            return new Bitmap(resource.Reference, UI.ScaleSize(resource.Reference.Size));
        }

        private ImageResource GetFloodModeImage(PaintDotNet.FloodMode cm) => 
            PdnResources.GetImageResource2("Icons.ToolConfigStrip.FloodMode." + cm.ToString() + ".png");

        private ImageResource GetLineCapImage(LineCap2 lineCap, bool isStartCap)
        {
            string format = "Images.LineCapButton.{0}.{1}.png";
            return PdnResources.GetImageResource2(string.Format(format, lineCap.ToString(), isStartCap ? "Start" : "End"));
        }

        private ImageResource GetSelectionCombineModeImage(PaintDotNet.SelectionCombineMode cm) => 
            PdnResources.GetImageResource2("Icons.ToolConfigStrip.SelectionCombineMode." + cm.ToString() + ".png");

        private Image GetSelectionDrawModeImage(SelectionDrawMode drawMode) => 
            PdnResources.GetImageResource2("Icons.ToolConfigStrip.SelectionDrawModeSplitButton." + drawMode.ToString() + ".png").Reference;

        private string GetSelectionDrawModeString(SelectionDrawMode drawMode) => 
            PdnResources.GetString2("ToolConfigStrip.SelectionDrawModeSplitButton." + drawMode.ToString() + ".Text");

        private void GradientChannelsSplitButton_DropDownOpening(object sender, EventArgs e)
        {
            ToolStripMenuItem item = new ToolStripMenuItem(PdnResources.GetString2("GradientChannels.AllColorChannels.Text"), this.gradientAllColorChannelsImage.Reference, (EventHandler) ((sender2, e2) => (this.GradientInfo = new PaintDotNet.GradientInfo(this.GradientInfo.GradientType, false))));
            ToolStripMenuItem item2 = new ToolStripMenuItem(PdnResources.GetString2("GradientChannels.AlphaChannelOnly.Text"), this.gradientAlphaChannelOnlyImage.Reference, (EventHandler) ((sender3, e3) => (this.GradientInfo = new PaintDotNet.GradientInfo(this.GradientInfo.GradientType, true))));
            item.Checked = !this.GradientInfo.AlphaOnly;
            item2.Checked = this.GradientInfo.AlphaOnly;
            this.gradientChannelsSplitButton.DropDownItems.Clear();
            this.gradientChannelsSplitButton.DropDownItems.AddRange(new ToolStripItem[] { item, item2 });
        }

        private void GradientTypeButtonClicked(object sender, EventArgs e)
        {
            GradientType tag = (GradientType) ((ToolStripButton) sender).Tag;
            this.GradientInfo = new PaintDotNet.GradientInfo(tag, this.GradientInfo.AlphaOnly);
        }

        private void InitializeComponent()
        {
            this.brushSeparator = new ToolStripSeparator();
            this.brushStyleLabel = new ToolStripLabel();
            this.brushStyleComboBox = new PdnToolStripComboBox(true);
            this.shapeSeparator = new ToolStripSeparator();
            this.shapeButton = new PdnToolStripSplitButton();
            this.gradientSeparator1 = new ToolStripSeparator();
            this.gradientLinearClampedButton = new ToolStripButton();
            this.gradientLinearReflectedButton = new ToolStripButton();
            this.gradientLinearDiamondButton = new ToolStripButton();
            this.gradientRadialButton = new ToolStripButton();
            this.gradientConicalButton = new ToolStripButton();
            this.gradientSeparator2 = new ToolStripSeparator();
            this.gradientChannelsSplitButton = new PdnToolStripSplitButton();
            this.penSeparator = new ToolStripSeparator();
            this.penSizeLabel = new ToolStripLabel();
            this.penSizeDecButton = new ToolStripButton();
            this.penSizeComboBox = new PdnToolStripComboBox(false);
            this.penSizeIncButton = new ToolStripButton();
            this.penStyleLabel = new ToolStripLabel();
            this.penStartCapSplitButton = new PdnToolStripSplitButton();
            this.penDashStyleSplitButton = new PdnToolStripSplitButton();
            this.penEndCapSplitButton = new PdnToolStripSplitButton();
            this.blendingSeparator = new ToolStripSeparator();
            this.antiAliasingSplitButton = new PdnToolStripSplitButton();
            this.alphaBlendingSplitButton = new PdnToolStripSplitButton();
            this.toleranceSeparator = new ToolStripSeparator();
            this.toleranceLabel = new ToolStripLabel();
            this.toleranceSlider = new ToleranceSliderControl();
            this.toleranceSliderStrip = new ToolStripControlHost(this.toleranceSlider);
            this.fontSeparator = new ToolStripSeparator();
            this.fontLabel = new ToolStripLabel();
            this.fontFamilyComboBox = new PdnToolStripComboBox(true);
            this.fontSizeComboBox = new PdnToolStripComboBox(false);
            this.fontSmoothingComboBox = new PdnToolStripComboBox(false);
            this.fontStyleSeparator = new ToolStripSeparator();
            this.fontBoldButton = new ToolStripButton();
            this.fontItalicsButton = new ToolStripButton();
            this.fontUnderlineButton = new ToolStripButton();
            this.fontStrikeoutButton = new ToolStripButton();
            this.fontAlignSeparator = new ToolStripSeparator();
            this.fontAlignLeftButton = new ToolStripButton();
            this.fontAlignCenterButton = new ToolStripButton();
            this.fontAlignRightButton = new ToolStripButton();
            this.resamplingSeparator = new ToolStripSeparator();
            this.resamplingLabel = new ToolStripLabel();
            this.resamplingComboBox = new PdnToolStripComboBox(false);
            this.colorPickerSeparator = new ToolStripSeparator();
            this.colorPickerLabel = new ToolStripLabel();
            this.colorPickerComboBox = new PdnToolStripComboBox(false);
            this.selectionCombineModeSeparator = new ToolStripSeparator();
            this.selectionCombineModeLabel = new ToolStripLabel();
            this.selectionCombineModeSplitButton = new PdnToolStripSplitButton();
            this.floodModeSeparator = new ToolStripSeparator();
            this.floodModeLabel = new ToolStripLabel();
            this.floodModeSplitButton = new PdnToolStripSplitButton();
            this.selectionDrawModeSeparator = new ToolStripSeparator();
            this.selectionDrawModeModeLabel = new ToolStripLabel();
            this.selectionDrawModeSplitButton = new PdnToolStripSplitButton();
            this.selectionDrawModeWidthLabel = new ToolStripLabel();
            this.selectionDrawModeWidthTextBox = new PdnToolStripTextBox();
            this.selectionDrawModeSwapButton = new ToolStripButton();
            this.selectionDrawModeHeightLabel = new ToolStripLabel();
            this.selectionDrawModeHeightTextBox = new PdnToolStripTextBox();
            this.selectionDrawModeUnits = new UnitsComboBoxStrip();
            base.SuspendLayout();
            this.brushStyleLabel.Name = "fillStyleLabel";
            this.brushStyleComboBox.Name = "styleComboBox";
            this.brushStyleComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this.brushStyleComboBox.DropDownWidth = 0xea;
            this.brushStyleComboBox.AutoSize = true;
            this.brushStyleComboBox.ComboBox.DrawMode = DrawMode.OwnerDrawVariable;
            this.brushStyleComboBox.ComboBox.MeasureItem += new MeasureItemEventHandler(this.ComboBoxStyle_MeasureItem);
            this.brushStyleComboBox.ComboBox.SelectedValueChanged += new EventHandler(this.ComboBoxStyle_SelectedValueChanged);
            this.brushStyleComboBox.ComboBox.DrawItem += new DrawItemEventHandler(this.ComboBoxStyle_DrawItem);
            this.shapeButton.Name = "shapeButton";
            this.shapeButton.DropDownOpening += new EventHandler(this.ShapeButton_DropDownOpening);
            this.shapeButton.DropDownClosed += (sender, e) => this.shapeButton.DropDownItems.Clear();
            this.shapeButton.ButtonClick += delegate (object sender, EventArgs e) {
                switch (this.ShapeDrawType)
                {
                    case PaintDotNet.ShapeDrawType.Interior:
                        this.ShapeDrawType = PaintDotNet.ShapeDrawType.Both;
                        return;

                    case PaintDotNet.ShapeDrawType.Outline:
                        this.ShapeDrawType = PaintDotNet.ShapeDrawType.Interior;
                        return;

                    case PaintDotNet.ShapeDrawType.Both:
                        this.ShapeDrawType = PaintDotNet.ShapeDrawType.Outline;
                        return;
                }
                throw new InvalidEnumArgumentException();
            };
            this.gradientSeparator1.Name = "gradientSeparator";
            this.gradientLinearClampedButton.Name = "gradientLinearClampedButton";
            this.gradientLinearClampedButton.Click += new EventHandler(this.GradientTypeButtonClicked);
            this.gradientLinearClampedButton.Tag = GradientType.LinearClamped;
            this.gradientLinearReflectedButton.Name = "gradientLinearReflectedButton";
            this.gradientLinearReflectedButton.Click += new EventHandler(this.GradientTypeButtonClicked);
            this.gradientLinearReflectedButton.Tag = GradientType.LinearReflected;
            this.gradientLinearDiamondButton.Name = "gradientLinearDiamondButton";
            this.gradientLinearDiamondButton.Click += new EventHandler(this.GradientTypeButtonClicked);
            this.gradientLinearDiamondButton.Tag = GradientType.LinearDiamond;
            this.gradientRadialButton.Name = "gradientRadialButton";
            this.gradientRadialButton.Click += new EventHandler(this.GradientTypeButtonClicked);
            this.gradientRadialButton.Tag = GradientType.Radial;
            this.gradientConicalButton.Name = "gradientConicalButton";
            this.gradientConicalButton.Click += new EventHandler(this.GradientTypeButtonClicked);
            this.gradientConicalButton.Tag = GradientType.Conical;
            this.gradientSeparator2.Name = "gradientSeparator2";
            this.gradientChannelsSplitButton.Name = "gradientChannelsSplitButton";
            this.gradientChannelsSplitButton.DropDownOpening += new EventHandler(this.GradientChannelsSplitButton_DropDownOpening);
            this.gradientChannelsSplitButton.DropDownClosed += (sender, e) => this.gradientChannelsSplitButton.DropDownItems.Clear();
            this.gradientChannelsSplitButton.ButtonClick += (sender, e) => (this.GradientInfo = new PaintDotNet.GradientInfo(this.GradientInfo.GradientType, !this.GradientInfo.AlphaOnly));
            this.penSeparator.Name = "penSeparator";
            this.penSizeLabel.Name = "brushSizeLabel";
            this.penSizeDecButton.Name = "penSizeDecButton";
            this.penSizeDecButton.Click += delegate (object sender, EventArgs e) {
                float delta = -1f;
                if ((Control.ModifierKeys & Keys.Control) != Keys.None)
                {
                    delta *= 5f;
                }
                this.AddToPenSize(delta);
            };
            this.penSizeComboBox.Name = "penSizeComboBox";
            this.penSizeComboBox.Validating += new CancelEventHandler(this.BrushSizeComboBox_Validating);
            this.penSizeComboBox.TextChanged += new EventHandler(this.SizeComboBox_TextChanged);
            this.penSizeComboBox.AutoSize = false;
            this.penSizeComboBox.Width = 0x2c;
            this.penSizeIncButton.Name = "penSizeIncButton";
            this.penSizeIncButton.Click += delegate (object sender, EventArgs e) {
                float delta = 1f;
                if ((Control.ModifierKeys & Keys.Control) != Keys.None)
                {
                    delta *= 5f;
                }
                this.AddToPenSize(delta);
            };
            this.penStyleLabel.Name = "penStartCapLabel";
            this.penStartCapSplitButton.Name = "penStartCapSplitButton";
            this.penStartCapSplitButton.DropDownOpening += new EventHandler(this.PenCapSplitButton_DropDownOpening);
            this.penStartCapSplitButton.DropDownClosed += (sender, e) => this.penStartCapSplitButton.DropDownItems.Clear();
            this.penStartCapSplitButton.ButtonClick += (sender, e) => this.CyclePenStartCap();
            this.penDashStyleSplitButton.Name = "penDashStyleSplitButton";
            this.penDashStyleSplitButton.ImageScaling = ToolStripItemImageScaling.None;
            this.penDashStyleSplitButton.DropDownOpening += new EventHandler(this.PenDashStyleButton_DropDownOpening);
            this.penDashStyleSplitButton.DropDownClosed += (sender, e) => this.penDashStyleSplitButton.DropDownItems.Clear();
            this.penDashStyleSplitButton.ButtonClick += (sender, e) => this.CyclePenDashStyle();
            this.penEndCapSplitButton.Name = "penEndCapSplitButton";
            this.penEndCapSplitButton.DropDownOpening += new EventHandler(this.PenCapSplitButton_DropDownOpening);
            this.penEndCapSplitButton.DropDownClosed += (sender, e) => this.penEndCapSplitButton.DropDownItems.Clear();
            this.penEndCapSplitButton.ButtonClick += (sender, e) => this.CyclePenEndCap();
            this.antiAliasingSplitButton.Name = "antiAliasingSplitButton";
            this.antiAliasingSplitButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            this.antiAliasingSplitButton.DropDownOpening += new EventHandler(this.AntiAliasingSplitButton_DropDownOpening);
            this.antiAliasingSplitButton.DropDownClosed += (sender, e) => this.antiAliasingSplitButton.DropDownItems.Clear();
            this.antiAliasingSplitButton.ButtonClick += (sender, e) => (this.AntiAliasing = !this.AntiAliasing);
            this.alphaBlendingSplitButton.Name = "alphaBlendingSplitButton";
            this.alphaBlendingSplitButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            this.alphaBlendingSplitButton.DropDownOpening += new EventHandler(this.AlphaBlendingSplitButton_DropDownOpening);
            this.alphaBlendingSplitButton.DropDownClosed += (sender, e) => this.alphaBlendingSplitButton.DropDownItems.Clear();
            this.alphaBlendingSplitButton.ButtonClick += (sender, e) => (this.AlphaBlending = !this.AlphaBlending);
            this.toleranceLabel.Name = "toleranceLabel";
            this.toleranceSlider.Name = "toleranceSlider";
            this.toleranceSlider.ToleranceChanged = (EventHandler) Delegate.Combine(this.toleranceSlider.ToleranceChanged, new EventHandler(this.ToleranceSlider_ToleranceChanged));
            this.toleranceSlider.Size = new System.Drawing.Size(150, 0x10);
            this.toleranceSliderStrip.Name = "toleranceSliderStrip";
            this.toleranceSliderStrip.AutoSize = false;
            this.fontLabel.Name = "fontLabel";
            this.fontFamilyComboBox.Name = "fontComboBox";
            this.fontFamilyComboBox.Size = new System.Drawing.Size(140, 0x15);
            this.fontFamilyComboBox.DropDownWidth = 300;
            this.fontFamilyComboBox.DropDownHeight = 600;
            this.fontFamilyComboBox.MaxDropDownItems = 12;
            this.fontFamilyComboBox.Sorted = true;
            this.fontFamilyComboBox.GotFocus += new EventHandler(this.FontFamilyComboBox_GotFocus);
            this.fontFamilyComboBox.Items.Add("Arial");
            this.fontFamilyComboBox.SelectedItem = "Arial";
            this.fontFamilyComboBox.SelectedIndexChanged += new EventHandler(this.FontFamilyComboBox_SelectedIndexChanged);
            this.fontFamilyComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this.fontFamilyComboBox.DropDown += (s, e) => (this.fontsComboBoxShown = true);
            this.fontFamilyComboBox.DropDownClosed += delegate (object s, EventArgs e) {
                if ((this.fontFamilyComboBoxBackgroundThread != null) && (this.fontFamilyComboBoxBackgroundThread is IDisposable))
                {
                    IDisposable thread = (IDisposable) this.fontFamilyComboBoxBackgroundThread;
                    this.fontFamilyComboBoxBackgroundThread = null;
                    ThreadPool.QueueUserWorkItem(_ => thread.Dispose());
                }
                else
                {
                    this.fontFamilyComboBoxBackgroundThread = null;
                }
            };
            this.fontFamilyComboBox.ComboBox.DrawMode = DrawMode.OwnerDrawVariable;
            this.fontFamilyComboBox.ComboBox.MeasureItem += new MeasureItemEventHandler(this.FontFamilyComboBox_MeasureItem);
            this.fontFamilyComboBox.ComboBox.DrawItem += new DrawItemEventHandler(this.FontFamilyComboBox_DrawItem);
            this.fontSizeComboBox.Name = "fontSizeComboBox";
            this.fontSizeComboBox.AutoSize = false;
            this.fontSizeComboBox.TextChanged += new EventHandler(this.FontSizeComboBox_TextChanged);
            this.fontSizeComboBox.Validating += new CancelEventHandler(this.FontSizeComboBox_Validating);
            this.fontSizeComboBox.Text = 12.ToString();
            this.fontSizeComboBox.Width = 0x2c;
            this.fontSmoothingComboBox.Name = "smoothingComboBox";
            this.fontSmoothingComboBox.AutoSize = false;
            this.fontSmoothingComboBox.Sorted = false;
            this.fontSmoothingComboBox.Width = 70;
            this.fontSmoothingComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this.fontSmoothingComboBox.SelectedIndexChanged += new EventHandler(this.SmoothingComboBox_SelectedIndexChanged);
            this.fontBoldButton.Name = "boldButton";
            this.fontItalicsButton.Name = "italicsButton";
            this.fontUnderlineButton.Name = "underlineButton";
            this.fontStrikeoutButton.Name = "strikeoutButton";
            this.fontAlignLeftButton.Name = "alignLeftButton";
            this.fontAlignCenterButton.Name = "alignCenterButton";
            this.fontAlignRightButton.Name = "alignRightButton";
            this.resamplingSeparator.Name = "resamplingSeparator";
            this.resamplingLabel.Name = "resamplingLabel";
            this.resamplingComboBox.Name = "resamplingComboBox";
            this.resamplingComboBox.AutoSize = true;
            this.resamplingComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this.resamplingComboBox.Sorted = false;
            this.resamplingComboBox.Width = 100;
            this.resamplingComboBox.DropDownWidth = 100;
            this.resamplingComboBox.SelectedIndexChanged += new EventHandler(this.ResamplingComboBox_SelectedIndexChanged);
            this.colorPickerSeparator.Name = "colorPickerSeparator";
            this.colorPickerLabel.Name = "colorPickerLabel";
            this.colorPickerComboBox.Name = "colorPickerComboBox";
            this.colorPickerComboBox.AutoSize = true;
            this.colorPickerComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this.colorPickerComboBox.Width = 200;
            this.colorPickerComboBox.DropDownWidth = 200;
            this.colorPickerComboBox.Sorted = false;
            this.colorPickerComboBox.SelectedIndexChanged += new EventHandler(this.ColorPickerComboBox_SelectedIndexChanged);
            this.selectionCombineModeSeparator.Name = "selectionCombineModeSeparator";
            this.selectionCombineModeLabel.Name = "selectionCombineModeLabel";
            this.selectionCombineModeSplitButton.Name = "selectionCombineModeSplitButton";
            this.selectionCombineModeSplitButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            this.selectionCombineModeSplitButton.DropDownOpening += new EventHandler(this.SelectionCombineModeSplitButton_DropDownOpening);
            this.selectionCombineModeSplitButton.DropDownClosed += (sender, e) => this.selectionCombineModeSplitButton.DropDownItems.Clear();
            this.selectionCombineModeSplitButton.ButtonClick += (sender, e) => (this.SelectionCombineMode = this.CycleSelectionCombineMode(this.SelectionCombineMode));
            this.floodModeSeparator.Name = "floodModeSeparator";
            this.floodModeLabel.Name = "floodModeLabel";
            this.floodModeSplitButton.Name = "floodModeSplitButton";
            this.floodModeSplitButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            this.floodModeSplitButton.DropDownOpening += new EventHandler(this.FloodModeSplitButton_DropDownOpening);
            this.floodModeSplitButton.DropDownClosed += (sender, e) => this.floodModeSplitButton.DropDownItems.Clear();
            this.floodModeSplitButton.ButtonClick += (sender, e) => (this.FloodMode = this.CycleFloodMode(this.FloodMode));
            this.selectionDrawModeSeparator.Name = "selectionDrawModeSeparator";
            this.selectionDrawModeModeLabel.Name = "selectionDrawModeModeLabel";
            this.selectionDrawModeSplitButton.Name = "selectionDrawModeSplitButton";
            this.selectionDrawModeSplitButton.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            this.selectionDrawModeSplitButton.DropDownOpening += new EventHandler(this.SelectionDrawModeSplitButton_DropDownOpening);
            this.selectionDrawModeSplitButton.DropDownClosed += (sender, e) => this.selectionDrawModeSplitButton.DropDownItems.Clear();
            this.selectionDrawModeSplitButton.ButtonClick += delegate (object sender, EventArgs e) {
                SelectionDrawMode newDrawMode = this.CycleSelectionDrawMode(this.SelectionDrawModeInfo.DrawMode);
                this.SelectionDrawModeInfo = this.SelectionDrawModeInfo.CloneWithNewDrawMode(newDrawMode);
            };
            this.selectionDrawModeWidthLabel.Name = "selectionDrawModeWidthLabel";
            this.selectionDrawModeWidthTextBox.Name = "selectionDrawModeWidthTextBox";
            this.selectionDrawModeWidthTextBox.TextBox.Width = 50;
            this.selectionDrawModeWidthTextBox.TextBoxTextAlign = HorizontalAlignment.Right;
            this.selectionDrawModeWidthTextBox.Enter += (sender, e) => this.selectionDrawModeWidthTextBox.TextBox.Select(0, this.selectionDrawModeWidthTextBox.TextBox.Text.Length);
            this.selectionDrawModeWidthTextBox.Leave += delegate (object sender, EventArgs e) {
                double num;
                if (double.TryParse(this.selectionDrawModeWidthTextBox.Text, out num))
                {
                    this.SelectionDrawModeInfo = this.selectionDrawModeInfo.CloneWithNewWidth(num);
                }
                else
                {
                    this.selectionDrawModeWidthTextBox.Text = this.selectionDrawModeInfo.Width.ToString();
                }
            };
            this.selectionDrawModeSwapButton.Name = "selectionDrawModeSwapButton";
            this.selectionDrawModeSwapButton.Click += delegate (object sender, EventArgs e) {
                PaintDotNet.SelectionDrawModeInfo selectionDrawModeInfo = this.SelectionDrawModeInfo;
                PaintDotNet.SelectionDrawModeInfo info2 = new PaintDotNet.SelectionDrawModeInfo(selectionDrawModeInfo.DrawMode, selectionDrawModeInfo.Height, selectionDrawModeInfo.Width, selectionDrawModeInfo.Units);
                this.SelectionDrawModeInfo = info2;
            };
            this.selectionDrawModeHeightLabel.Name = "selectionDrawModeHeightLabel";
            this.selectionDrawModeHeightTextBox.Name = "selectionDrawModeHeightTextBox";
            this.selectionDrawModeHeightTextBox.TextBox.Width = 50;
            this.selectionDrawModeHeightTextBox.TextBoxTextAlign = HorizontalAlignment.Right;
            this.selectionDrawModeHeightTextBox.Enter += (sender, e) => this.selectionDrawModeHeightTextBox.TextBox.Select(0, this.selectionDrawModeHeightTextBox.TextBox.Text.Length);
            this.selectionDrawModeHeightTextBox.Leave += delegate (object sender, EventArgs e) {
                double num;
                if (double.TryParse(this.selectionDrawModeHeightTextBox.Text, out num))
                {
                    this.SelectionDrawModeInfo = this.selectionDrawModeInfo.CloneWithNewHeight(num);
                }
                else
                {
                    this.selectionDrawModeHeightTextBox.Text = this.selectionDrawModeInfo.Height.ToString();
                }
            };
            this.selectionDrawModeUnits.Name = "selectionDrawModeUnits";
            this.selectionDrawModeUnits.UnitsDisplayType = UnitsDisplayType.Plural;
            this.selectionDrawModeUnits.LowercaseStrings = true;
            this.selectionDrawModeUnits.Size = new System.Drawing.Size(100, this.selectionDrawModeUnits.Height);
            this.AutoSize = true;
            this.Items.AddRange(new ToolStripItem[] { 
                this.selectionCombineModeSeparator, this.selectionCombineModeLabel, this.selectionCombineModeSplitButton, this.selectionDrawModeSeparator, this.selectionDrawModeModeLabel, this.selectionDrawModeSplitButton, this.selectionDrawModeWidthLabel, this.selectionDrawModeWidthTextBox, this.selectionDrawModeSwapButton, this.selectionDrawModeHeightLabel, this.selectionDrawModeHeightTextBox, this.selectionDrawModeUnits, this.floodModeSeparator, this.floodModeLabel, this.floodModeSplitButton, this.resamplingSeparator,
                this.resamplingLabel, this.resamplingComboBox, this.colorPickerSeparator, this.colorPickerLabel, this.colorPickerComboBox, this.fontSeparator, this.fontLabel, this.fontFamilyComboBox, this.fontSizeComboBox, this.fontSmoothingComboBox, this.fontStyleSeparator, this.fontBoldButton, this.fontItalicsButton, this.fontUnderlineButton, this.fontStrikeoutButton, this.fontAlignSeparator,
                this.fontAlignLeftButton, this.fontAlignCenterButton, this.fontAlignRightButton, this.shapeSeparator, this.shapeButton, this.gradientSeparator1, this.gradientLinearClampedButton, this.gradientLinearReflectedButton, this.gradientLinearDiamondButton, this.gradientRadialButton, this.gradientConicalButton, this.gradientSeparator2, this.gradientChannelsSplitButton, this.penSeparator, this.penSizeLabel, this.penSizeDecButton,
                this.penSizeComboBox, this.penSizeIncButton, this.penStyleLabel, this.penStartCapSplitButton, this.penDashStyleSplitButton, this.penEndCapSplitButton, this.brushSeparator, this.brushStyleLabel, this.brushStyleComboBox, this.toleranceSeparator, this.toleranceLabel, this.toleranceSliderStrip, this.blendingSeparator, this.antiAliasingSplitButton, this.alphaBlendingSplitButton
            });
            base.ResumeLayout(false);
        }

        public void LoadFromAppEnvironment(AppEnvironment appEnvironment)
        {
            this.AlphaBlending = appEnvironment.AlphaBlending;
            this.AntiAliasing = appEnvironment.AntiAliasing;
            this.BrushInfo = appEnvironment.BrushInfo;
            this.ColorPickerClickBehavior = appEnvironment.ColorPickerClickBehavior;
            this.GradientInfo = appEnvironment.GradientInfo;
            this.PenInfo = appEnvironment.PenInfo;
            this.ResamplingAlgorithm = appEnvironment.ResamplingAlgorithm;
            this.ShapeDrawType = appEnvironment.ShapeDrawType;
            this.FontInfo = appEnvironment.FontInfo;
            this.FontSmoothing = appEnvironment.FontSmoothing;
            this.FontAlignment = appEnvironment.TextAlignment;
            this.Tolerance = appEnvironment.Tolerance;
            this.SelectionCombineMode = appEnvironment.SelectionCombineMode;
            this.FloodMode = appEnvironment.FloodMode;
            this.SelectionDrawModeInfo = appEnvironment.SelectionDrawModeInfo;
        }

        private DashStyle NextDashStyle(DashStyle oldDash)
        {
            int index = (Array.IndexOf<DashStyle>(this.dashStyles, oldDash) + 1) % this.dashStyles.Length;
            return this.dashStyles[index];
        }

        private LineCap2 NextLineCap(LineCap2 oldCap)
        {
            int index = (Array.IndexOf<LineCap2>(this.lineCaps, oldCap) + 1) % this.lineCaps.Length;
            return this.lineCaps[index];
        }

        protected virtual void OnAlphaBlendingChanged()
        {
            if (this.AlphaBlendingChanged != null)
            {
                this.AlphaBlendingChanged(this, EventArgs.Empty);
            }
        }

        protected virtual void OnAntiAliasingChanged()
        {
            if (this.AntiAliasingChanged != null)
            {
                this.AntiAliasingChanged(this, EventArgs.Empty);
            }
        }

        protected virtual void OnBrushChanged()
        {
            if (this.BrushInfoChanged != null)
            {
                this.BrushInfoChanged(this, EventArgs.Empty);
            }
        }

        protected void OnColorPickerClickBehaviorChanged()
        {
            if (this.ColorPickerClickBehaviorChanged != null)
            {
                this.ColorPickerClickBehaviorChanged(this, EventArgs.Empty);
            }
        }

        protected void OnFloodModeChanged()
        {
            if (this.FloodModeChanged != null)
            {
                this.FloodModeChanged(this, EventArgs.Empty);
            }
        }

        protected virtual void OnFontInfoChanged()
        {
            if (this.FontInfoChanged != null)
            {
                this.FontInfoChanged(this, EventArgs.Empty);
            }
        }

        protected virtual void OnFontSmoothingChanged()
        {
            if (this.FontSmoothingChanged != null)
            {
                this.FontSmoothingChanged(this, EventArgs.Empty);
            }
        }

        protected virtual void OnGradientInfoChanged()
        {
            if (this.GradientInfoChanged != null)
            {
                this.GradientInfoChanged(this, EventArgs.Empty);
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            if ((this.toolBarConfigItems & (PaintDotNet.ToolBarConfigItems.None | PaintDotNet.ToolBarConfigItems.Text)) == (PaintDotNet.ToolBarConfigItems.None | PaintDotNet.ToolBarConfigItems.Text))
            {
                this.AsyncPrefetchFontNames();
            }
            base.OnHandleCreated(e);
        }

        protected override void OnItemClicked(ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem == this.fontBoldButton)
            {
                this.fontStyle ^= System.Drawing.FontStyle.Bold;
                this.SetFontStyleButtons(this.fontStyle);
                this.OnFontInfoChanged();
            }
            else if (e.ClickedItem == this.fontItalicsButton)
            {
                this.fontStyle ^= System.Drawing.FontStyle.Italic;
                this.SetFontStyleButtons(this.fontStyle);
                this.OnFontInfoChanged();
            }
            else if (e.ClickedItem == this.fontUnderlineButton)
            {
                this.fontStyle ^= System.Drawing.FontStyle.Underline;
                this.SetFontStyleButtons(this.fontStyle);
                this.OnFontInfoChanged();
            }
            else if (e.ClickedItem == this.fontStrikeoutButton)
            {
                this.fontStyle ^= System.Drawing.FontStyle.Strikeout;
                this.SetFontStyleButtons(this.fontStyle);
                this.OnFontInfoChanged();
            }
            else if (e.ClickedItem == this.fontAlignLeftButton)
            {
                this.FontAlignment = TextAlignment.Left;
            }
            else if (e.ClickedItem == this.fontAlignCenterButton)
            {
                this.FontAlignment = TextAlignment.Center;
            }
            else if (e.ClickedItem == this.fontAlignRightButton)
            {
                this.FontAlignment = TextAlignment.Right;
            }
            base.OnItemClicked(e);
        }

        protected virtual void OnPenChanged()
        {
            if (this.PenInfoChanged != null)
            {
                this.PenInfoChanged(this, EventArgs.Empty);
            }
        }

        protected void OnResamplingAlgorithmChanged()
        {
            if (this.ResamplingAlgorithmChanged != null)
            {
                this.ResamplingAlgorithmChanged(this, EventArgs.Empty);
            }
        }

        protected void OnSelectionCombineModeChanged()
        {
            if (this.SelectionCombineModeChanged != null)
            {
                this.SelectionCombineModeChanged(this, EventArgs.Empty);
            }
        }

        protected void OnSelectionDrawModeInfoChanged()
        {
            if (this.SelectionDrawModeInfoChanged != null)
            {
                this.SelectionDrawModeInfoChanged(this, EventArgs.Empty);
            }
        }

        protected void OnSelectionDrawModeUnitsChanged()
        {
            if (this.SelectionDrawModeUnitsChanged != null)
            {
                this.SelectionDrawModeUnitsChanged(this, EventArgs.Empty);
            }
        }

        protected void OnSelectionDrawModeUnitsChanging()
        {
            if (this.SelectionDrawModeUnitsChanging != null)
            {
                this.SelectionDrawModeUnitsChanging(this, EventArgs.Empty);
            }
        }

        protected virtual void OnShapeDrawTypeChanged()
        {
            if (this.ShapeDrawTypeChanged != null)
            {
                this.ShapeDrawTypeChanged(this, EventArgs.Empty);
            }
        }

        protected virtual void OnTextAlignmentChanged()
        {
            if (this.FontAlignmentChanged != null)
            {
                this.FontAlignmentChanged(this, EventArgs.Empty);
            }
        }

        protected void OnToleranceChanged()
        {
            if (this.ToleranceChanged != null)
            {
                this.ToleranceChanged(this, EventArgs.Empty);
            }
        }

        private void OnToolBarConfigItemsChanged()
        {
            if (this.ToolBarConfigItemsChanged != null)
            {
                this.ToolBarConfigItemsChanged(this, EventArgs.Empty);
            }
        }

        private void PenCapSplitButton_DropDownOpening(object sender, EventArgs e)
        {
            bool flag;
            EventHandler onClick = null;
            PdnToolStripSplitButton objA = (PdnToolStripSplitButton) sender;
            List<ToolStripMenuItem> items = new List<ToolStripMenuItem>();
            LineCap2 flat = LineCap2.Flat;
            if (object.ReferenceEquals(objA, this.penStartCapSplitButton))
            {
                flag = true;
                flat = this.PenInfo.StartCap;
            }
            else
            {
                if (!object.ReferenceEquals(objA, this.penEndCapSplitButton))
                {
                    throw new InvalidOperationException();
                }
                flag = false;
                flat = this.PenInfo.EndCap;
            }
            foreach (LineCap2 cap2 in this.lineCaps)
            {
                if (onClick == null)
                {
                    onClick = delegate (object sender2, EventArgs e2) {
                        ToolStripMenuItem item = (ToolStripMenuItem) sender2;
                        Pair<PdnToolStripSplitButton, LineCap2> tag = (Pair<PdnToolStripSplitButton, LineCap2>) item.Tag;
                        PaintDotNet.PenInfo info = this.PenInfo.Clone();
                        if (object.ReferenceEquals(tag.First, this.penStartCapSplitButton))
                        {
                            info.StartCap = tag.Second;
                        }
                        else if (object.ReferenceEquals(tag.First, this.penEndCapSplitButton))
                        {
                            info.EndCap = tag.Second;
                        }
                        this.PenInfo = info;
                    };
                }
                ToolStripMenuItem item = new ToolStripMenuItem(this.lineCapLocalizer.GetLocalizedEnumValue(cap2).LocalizedName, this.GetLineCapImage(cap2, flag).Reference, onClick) {
                    Tag = Pair.Create<PdnToolStripSplitButton, LineCap2>(objA, cap2)
                };
                if (cap2 == flat)
                {
                    item.Checked = true;
                }
                items.Add(item);
            }
            objA.DropDownItems.Clear();
            objA.DropDownItems.AddRange(items.ToArrayEx<ToolStripMenuItem>());
        }

        private void PenDashStyleButton_DropDownOpening(object sender, EventArgs e)
        {
            EventHandler onClick = null;
            List<ToolStripMenuItem> items = new List<ToolStripMenuItem>();
            foreach (DashStyle style in this.dashStyles)
            {
                if (onClick == null)
                {
                    onClick = delegate (object sender2, EventArgs e2) {
                        ToolStripMenuItem item = (ToolStripMenuItem) sender2;
                        DashStyle tag = (DashStyle) item.Tag;
                        PaintDotNet.PenInfo info = this.PenInfo.Clone();
                        info.DashStyle = tag;
                        this.PenInfo = info;
                    };
                }
                ToolStripMenuItem item = new ToolStripMenuItem(this.dashStyleLocalizer.GetLocalizedEnumValue(style).LocalizedName, this.GetDashStyleImage(style), onClick) {
                    ImageScaling = ToolStripItemImageScaling.None
                };
                if (style == this.PenInfo.DashStyle)
                {
                    item.Checked = true;
                }
                item.Tag = style;
                items.Add(item);
            }
            this.penDashStyleSplitButton.DropDownItems.Clear();
            this.penDashStyleSplitButton.DropDownItems.AddRange(items.ToArrayEx<ToolStripMenuItem>());
        }

        public void PerformAlphaBlendingChanged()
        {
            this.OnAlphaBlendingChanged();
        }

        public void PerformAntiAliasingChanged()
        {
            this.OnAntiAliasingChanged();
        }

        public void PerformBrushChanged()
        {
            this.OnBrushChanged();
        }

        public void PerformColorPickerClickBehaviorChanged()
        {
            this.OnColorPickerClickBehaviorChanged();
        }

        public void PerformFloodModeChanged()
        {
            this.OnFloodModeChanged();
        }

        public void PerformGradientInfoChanged()
        {
            this.OnGradientInfoChanged();
        }

        public void PerformPenChanged()
        {
            this.OnPenChanged();
        }

        public void PerformResamplingAlgorithmChanged()
        {
            this.OnResamplingAlgorithmChanged();
        }

        public void PerformSelectionCombineModeChanged()
        {
            this.OnSelectionCombineModeChanged();
        }

        public void PerformSelectionDrawModeInfoChanged()
        {
            this.OnSelectionDrawModeInfoChanged();
        }

        public void PerformShapeDrawTypeChanged()
        {
            this.OnShapeDrawTypeChanged();
        }

        public void PerformToleranceChanged()
        {
            this.OnToleranceChanged();
        }

        private void RefreshSelectionDrawModeInfoVisibilities()
        {
            if (this.selectionDrawModeInfo != null)
            {
                base.SuspendLayout();
                bool flag = (this.ToolBarConfigItems & (PaintDotNet.ToolBarConfigItems.None | PaintDotNet.ToolBarConfigItems.SelectionDrawMode)) != PaintDotNet.ToolBarConfigItems.None;
                this.selectionDrawModeModeLabel.Visible = false;
                bool flag2 = flag & (this.selectionDrawModeInfo.DrawMode != SelectionDrawMode.Normal);
                this.selectionDrawModeWidthTextBox.Visible = flag2;
                this.selectionDrawModeHeightTextBox.Visible = flag2;
                this.selectionDrawModeWidthLabel.Visible = flag2;
                this.selectionDrawModeHeightLabel.Visible = flag2;
                this.selectionDrawModeSwapButton.Visible = flag2;
                this.selectionDrawModeUnits.Visible = flag & (this.selectionDrawModeInfo.DrawMode == SelectionDrawMode.FixedSize);
                base.ResumeLayout(false);
                base.PerformLayout();
            }
        }

        private void ResamplingComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.OnResamplingAlgorithmChanged();
        }

        private void SelectionCombineModeSplitButton_DropDownOpening(object sender, EventArgs e)
        {
            EventHandler onClick = null;
            this.selectionCombineModeSplitButton.DropDownItems.Clear();
            PaintDotNet.SelectionCombineMode[] modeArray = new PaintDotNet.SelectionCombineMode[5];
            modeArray[1] = PaintDotNet.SelectionCombineMode.Union;
            modeArray[2] = PaintDotNet.SelectionCombineMode.Exclude;
            modeArray[3] = PaintDotNet.SelectionCombineMode.Intersect;
            modeArray[4] = PaintDotNet.SelectionCombineMode.Xor;
            foreach (PaintDotNet.SelectionCombineMode mode in modeArray)
            {
                if (onClick == null)
                {
                    onClick = delegate (object sender2, EventArgs e2) {
                        ToolStripMenuItem item = (ToolStripMenuItem) sender2;
                        PaintDotNet.SelectionCombineMode tag = (PaintDotNet.SelectionCombineMode) item.Tag;
                        this.SelectionCombineMode = tag;
                    };
                }
                ToolStripMenuItem item = new ToolStripMenuItem(PdnResources.GetString2("ToolConfigStrip.SelectionCombineModeSplitButton." + mode.ToString() + ".Text"), this.GetSelectionCombineModeImage(mode).Reference, onClick) {
                    Tag = mode,
                    Checked = mode == this.SelectionCombineMode
                };
                this.selectionCombineModeSplitButton.DropDownItems.Add(item);
            }
        }

        private void SelectionDrawModeSplitButton_DropDownOpening(object sender, EventArgs e)
        {
            EventHandler onClick = null;
            this.selectionDrawModeSplitButton.DropDownItems.Clear();
            SelectionDrawMode[] modeArray = new SelectionDrawMode[3];
            modeArray[1] = SelectionDrawMode.FixedRatio;
            modeArray[2] = SelectionDrawMode.FixedSize;
            foreach (SelectionDrawMode mode in modeArray)
            {
                if (onClick == null)
                {
                    onClick = delegate (object sender2, EventArgs e2) {
                        ToolStripMenuItem item = (ToolStripMenuItem) sender2;
                        SelectionDrawMode newDrawMode = (SelectionDrawMode) item.Tag;
                        this.SelectionDrawModeInfo = this.SelectionDrawModeInfo.CloneWithNewDrawMode(newDrawMode);
                    };
                }
                ToolStripMenuItem item = new ToolStripMenuItem(this.GetSelectionDrawModeString(mode), this.GetSelectionDrawModeImage(mode), onClick) {
                    Tag = mode,
                    Checked = mode == this.SelectionDrawModeInfo.DrawMode
                };
                this.selectionDrawModeSplitButton.DropDownItems.Add(item);
            }
        }

        private void SelectionDrawModeUnits_UnitsChanged(object sender, EventArgs e)
        {
            this.OnSelectionDrawModeUnitsChanging();
            this.SelectionDrawModeInfo = this.selectionDrawModeInfo.CloneWithNewUnits(this.selectionDrawModeUnits.Units);
            this.OnSelectionDrawModeUnitsChanged();
        }

        private void SetFontStyleButtons(System.Drawing.FontStyle style)
        {
            bool flag = (style & System.Drawing.FontStyle.Bold) != System.Drawing.FontStyle.Regular;
            bool flag2 = (style & System.Drawing.FontStyle.Italic) != System.Drawing.FontStyle.Regular;
            bool flag3 = (style & System.Drawing.FontStyle.Underline) != System.Drawing.FontStyle.Regular;
            bool flag4 = (style & System.Drawing.FontStyle.Strikeout) != System.Drawing.FontStyle.Regular;
            this.fontBoldButton.Checked = flag;
            this.fontItalicsButton.Checked = flag2;
            this.fontUnderlineButton.Checked = flag3;
            this.fontStrikeoutButton.Checked = flag4;
        }

        private void ShapeButton_DropDownOpening(object sender, EventArgs e)
        {
            ToolStripMenuItem item = new ToolStripMenuItem {
                Text = PdnResources.GetString2("ShapeDrawTypeConfigWidget.OutlineButton.ToolTipText"),
                Image = this.shapeOutlineImage.Reference,
                Tag = PaintDotNet.ShapeDrawType.Outline
            };
            item.Click += new EventHandler(this.ShapeMI_Click);
            ToolStripMenuItem item2 = new ToolStripMenuItem {
                Text = PdnResources.GetString2("ShapeDrawTypeConfigWidget.InteriorButton.ToolTipText"),
                Image = this.shapeInteriorImage.Reference,
                Tag = PaintDotNet.ShapeDrawType.Interior
            };
            item2.Click += new EventHandler(this.ShapeMI_Click);
            ToolStripMenuItem item3 = new ToolStripMenuItem {
                Text = PdnResources.GetString2("ShapeDrawTypeConfigWidget.BothButton.ToolTipText"),
                Image = this.shapeBothImage.Reference,
                Tag = PaintDotNet.ShapeDrawType.Both
            };
            item3.Click += new EventHandler(this.ShapeMI_Click);
            switch (this.shapeDrawType)
            {
                case PaintDotNet.ShapeDrawType.Interior:
                    item2.Checked = true;
                    break;

                case PaintDotNet.ShapeDrawType.Outline:
                    item.Checked = true;
                    break;

                case PaintDotNet.ShapeDrawType.Both:
                    item3.Checked = true;
                    break;

                default:
                    throw new InvalidEnumArgumentException();
            }
            this.shapeButton.DropDownItems.AddRange(new ToolStripItem[] { item, item2, item3 });
        }

        private void ShapeMI_Click(object sender, EventArgs e)
        {
            PaintDotNet.ShapeDrawType tag = (PaintDotNet.ShapeDrawType) ((ToolStripMenuItem) sender).Tag;
            this.ShapeDrawType = tag;
        }

        private void SizeComboBox_TextChanged(object sender, EventArgs e)
        {
            this.BrushSizeComboBox_Validating(this, new CancelEventArgs());
        }

        private void SmoothingComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.OnFontSmoothingChanged();
        }

        private void SyncGradientInfo()
        {
            this.gradientConicalButton.Checked = false;
            this.gradientRadialButton.Checked = false;
            this.gradientLinearClampedButton.Checked = false;
            this.gradientLinearReflectedButton.Checked = false;
            this.gradientLinearDiamondButton.Checked = false;
            switch (this.gradientInfo.GradientType)
            {
                case GradientType.LinearClamped:
                    this.gradientLinearClampedButton.Checked = true;
                    break;

                case GradientType.LinearReflected:
                    this.gradientLinearReflectedButton.Checked = true;
                    break;

                case GradientType.LinearDiamond:
                    this.gradientLinearDiamondButton.Checked = true;
                    break;

                case GradientType.Radial:
                    this.gradientRadialButton.Checked = true;
                    break;

                case GradientType.Conical:
                    this.gradientConicalButton.Checked = true;
                    break;

                default:
                    throw new InvalidEnumArgumentException();
            }
            if (this.gradientInfo.AlphaOnly)
            {
                this.gradientChannelsSplitButton.Image = this.gradientAlphaChannelOnlyImage.Reference;
            }
            else
            {
                this.gradientChannelsSplitButton.Image = this.gradientAllColorChannelsImage.Reference;
            }
        }

        private void SyncSelectionDrawModeInfoUI()
        {
            this.selectionDrawModeSplitButton.Text = this.GetSelectionDrawModeString(this.selectionDrawModeInfo.DrawMode);
            this.selectionDrawModeSplitButton.Image = this.GetSelectionDrawModeImage(this.selectionDrawModeInfo.DrawMode);
            this.selectionDrawModeWidthTextBox.Text = this.selectionDrawModeInfo.Width.ToString();
            this.selectionDrawModeHeightTextBox.Text = this.selectionDrawModeInfo.Height.ToString();
            this.selectionDrawModeUnits.UnitsChanged -= new EventHandler(this.SelectionDrawModeUnits_UnitsChanged);
            this.selectionDrawModeUnits.Units = this.selectionDrawModeInfo.Units;
            this.selectionDrawModeUnits.UnitsChanged += new EventHandler(this.SelectionDrawModeUnits_UnitsChanged);
            this.RefreshSelectionDrawModeInfoVisibilities();
        }

        private void ToleranceSlider_ToleranceChanged(object sender, EventArgs e)
        {
            this.OnToleranceChanged();
        }

        public bool AlphaBlending
        {
            get => 
                this.alphaBlendingEnabled;
            set
            {
                if (value != this.alphaBlendingEnabled)
                {
                    this.alphaBlendingEnabled = value;
                    if (value)
                    {
                        this.alphaBlendingSplitButton.Image = this.alphaBlendingEnabledImage.Reference;
                    }
                    else
                    {
                        this.alphaBlendingSplitButton.Image = this.alphaBlendingOverwriteImage.Reference;
                    }
                    this.OnAlphaBlendingChanged();
                }
            }
        }

        public bool AntiAliasing
        {
            get => 
                this.antiAliasingEnabled;
            set
            {
                if (this.antiAliasingEnabled != value)
                {
                    if (value)
                    {
                        this.antiAliasingSplitButton.Image = this.antiAliasingEnabledImage.Reference;
                    }
                    else
                    {
                        this.antiAliasingSplitButton.Image = this.antiAliasingDisabledImage.Reference;
                    }
                    this.antiAliasingEnabled = value;
                    this.OnAntiAliasingChanged();
                }
            }
        }

        public PaintDotNet.BrushInfo BrushInfo
        {
            get
            {
                if (this.brushStyleComboBox.SelectedItem.ToString() == this.solidBrushText)
                {
                    return new PaintDotNet.BrushInfo(PaintDotNet.BrushType.Solid, HatchStyle.BackwardDiagonal);
                }
                if (this.brushStyleComboBox.SelectedIndex == -1)
                {
                    return new PaintDotNet.BrushInfo(PaintDotNet.BrushType.Solid, HatchStyle.BackwardDiagonal);
                }
                return new PaintDotNet.BrushInfo(PaintDotNet.BrushType.Hatch, (HatchStyle) ((LocalizedEnumValue) this.brushStyleComboBox.SelectedItem).EnumValue);
            }
            set
            {
                if (value.BrushType == PaintDotNet.BrushType.Solid)
                {
                    this.brushStyleComboBox.SelectedItem = this.solidBrushText;
                }
                else
                {
                    this.brushStyleComboBox.SelectedItem = this.hatchStyleNames.GetLocalizedEnumValue(value.HatchStyle);
                }
            }
        }

        public PaintDotNet.ColorPickerClickBehavior ColorPickerClickBehavior
        {
            get => 
                ((PaintDotNet.ColorPickerClickBehavior) ((LocalizedEnumValue) this.colorPickerComboBox.SelectedItem).EnumValue);
            set
            {
                if (value != this.ColorPickerClickBehavior)
                {
                    this.colorPickerComboBox.SelectedItem = this.colorPickerBehaviorNames.GetLocalizedEnumValue(value);
                }
            }
        }

        public PaintDotNet.FloodMode FloodMode
        {
            get
            {
                object tag = this.floodModeSplitButton.Tag;
                return ((tag == null) ? PaintDotNet.FloodMode.Local : ((PaintDotNet.FloodMode) tag));
            }
            set
            {
                object tag = this.floodModeSplitButton.Tag;
                PaintDotNet.FloodMode mode = (tag == null) ? PaintDotNet.FloodMode.Local : ((PaintDotNet.FloodMode) tag);
                if ((tag == null) || ((tag != null) && (value != mode)))
                {
                    this.floodModeSplitButton.Tag = value;
                    this.floodModeSplitButton.Image = this.GetFloodModeImage(value).Reference;
                    this.OnFloodModeChanged();
                }
            }
        }

        public TextAlignment FontAlignment
        {
            get => 
                this.alignment;
            set
            {
                if (this.alignment != value)
                {
                    this.alignment = value;
                    if (this.alignment == TextAlignment.Left)
                    {
                        this.fontAlignLeftButton.Checked = true;
                        this.fontAlignCenterButton.Checked = false;
                        this.fontAlignRightButton.Checked = false;
                    }
                    else if (this.alignment == TextAlignment.Center)
                    {
                        this.fontAlignLeftButton.Checked = false;
                        this.fontAlignCenterButton.Checked = true;
                        this.fontAlignRightButton.Checked = false;
                    }
                    else
                    {
                        if (this.alignment != TextAlignment.Right)
                        {
                            throw new InvalidOperationException("Text alignment type is invalid");
                        }
                        this.fontAlignLeftButton.Checked = false;
                        this.fontAlignCenterButton.Checked = false;
                        this.fontAlignRightButton.Checked = true;
                    }
                    this.OnTextAlignmentChanged();
                }
            }
        }

        public string FontFamilyName
        {
            get
            {
                try
                {
                    return (string) this.fontFamilyComboBox.SelectedItem;
                }
                catch (ArgumentException)
                {
                    return this.defaultFontFamily.Name;
                }
            }
            set
            {
                string selectedItem = (string) this.fontFamilyComboBox.SelectedItem;
                if (selectedItem != value)
                {
                    int index = this.fontFamilyComboBox.Items.IndexOf(value);
                    if (index != -1)
                    {
                        this.fontFamilyComboBox.SelectedIndex = index;
                        this.OnFontInfoChanged();
                    }
                    else
                    {
                        this.fontFamilyComboBox.Items.Add(value);
                        this.fontFamilyComboBox.SelectedItem = value;
                    }
                }
            }
        }

        public PaintDotNet.FontInfo FontInfo
        {
            get => 
                new PaintDotNet.FontInfo(this.FontFamilyName, this.FontSize, this.FontStyle);
            set
            {
                this.FontFamilyName = value.FontFamilyName;
                this.FontSize = value.Size;
                this.FontStyle = value.FontStyle;
            }
        }

        public float FontSize
        {
            get
            {
                bool flag = false;
                float oldSizeValue = this.oldSizeValue;
                try
                {
                    oldSizeValue = float.Parse(this.fontSizeComboBox.Text);
                }
                catch (FormatException)
                {
                    flag = true;
                }
                catch (OverflowException)
                {
                    flag = true;
                }
                if (!flag)
                {
                    this.oldSizeValue = oldSizeValue;
                }
                return this.oldSizeValue;
            }
            set
            {
                bool flag = false;
                try
                {
                    float.Parse(this.fontSizeComboBox.Text);
                }
                catch (FormatException)
                {
                    flag = true;
                }
                catch (OverflowException)
                {
                    flag = true;
                }
                if (!flag && (float.Parse(this.fontSizeComboBox.Text) != value))
                {
                    this.fontSizeComboBox.Text = value.ToString();
                    this.OnFontInfoChanged();
                }
            }
        }

        public PaintDotNet.SystemLayer.FontSmoothing FontSmoothing
        {
            get => 
                ((PaintDotNet.SystemLayer.FontSmoothing) ((LocalizedEnumValue) this.fontSmoothingComboBox.SelectedItem).EnumValue);
            set
            {
                if (value != this.FontSmoothing)
                {
                    this.fontSmoothingComboBox.SelectedItem = this.fontSmoothingLocalizer.GetLocalizedEnumValue(value);
                }
            }
        }

        public System.Drawing.FontStyle FontStyle
        {
            get => 
                this.fontStyle;
            set
            {
                if (this.fontStyle != value)
                {
                    this.fontStyle = value;
                    this.SetFontStyleButtons(this.FontStyle);
                    this.OnFontInfoChanged();
                }
            }
        }

        public PaintDotNet.GradientInfo GradientInfo
        {
            get => 
                this.gradientInfo;
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException();
                }
                this.gradientInfo = value;
                this.OnGradientInfoChanged();
                this.SyncGradientInfo();
            }
        }

        public PaintDotNet.PenInfo PenInfo
        {
            get
            {
                float num;
                LineCap2 tag;
                LineCap2 flat;
                try
                {
                    num = float.Parse(this.penSizeComboBox.Text);
                }
                catch (FormatException)
                {
                    num = 2f;
                }
                try
                {
                    tag = (LineCap2) this.penStartCapSplitButton.Tag;
                }
                catch (Exception)
                {
                    tag = LineCap2.Flat;
                }
                try
                {
                    flat = (LineCap2) this.penEndCapSplitButton.Tag;
                }
                catch (Exception)
                {
                    flat = LineCap2.Flat;
                }
                return new PaintDotNet.PenInfo((DashStyle) this.penDashStyleSplitButton.Tag, num, tag, flat, 1f);
            }
            set
            {
                if (this.PenInfo != value)
                {
                    this.penSizeComboBox.Text = value.Width.ToString();
                    this.penStartCapSplitButton.Tag = value.StartCap;
                    this.penStartCapSplitButton.Image = this.GetLineCapImage(value.StartCap, true).Reference;
                    this.penDashStyleSplitButton.Tag = value.DashStyle;
                    this.penDashStyleSplitButton.Image = this.GetDashStyleImage(value.DashStyle);
                    this.penEndCapSplitButton.Tag = value.EndCap;
                    this.penEndCapSplitButton.Image = this.GetLineCapImage(value.EndCap, false).Reference;
                    this.OnPenChanged();
                }
            }
        }

        public PaintDotNet.ResamplingAlgorithm ResamplingAlgorithm
        {
            get => 
                ((PaintDotNet.ResamplingAlgorithm) ((LocalizedEnumValue) this.resamplingComboBox.SelectedItem).EnumValue);
            set
            {
                if (value != this.ResamplingAlgorithm)
                {
                    if ((value != PaintDotNet.ResamplingAlgorithm.NearestNeighbor) && (value != PaintDotNet.ResamplingAlgorithm.Bilinear))
                    {
                        throw new InvalidEnumArgumentException();
                    }
                    this.resamplingComboBox.SelectedItem = this.resamplingAlgorithmNames.GetLocalizedEnumValue(value);
                }
            }
        }

        public PaintDotNet.SelectionCombineMode SelectionCombineMode
        {
            get
            {
                object tag = this.selectionCombineModeSplitButton.Tag;
                return ((tag == null) ? PaintDotNet.SelectionCombineMode.Replace : ((PaintDotNet.SelectionCombineMode) tag));
            }
            set
            {
                object tag = this.selectionCombineModeSplitButton.Tag;
                PaintDotNet.SelectionCombineMode mode = (tag == null) ? PaintDotNet.SelectionCombineMode.Replace : ((PaintDotNet.SelectionCombineMode) tag);
                if ((tag == null) || ((tag != null) && (value != mode)))
                {
                    this.selectionCombineModeSplitButton.Tag = value;
                    UI.SuspendControlPainting(this);
                    this.selectionCombineModeSplitButton.Image = this.GetSelectionCombineModeImage(value).Reference;
                    UI.ResumeControlPainting(this);
                    this.OnSelectionCombineModeChanged();
                    base.Invalidate(true);
                }
            }
        }

        public PaintDotNet.SelectionDrawModeInfo SelectionDrawModeInfo
        {
            get => 
                (this.selectionDrawModeInfo ?? PaintDotNet.SelectionDrawModeInfo.CreateDefault()).Clone();
            set
            {
                if ((this.selectionDrawModeInfo == null) || !this.selectionDrawModeInfo.Equals(value))
                {
                    this.selectionDrawModeInfo = value.Clone();
                    this.OnSelectionDrawModeInfoChanged();
                    this.SyncSelectionDrawModeInfoUI();
                }
            }
        }

        public PaintDotNet.ShapeDrawType ShapeDrawType
        {
            get => 
                this.shapeDrawType;
            set
            {
                if (this.shapeDrawType != value)
                {
                    this.shapeDrawType = value;
                    if (this.shapeDrawType == PaintDotNet.ShapeDrawType.Outline)
                    {
                        this.shapeButton.Image = this.shapeOutlineImage.Reference;
                    }
                    else if (this.shapeDrawType == PaintDotNet.ShapeDrawType.Both)
                    {
                        this.shapeButton.Image = this.shapeBothImage.Reference;
                    }
                    else
                    {
                        if (this.shapeDrawType != PaintDotNet.ShapeDrawType.Interior)
                        {
                            throw new InvalidOperationException("Shape draw type is invalid");
                        }
                        this.shapeButton.Image = this.shapeInteriorImage.Reference;
                    }
                    this.OnShapeDrawTypeChanged();
                }
            }
        }

        public float Tolerance
        {
            get => 
                this.toleranceSlider.Tolerance;
            set
            {
                if (value != this.toleranceSlider.Tolerance)
                {
                    this.toleranceSlider.Tolerance = value;
                }
            }
        }

        public PaintDotNet.ToolBarConfigItems ToolBarConfigItems
        {
            get => 
                this.toolBarConfigItems;
            set
            {
                if (this.toolBarConfigItems != value)
                {
                    this.toolBarConfigItems = value;
                    bool flag = base.Visible && base.IsHandleCreated;
                    if (flag)
                    {
                        UI.SuspendControlPainting(this);
                    }
                    base.SuspendLayout();
                    bool flag2 = (value & (PaintDotNet.ToolBarConfigItems.None | PaintDotNet.ToolBarConfigItems.Pen)) != PaintDotNet.ToolBarConfigItems.None;
                    this.penSeparator.Visible = flag2;
                    this.penSizeLabel.Visible = flag2;
                    this.penSizeDecButton.Visible = flag2;
                    this.penSizeComboBox.Visible = flag2;
                    this.penSizeIncButton.Visible = flag2;
                    bool flag3 = (value & (PaintDotNet.ToolBarConfigItems.None | PaintDotNet.ToolBarConfigItems.PenCaps)) != PaintDotNet.ToolBarConfigItems.None;
                    this.penStyleLabel.Visible = flag3;
                    this.penStartCapSplitButton.Visible = flag3;
                    this.penDashStyleSplitButton.Visible = flag3;
                    this.penEndCapSplitButton.Visible = flag3;
                    bool flag4 = (value & PaintDotNet.ToolBarConfigItems.Brush) != PaintDotNet.ToolBarConfigItems.None;
                    this.brushSeparator.Visible = flag4;
                    this.brushStyleLabel.Visible = flag4;
                    this.brushStyleComboBox.Visible = flag4;
                    bool flag5 = (value & (PaintDotNet.ToolBarConfigItems.None | PaintDotNet.ToolBarConfigItems.ShapeType)) != PaintDotNet.ToolBarConfigItems.None;
                    this.shapeSeparator.Visible = flag5;
                    this.shapeButton.Visible = flag5;
                    bool flag6 = (value & PaintDotNet.ToolBarConfigItems.Gradient) != PaintDotNet.ToolBarConfigItems.None;
                    this.gradientSeparator1.Visible = flag6;
                    this.gradientLinearClampedButton.Visible = flag6;
                    this.gradientLinearReflectedButton.Visible = flag6;
                    this.gradientLinearDiamondButton.Visible = flag6;
                    this.gradientRadialButton.Visible = flag6;
                    this.gradientConicalButton.Visible = flag6;
                    this.gradientSeparator2.Visible = flag6;
                    this.gradientChannelsSplitButton.Visible = flag6;
                    bool flag7 = (value & PaintDotNet.ToolBarConfigItems.Antialiasing) != PaintDotNet.ToolBarConfigItems.None;
                    this.antiAliasingSplitButton.Visible = flag7;
                    bool flag8 = (value & PaintDotNet.ToolBarConfigItems.AlphaBlending) != PaintDotNet.ToolBarConfigItems.None;
                    this.alphaBlendingSplitButton.Visible = flag8;
                    bool flag9 = (value & (PaintDotNet.ToolBarConfigItems.AlphaBlending | PaintDotNet.ToolBarConfigItems.Antialiasing)) != PaintDotNet.ToolBarConfigItems.None;
                    this.blendingSeparator.Visible = flag9;
                    bool flag10 = (value & (PaintDotNet.ToolBarConfigItems.None | PaintDotNet.ToolBarConfigItems.Tolerance)) != PaintDotNet.ToolBarConfigItems.None;
                    this.toleranceSeparator.Visible = flag10;
                    this.toleranceLabel.Visible = flag10;
                    this.toleranceSliderStrip.Visible = flag10;
                    bool flag11 = (value & (PaintDotNet.ToolBarConfigItems.None | PaintDotNet.ToolBarConfigItems.Text)) != PaintDotNet.ToolBarConfigItems.None;
                    this.fontSeparator.Visible = flag11;
                    this.fontLabel.Visible = flag11;
                    this.fontFamilyComboBox.Visible = flag11;
                    this.fontSizeComboBox.Visible = flag11;
                    this.fontSmoothingComboBox.Visible = false;
                    this.fontStyleSeparator.Visible = flag11;
                    this.fontBoldButton.Visible = flag11;
                    this.fontItalicsButton.Visible = flag11;
                    this.fontUnderlineButton.Visible = flag11;
                    this.fontStrikeoutButton.Visible = flag11;
                    this.fontAlignSeparator.Visible = flag11;
                    this.fontAlignLeftButton.Visible = flag11;
                    this.fontAlignCenterButton.Visible = flag11;
                    this.fontAlignRightButton.Visible = flag11;
                    if (flag11 && base.IsHandleCreated)
                    {
                        this.AsyncPrefetchFontNames();
                    }
                    bool flag12 = (value & (PaintDotNet.ToolBarConfigItems.None | PaintDotNet.ToolBarConfigItems.Resampling)) != PaintDotNet.ToolBarConfigItems.None;
                    this.resamplingSeparator.Visible = flag12;
                    this.resamplingLabel.Visible = flag12;
                    this.resamplingComboBox.Visible = flag12;
                    bool flag13 = (value & PaintDotNet.ToolBarConfigItems.ColorPickerBehavior) != PaintDotNet.ToolBarConfigItems.None;
                    this.colorPickerSeparator.Visible = flag13;
                    this.colorPickerLabel.Visible = flag13;
                    this.colorPickerComboBox.Visible = flag13;
                    bool flag14 = (value & (PaintDotNet.ToolBarConfigItems.None | PaintDotNet.ToolBarConfigItems.SelectionCombineMode)) != PaintDotNet.ToolBarConfigItems.None;
                    this.selectionCombineModeSeparator.Visible = flag14;
                    this.selectionCombineModeLabel.Visible = flag14;
                    this.selectionCombineModeSplitButton.Visible = flag14;
                    bool flag15 = (value & PaintDotNet.ToolBarConfigItems.FloodMode) != PaintDotNet.ToolBarConfigItems.None;
                    this.floodModeSeparator.Visible = flag15;
                    this.floodModeLabel.Visible = flag15;
                    this.floodModeSplitButton.Visible = flag15;
                    bool flag16 = (value & (PaintDotNet.ToolBarConfigItems.None | PaintDotNet.ToolBarConfigItems.SelectionDrawMode)) != PaintDotNet.ToolBarConfigItems.None;
                    this.selectionDrawModeSeparator.Visible = flag16;
                    this.selectionDrawModeModeLabel.Visible = flag16;
                    this.selectionDrawModeSplitButton.Visible = flag16;
                    this.selectionDrawModeWidthLabel.Visible = flag16;
                    this.selectionDrawModeSwapButton.Visible = flag16;
                    this.selectionDrawModeHeightLabel.Visible = flag16;
                    this.RefreshSelectionDrawModeInfoVisibilities();
                    if (value == PaintDotNet.ToolBarConfigItems.None)
                    {
                        base.Visible = false;
                    }
                    else
                    {
                        base.Visible = true;
                    }
                    base.ResumeLayout(false);
                    base.PerformLayout();
                    if (flag)
                    {
                        UI.ResumeControlPainting(this);
                        base.Invalidate(true);
                    }
                    this.OnToolBarConfigItemsChanged();
                }
            }
        }

        private sealed class FontPreviewMask : IDisposable
        {
            private MemoryBlock mask;

            public FontPreviewMask(int width, int height, Int32Point drawOffset)
            {
                this.mask = new MemoryBlock((long) (width * height));
                this.Width = width;
                this.Height = height;
                this.DrawOffset = drawOffset;
            }

            public void Dispose()
            {
                if (this.mask != null)
                {
                    this.mask.Dispose();
                    this.mask = null;
                }
            }

            public unsafe byte* GetRowAddress(int row)
            {
                if ((row < 0) || (row >= this.Height))
                {
                    throw new ArgumentException($"row={row} is out of bounds, height={this.Height}");
                }
                if (this.mask == null)
                {
                    throw new ObjectDisposedException("FontPreviewMask");
                }
                return (byte*) (((void*) this.mask.Pointer) + (row * this.Width));
            }

            public Int32Point DrawOffset { get; private set; }

            public int Height { get; private set; }

            public int Width { get; private set; }
        }
    }
}


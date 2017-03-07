namespace PaintDotNet
{
    using PaintDotNet.IO;
    using PaintDotNet.SystemLayer;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Threading;

    [Serializable]
    internal sealed class AppEnvironment : IDisposable, ICloneable, IDeserializationCallback, INotifyPropertyChanged
    {
        private bool alphaBlending;
        private bool antiAliasing;
        private PaintDotNet.BrushInfo brushInfo;
        private PaintDotNet.ColorPickerClickBehavior colorPickerClickBehavior;
        [OptionalField]
        private PaintDotNet.FloodMode floodMode;
        private PaintDotNet.FontInfo fontInfo;
        private PaintDotNet.SystemLayer.FontSmoothing fontSmoothing;
        private PaintDotNet.GradientInfo gradientInfo;
        private PaintDotNet.PenInfo penInfo;
        private ColorBgra primaryColor;
        private PaintDotNet.ResamplingAlgorithm resamplingAlgorithm;
        private ColorBgra secondaryColor;
        [OptionalField]
        private PaintDotNet.SelectionCombineMode selectionCombineMode;
        [OptionalField]
        private PaintDotNet.SelectionDrawModeInfo selectionDrawModeInfo;
        private PaintDotNet.ShapeDrawType shapeDrawType;
        private PaintDotNet.TextAlignment textAlignment;
        private float tolerance;

        [field: NonSerialized]
        public event EventHandler AlphaBlendingChanged;

        [field: NonSerialized]
        public event EventHandler AlphaBlendingChanging;

        [field: NonSerialized]
        public event EventHandler AntiAliasingChanged;

        [field: NonSerialized]
        public event EventHandler AntiAliasingChanging;

        [field: NonSerialized]
        public event EventHandler BrushInfoChanged;

        [field: NonSerialized]
        public event EventHandler BrushInfoChanging;

        [field: NonSerialized]
        public event EventHandler ColorPickerClickBehaviorChanged;

        [field: NonSerialized]
        public event EventHandler ColorPickerClickBehaviorChanging;

        [field: NonSerialized]
        public event EventHandler FloodModeChanged;

        [field: NonSerialized]
        public event EventHandler FontInfoChanged;

        [field: NonSerialized]
        public event EventHandler FontInfoChanging;

        [field: NonSerialized]
        public event EventHandler FontSmoothingChanged;

        [field: NonSerialized]
        public event EventHandler FontSmoothingChanging;

        [field: NonSerialized]
        public event EventHandler GradientInfoChanged;

        [field: NonSerialized]
        public event EventHandler GradientInfoChanging;

        [field: NonSerialized]
        public event EventHandler PenInfoChanged;

        [field: NonSerialized]
        public event EventHandler PenInfoChanging;

        [field: NonSerialized]
        public event EventHandler PrimaryColorChanged;

        [field: NonSerialized]
        public event EventHandler PrimaryColorChanging;

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        [field: NonSerialized]
        public event EventHandler ResamplingAlgorithmChanged;

        [field: NonSerialized]
        public event EventHandler ResamplingAlgorithmChanging;

        [field: NonSerialized]
        public event EventHandler SecondaryColorChanged;

        [field: NonSerialized]
        public event EventHandler SecondaryColorChanging;

        [field: NonSerialized]
        public event EventHandler SelectionCombineModeChanged;

        [field: NonSerialized]
        public event EventHandler SelectionDrawModeInfoChanged;

        [field: NonSerialized]
        public event EventHandler ShapeDrawTypeChanged;

        [field: NonSerialized]
        public event EventHandler ShapeDrawTypeChanging;

        [field: NonSerialized]
        public event EventHandler TextAlignmentChanged;

        [field: NonSerialized]
        public event EventHandler TextAlignmentChanging;

        [field: NonSerialized]
        public event EventHandler ToleranceChanged;

        [field: NonSerialized]
        public event EventHandler ToleranceChanging;

        public AppEnvironment()
        {
            this.SetToDefaults();
        }

        public AppEnvironment Clone()
        {
            BinaryFormatter formatter = new BinaryFormatter();
            SegmentedMemoryStream serializationStream = new SegmentedMemoryStream();
            formatter.Serialize(serializationStream, this);
            serializationStream.Seek(0L, SeekOrigin.Begin);
            object obj2 = formatter.Deserialize(serializationStream);
            serializationStream.Dispose();
            serializationStream = null;
            return (AppEnvironment) obj2;
        }

        public Brush CreateBrush(bool swapColors)
        {
            if (!swapColors)
            {
                return this.BrushInfo.CreateBrush((Color) this.PrimaryColor, (Color) this.SecondaryColor);
            }
            return this.BrushInfo.CreateBrush((Color) this.SecondaryColor, (Color) this.PrimaryColor);
        }

        public Pen CreatePen(bool swapColors)
        {
            if (!swapColors)
            {
                return this.PenInfo.CreatePen(this.BrushInfo, (Color) this.PrimaryColor, (Color) this.SecondaryColor);
            }
            return this.PenInfo.CreatePen(this.BrushInfo, (Color) this.SecondaryColor, (Color) this.PrimaryColor);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
        }

        ~AppEnvironment()
        {
            this.Dispose(false);
        }

        public CompositingMode GetCompositingMode()
        {
            if (!this.alphaBlending)
            {
                return CompositingMode.SourceCopy;
            }
            return CompositingMode.SourceOver;
        }

        public static AppEnvironment GetDefaultAppEnvironment()
        {
            AppEnvironment environment;
            try
            {
                string s = Settings.CurrentUser.GetString("DefaultAppEnvironment", null);
                if (s == null)
                {
                    environment = null;
                }
                else
                {
                    byte[] buffer = Convert.FromBase64String(s);
                    BinaryFormatter formatter = new BinaryFormatter();
                    using (MemoryStream stream = new MemoryStream(buffer, false))
                    {
                        environment = (AppEnvironment) formatter.Deserialize(stream);
                    }
                }
            }
            catch (Exception)
            {
                environment = null;
            }
            if (environment == null)
            {
                environment = new AppEnvironment();
                environment.SetToDefaults();
            }
            return environment;
        }

        public void LoadFrom(AppEnvironment appEnvironment)
        {
            this.textAlignment = appEnvironment.textAlignment;
            this.gradientInfo = appEnvironment.gradientInfo.Clone();
            this.fontSmoothing = appEnvironment.fontSmoothing;
            this.fontInfo = appEnvironment.fontInfo.Clone();
            this.penInfo = appEnvironment.penInfo.Clone();
            this.brushInfo = appEnvironment.brushInfo.Clone();
            this.primaryColor = appEnvironment.primaryColor;
            this.secondaryColor = appEnvironment.secondaryColor;
            this.alphaBlending = appEnvironment.alphaBlending;
            this.shapeDrawType = appEnvironment.shapeDrawType;
            this.antiAliasing = appEnvironment.antiAliasing;
            this.colorPickerClickBehavior = appEnvironment.colorPickerClickBehavior;
            this.resamplingAlgorithm = appEnvironment.resamplingAlgorithm;
            this.tolerance = appEnvironment.tolerance;
            this.selectionCombineMode = appEnvironment.selectionCombineMode;
            this.floodMode = appEnvironment.floodMode;
            this.selectionDrawModeInfo = appEnvironment.selectionDrawModeInfo.Clone();
            this.PerformAllChanged();
        }

        private void OnAlphaBlendingChanged()
        {
            if (this.AlphaBlendingChanged != null)
            {
                this.AlphaBlendingChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("AlphaBlending");
        }

        private void OnAlphaBlendingChanging()
        {
            if (this.AlphaBlendingChanging != null)
            {
                this.AlphaBlendingChanging(this, EventArgs.Empty);
            }
        }

        private void OnAntiAliasingChanged()
        {
            if (this.AntiAliasingChanged != null)
            {
                this.AntiAliasingChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("AntiAliasing");
        }

        private void OnAntiAliasingChanging()
        {
            if (this.AntiAliasingChanging != null)
            {
                this.AntiAliasingChanging(this, EventArgs.Empty);
            }
        }

        private void OnBackColorChanging()
        {
            if (this.SecondaryColorChanging != null)
            {
                this.SecondaryColorChanging(this, EventArgs.Empty);
            }
        }

        private void OnBrushInfoChanged()
        {
            if (this.BrushInfoChanged != null)
            {
                this.BrushInfoChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("BrushInfo");
        }

        private void OnBrushInfoChanging()
        {
            if (this.BrushInfoChanging != null)
            {
                this.BrushInfoChanging(this, EventArgs.Empty);
            }
        }

        private void OnColorPickerClickBehaviorChanged()
        {
            if (this.ColorPickerClickBehaviorChanged != null)
            {
                this.ColorPickerClickBehaviorChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("ColorPickerClickBehavior");
        }

        private void OnColorPickerClickBehaviorChanging()
        {
            if (this.ColorPickerClickBehaviorChanging != null)
            {
                this.ColorPickerClickBehaviorChanging(this, EventArgs.Empty);
            }
        }

        private void OnFloodModeChanged()
        {
            if (this.FloodModeChanged != null)
            {
                this.FloodModeChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("FloodMode");
        }

        private void OnFontInfoChanged()
        {
            if (this.FontInfoChanged != null)
            {
                this.FontInfoChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("FontInfo");
        }

        private void OnFontInfoChanging()
        {
            if (this.FontInfoChanging != null)
            {
                this.FontInfoChanging(this, EventArgs.Empty);
            }
        }

        private void OnFontSmoothingChanged()
        {
            if (this.FontSmoothingChanged != null)
            {
                this.FontSmoothingChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("FontSmoothing");
        }

        private void OnFontSmoothingChanging()
        {
            if (this.FontSmoothingChanging != null)
            {
                this.FontSmoothingChanging(this, EventArgs.Empty);
            }
        }

        private void OnGradientInfoChanged()
        {
            if (this.GradientInfoChanged != null)
            {
                this.GradientInfoChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("GradientInfo");
        }

        private void OnGradientInfoChanging()
        {
            if (this.GradientInfoChanging != null)
            {
                this.GradientInfoChanging(this, EventArgs.Empty);
            }
        }

        private void OnPenInfoChanged()
        {
            if (this.PenInfoChanged != null)
            {
                this.PenInfoChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("PenInfo");
        }

        private void OnPenInfoChanging()
        {
            if (this.PenInfoChanging != null)
            {
                this.PenInfoChanging(this, EventArgs.Empty);
            }
        }

        private void OnPrimaryColorChanged()
        {
            if (this.PrimaryColorChanged != null)
            {
                this.PrimaryColorChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("PrimaryColor");
        }

        private void OnPrimaryColorChanging()
        {
            if (this.PrimaryColorChanging != null)
            {
                this.PrimaryColorChanging(this, EventArgs.Empty);
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void OnResamplingAlgorithmChanged()
        {
            if (this.ResamplingAlgorithmChanged != null)
            {
                this.ResamplingAlgorithmChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("ResamplingAlgorithm");
        }

        private void OnResamplingAlgorithmChanging()
        {
            if (this.ResamplingAlgorithmChanging != null)
            {
                this.ResamplingAlgorithmChanging(this, EventArgs.Empty);
            }
        }

        private void OnSecondaryColorChanged()
        {
            if (this.SecondaryColorChanged != null)
            {
                this.SecondaryColorChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("SecondaryColor");
        }

        private void OnSelectionCombineModeChanged()
        {
            if (this.SelectionCombineModeChanged != null)
            {
                this.SelectionCombineModeChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("SelectionCombineMode");
        }

        private void OnSelectionDrawModeInfoChanged()
        {
            if (this.SelectionDrawModeInfoChanged != null)
            {
                this.SelectionDrawModeInfoChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("SelectionDrawModeInfo");
        }

        private void OnShapeDrawTypeChanged()
        {
            if (this.ShapeDrawTypeChanged != null)
            {
                this.ShapeDrawTypeChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("ShapeDrawType");
        }

        private void OnShapeDrawTypeChanging()
        {
            if (this.ShapeDrawTypeChanging != null)
            {
                this.ShapeDrawTypeChanging(this, EventArgs.Empty);
            }
        }

        private void OnTextAlignmentChanged()
        {
            if (this.TextAlignmentChanged != null)
            {
                this.TextAlignmentChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("TextAlignment");
        }

        private void OnTextAlignmentChanging()
        {
            if (this.TextAlignmentChanging != null)
            {
                this.TextAlignmentChanging(this, EventArgs.Empty);
            }
        }

        private void OnToleranceChanged()
        {
            if (this.ToleranceChanged != null)
            {
                this.ToleranceChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Tolerance");
        }

        private void OnToleranceChanging()
        {
            if (this.ToleranceChanging != null)
            {
                this.ToleranceChanging(this, EventArgs.Empty);
            }
        }

        public void PerformAllChanged()
        {
            this.OnFontInfoChanged();
            this.OnFontSmoothingChanged();
            this.OnPenInfoChanged();
            this.OnBrushInfoChanged();
            this.OnGradientInfoChanged();
            this.OnSecondaryColorChanged();
            this.OnPrimaryColorChanged();
            this.OnAlphaBlendingChanged();
            this.OnShapeDrawTypeChanged();
            this.OnAntiAliasingChanged();
            this.OnTextAlignmentChanged();
            this.OnToleranceChanged();
            this.OnColorPickerClickBehaviorChanged();
            this.OnResamplingAlgorithmChanging();
            this.OnSelectionCombineModeChanged();
            this.OnFloodModeChanged();
            this.OnSelectionDrawModeInfoChanged();
        }

        public void SaveAsDefaultAppEnvironment()
        {
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream serializationStream = new MemoryStream();
            formatter.Serialize(serializationStream, this);
            string str = Convert.ToBase64String(serializationStream.GetBuffer());
            Settings.CurrentUser.SetString("DefaultAppEnvironment", str);
        }

        public void SetToDefaults()
        {
            this.antiAliasing = true;
            this.fontSmoothing = PaintDotNet.SystemLayer.FontSmoothing.Smooth;
            this.primaryColor = ColorBgra.FromBgra(0, 0, 0, 0xff);
            this.secondaryColor = ColorBgra.FromBgra(0xff, 0xff, 0xff, 0xff);
            this.gradientInfo = new PaintDotNet.GradientInfo(GradientType.LinearClamped, false);
            this.penInfo = new PaintDotNet.PenInfo(DashStyle.Solid, 2f, LineCap2.Flat, LineCap2.Flat, 1f);
            this.brushInfo = new PaintDotNet.BrushInfo(PaintDotNet.BrushType.Solid, HatchStyle.BackwardDiagonal);
            this.fontInfo = new PaintDotNet.FontInfo("Arial", 12f, FontStyle.Regular);
            this.textAlignment = PaintDotNet.TextAlignment.Left;
            this.shapeDrawType = PaintDotNet.ShapeDrawType.Outline;
            this.alphaBlending = true;
            this.tolerance = 0.5f;
            this.colorPickerClickBehavior = PaintDotNet.ColorPickerClickBehavior.NoToolSwitch;
            this.resamplingAlgorithm = PaintDotNet.ResamplingAlgorithm.Bilinear;
            this.selectionCombineMode = PaintDotNet.SelectionCombineMode.Replace;
            this.floodMode = PaintDotNet.FloodMode.Local;
            this.selectionDrawModeInfo = PaintDotNet.SelectionDrawModeInfo.CreateDefault();
        }

        object ICloneable.Clone() => 
            this.Clone();

        void IDeserializationCallback.OnDeserialization(object sender)
        {
            if (this.selectionDrawModeInfo == null)
            {
                this.selectionDrawModeInfo = PaintDotNet.SelectionDrawModeInfo.CreateDefault();
            }
        }

        public bool AlphaBlending
        {
            get => 
                this.alphaBlending;
            set
            {
                if (value != this.alphaBlending)
                {
                    this.OnAlphaBlendingChanging();
                    this.alphaBlending = value;
                    this.OnAlphaBlendingChanged();
                }
            }
        }

        public bool AntiAliasing
        {
            get => 
                this.antiAliasing;
            set
            {
                if (this.antiAliasing != value)
                {
                    this.OnAntiAliasingChanging();
                    this.antiAliasing = value;
                    this.OnAntiAliasingChanged();
                }
            }
        }

        public PaintDotNet.BrushInfo BrushInfo
        {
            get => 
                this.brushInfo.Clone();
            set
            {
                this.OnBrushInfoChanging();
                this.brushInfo = value.Clone();
                this.OnBrushInfoChanged();
            }
        }

        public PaintDotNet.ColorPickerClickBehavior ColorPickerClickBehavior
        {
            get => 
                this.colorPickerClickBehavior;
            set
            {
                if (this.colorPickerClickBehavior != value)
                {
                    this.OnColorPickerClickBehaviorChanging();
                    this.colorPickerClickBehavior = value;
                    this.OnColorPickerClickBehaviorChanged();
                }
            }
        }

        public PaintDotNet.FloodMode FloodMode
        {
            get => 
                this.floodMode;
            set
            {
                if (this.floodMode != value)
                {
                    this.floodMode = value;
                    this.OnFloodModeChanged();
                }
            }
        }

        public PaintDotNet.FontInfo FontInfo
        {
            get => 
                this.fontInfo;
            set
            {
                if (this.fontInfo != value)
                {
                    this.OnFontInfoChanging();
                    this.fontInfo = value;
                    this.OnFontInfoChanged();
                }
            }
        }

        public PaintDotNet.SystemLayer.FontSmoothing FontSmoothing
        {
            get => 
                this.fontSmoothing;
            set
            {
                if (this.fontSmoothing != value)
                {
                    this.OnFontSmoothingChanging();
                    this.fontSmoothing = value;
                    this.OnFontSmoothingChanged();
                }
            }
        }

        public PaintDotNet.GradientInfo GradientInfo
        {
            get => 
                this.gradientInfo;
            set
            {
                this.OnGradientInfoChanging();
                this.gradientInfo = value;
                this.OnGradientInfoChanged();
            }
        }

        public PaintDotNet.PenInfo PenInfo
        {
            get => 
                this.penInfo.Clone();
            set
            {
                if (this.penInfo != value)
                {
                    this.OnPenInfoChanging();
                    this.penInfo = value.Clone();
                    this.OnPenInfoChanged();
                }
            }
        }

        public ColorBgra PrimaryColor
        {
            get => 
                this.primaryColor;
            set
            {
                if (this.primaryColor != value)
                {
                    this.OnPrimaryColorChanging();
                    this.primaryColor = value;
                    this.OnPrimaryColorChanged();
                }
            }
        }

        public PaintDotNet.ResamplingAlgorithm ResamplingAlgorithm
        {
            get => 
                this.resamplingAlgorithm;
            set
            {
                if (value != this.resamplingAlgorithm)
                {
                    this.OnResamplingAlgorithmChanging();
                    this.resamplingAlgorithm = value;
                    this.OnResamplingAlgorithmChanged();
                }
            }
        }

        public ColorBgra SecondaryColor
        {
            get => 
                this.secondaryColor;
            set
            {
                if (this.secondaryColor != value)
                {
                    this.OnBackColorChanging();
                    this.secondaryColor = value;
                    this.OnSecondaryColorChanged();
                }
            }
        }

        public PaintDotNet.SelectionCombineMode SelectionCombineMode
        {
            get => 
                this.selectionCombineMode;
            set
            {
                if (this.selectionCombineMode != value)
                {
                    this.selectionCombineMode = value;
                    this.OnSelectionCombineModeChanged();
                }
            }
        }

        public PaintDotNet.SelectionDrawModeInfo SelectionDrawModeInfo
        {
            get => 
                this.selectionDrawModeInfo.Clone();
            set
            {
                if (!this.selectionDrawModeInfo.Equals(value))
                {
                    this.selectionDrawModeInfo = value.Clone();
                    this.OnSelectionDrawModeInfoChanged();
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
                    this.OnShapeDrawTypeChanging();
                    this.shapeDrawType = value;
                    this.OnShapeDrawTypeChanged();
                }
            }
        }

        public PaintDotNet.TextAlignment TextAlignment
        {
            get => 
                this.textAlignment;
            set
            {
                if (value != this.textAlignment)
                {
                    this.OnTextAlignmentChanging();
                    this.textAlignment = value;
                    this.OnTextAlignmentChanged();
                }
            }
        }

        public float Tolerance
        {
            get => 
                this.tolerance;
            set
            {
                if (this.tolerance != value)
                {
                    this.OnToleranceChanging();
                    this.tolerance = value;
                    this.OnToleranceChanged();
                }
            }
        }
    }
}


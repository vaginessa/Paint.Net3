namespace PaintDotNet.Canvas
{
    using PaintDotNet;
    using PaintDotNet.Rendering;
    using System;
    using System.Threading;
    using System.Windows;
    using System.Windows.Forms;

    internal abstract class CanvasControl : CanvasGdipRenderer
    {
        private System.Windows.Forms.Cursor cursor;
        private Point location;
        private System.Windows.Size size;

        public event EventHandler CursorChanged;

        public event EventHandler LocationChanged;

        public event EventHandler LocationChanging;

        public event EventHandler SizeChanged;

        public event EventHandler SizeChanging;

        protected CanvasControl(CanvasRenderer ownerCanvas) : base(ownerCanvas)
        {
        }

        public Point CanvasPointToControlPoint(Point canvasPtF) => 
            new Point(canvasPtF.X - this.location.X, canvasPtF.Y - this.location.Y);

        public Rect CanvasRectToControlRect(Rect canvasRectF) => 
            new Rect(this.CanvasPointToControlPoint(canvasRectF.Location), canvasRectF.Size);

        public Point ControlPointToCanvasPoint(Point controlPtF) => 
            new Point(controlPtF.X + this.location.X, controlPtF.Y + this.location.Y);

        public Rect ControlRectToCanvasRect(Rect controlRectF) => 
            new Rect(this.ControlPointToCanvasPoint(controlRectF.Location), controlRectF.Size);

        private void MouseDown(MouseEventArgs e)
        {
            this.MouseDown(e);
        }

        private void MouseEnter()
        {
            this.OnMouseEnter();
        }

        private void MouseLeave()
        {
            this.OnMouseLeave();
        }

        private void MouseUp(MouseEventArgs e)
        {
            this.OnMouseUp(e);
        }

        protected virtual void OnCursorChanged()
        {
            if (this.CursorChanged != null)
            {
                this.CursorChanged(this, EventArgs.Empty);
            }
        }

        protected virtual void OnLocationChanged()
        {
            if (this.LocationChanged != null)
            {
                this.LocationChanged(this, EventArgs.Empty);
            }
        }

        protected virtual void OnLocationChanging()
        {
            if (this.LocationChanging != null)
            {
                this.LocationChanging(this, EventArgs.Empty);
            }
        }

        protected virtual void OnMouseDown(MouseEventArgs e)
        {
        }

        protected virtual void OnMouseEnter()
        {
        }

        protected virtual void OnMouseLeave()
        {
        }

        protected virtual void OnMouseUp(MouseEventArgs e)
        {
        }

        protected virtual void OnPulse()
        {
        }

        protected virtual void OnRender(RenderArgs ra, Int32Point offset)
        {
        }

        protected virtual void OnSizeChanged()
        {
            if (this.SizeChanged != null)
            {
                this.SizeChanged(this, EventArgs.Empty);
            }
        }

        protected virtual void OnSizeChanging()
        {
            if (this.SizeChanging != null)
            {
                this.SizeChanging(this, EventArgs.Empty);
            }
        }

        public void PerformMouseDown(MouseEventArgs e)
        {
            this.MouseDown(e);
        }

        public void PerformMouseEnter()
        {
            this.MouseEnter();
        }

        public void PerformMouseLeave()
        {
            this.MouseLeave();
        }

        public void PerformMouseUp(MouseEventArgs e)
        {
            this.MouseUp(e);
        }

        public void PerformPulse()
        {
            this.Pulse();
        }

        private void Pulse()
        {
            this.OnPulse();
        }

        public sealed override void RenderToGraphics(RenderArgs ra, Int32Point offset)
        {
            this.OnRender(ra, offset);
        }

        public Rect Bounds
        {
            get => 
                new Rect(this.location, this.size);
            set
            {
                this.Location = value.Location;
                this.Size = value.Size;
            }
        }

        public System.Windows.Forms.Cursor Cursor
        {
            get => 
                this.cursor;
            set
            {
                if (this.cursor != value)
                {
                    this.cursor = value;
                    this.OnCursorChanged();
                }
            }
        }

        public double Height
        {
            get => 
                this.Size.Height;
            set
            {
                this.Size = new System.Windows.Size(this.Size.Width, value);
            }
        }

        public Point Location
        {
            get => 
                this.location;
            set
            {
                if (this.location != value)
                {
                    this.OnLocationChanging();
                    this.location = value;
                    this.OnLocationChanged();
                }
            }
        }

        public System.Windows.Size Size
        {
            get => 
                this.size;
            set
            {
                if (this.size != value)
                {
                    this.OnSizeChanging();
                    this.size = value;
                    this.OnSizeChanged();
                }
            }
        }

        public double Width
        {
            get => 
                this.Size.Width;
            set
            {
                this.Size = new System.Windows.Size(value, this.Size.Height);
            }
        }
    }
}


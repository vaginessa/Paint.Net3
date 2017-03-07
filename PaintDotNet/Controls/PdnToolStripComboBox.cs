namespace PaintDotNet.Controls
{
    using PaintDotNet;
    using PaintDotNet.Rendering;
    using PaintDotNet.SystemLayer;
    using PaintDotNet.VisualStyling;
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.Drawing.Text;
    using System.Windows.Forms;

    internal class PdnToolStripComboBox : ToolStripComboBox
    {
        private PenBrushCache penBrushCache;

        public PdnToolStripComboBox(bool enableOwnerDraw)
        {
            EventHandler handler = null;
            this.penBrushCache = PenBrushCache.ThreadInstance;
            base.ComboBox.Select(0, 0);
            base.ComboBox.FlatStyle = FlatStyle.Standard;
            if (!enableOwnerDraw)
            {
                base.ComboBox.HandleCreated += new EventHandler(this.OnComboBoxFirstHandleCreated);
                if (handler == null)
                {
                    handler = (s, e) => base.ComboBox.Select(0, 0);
                }
                base.ComboBox.LostFocus += handler;
            }
        }

        private StringFormat CreateStringFormat()
        {
            StringFormat format = StringFormat.GenericDefault.CloneT<StringFormat>();
            format.LineAlignment = StringAlignment.Center;
            format.FormatFlags |= StringFormatFlags.NoWrap;
            format.HotkeyPrefix = HotkeyPrefix.None;
            format.Trimming = StringTrimming.EllipsisCharacter;
            return format;
        }

        private void InitializeCustomDrawing()
        {
            base.ComboBox.DrawMode = DrawMode.OwnerDrawFixed;
            base.ComboBox.DrawItem += new DrawItemEventHandler(this.OnComboBoxDrawItem);
            base.ComboBox.Select(0, 0);
        }

        private void OnComboBoxDrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index != -1)
            {
                using (Bitmap bitmap = new Bitmap(e.Bounds.Width, e.Bounds.Height, PixelFormat.Format24bppRgb))
                {
                    Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                    using (Graphics graphics = Graphics.FromImage(bitmap))
                    {
                        graphics.Clear(SystemColors.Window);
                        HighlightState state = ((e.State & DrawItemState.Selected) != DrawItemState.None) ? HighlightState.Hover : HighlightState.Default;
                        Rectangle layoutRectangle = rect;
                        object item = base.Items[e.Index];
                        string itemText = base.ComboBox.GetItemText(item);
                        Color selectionForeColor = SelectionHighlight.GetSelectionForeColor(state);
                        Color selectionBackColor = SelectionHighlight.GetSelectionBackColor(state);
                        graphics.FillRectangle(this.penBrushCache.GetSolidBrush(selectionBackColor), e.Bounds);
                        if (SelectionHighlight.ShouldDrawFirst)
                        {
                            SelectionHighlight.DrawBackground(graphics, this.penBrushCache, rect, state);
                        }
                        using (StringFormat format = this.CreateStringFormat())
                        {
                            graphics.DrawString(itemText, base.ComboBox.Font, this.penBrushCache.GetSolidBrush(selectionForeColor), layoutRectangle, format);
                        }
                        if (!SelectionHighlight.ShouldDrawFirst)
                        {
                            SelectionHighlight.DrawBackground(graphics, this.penBrushCache, rect, state);
                        }
                        if (((e.State & DrawItemState.Focus) == DrawItemState.Focus) && UI.ForciblyGetShowFocusCues(base.ComboBox))
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

        private void OnComboBoxFirstHandleCreated(object sender, EventArgs e)
        {
            Action method = null;
            base.ComboBox.HandleCreated -= new EventHandler(this.OnComboBoxFirstHandleCreated);
            if (base.ComboBox.DrawMode == DrawMode.Normal)
            {
                base.ComboBox.BeginInvoke(new Action(this.InitializeCustomDrawing));
            }
            if (!OS.IsVistaOrLater)
            {
                if (method == null)
                {
                    method = () => base.Invalidate();
                }
                base.ComboBox.BeginInvoke(method);
            }
        }
    }
}


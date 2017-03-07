namespace PaintDotNet
{
    using PaintDotNet.Rendering;
    using PaintDotNet.VisualStyling;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Windows.Forms;

    internal class PdnToolStripRenderer : ToolStripProfessionalRenderer
    {
        private Color borderInnerColor;
        private Color borderOuterColor;
        private Dictionary<ToolStrip, int> dropDownToHSepLeftMap = new Dictionary<ToolStrip, int>();
        private Color imageMarginBackgroundColor;
        private Color imageMarginSeparatorColor1;
        private Color imageMarginSeparatorColor2;
        private PenBrushCache penBrushCache;
        private Color separatorColor1;
        private Color separatorColor2;

        public PdnToolStripRenderer()
        {
            base.RoundedEdges = false;
            this.penBrushCache = PenBrushCache.ThreadInstance;
            this.borderOuterColor = Color.FromArgb(0xff, 0x97, 0x97, 0x97);
            this.borderInnerColor = Color.FromArgb(0xff, 0xf5, 0xf5, 0xf5);
            this.separatorColor1 = Color.FromArgb(0xff, 0xe0, 0xe0, 0xe0);
            this.separatorColor2 = Color.FromArgb(0xff, 0xff, 0xff, 0xff);
            this.imageMarginBackgroundColor = Color.FromArgb(0xff, 0xff, 0xff, 0xff);
            this.imageMarginSeparatorColor1 = Color.FromArgb(0xff, 0xe2, 0xe3, 0xe3);
            this.imageMarginSeparatorColor2 = Color.FromArgb(0xff, 0xff, 0xff, 0xff);
        }

        private void DrawAeroSeparator(Graphics g, Rectangle contentRect)
        {
            int x = contentRect.Left + (contentRect.Width / 2);
            int y = contentRect.Top + 5;
            int num3 = contentRect.Bottom - 6;
            if ((num3 - y) >= 1)
            {
                Point point = new Point(x, y);
                Point point2 = new Point(x, num3);
                Color color = Color.FromArgb(0xff, 0xae, 0xbf, 0xd3);
                Color color2 = Color.FromArgb(0xff, 0xa5, 0xb8, 0xd0);
                using (LinearGradientBrush brush = new LinearGradientBrush(point, point2, color, color2))
                {
                    g.FillRectangle(brush, new Rectangle(x, y, 1, num3 - y));
                }
                g.DrawRectangle(this.penBrushCache.GetPen(Color.FromArgb(0x80, 0xff, 0xff, 0xff)), x - 1, y - 1, 2, (num3 - y) + 1);
            }
        }

        protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e)
        {
            if (ThemeConfig.EffectiveTheme != PdnTheme.Aero)
            {
                base.OnRenderButtonBackground(e);
            }
            else
            {
                ToolStripButton item = (ToolStripButton) e.Item;
                Rectangle rect = new Rectangle(Point.Empty, e.Item.Size);
                this.RenderAeroButtonBackground(e.Graphics, rect, item.Enabled, item.Selected, item.Pressed, item.Checked);
            }
        }

        protected override void OnRenderImageMargin(ToolStripRenderEventArgs e)
        {
            if (ThemeConfig.EffectiveTheme != PdnTheme.Aero)
            {
                base.OnRenderImageMargin(e);
            }
            else if (e.ToolStrip is ToolStripDropDown)
            {
                if (this.dropDownToHSepLeftMap.Count > 100)
                {
                    this.dropDownToHSepLeftMap.Clear();
                }
                int right = e.AffectedBounds.Right;
                e.Graphics.FillRectangle(this.penBrushCache.GetSolidBrush(this.imageMarginBackgroundColor), e.AffectedBounds);
                int num2 = right;
                e.Graphics.DrawLine(this.penBrushCache.GetPen(this.imageMarginSeparatorColor1), num2, e.AffectedBounds.Top, num2, e.AffectedBounds.Bottom);
                int num3 = right + 1;
                e.Graphics.DrawLine(this.penBrushCache.GetPen(this.imageMarginSeparatorColor2), num3, e.AffectedBounds.Top, num3, e.AffectedBounds.Bottom);
                this.dropDownToHSepLeftMap[e.ToolStrip] = num3 - 1;
            }
            else
            {
                base.OnRenderImageMargin(e);
            }
        }

        protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
        {
            if (ThemeConfig.EffectiveTheme != PdnTheme.Aero)
            {
                base.OnRenderItemCheck(e);
            }
            else
            {
                Rectangle imageRectangle = e.ImageRectangle;
                ToolStripItem item = e.Item;
                Image image = item.Image;
                ToolStripMenuItem item2 = item as ToolStripMenuItem;
                if (item2 != null)
                {
                    Rectangle rect = imageRectangle;
                    rect.Inflate(2, 2);
                    HighlightState state = item.Enabled ? HighlightState.Hover : HighlightState.Disabled;
                    SelectionHighlight.DrawBackground(e.Graphics, this.penBrushCache, rect, state);
                    bool flag = false;
                    if ((image == null) && item2.Checked)
                    {
                        Image reference = PdnResources.GetImageResource2("Icons.ToolStrip.Checked.png").Reference;
                        if (item.Enabled)
                        {
                            image = reference;
                            flag = false;
                        }
                        else
                        {
                            image = ToolStripRenderer.CreateDisabledImage(reference);
                            flag = true;
                        }
                    }
                    if (image != null)
                    {
                        Rectangle srcRect = new Rectangle(Point.Empty, image.Size);
                        e.Graphics.DrawImage(image, imageRectangle, srcRect, GraphicsUnit.Pixel);
                    }
                    if (flag)
                    {
                        image.Dispose();
                        image = null;
                    }
                }
            }
        }

        protected override void OnRenderItemImage(ToolStripItemImageRenderEventArgs e)
        {
            if (ThemeConfig.EffectiveTheme != PdnTheme.Aero)
            {
                base.OnRenderItemImage(e);
            }
            else
            {
                Rectangle imageRectangle = e.ImageRectangle;
                Image normalImage = e.Image;
                ToolStripItem item = e.Item;
                if (normalImage != null)
                {
                    Image image2 = null;
                    if (!item.Enabled)
                    {
                        image2 = ToolStripRenderer.CreateDisabledImage(normalImage);
                    }
                    Image image = image2 ?? normalImage;
                    Rectangle srcRect = new Rectangle(Point.Empty, image.Size);
                    e.Graphics.DrawImage(image, imageRectangle, srcRect, GraphicsUnit.Pixel);
                    if (image2 != null)
                    {
                        image2.Dispose();
                        image2 = null;
                    }
                }
            }
        }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            if (ThemeConfig.EffectiveTheme != PdnTheme.Aero)
            {
                base.OnRenderMenuItemBackground(e);
            }
            else
            {
                HighlightState disabled;
                ToolStripItem item = e.Item;
                Rectangle rect = new Rectangle(0, 0, item.Width, item.Height);
                if (item.IsOnDropDown)
                {
                    rect = Rectangle.Inflate(rect, -1, -1);
                    rect.X += 2;
                    rect.Width -= 3;
                }
                if (!e.Item.Enabled && e.Item.Selected)
                {
                    disabled = HighlightState.Disabled;
                }
                else if (e.Item.Pressed && e.Item.IsOnDropDown)
                {
                    disabled = HighlightState.Hover;
                }
                else if (e.Item.Pressed)
                {
                    disabled = HighlightState.Hover;
                }
                else if (e.Item.Selected)
                {
                    disabled = HighlightState.Hover;
                }
                else
                {
                    disabled = HighlightState.Default;
                }
                SelectionHighlight.DrawBackground(e.Graphics, this.penBrushCache, rect, disabled);
            }
        }

        protected override void OnRenderOverflowButtonBackground(ToolStripItemRenderEventArgs e)
        {
            if (ThemeConfig.EffectiveTheme != PdnTheme.Aero)
            {
                base.OnRenderOverflowButtonBackground(e);
            }
            else
            {
                this.RenderAeroButtonBackground(e.Graphics, new Rectangle(1, 0, e.Item.Width - 1, e.Item.Height), true, e.Item.Selected, e.Item.Pressed, false);
                Color arrowColor = Color.FromArgb(0xff, 160, 160, 160);
                base.DrawArrow(new ToolStripArrowRenderEventArgs(e.Graphics, e.Item, new Rectangle(0, 0, e.Item.Width, e.Item.Height), arrowColor, ArrowDirection.Down));
            }
        }

        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
        {
            if (ThemeConfig.EffectiveTheme != PdnTheme.Aero)
            {
                base.OnRenderSeparator(e);
            }
            else if (!(e.Item.Owner is StatusStrip))
            {
                if ((e.Item.IsOnDropDown && (e.ToolStrip is ToolStripDropDownMenu)) && !e.Vertical)
                {
                    int left;
                    if (!this.dropDownToHSepLeftMap.TryGetValue(e.ToolStrip, out left))
                    {
                        left = e.Item.ContentRectangle.Left;
                    }
                    int right = e.Item.ContentRectangle.Right;
                    int num3 = ((e.Item.ContentRectangle.Top + e.Item.ContentRectangle.Bottom) / 2) - 1;
                    e.Graphics.DrawLine(this.penBrushCache.GetPen(this.separatorColor1), left, num3, right, num3);
                    int num4 = num3 + 1;
                    e.Graphics.DrawLine(this.penBrushCache.GetPen(this.separatorColor2), left, num4, right, num4);
                }
                else if (e.Vertical && !e.Item.IsOnDropDown)
                {
                    this.DrawAeroSeparator(e.Graphics, e.Item.ContentRectangle);
                }
                else
                {
                    base.OnRenderSeparator(e);
                }
            }
        }

        protected override void OnRenderSplitButtonBackground(ToolStripItemRenderEventArgs e)
        {
            if (ThemeConfig.EffectiveTheme != PdnTheme.Aero)
            {
                base.OnRenderSplitButtonBackground(e);
            }
            else
            {
                ToolStripSplitButton item = (ToolStripSplitButton) e.Item;
                Rectangle rect = new Rectangle(Point.Empty, item.Size);
                this.RenderAeroButtonBackground(e.Graphics, rect, true, item.Selected, item.Pressed, false);
                Color arrowColor = Color.FromArgb(0xff, 0, 0, 0);
                base.DrawArrow(new ToolStripArrowRenderEventArgs(e.Graphics, item, item.DropDownButtonBounds, arrowColor, ArrowDirection.Down));
                if (item.Selected || item.Pressed)
                {
                    this.DrawAeroSeparator(e.Graphics, new Rectangle(item.DropDownButtonBounds.Left, item.DropDownButtonBounds.Top, 1, item.DropDownButtonBounds.Height));
                }
            }
        }

        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
            if (ThemeConfig.EffectiveTheme != PdnTheme.Aero)
            {
                base.OnRenderToolStripBackground(e);
            }
            else
            {
                if (e.ToolStrip is StatusStrip)
                {
                    Color color = Color.FromArgb(0xff, 0xfb, 0xfd, 0xff);
                    Color color2 = Color.FromArgb(0xff, 220, 0xe7, 0xf5);
                    Color.FromArgb(0xff, 0xbd, 0xd1, 0xeb);
                    Rectangle affectedBounds = e.AffectedBounds;
                    affectedBounds.Y++;
                    affectedBounds.Height--;
                    using (LinearGradientBrush brush = new LinearGradientBrush(e.AffectedBounds, color, color2, LinearGradientMode.Vertical))
                    {
                        e.Graphics.FillRectangle(brush, e.AffectedBounds);
                        return;
                    }
                }
                base.OnRenderToolStripBackground(e);
            }
        }

        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            if (ThemeConfig.EffectiveTheme != PdnTheme.Aero)
            {
                base.OnRenderToolStripBorder(e);
            }
            else if (e.ToolStrip is ToolStripDropDown)
            {
                Rectangle affectedBounds = e.AffectedBounds;
                affectedBounds.Width--;
                affectedBounds.Height--;
                e.Graphics.DrawRectangle(this.penBrushCache.GetPen(this.borderOuterColor), affectedBounds);
                Rectangle rect = e.AffectedBounds;
                rect.Inflate(-1, -1);
                rect.Width--;
                rect.Height--;
                e.Graphics.DrawRectangle(this.penBrushCache.GetPen(this.borderInnerColor), rect);
            }
            else if (e.ToolStrip is StatusStrip)
            {
                Color color = Color.FromArgb(0xff, 0x9f, 0xae, 0xc2);
                e.Graphics.DrawLine(this.penBrushCache.GetPen(color), new Point(e.AffectedBounds.Left, e.AffectedBounds.Top), new Point(e.AffectedBounds.Right, e.AffectedBounds.Top));
                e.Graphics.DrawLine(this.penBrushCache.GetPen(Color.White), new Point(e.AffectedBounds.Left, e.AffectedBounds.Top + 1), new Point(e.AffectedBounds.Right, e.AffectedBounds.Top + 1));
            }
            else
            {
                base.OnRenderToolStripBorder(e);
            }
        }

        private void RenderAeroButtonBackground(Graphics g, Rectangle rect, bool isEnabled, bool isSelected, bool isPressed, bool isChecked)
        {
            HighlightState hover;
            if (isPressed)
            {
                if (isEnabled)
                {
                    hover = HighlightState.Checked;
                }
                else
                {
                    hover = HighlightState.Default;
                }
            }
            else if (isSelected)
            {
                if (isEnabled)
                {
                    hover = HighlightState.Hover;
                }
                else
                {
                    hover = HighlightState.Disabled;
                }
            }
            else if (isChecked)
            {
                if (isEnabled)
                {
                    hover = HighlightState.Checked;
                }
                else
                {
                    hover = HighlightState.Disabled;
                }
            }
            else
            {
                hover = HighlightState.Default;
            }
            SelectionHighlight.DrawBackground(g, this.penBrushCache, rect, hover);
        }
    }
}


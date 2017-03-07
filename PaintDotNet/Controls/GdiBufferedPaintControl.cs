namespace PaintDotNet.Controls
{
    using PaintDotNet;
    using PaintDotNet.Rendering;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Drawing;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal abstract class GdiBufferedPaintControl : GdiPaintControl
    {
        private Surface doubleBufferSurface;
        [ThreadStatic]
        private static WeakReferenceT<Surface> doubleBufferSurfaceWeakRef;

        protected GdiBufferedPaintControl()
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.doubleBufferSurface != null))
            {
                this.doubleBufferSurface.Dispose();
                this.doubleBufferSurface = null;
            }
            base.Dispose(disposing);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        protected void DrawDoubleBuffer(IntPtr hdc, Surface surface, Rectangle dstRect)
        {
            IntPtr ptr;
            Int32Point point;
            Int32Size size;
            this.VerifyAccess();
            GetDrawBitmapInfo(surface, out ptr, out point, out size);
            PdnGraphics.DrawBitmap(hdc, dstRect, null, ptr, point.X, point.Y);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        protected Surface GetDoubleBuffer(Size size)
        {
            this.VerifyAccess();
            if ((size.Width < 1) || (size.Height < 1))
            {
                throw new ArgumentException($"size, {size}, must have positive values and area");
            }
            Surface target = null;
            Size size2 = new Size(0, 0);
            if ((this.doubleBufferSurface != null) && this.doubleBufferSurface.IsDisposed)
            {
                size2 = this.doubleBufferSurface.Size;
                this.doubleBufferSurface = null;
            }
            if ((this.doubleBufferSurface != null) && ((this.doubleBufferSurface.Width < size.Width) || (this.doubleBufferSurface.Height < size.Height)))
            {
                size2 = this.doubleBufferSurface.Size;
                this.doubleBufferSurface.Dispose();
                this.doubleBufferSurface = null;
                doubleBufferSurfaceWeakRef = null;
            }
            if (this.doubleBufferSurface != null)
            {
                target = this.doubleBufferSurface;
            }
            else if (doubleBufferSurfaceWeakRef != null)
            {
                target = doubleBufferSurfaceWeakRef.Target;
                if ((target != null) && target.IsDisposed)
                {
                    size2 = target.Size;
                    target = null;
                    doubleBufferSurfaceWeakRef = null;
                }
            }
            if ((target != null) && ((target.Width < size.Width) || (target.Height < size.Height)))
            {
                size2 = target.Size;
                target.Dispose();
                target = null;
                doubleBufferSurfaceWeakRef = null;
            }
            if (target == null)
            {
                Size size3 = new Size(Math.Max(size.Width, size2.Width), Math.Max(size.Height, size2.Height));
                try
                {
                    target = new Surface(size3.Width, size3.Height, SurfaceCreationFlags.Win32BitBltHint | SurfaceCreationFlags.DoNotZeroFillHint);
                }
                catch (OutOfMemoryException)
                {
                    target = new Surface(size.Width, size.Height, SurfaceCreationFlags.Win32BitBltHint | SurfaceCreationFlags.DoNotZeroFillHint);
                }
                doubleBufferSurfaceWeakRef = new WeakReferenceT<Surface>(target);
            }
            this.doubleBufferSurface = target;
            if (this.doubleBufferSurface.IsDisposed)
            {
                throw new InternalErrorException("this.doubleBufferSurface is disposed");
            }
            return target.CreateWindow(0, 0, size.Width, size.Height);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void GetDrawBitmapInfo(Surface surface, out IntPtr bitmapHandle, out Int32Point childOffset, out Int32Size parentSize)
        {
            surface.VerifyNotDisposed<Surface>();
            MemoryBlock rootMemoryBlock = surface.Scan0.GetRootMemoryBlock();
            rootMemoryBlock.VerifyNotDisposed<MemoryBlock>();
            long num = surface.Scan0.Pointer.ToInt64() - rootMemoryBlock.Pointer.ToInt64();
            int y = (int) (num / ((long) surface.Stride));
            int x = (int) ((num - (y * surface.Stride)) / 4L);
            childOffset = new Int32Point(x, y);
            parentSize = new Int32Size(surface.Stride / 4, y + surface.Height);
            bitmapHandle = rootMemoryBlock.BitmapHandle;
        }
    }
}


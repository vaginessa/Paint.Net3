namespace PaintDotNet
{
    using PaintDotNet.IO;
    using PaintDotNet.Rendering;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Drawing;
    using System.IO;
    using System.Windows.Forms;

    internal static class ClipboardUtil
    {
        private static readonly string[] fileDropImageExtensions = new string[] { ".bmp", ".png", ".jpg", ".jpe", ".jpeg", ".jfif", ".gif" };

        public static MaskedSurface GetClipboardImage(IWin32Window currentWindow, IDataObject clipData) => 
            GetClipboardImageImpl(currentWindow, clipData);

        private static Surface GetClipboardImageAsSurface(IWin32Window currentWindow, IDataObject clipData) => 
            GetClipboardImageAsSurfaceImpl(currentWindow, clipData);

        private static unsafe Surface GetClipboardImageAsSurfaceImpl(IWin32Window currentWindow, IDataObject clipData)
        {
            Image image = null;
            Surface surface = null;
            if (((image == null) && (surface == null)) && clipData.GetDataPresent(System.Windows.Forms.DataFormats.FileDrop))
            {
                try
                {
                    string[] data = clipData.GetData(System.Windows.Forms.DataFormats.FileDrop) as string[];
                    if ((data != null) && (data.Length == 1))
                    {
                        string fileName = data[0];
                        if (IsImageFileName(fileName) && File.Exists(fileName))
                        {
                            image = Image.FromFile(fileName);
                            surface = Surface.CopyFromGdipImage(image, false);
                            image.Dispose();
                            image = null;
                        }
                    }
                }
                catch (OutOfMemoryException)
                {
                    throw;
                }
                catch (Exception)
                {
                }
            }
            if (((image == null) && (surface == null)) && clipData.GetDataPresent(System.Windows.Forms.DataFormats.Dib, true))
            {
                try
                {
                    MemoryStream memoryStream = clipData.GetData(System.Windows.Forms.DataFormats.Dib, false) as MemoryStream;
                    if (memoryStream != null)
                    {
                        byte[] buffer = memoryStream.ToArrayEx();
                        memoryStream = null;
                        try
                        {
                            fixed (byte* numRef = buffer)
                            {
                                Size size;
                                if (PdnGraphics.TryGetBitmapInfoSize(numRef, buffer.Length, out size))
                                {
                                    surface = new Surface(size.Width, size.Height);
                                    bool flag = false;
                                    try
                                    {
                                        using (Bitmap bitmap = surface.CreateAliasedBitmap(true))
                                        {
                                            flag = PdnGraphics.TryCopyFromBitmapInfo(bitmap, numRef, buffer.Length);
                                        }
                                        surface.DetectAndFixDishonestAlpha();
                                    }
                                    finally
                                    {
                                        if ((surface != null) && !flag)
                                        {
                                            surface.Dispose();
                                            surface = null;
                                        }
                                    }
                                }
                            }
                        }
                        finally
                        {
                            numRef = null;
                        }
                    }
                }
                catch (OutOfMemoryException)
                {
                    throw;
                }
                catch (Exception)
                {
                }
            }
            if (((image == null) && (surface == null)) && (clipData.GetDataPresent(System.Windows.Forms.DataFormats.Bitmap, true) || clipData.GetDataPresent(System.Windows.Forms.DataFormats.EnhancedMetafile, true)))
            {
                try
                {
                    image = clipData.GetData(System.Windows.Forms.DataFormats.Bitmap, true) as Image;
                }
                catch (OutOfMemoryException)
                {
                    throw;
                }
                catch (Exception)
                {
                }
                if (image == null)
                {
                    try
                    {
                        using (PaintDotNet.SystemLayer.Clipboard.Transaction transaction = PaintDotNet.SystemLayer.Clipboard.Open(currentWindow))
                        {
                            image = transaction.TryGetEmf();
                        }
                    }
                    catch (OutOfMemoryException)
                    {
                        throw;
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            if (((image == null) && (surface == null)) && clipData.GetDataPresent("PNG", false))
            {
                try
                {
                    MemoryStream stream = clipData.GetData("PNG", false) as MemoryStream;
                    if (stream != null)
                    {
                        image = Image.FromStream(stream, false, true);
                        if (image != null)
                        {
                            surface = Surface.CopyFromGdipImage(image, false);
                            image.Dispose();
                            image = null;
                        }
                    }
                }
                catch (OutOfMemoryException)
                {
                    throw;
                }
                catch (Exception)
                {
                }
            }
            if ((surface != null) && (image != null))
            {
                throw new InternalErrorException("both surface and image are non-null");
            }
            if ((surface == null) && (image != null))
            {
                surface = Surface.CopyFromGdipImage(image, true);
            }
            return surface;
        }

        private static MaskedSurface GetClipboardImageImpl(IWin32Window currentWindow, IDataObject clipData)
        {
            Utility.GCFullCollect();
            using (PaintDotNet.SystemLayer.Clipboard.Transaction transaction = PaintDotNet.SystemLayer.Clipboard.Open(currentWindow))
            {
                if (transaction.IsDataPresent(typeof(MaskedSurface)))
                {
                    try
                    {
                        MaskedSurface surface = transaction.TryGetData(typeof(MaskedSurface)) as MaskedSurface;
                        if ((surface != null) && !surface.IsDisposed)
                        {
                            return surface;
                        }
                        if (surface != null)
                        {
                            bool isDisposed = surface.IsDisposed;
                        }
                    }
                    catch (OutOfMemoryException)
                    {
                        throw;
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            Surface clipboardImageAsSurface = GetClipboardImageAsSurface(currentWindow, clipData);
            if (clipboardImageAsSurface != null)
            {
                return new MaskedSurface(ref clipboardImageAsSurface, true);
            }
            return null;
        }

        public static Int32Size? GetClipboardImageSize(IWin32Window currentWindow, IDataObject clipData)
        {
            try
            {
                return GetClipboardImageSizeImpl(currentWindow, clipData);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static Int32Size? GetClipboardImageSizeImpl(IWin32Window currentWindow, IDataObject clipData)
        {
            Utility.GCFullCollect();
            using (MaskedSurface surface = GetClipboardImage(currentWindow, clipData))
            {
                if (surface != null)
                {
                    return new Int32Size?(surface.GetGeometryMaskScans().Bounds().Size());
                }
            }
            return null;
        }

        public static bool IsClipboardImageMaybeAvailable(IWin32Window currentWindow, IDataObject clipData)
        {
            try
            {
                bool flag3;
                bool flag = false;
                using (PaintDotNet.SystemLayer.Clipboard.Transaction transaction = PaintDotNet.SystemLayer.Clipboard.Open(currentWindow))
                {
                    if (transaction.IsDataPresent(typeof(MaskedSurface)))
                    {
                        flag = clipData.GetDataPresent(typeof(MaskedSurface));
                    }
                }
                if (!clipData.GetDataPresent(System.Windows.Forms.DataFormats.FileDrop))
                {
                    flag3 = false;
                }
                else
                {
                    string[] data = clipData.GetData(System.Windows.Forms.DataFormats.FileDrop) as string[];
                    if (((data != null) && (data.Length == 1)) && (IsImageFileName(data[0]) && File.Exists(data[0])))
                    {
                        flag3 = true;
                    }
                    else
                    {
                        flag3 = false;
                    }
                }
                bool dataPresent = clipData.GetDataPresent(System.Windows.Forms.DataFormats.Bitmap, true);
                bool flag5 = clipData.GetDataPresent(System.Windows.Forms.DataFormats.Dib, true);
                bool flag6 = clipData.GetDataPresent(System.Windows.Forms.DataFormats.EnhancedMetafile, true);
                bool flag7 = clipData.GetDataPresent("PNG", false);
                return (((flag || flag3) || (dataPresent || flag5)) || (flag6 || flag7));
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static bool IsImageFileName(string fileName)
        {
            try
            {
                foreach (string str in fileDropImageExtensions)
                {
                    if (Path.HasExtension(str))
                    {
                        return true;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            return false;
        }
    }
}


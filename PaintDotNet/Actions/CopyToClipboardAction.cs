namespace PaintDotNet.Actions
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.Controls;
    using PaintDotNet.Rendering;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Drawing;
    using System.Threading;
    using System.Windows;

    internal sealed class CopyToClipboardAction
    {
        private DocumentWorkspace documentWorkspace;

        public CopyToClipboardAction(DocumentWorkspace documentWorkspace)
        {
            this.documentWorkspace = documentWorkspace;
        }

        public unsafe bool PerformAction()
        {
            bool flag = true;
            if (this.documentWorkspace.Selection.IsEmpty || !(this.documentWorkspace.ActiveLayer is BitmapLayer))
            {
                return false;
            }
            try
            {
                using (new WaitCursorChanger(this.documentWorkspace))
                {
                    GeometryList geometryMask = this.documentWorkspace.Selection.CreateGeometryListClippingMask();
                    BitmapLayer activeLayer = (BitmapLayer) this.documentWorkspace.ActiveLayer;
                    Surface source = activeLayer.Surface;
                    UnsafeList<Int32Rect> interiorScansUnsafeList = geometryMask.GetInteriorScansUnsafeList();
                    Int32Rect rect = interiorScansUnsafeList.Bounds();
                    if ((rect.Width > 0) && (rect.Height > 0))
                    {
                        int num = 10;
                        while (num >= 0)
                        {
                            try
                            {
                                try
                                {
                                    using (Clipboard.Transaction transaction = Clipboard.Open(this.documentWorkspace))
                                    {
                                        transaction.Empty();
                                        using (MaskedSurface surface2 = new MaskedSurface(source, geometryMask))
                                        {
                                            transaction.AddData(surface2);
                                            using (Surface surface3 = surface2.Surface.CreateWindow(new Rectangle(0, 0, rect.Width, rect.Height)))
                                            {
                                                surface3.Clear(ColorBgra.FromUInt32(0));
                                                foreach (Int32Rect rect2 in interiorScansUnsafeList)
                                                {
                                                    surface3.CopySurface(source, new Int32Point(rect2.X - rect.X, rect2.Y - rect.Y), rect2);
                                                }
                                                ColorBgra white = ColorBgra.White;
                                                UserBlendOps.NormalBlendOp @static = UserBlendOps.NormalBlendOp.Static;
                                                for (int i = 0; i < surface3.Height; i++)
                                                {
                                                    ColorBgra* rowAddress = surface3.GetRowAddress(i);
                                                    ColorBgra* bgraPtr2 = rowAddress + surface3.Width;
                                                    while (rowAddress < bgraPtr2)
                                                    {
                                                        rowAddress->Bgra = @static.Apply(white, rowAddress[0]).Bgra;
                                                        rowAddress++;
                                                    }
                                                }
                                                using (Bitmap bitmap = surface3.CreateAliasedBitmap(false))
                                                {
                                                    transaction.AddDibV5(bitmap);
                                                }
                                            }
                                            break;
                                        }
                                    }
                                }
                                catch (Exception)
                                {
                                    if (num == 0)
                                    {
                                        flag = false;
                                        Utility.ErrorBox(this.documentWorkspace, PdnResources.GetString2("CopyAction.Error.TransferToClipboard"));
                                    }
                                    else
                                    {
                                        Thread.Sleep(50);
                                    }
                                }
                                continue;
                            }
                            finally
                            {
                                num--;
                            }
                        }
                    }
                    if (geometryMask != null)
                    {
                        geometryMask.Dispose();
                        geometryMask = null;
                    }
                }
            }
            catch (OutOfMemoryException)
            {
                flag = false;
                Utility.ErrorBox(this.documentWorkspace, PdnResources.GetString2("CopyAction.Error.OutOfMemory"));
            }
            catch (Exception)
            {
                flag = false;
                Utility.ErrorBox(this.documentWorkspace, PdnResources.GetString2("CopyAction.Error.Generic"));
            }
            Utility.GCFullCollect();
            return flag;
        }
    }
}


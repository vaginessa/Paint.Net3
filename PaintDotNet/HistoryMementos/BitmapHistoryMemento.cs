namespace PaintDotNet.HistoryMementos
{
    using PaintDotNet;
    using PaintDotNet.Rendering;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Drawing;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Windows;

    internal class BitmapHistoryMemento : HistoryMemento
    {
        private IHistoryWorkspace historyWorkspace;
        private int layerIndex;
        private Guid poMaskedSurfaceRef;
        private Guid poUndoMaskedSurfaceRef;
        private DeleteFileOnFree tempFileHandle;
        private string tempFileName;

        public BitmapHistoryMemento(string name, ImageResource image, IHistoryWorkspace historyWorkspace, int layerIndex, IrregularSurface saved) : this(name, image, historyWorkspace, layerIndex, saved, false)
        {
        }

        public BitmapHistoryMemento(string name, ImageResource image, IHistoryWorkspace historyWorkspace, int layerIndex, GeometryList changedRegion) : this(name, image, historyWorkspace, layerIndex, changedRegion, ((BitmapLayer) historyWorkspace.Document.Layers[layerIndex]).Surface)
        {
        }

        public BitmapHistoryMemento(string name, ImageResource image, IHistoryWorkspace historyWorkspace, int layerIndex, Guid poMaskedSurfaceRef) : base(name, image)
        {
            this.layerIndex = layerIndex;
            this.historyWorkspace = historyWorkspace;
            this.poMaskedSurfaceRef = poMaskedSurfaceRef;
        }

        public BitmapHistoryMemento(string name, ImageResource image, IHistoryWorkspace historyWorkspace, int layerIndex, IrregularSurface saved, bool takeOwnershipOfSaved) : base(name, image)
        {
            IrregularSurface surface;
            this.historyWorkspace = historyWorkspace;
            this.layerIndex = layerIndex;
            if (takeOwnershipOfSaved)
            {
                surface = saved;
            }
            else
            {
                surface = (IrregularSurface) saved.Clone();
            }
            BitmapHistoryMementoData data = new BitmapHistoryMementoData(surface, null);
            base.Data = data;
        }

        public BitmapHistoryMemento(string name, ImageResource image, IHistoryWorkspace historyWorkspace, int layerIndex, GeometryList changedRegion, Surface copyFromThisSurface) : base(name, image)
        {
            this.historyWorkspace = historyWorkspace;
            this.layerIndex = layerIndex;
            GeometryList region = changedRegion.Clone();
            this.tempFileName = FileSystem.GetTempFileName();
            FileStream output = null;
            try
            {
                output = FileSystem.OpenStreamingFile(this.tempFileName, FileAccess.Write);
                SaveSurfaceRegion(output, copyFromThisSurface, region);
            }
            finally
            {
                if (output != null)
                {
                    output.Dispose();
                    output = null;
                }
            }
            this.tempFileHandle = new DeleteFileOnFree(this.tempFileName);
            BitmapHistoryMementoData data = new BitmapHistoryMementoData(null, region);
            base.Data = data;
        }

        private static unsafe void LoadOrSaveSurfaceRegion(Stream stream, Surface surface, GeometryList region, bool trueForSave)
        {
            void*[] voidPtrArray;
            ulong[] numArray;
            Int32Rect[] interiorScans = region.GetInteriorScans();
            Int32Rect rect = surface.Bounds<ColorBgra>();
            Int32Rect bounds = region.Bounds.Int32Bound().IntersectCopy(rect);
            int num = 0;
            long num2 = (bounds.Width * bounds.Height) * 4L;
            if (((interiorScans.Length == 1) && (num2 <= 0xffffffffL)) && surface.IsContiguousMemoryRegion(bounds))
            {
                voidPtrArray = new void*[] { surface.GetPointAddressUnchecked((System.Drawing.Point) bounds.Location()) };
                numArray = new ulong[] { num2 };
            }
            else
            {
                for (int i = 0; i < interiorScans.Length; i++)
                {
                    Int32Rect rect3 = interiorScans[i].IntersectCopy(rect);
                    if ((rect3.Width != 0) && (rect3.Height != 0))
                    {
                        num += rect3.Height;
                    }
                }
                int index = 0;
                voidPtrArray = new void*[num];
                numArray = new ulong[num];
                for (int j = 0; j < interiorScans.Length; j++)
                {
                    Int32Rect rect4 = interiorScans[j].IntersectCopy(rect);
                    if ((rect4.Width != 0) && (rect4.Height != 0))
                    {
                        for (int k = rect4.Y; k < (rect4.Y + rect4.Height); k++)
                        {
                            voidPtrArray[index] = (void*) surface.GetPointAddress(rect4.X, k);
                            numArray[index] = (ulong) (rect4.Width * 4L);
                            index++;
                        }
                    }
                }
            }
            if (trueForSave)
            {
                WriteToStreamGather(stream, voidPtrArray, numArray);
            }
            else
            {
                ReadFromStreamScatter(stream, voidPtrArray, numArray);
            }
        }

        private static void LoadSurfaceRegion(FileStream input, Surface surface, GeometryList region)
        {
            LoadOrSaveSurfaceRegion(input, surface, region, false);
        }

        protected override HistoryMemento OnUndo()
        {
            GeometryList geometryMaskCopy;
            BitmapHistoryMemento memento;
            BitmapHistoryMementoData data = base.Data as BitmapHistoryMementoData;
            BitmapLayer layer = (BitmapLayer) this.historyWorkspace.Document.Layers[this.layerIndex];
            MaskedSurface surface = null;
            if (this.poMaskedSurfaceRef != Guid.Empty)
            {
                surface = PersistedObjectLocker.Get<MaskedSurface>(this.poMaskedSurfaceRef).Object;
                geometryMaskCopy = surface.GetGeometryMaskCopy();
            }
            else if (data.UndoImage == null)
            {
                geometryMaskCopy = data.SavedRegion;
            }
            else
            {
                geometryMaskCopy = data.UndoImage.Geometry;
            }
            if (this.poUndoMaskedSurfaceRef == Guid.Empty)
            {
                memento = new BitmapHistoryMemento(base.Name, base.Image, this.historyWorkspace, this.layerIndex, geometryMaskCopy) {
                    poUndoMaskedSurfaceRef = this.poMaskedSurfaceRef
                };
            }
            else
            {
                memento = new BitmapHistoryMemento(base.Name, base.Image, this.historyWorkspace, this.layerIndex, this.poUndoMaskedSurfaceRef);
            }
            if (surface != null)
            {
                surface.Draw(layer.Surface);
            }
            else if (data.UndoImage == null)
            {
                using (FileStream stream = FileSystem.OpenStreamingFile(this.tempFileName, FileAccess.Read))
                {
                    LoadSurfaceRegion(stream, layer.Surface, data.SavedRegion);
                }
                this.tempFileHandle.Dispose();
                this.tempFileHandle = null;
            }
            else
            {
                data.UndoImage.Draw(layer.Surface);
            }
            layer.Invalidate(geometryMaskCopy);
            geometryMaskCopy.Dispose();
            return memento;
        }

        private static unsafe void ReadFromStream(Stream input, void* pBuffer, ulong cbBuffer)
        {
            int num = (int) Math.Min(0x2000L, cbBuffer);
            byte[] buffer = new byte[num];
            ulong num2 = cbBuffer;
            fixed (byte* numRef = buffer)
            {
                while (num2 != 0L)
                {
                    int count = (int) Math.Min((ulong) buffer.Length, num2);
                    int num4 = input.Read(buffer, 0, count);
                    if (num4 == 0)
                    {
                        throw new EndOfStreamException();
                    }
                    Memory.Copy(pBuffer, (void*) numRef, (ulong) num4);
                    pBuffer += num4;
                    num2 -= num4;
                    if (num2 > cbBuffer)
                    {
                        throw new InternalErrorException("cb > cbBuffer");
                    }
                }
            }
        }

        private static unsafe void ReadFromStreamScatter(Stream input, void*[] ppvBuffers, ulong[] lengths)
        {
            for (int i = 0; i < ppvBuffers.Length; i++)
            {
                ReadFromStream(input, ppvBuffers[i], lengths[i]);
            }
        }

        private static void SaveSurfaceRegion(FileStream output, Surface surface, GeometryList region)
        {
            LoadOrSaveSurfaceRegion(output, surface, region, true);
        }

        private static unsafe void WriteToStream(Stream output, void* pBuffer, ulong cbBuffer)
        {
            int num = (int) Math.Min(0x2000L, cbBuffer);
            byte[] buffer = new byte[num];
            ulong num2 = cbBuffer;
            fixed (byte* numRef = buffer)
            {
                while (num2 != 0L)
                {
                    ulong length = Math.Min((ulong) num, num2);
                    Memory.Copy((void*) numRef, pBuffer, length);
                    num2 -= length;
                    pBuffer += (void*) length;
                    output.Write(buffer, 0, (int) length);
                }
            }
        }

        private static unsafe void WriteToStreamGather(Stream output, void*[] ppvBuffers, ulong[] lengths)
        {
            for (int i = 0; i < ppvBuffers.Length; i++)
            {
                WriteToStream(output, ppvBuffers[i], lengths[i]);
            }
        }

        [Serializable]
        private sealed class BitmapHistoryMementoData : HistoryMementoData
        {
            private GeometryList savedRegion;
            private IrregularSurface undoImage;

            public BitmapHistoryMementoData(IrregularSurface undoImage, GeometryList savedRegion)
            {
                if ((undoImage != null) && (savedRegion != null))
                {
                    throw new ArgumentException("Only one of undoImage or savedRegion may be non-null");
                }
                this.undoImage = undoImage;
                this.savedRegion = savedRegion;
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    if (this.undoImage != null)
                    {
                        this.undoImage.Dispose();
                        this.undoImage = null;
                    }
                    if (this.savedRegion != null)
                    {
                        this.savedRegion.Dispose();
                        this.savedRegion = null;
                    }
                }
                base.Dispose(disposing);
            }

            public GeometryList SavedRegion =>
                this.savedRegion;

            public IrregularSurface UndoImage =>
                this.undoImage;
        }

        private class DeleteFileOnFree : IDisposable
        {
            private IntPtr bstrFileName;

            public DeleteFileOnFree(string fileName)
            {
                this.bstrFileName = Marshal.StringToBSTR(fileName);
            }

            public void Dispose()
            {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }

            private void Dispose(bool disposing)
            {
                if (this.bstrFileName != IntPtr.Zero)
                {
                    string filePath = Marshal.PtrToStringBSTR(this.bstrFileName);
                    Marshal.FreeBSTR(this.bstrFileName);
                    FileSystem.TryDeleteFile(filePath);
                    this.bstrFileName = IntPtr.Zero;
                }
            }

            ~DeleteFileOnFree()
            {
                this.Dispose(false);
            }
        }
    }
}


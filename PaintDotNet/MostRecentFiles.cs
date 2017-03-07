namespace PaintDotNet
{
    using PaintDotNet.SystemLayer;
    using System;
    using System.Collections;
    using System.Drawing;

    internal class MostRecentFiles
    {
        private Queue files;
        private const int iconSize = 0x38;
        private bool loaded;
        private int maxCount;

        public MostRecentFiles(int maxCount)
        {
            this.maxCount = maxCount;
            this.files = new Queue();
        }

        public void Add(MostRecentFile mrf)
        {
            if (!this.Loaded)
            {
                this.LoadMruList();
            }
            if (!this.Contains(mrf.FileName))
            {
                this.files.Enqueue(mrf);
                while (this.files.Count > this.maxCount)
                {
                    this.files.Dequeue();
                }
            }
        }

        public void Clear()
        {
            if (!this.Loaded)
            {
                this.LoadMruList();
            }
            foreach (MostRecentFile file in this.GetFileList())
            {
                this.Remove(file.FileName);
            }
        }

        public bool Contains(string fileName)
        {
            if (!this.Loaded)
            {
                this.LoadMruList();
            }
            foreach (MostRecentFile file in this.files)
            {
                if (string.Equals(fileName, file.FileName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        public MostRecentFile[] GetFileList()
        {
            if (!this.Loaded)
            {
                this.LoadMruList();
            }
            object[] objArray = this.files.ToArray();
            MostRecentFile[] array = new MostRecentFile[objArray.Length];
            objArray.CopyTo(array, 0);
            return array;
        }

        public void LoadMruList()
        {
            try
            {
                this.loaded = true;
                this.Clear();
                for (int i = 0; i < this.MaxCount; i++)
                {
                    try
                    {
                        string key = "MRU" + i.ToString();
                        string fileName = Settings.CurrentUser.GetString(key);
                        if (fileName != null)
                        {
                            Image thumb = Settings.CurrentUser.GetImage(key + "Thumb");
                            if ((fileName != null) && (thumb != null))
                            {
                                MostRecentFile mrf = new MostRecentFile(fileName, thumb);
                                this.Add(mrf);
                            }
                        }
                    }
                    catch
                    {
                        return;
                    }
                }
            }
            catch (Exception)
            {
                this.Clear();
            }
        }

        public void Remove(string fileName)
        {
            if (!this.Loaded)
            {
                this.LoadMruList();
            }
            if (this.Contains(fileName))
            {
                Queue queue = new Queue();
                foreach (MostRecentFile file in this.files)
                {
                    if (string.Compare(file.FileName, fileName, true) != 0)
                    {
                        queue.Enqueue(file);
                    }
                }
                this.files = queue;
            }
        }

        public void SaveMruList()
        {
            if (this.Loaded)
            {
                Settings.CurrentUser.SetInt32("MRUMax", this.MaxCount);
                MostRecentFile[] fileList = this.GetFileList();
                for (int i = 0; i < this.MaxCount; i++)
                {
                    string key = "MRU" + i.ToString();
                    string str2 = key + "Thumb";
                    if (i >= fileList.Length)
                    {
                        Settings.CurrentUser.Delete(key);
                        Settings.CurrentUser.Delete(str2);
                    }
                    else
                    {
                        MostRecentFile file = fileList[i];
                        Settings.CurrentUser.SetString(key, file.FileName);
                        Settings.CurrentUser.SetImage(str2, file.Thumb);
                    }
                }
            }
        }

        public int Count
        {
            get
            {
                if (!this.loaded)
                {
                    this.LoadMruList();
                }
                return this.files.Count;
            }
        }

        public int IconSize =>
            UI.ScaleWidth(0x38);

        public bool Loaded =>
            this.loaded;

        public int MaxCount =>
            this.maxCount;
    }
}


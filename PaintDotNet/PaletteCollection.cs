namespace PaintDotNet
{
    using PaintDotNet.Collections;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;

    internal sealed class PaletteCollection
    {
        private const char lineCommentChar = ';';
        public const int PaletteColorCount = 0x60;
        private static readonly Encoding paletteFileEncoding = Encoding.UTF8;
        private Dictionary<string, ColorBgra[]> palettes = new Dictionary<string, ColorBgra[]>();

        public void AddOrUpdate(string name, ColorBgra[] colors)
        {
            if (colors.Length != 0x60)
            {
                string[] strArray = new string[] { "palette must have exactly ", 0x60.ToString(), " colors (actual: ", colors.Length.ToString(), ")" };
                throw new ArgumentException(string.Concat(strArray));
            }
            this.Delete(name);
            this.palettes.Add(name, colors);
        }

        public bool Contains(string name, out string existingKeyName)
        {
            foreach (string str in this.palettes.Keys)
            {
                if (string.Compare(str, name, StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    existingKeyName = str;
                    return true;
                }
            }
            existingKeyName = null;
            return false;
        }

        public bool Delete(string name)
        {
            string str;
            if (this.Contains(name, out str))
            {
                this.palettes.Remove(str);
                return true;
            }
            return false;
        }

        private bool DoesPalettesPathExist()
        {
            string palettesPath = PalettesPath;
            try
            {
                return Directory.Exists(palettesPath);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static void EnsurePalettesPathExists()
        {
            string palettesPath = PalettesPath;
            try
            {
                if (!Directory.Exists(palettesPath))
                {
                    Directory.CreateDirectory(palettesPath);
                }
            }
            catch (Exception)
            {
            }
        }

        public static ColorBgra[] EnsureValidPaletteSize(ColorBgra[] colors)
        {
            ColorBgra[] bgraArray = new ColorBgra[0x60];
            for (int i = 0; i < 0x60; i++)
            {
                if (i >= colors.Length)
                {
                    bgraArray[i] = DefaultColor;
                }
                else
                {
                    bgraArray[i] = colors[i];
                }
            }
            return bgraArray;
        }

        private static string FormatColor(ColorBgra color) => 
            color.ToHexString();

        public ColorBgra[] Get(string name)
        {
            string str;
            if (this.Contains(name, out str))
            {
                ColorBgra[] bgraArray = this.palettes[str];
                return (ColorBgra[]) bgraArray.Clone();
            }
            return null;
        }

        public static string GetPaletteSaveString(ColorBgra[] palette)
        {
            StringWriter writer = new StringWriter();
            string str = PdnResources.GetString2("ColorPalette.SaveHeader");
            writer.WriteLine(str);
            foreach (ColorBgra bgra in palette)
            {
                string str2 = FormatColor(bgra);
                writer.WriteLine(str2);
            }
            return writer.ToString();
        }

        public void Load()
        {
            if (!this.DoesPalettesPathExist())
            {
                this.palettes = new Dictionary<string, ColorBgra[]>();
            }
            else
            {
                string[] files = ArrayUtil.Empty<string>();
                try
                {
                    files = Directory.GetFiles(PalettesPath, "*" + PalettesFileExtension);
                }
                catch (Exception)
                {
                }
                Dictionary<string, ColorBgra[]> dictionary = new Dictionary<string, ColorBgra[]>();
                foreach (string str in files)
                {
                    ColorBgra[] bgraArray2 = EnsureValidPaletteSize(LoadPalette(str));
                    string key = Path.ChangeExtension(Path.GetFileName(str), null);
                    dictionary.Add(key, bgraArray2);
                }
                this.palettes = dictionary;
            }
        }

        public static ColorBgra[] LoadPalette(string palettePath)
        {
            ColorBgra[] bgraArray = null;
            FileStream stream = new FileStream(palettePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            try
            {
                StreamReader reader = new StreamReader(stream, paletteFileEncoding);
                try
                {
                    bgraArray = ParsePaletteString(reader.ReadToEnd());
                }
                finally
                {
                    reader.Close();
                    reader = null;
                    stream = null;
                }
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                    stream = null;
                }
            }
            if (bgraArray == null)
            {
                return ArrayUtil.Empty<ColorBgra>();
            }
            return bgraArray;
        }

        private static bool ParseColor(string colorString, out ColorBgra color)
        {
            try
            {
                color = ColorBgra.ParseHexString(colorString);
                return true;
            }
            catch (Exception)
            {
                color = DefaultColor;
                return false;
            }
        }

        public static bool ParsePaletteLine(string line, out ColorBgra color)
        {
            color = DefaultColor;
            if (line == null)
            {
                return false;
            }
            string colorString = RemoveComments(line).Trim();
            if (colorString.Length == 0)
            {
                return false;
            }
            return ParseColor(colorString, out color);
        }

        public static ColorBgra[] ParsePaletteString(string paletteString)
        {
            List<ColorBgra> items = new List<ColorBgra>();
            StringReader reader = new StringReader(paletteString);
            while (true)
            {
                ColorBgra bgra;
                string line = reader.ReadLine();
                if (line == null)
                {
                    return items.ToArrayEx<ColorBgra>();
                }
                if (ParsePaletteLine(line, out bgra) && (items.Count < 0x60))
                {
                    items.Add(bgra);
                }
            }
        }

        public static string RemoveComments(string line)
        {
            int index = line.IndexOf(';');
            if (index != -1)
            {
                return line.Substring(0, index);
            }
            return line;
        }

        public void Save()
        {
            EnsurePalettesPathExists();
            string palettesPath = PalettesPath;
            foreach (string str2 in this.palettes.Keys)
            {
                ColorBgra[] colors = this.palettes[str2];
                ColorBgra[] palette = EnsureValidPaletteSize(colors);
                string str3 = Path.ChangeExtension(str2, PalettesFileExtension);
                SavePalette(Path.Combine(palettesPath, str3), palette);
            }
        }

        public static void SavePalette(string palettePath, ColorBgra[] palette)
        {
            FileStream stream = new FileStream(palettePath, FileMode.Create, FileAccess.Write, FileShare.Read);
            try
            {
                StreamWriter writer = new StreamWriter(stream, paletteFileEncoding);
                try
                {
                    string paletteSaveString = GetPaletteSaveString(palette);
                    writer.WriteLine(paletteSaveString);
                }
                finally
                {
                    writer.Close();
                    writer = null;
                    stream = null;
                }
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                    stream = null;
                }
            }
        }

        public static bool ValidatePaletteName(string paletteName)
        {
            if (string.IsNullOrEmpty(paletteName))
            {
                return false;
            }
            try
            {
                string str = Path.ChangeExtension(paletteName, PalettesFileExtension);
                string str2 = Path.Combine(PalettesPath, str);
                char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
                char[] invalidPathChars = Path.GetInvalidPathChars();
                if (str2.IndexOfAny(invalidPathChars) != -1)
                {
                    return false;
                }
                if (str.IndexOfAny(invalidFileNameChars) != -1)
                {
                    return false;
                }
                return true;
            }
            catch (ArgumentNullException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        public static ColorBgra DefaultColor =>
            ColorBgra.White;

        public static ColorBgra[] DefaultPalette =>
            new ColorBgra[] { 
                ColorBgra.FromUInt32(0xff000000), ColorBgra.FromUInt32(0xff404040), ColorBgra.FromUInt32(0xffff0000), ColorBgra.FromUInt32(0xffff6a00), ColorBgra.FromUInt32(0xffffd800), ColorBgra.FromUInt32(0xffb6ff00), ColorBgra.FromUInt32(0xff4cff00), ColorBgra.FromUInt32(0xff00ff21), ColorBgra.FromUInt32(0xff00ff90), ColorBgra.FromUInt32(0xff00ffff), ColorBgra.FromUInt32(0xff0094ff), ColorBgra.FromUInt32(0xff0026ff), ColorBgra.FromUInt32(0xff4800ff), ColorBgra.FromUInt32(0xffb200ff), ColorBgra.FromUInt32(0xffff00dc), ColorBgra.FromUInt32(0xffff006e),
                ColorBgra.FromUInt32(uint.MaxValue), ColorBgra.FromUInt32(0xff808080), ColorBgra.FromUInt32(0xff7f0000), ColorBgra.FromUInt32(0xff7f3300), ColorBgra.FromUInt32(0xff7f6a00), ColorBgra.FromUInt32(0xff5b7f00), ColorBgra.FromUInt32(0xff267f00), ColorBgra.FromUInt32(0xff007f0e), ColorBgra.FromUInt32(0xff007f46), ColorBgra.FromUInt32(0xff007f7f), ColorBgra.FromUInt32(0xff004a7f), ColorBgra.FromUInt32(0xff00137f), ColorBgra.FromUInt32(0xff21007f), ColorBgra.FromUInt32(0xff57007f), ColorBgra.FromUInt32(0xff7f006e), ColorBgra.FromUInt32(0xff7f0037),
                ColorBgra.FromUInt32(0xffa0a0a0), ColorBgra.FromUInt32(0xff303030), ColorBgra.FromUInt32(0xffff7f7f), ColorBgra.FromUInt32(0xffffb27f), ColorBgra.FromUInt32(0xffffe97f), ColorBgra.FromUInt32(0xffdaff7f), ColorBgra.FromUInt32(0xffa5ff7f), ColorBgra.FromUInt32(0xff7fff8e), ColorBgra.FromUInt32(0xff7fffc5), ColorBgra.FromUInt32(0xff7fffff), ColorBgra.FromUInt32(0xff7fc9ff), ColorBgra.FromUInt32(0xff7f92ff), ColorBgra.FromUInt32(0xffa17fff), ColorBgra.FromUInt32(0xffd67fff), ColorBgra.FromUInt32(0xffff7fed), ColorBgra.FromUInt32(0xffff7fb6),
                ColorBgra.FromUInt32(0xffc0c0c0), ColorBgra.FromUInt32(0xff606060), ColorBgra.FromUInt32(0xff7f3f3f), ColorBgra.FromUInt32(0xff7f593f), ColorBgra.FromUInt32(0xff7f743f), ColorBgra.FromUInt32(0xff6d7f3f), ColorBgra.FromUInt32(0xff527f3f), ColorBgra.FromUInt32(0xff3f7f47), ColorBgra.FromUInt32(0xff3f7f62), ColorBgra.FromUInt32(0xff3f7f7f), ColorBgra.FromUInt32(0xff3f647f), ColorBgra.FromUInt32(0xff3f497f), ColorBgra.FromUInt32(0xff503f7f), ColorBgra.FromUInt32(0xff6b3f7f), ColorBgra.FromUInt32(0xff7f3f76), ColorBgra.FromUInt32(0xff7f3f5b),
                ColorBgra.FromUInt32(0x80000000), ColorBgra.FromUInt32(0x80404040), ColorBgra.FromUInt32(0x80ff0000), ColorBgra.FromUInt32(0x80ff6a00), ColorBgra.FromUInt32(0x80ffd800), ColorBgra.FromUInt32(0x80b6ff00), ColorBgra.FromUInt32(0x804cff00), ColorBgra.FromUInt32(0x8000ff21), ColorBgra.FromUInt32(0x8000ff90), ColorBgra.FromUInt32(0x8000ffff), ColorBgra.FromUInt32(0x800094ff), ColorBgra.FromUInt32(0x800026ff), ColorBgra.FromUInt32(0x804800ff), ColorBgra.FromUInt32(0x80b200ff), ColorBgra.FromUInt32(0x80ff00dc), ColorBgra.FromUInt32(0x80ff006e),
                ColorBgra.FromUInt32(0x80ffffff), ColorBgra.FromUInt32(0x80808080), ColorBgra.FromUInt32(0x807f0000), ColorBgra.FromUInt32(0x807f3300), ColorBgra.FromUInt32(0x807f6a00), ColorBgra.FromUInt32(0x805b7f00), ColorBgra.FromUInt32(0x80267f00), ColorBgra.FromUInt32(0x80007f0e), ColorBgra.FromUInt32(0x80007f46), ColorBgra.FromUInt32(0x80007f7f), ColorBgra.FromUInt32(0x80004a7f), ColorBgra.FromUInt32(0x8000137f), ColorBgra.FromUInt32(0x8021007f), ColorBgra.FromUInt32(0x8057007f), ColorBgra.FromUInt32(0x807f006e), ColorBgra.FromUInt32(0x807f0037)
            };

        public string[] PaletteNames
        {
            get
            {
                Dictionary<string, ColorBgra[]>.KeyCollection keys = this.palettes.Keys;
                string[] strArray = new string[keys.Count];
                int index = 0;
                foreach (string str in keys)
                {
                    strArray[index] = str;
                    index++;
                }
                return strArray;
            }
        }

        public static string PalettesFileExtension =>
            ".txt";

        public static string PalettesPath
        {
            get
            {
                string str = PdnInfo.UserDataPath3;
                string str2 = PdnResources.GetString2("ColorPalettes.UserDataSubDirName");
                return Path.Combine(str, str2);
            }
        }
    }
}


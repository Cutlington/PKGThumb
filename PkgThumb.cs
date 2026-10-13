// PkgThumb.cs - Explorer thumbnail provider for PS4 .pkg files (shows icon0.png).
// Compiled by the C# compiler that is already built into Windows (no installs needed).
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using Microsoft.Win32;

namespace PkgThumb
{
    [ComImport, Guid("e357fccd-a995-4576-b01f-234630154e96"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IThumbnailProvider
    {
        [PreserveSig] int GetThumbnail(uint cx, out IntPtr hbitmap, out uint alphaType);
    }

    [ComImport, Guid("b824b49d-22ac-4161-ac8a-9916e8fa3f7f"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IInitializeWithStream
    {
        void Initialize(IStream stream, uint mode);
    }

    [ComVisible(true), Guid("7B3C1D52-4E8A-4F6B-9A21-5C0D8E3F6A14"), ClassInterface(ClassInterfaceType.None), ProgId("PkgThumb.Provider")]
    public class PkgThumbProvider : IInitializeWithStream, IThumbnailProvider
    {
        const string ThumbKey = ".pkg\\ShellEx\\{E357FCCD-A995-4576-B01F-234630154E96}";
        const string ClsidStr = "{7B3C1D52-4E8A-4F6B-9A21-5C0D8E3F6A14}";
        IStream stream;

        [StructLayout(LayoutKind.Sequential)]
        struct BITMAPINFOHEADER
        {
            public uint biSize; public int biWidth; public int biHeight; public short biPlanes; public short biBitCount;
            public uint biCompression; public uint biSizeImage; public int biXPelsPerMeter; public int biYPelsPerMeter;
            public uint biClrUsed; public uint biClrImportant;
        }
        [DllImport("gdi32.dll")]
        static extern IntPtr CreateDIBSection(IntPtr hdc, ref BITMAPINFOHEADER bmi, uint usage, out IntPtr bits, IntPtr section, uint offset);

        [ComRegisterFunction]
        public static void Register(Type t)
        {
            using (RegistryKey k = Registry.ClassesRoot.CreateSubKey(ThumbKey)) k.SetValue(null, ClsidStr);
        }

        [ComUnregisterFunction]
        public static void Unregister(Type t)
        {
            try { Registry.ClassesRoot.DeleteSubKeyTree(ThumbKey, false); } catch { }
        }

        public void Initialize(IStream s, uint mode) { stream = s; }

        public int GetThumbnail(uint cx, out IntPtr hbitmap, out uint alphaType)
        {
            hbitmap = IntPtr.Zero; alphaType = 0;
            try
            {
                byte[] png = ExtractIcon(stream);
                if (png == null) return unchecked((int)0x80004005);
                hbitmap = ToHBitmap(png, (int)(cx == 0 ? 256 : cx));
                alphaType = 2; // WTSAT_ARGB
                return 0;
            }
            catch { return unchecked((int)0x80004005); }
        }

        static byte[] ReadAt(IStream s, long pos, int n)
        {
            if (n <= 0) return null;
            s.Seek(pos, 0, IntPtr.Zero);
            byte[] buf = new byte[n];
            IntPtr p = Marshal.AllocCoTaskMem(4);
            try
            {
                s.Read(buf, n, p);
                return Marshal.ReadInt32(p) == n ? buf : null;
            }
            finally { Marshal.FreeCoTaskMem(p); }
        }

        static uint BE32(byte[] b, int o)
        {
            return ((uint)b[o] << 24) | ((uint)b[o + 1] << 16) | ((uint)b[o + 2] << 8) | b[o + 3];
        }

        static bool IsPng(byte[] b, int o)
        {
            if (o + 8 > b.Length) return false;
            byte[] sig = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
            for (int i = 0; i < 8; i++) if (b[o + i] != sig[i]) return false;
            return true;
        }

        static byte[] ExtractIcon(IStream s)
        {
            System.Runtime.InteropServices.ComTypes.STATSTG st;
            s.Stat(out st, 1);
            long size = st.cbSize;
            byte[] h = ReadAt(s, 0, 0x40);
            if (h == null || BE32(h, 0) != 0x7F434E54) return null; // "\x7FCNT"

            // 1) Walk the entry table for ICON0_PNG (id 0x1200)
            uint count = BE32(h, 0x14), table = BE32(h, 0x1C);
            if (count > 0 && count < 8192 && table < size)
            {
                for (uint i = 0; i < count; i++)
                {
                    byte[] e = ReadAt(s, (long)table + (long)i * 32, 32);
                    if (e == null) break;
                    if (BE32(e, 0) != 0x1200) continue;
                    uint flags = BE32(e, 8), off = BE32(e, 16), len = BE32(e, 20);
                    if ((flags & 0x80000000u) != 0 || len < 16 || len > (8u << 20) || (long)off + len > size) break;
                    byte[] data = ReadAt(s, off, (int)len);
                    if (data != null && IsPng(data, 0)) return data;
                    break;
                }
            }

            // 2) Fallback: first PNG in the first 4 MB
            int scan = (int)Math.Min(size, 4L << 20);
            byte[] buf = ReadAt(s, 0, scan);
            if (buf == null) return null;
            for (int i = 0; i + 8 < scan; i++)
            {
                if (buf[i] != 0x89 || !IsPng(buf, i)) continue;
                for (int j = i + 8; j + 8 <= scan; j++)
                {
                    if (buf[j] == (byte)'I' && buf[j + 1] == (byte)'E' && buf[j + 2] == (byte)'N' && buf[j + 3] == (byte)'D')
                    {
                        byte[] outp = new byte[j + 8 - i];
                        Array.Copy(buf, i, outp, 0, outp.Length);
                        return outp;
                    }
                }
                break;
            }
            return null;
        }

        static IntPtr ToHBitmap(byte[] png, int cx)
        {
            using (Image src = Image.FromStream(new MemoryStream(png)))
            {
                int w = src.Width, h = src.Height;
                int tw = cx, th = cx;
                if (w >= h) th = Math.Max(1, (int)((long)h * cx / w)); else tw = Math.Max(1, (int)((long)w * cx / h));

                using (Bitmap bmp = new Bitmap(tw, th, PixelFormat.Format32bppPArgb))
                {
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        g.Clear(Color.Transparent);
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        g.DrawImage(src, 0, 0, tw, th);
                    }

                    BITMAPINFOHEADER bi = new BITMAPINFOHEADER();
                    bi.biSize = (uint)Marshal.SizeOf(typeof(BITMAPINFOHEADER));
                    bi.biWidth = tw; bi.biHeight = -th; bi.biPlanes = 1; bi.biBitCount = 32; bi.biCompression = 0;
                    IntPtr bits;
                    IntPtr hb = CreateDIBSection(IntPtr.Zero, ref bi, 0, out bits, IntPtr.Zero, 0);
                    if (hb == IntPtr.Zero) throw new OutOfMemoryException();

                    BitmapData bd = bmp.LockBits(new Rectangle(0, 0, tw, th), ImageLockMode.ReadOnly, PixelFormat.Format32bppPArgb);
                    try
                    {
                        byte[] row = new byte[tw * 4];
                        for (int y = 0; y < th; y++)
                        {
                            Marshal.Copy(new IntPtr(bd.Scan0.ToInt64() + (long)y * bd.Stride), row, 0, row.Length);
                            Marshal.Copy(row, 0, new IntPtr(bits.ToInt64() + (long)y * tw * 4), row.Length);
                        }
                    }
                    finally { bmp.UnlockBits(bd); }
                    return hb;
                }
            }
        }
    }
}

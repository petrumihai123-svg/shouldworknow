using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace PortableWinFormsRecorder;

public static class WindowCapture
{
    public static byte[] CaptureScreenRect(Rectangle rect)
    {
        using var bmp = new Bitmap(Math.Max(1, rect.Width), Math.Max(1, rect.Height), PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(bmp))
        {
            g.CopyFromScreen(rect.Left, rect.Top, 0, 0, bmp.Size, CopyPixelOperation.SourceCopy);
        }

        using var ms = new MemoryStream();
        bmp.Save(ms, ImageFormat.Png);
        return ms.ToArray();
    }

    public static Rectangle Normalize(Rectangle r)
    {
        int x1 = Math.Min(r.Left, r.Right);
        int x2 = Math.Max(r.Left, r.Right);
        int y1 = Math.Min(r.Top, r.Bottom);
        int y2 = Math.Max(r.Top, r.Bottom);
        return Rectangle.FromLTRB(x1, y1, x2, y2);
    }

    public static Rectangle InflateSafe(Rectangle r, int px)
    {
        var rr = Rectangle.Inflate(r, px, px);
        return rr.Width <= 0 || rr.Height <= 0 ? r : rr;
    }
}

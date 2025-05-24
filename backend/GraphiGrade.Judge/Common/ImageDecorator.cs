using System.Drawing;
using System.Drawing.Imaging;

namespace GraphiGrade.Judge.Common;

public class ImageDecorator
{
    private readonly Bitmap _bitmap;

    public ImageDecorator(int width, int height, PixelFormat pixelFormat)
    {
        _bitmap = new Bitmap(width, height, pixelFormat);
    }

    public ImageDecorator(int width, int height)
    {
        _bitmap = new Bitmap(width, height);
    }

    public ImageDecorator(Stream stream)
    {
        _bitmap = new Bitmap(stream);
    }

    public int Height => _bitmap.Height;

    public int Width => _bitmap.Width;

    public Color GetPixel(int x, int y)
    {
        return _bitmap.GetPixel(x, y);
    }

    public void SetPixel(int x, int y, Color pixel)
    {
        _bitmap.SetPixel(x, y, pixel);
    }

    public Graphics GraphicsFromImage()
    {
        return Graphics.FromImage(_bitmap);
    }

    public static ImageDecorator FromBase64(string base64Image)
    {
        var imageBytes = Convert.FromBase64String(base64Image);
        using (var ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
        {
            return new(ms);
        }
    }

    public static bool IsValidImageFromBase64(string base64Image)
    {
        try
        {
            FromBase64(base64Image); // This will throw exception if image is invalid.
        }
        catch
        {
            return false;
        }

        return true;
    }

    public string ToBase64()
    {
        using (var memoryStream = new MemoryStream())
        {
            _bitmap.Save(memoryStream, ImageFormat.Png);
            byte[] imageBytes = memoryStream.ToArray();
            return Convert.ToBase64String(imageBytes);
        }
    }
}

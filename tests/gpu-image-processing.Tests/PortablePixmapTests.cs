using System.Text;
using GpuImageProcessing.Domain;
using GpuImageProcessing.Imaging;
using Xunit;

namespace GpuImageProcessing.Tests;

public class PortablePixmapMemoryTests
{
    [Fact]
    public void P6_RoundTrip_PreservesRgbBytes()
    {
        var pixels = new byte[]
        {
            255, 0, 0,
            0, 255, 0,
            0, 0, 255,
            17, 34, 51
        };
        var image = CreateImage(2, 2, 3, pixels);

        var decoded = EncodeAndDecode(image);

        Assert.Equal(2, decoded.Width);
        Assert.Equal(2, decoded.Height);
        Assert.Equal(3, decoded.Channels);
        Assert.Equal(24, decoded.BitsPerPixel);
        Assert.Equal(pixels, decoded.PixelData);
    }

    [Fact]
    public void P5_RoundTrip_PreservesGrayscaleBytes()
    {
        var pixels = new byte[] { 0, 63, 127, 255, 19, 201 };
        var image = CreateImage(3, 2, 1, pixels);

        var decoded = EncodeAndDecode(image);

        Assert.Equal(3, decoded.Width);
        Assert.Equal(2, decoded.Height);
        Assert.Equal(1, decoded.Channels);
        Assert.Equal(8, decoded.BitsPerPixel);
        Assert.Equal(pixels, decoded.PixelData);
    }

    [Theory]
    [InlineData("image.ppm")]
    [InlineData("image.PPM")]
    [InlineData("image.pgm")]
    [InlineData("image.PGM")]
    public void IsSupported_PortablePixmapExtension_ReturnsTrue(string path)
    {
        Assert.True(PortablePixmap.IsSupported(path));
    }

    [Theory]
    [InlineData("image.png")]
    [InlineData("image.jpg")]
    [InlineData("image")]
    public void IsSupported_OtherExtension_ReturnsFalse(string path)
    {
        Assert.False(PortablePixmap.IsSupported(path));
    }

    [Fact]
    public void PixelHash_IdenticalPixels_IsStableAndPixelChangeDiffers()
    {
        var first = CreateImage(1, 1, 3, new byte[] { 10, 20, 30 });
        var identical = CreateImage(1, 1, 3, new byte[] { 10, 20, 30 });
        var changed = CreateImage(1, 1, 3, new byte[] { 10, 20, 31 });

        var firstHash = PortablePixmap.PixelHash(first);

        Assert.Equal(firstHash, PortablePixmap.PixelHash(first));
        Assert.Equal(firstHash, PortablePixmap.PixelHash(identical));
        Assert.NotEqual(firstHash, PortablePixmap.PixelHash(changed));
    }

    [Fact]
    public void Decode_UnsupportedMagic_ThrowsNotSupportedException()
    {
        using var stream = PixmapStream("P7\n1 1\n255\n");

        Assert.Throws<NotSupportedException>(() => PortablePixmap.Decode(stream));
    }

    [Theory]
    [InlineData("P6\n-1 1\n255\n")]
    [InlineData("P6\n1 -1\n255\n")]
    public void Decode_NegativeDimension_ThrowsOverflowException(string header)
    {
        using var stream = PixmapStream(header);

        Assert.Throws<OverflowException>(() => PortablePixmap.Decode(stream));
    }

    private static Image EncodeAndDecode(Image image)
    {
        using var stream = new MemoryStream();
        PortablePixmapCodec.Instance.Write(image, stream);
        stream.Position = 0;
        return PortablePixmap.Decode(stream);
    }

    private static Image CreateImage(int width, int height, int channels, byte[] pixels) => new()
    {
        Width = width,
        Height = height,
        Channels = channels,
        BitsPerPixel = channels * 8,
        PixelData = pixels
    };

    private static MemoryStream PixmapStream(string contents) =>
        new(Encoding.ASCII.GetBytes(contents));
}

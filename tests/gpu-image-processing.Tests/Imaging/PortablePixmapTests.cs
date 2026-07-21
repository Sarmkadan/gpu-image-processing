#nullable enable
using System;
using System.IO;
using System.Text;
using System.Linq;
using Xunit;
using GpuImageProcessing.Imaging;
using GpuImageProcessing.Domain;

namespace GpuImageProcessing.Tests.Imaging
{
    public class PortablePixmapTests
    {
        [Fact]
        public void P6_RoundTrip_Succeeds()
        {
            // Arrange: create a simple 2x2 RGB image
            var image = new Image
            {
                Width = 2,
                Height = 2,
                Channels = 3,
                BitsPerPixel = 24,
                PixelData = new byte[]
                {
                    // pixel 0
                    255, 0, 0,   // red
                    // pixel 1
                    0, 255, 0,   // green
                    // pixel 2
                    0, 0, 255,   // blue
                    // pixel 3
                    255, 255, 255 // white
                }
            };

            var tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".ppm");
            try
            {
                // Act: save and load
                PortablePixmap.Save(image, tempFile, PortablePixmap.PpmFormat.P6);
                var loaded = PortablePixmap.Load(tempFile);

                // Assert: properties match
                Assert.Equal(image.Width, loaded.Width);
                Assert.Equal(image.Height, loaded.Height);
                Assert.Equal(image.Channels, loaded.Channels);
                Assert.Equal(image.BitsPerPixel, loaded.BitsPerPixel);
                Assert.Equal(image.PixelData, loaded.PixelData);
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [Fact]
        public void Decode_TruncatedHeader_ThrowsEndOfStreamException()
        {
            // Arrange: header missing height and maxVal
            var header = Encoding.ASCII.GetBytes("P6\n1");
            using var stream = new MemoryStream(header);

            // Act & Assert
            Assert.Throws<EndOfStreamException>(() => PortablePixmap.Decode(stream));
        }

        [Fact]
        public void Decode_WrongMagicNumber_ThrowsNotSupportedException()
        {
            // Arrange: unsupported magic "P7"
            var header = Encoding.ASCII.GetBytes("P7\n1 1\n255\n");
            using var stream = new MemoryStream(header);

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => PortablePixmap.Decode(stream));
        }

        [Fact]
        public void Decode_ZeroDimensions_ReturnsImageWithZeroSize()
        {
            // Arrange: width and height zero
            var header = Encoding.ASCII.GetBytes("P6\n0 0\n255\n");
            using var stream = new MemoryStream(header);

            // Act
            var image = PortablePixmap.Decode(stream);

            // Assert
            Assert.Equal(0, image.Width);
            Assert.Equal(0, image.Height);
            Assert.Equal(0, image.PixelData.Length);
        }

        [Fact]
        public void Decode_MaxValueNot255_ThrowsNotSupportedException()
        {
            // Arrange: maxVal 128
            var header = Encoding.ASCII.GetBytes("P6\n1 1\n128\n");
            using var stream = new MemoryStream(header);

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => PortablePixmap.Decode(stream));
        }
    }
}

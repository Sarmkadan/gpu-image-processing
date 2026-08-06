using System;
using Xunit;
using GpuImageProcessing.Repository;

namespace GpuImageProcessing.Tests
{
    public class ImageRepositoryJsonExtensionsTests
    {
        [Fact]
        public void ToJson_NullRepository_ThrowsArgumentNullException()
        {
            ImageRepository? repository = null;
            Assert.Throws<ArgumentNullException>(() => ImageRepositoryJsonExtensions.ToJson(repository!));
        }

        [Fact]
        public void ToJson_ValidRepository_ReturnsJsonString()
        {
            var repository = new ImageRepository();
            string json = ImageRepositoryJsonExtensions.ToJson(repository);
            Assert.False(string.IsNullOrWhiteSpace(json));
        }

        [Fact]
        public void ToJson_ValidRepository_Indented_ReturnsJsonString()
        {
            var repository = new ImageRepository();
            string json = ImageRepositoryJsonExtensions.ToJson(repository, indented: true);
            Assert.False(string.IsNullOrWhiteSpace(json));
        }

        [Fact]
        public void FromJson_NullJson_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => ImageRepositoryJsonExtensions.FromJson(null!));
        }

        [Fact]
        public void FromJson_EmptyOrWhitespaceJson_ReturnsNull()
        {
            Assert.Null(ImageRepositoryJsonExtensions.FromJson(string.Empty));
            Assert.Null(ImageRepositoryJsonExtensions.FromJson("   "));
            Assert.Null(ImageRepositoryJsonExtensions.FromJson("\t\n\r"));
        }

        [Fact]
        public void FromJson_ValidJson_ReturnsRepository()
        {
            var repository = new ImageRepository();
            string json = ImageRepositoryJsonExtensions.ToJson(repository);
            var result = ImageRepositoryJsonExtensions.FromJson(json);
            Assert.NotNull(result);
        }

        [Fact]
        public void FromJson_InvalidJson_ReturnsNull()
        {
            const string invalidJson = "{ this is not valid json }";
            Assert.Null(ImageRepositoryJsonExtensions.FromJson(invalidJson));
        }

        [Fact]
        public void TryFromJson_NullJson_ReturnsFalse()
        {
            ImageRepository? repository = null;
            bool result = ImageRepositoryJsonExtensions.TryFromJson(null!, out repository);
            Assert.False(result);
            Assert.Null(repository);
        }

        [Fact]
        public void TryFromJson_EmptyOrWhitespaceJson_ReturnsFalse()
        {
            ImageRepository? repository = null;
            bool result = ImageRepositoryJsonExtensions.TryFromJson(string.Empty, out repository);
            Assert.False(result);
            Assert.Null(repository);

            repository = null;
            result = ImageRepositoryJsonExtensions.TryFromJson("   ", out repository);
            Assert.False(result);
            Assert.Null(repository);
        }

        [Fact]
        public void TryFromJson_ValidJson_ReturnsTrueAndRepository()
        {
            var repository = new ImageRepository();
            string json = ImageRepositoryJsonExtensions.ToJson(repository);
            ImageRepository? result = null;
            bool success = ImageRepositoryJsonExtensions.TryFromJson(json, out result);
            Assert.True(success);
            Assert.NotNull(result);
        }

        [Fact]
        public void TryFromJson_InvalidJson_ReturnsFalseAndNull()
        {
            const string invalidJson = "{ this is not valid json }";
            ImageRepository? result = null;
            bool success = ImageRepositoryJsonExtensions.TryFromJson(invalidJson, out result);
            Assert.False(success);
            Assert.Null(result);
        }
    }
}
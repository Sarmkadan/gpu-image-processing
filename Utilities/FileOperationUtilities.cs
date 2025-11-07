#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GpuImageProcessing.Utilities
{
    /// <summary>
    /// Utilities for file operations including hashing, validation, and safe handling.
    /// Provides checksums, integrity checks, and atomic file operations.
    /// </summary>
    public class FileOperationUtilities
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileOperationUtilities"/> class.
        /// </summary>
        public FileOperationUtilities()
        {
        }

        /// <summary>
        /// Calculates SHA256 hash of a file
        /// </summary>
        public async Task<string> CalculateFileHashAsync(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not found: {filePath}");

            using (var sha256 = SHA256.Create())
            using (var fileStream = File.OpenRead(filePath))
            {
                var hash = await Task.Run(() => sha256.ComputeHash(fileStream));
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }

        /// <summary>
        /// Verifies file integrity by comparing hashes
        /// </summary>
        public async Task<bool> VerifyFileHashAsync(string filePath, string expectedHash)
        {
            var actualHash = await CalculateFileHashAsync(filePath);
            return actualHash.Equals(expectedHash, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Safely copies a file with verification
        /// </summary>
        public async Task<bool> SafeCopyFileAsync(
            string sourceFile,
            string destinationFile,
            bool overwrite = true,
            bool verifyHash = true)
        {
            if (!File.Exists(sourceFile))
                throw new FileNotFoundException($"Source file not found: {sourceFile}");

            if (File.Exists(destinationFile) && !overwrite)
                return false;

            try
            {
                string sourceHash = null;
                if (verifyHash)
                    sourceHash = await CalculateFileHashAsync(sourceFile);

                File.Copy(sourceFile, destinationFile, overwrite);

                if (verifyHash)
                {
                    var destHash = await CalculateFileHashAsync(destinationFile);
                    return destHash == sourceHash;
                }

                return true;
            }
            catch
            {
                // Clean up partially copied file
                if (File.Exists(destinationFile))
                    File.Delete(destinationFile);

                return false;
            }
        }

        /// <summary>
        /// Gets file metadata in a structured format
        /// </summary>
        public FileMetadata GetFileMetadata(string filePath)
        {
            if (!File.Exists(filePath))
                return null;

            var fileInfo = new FileInfo(filePath);

            return new FileMetadata
            {
                Name = fileInfo.Name,
                FullPath = fileInfo.FullName,
                SizeBytes = fileInfo.Length,
                SizeFormatted = DataConversionUtilities.FormatFileSize(fileInfo.Length),
                CreatedAt = fileInfo.CreationTimeUtc,
                ModifiedAt = fileInfo.LastWriteTimeUtc,
                Extension = fileInfo.Extension,
                IsReadOnly = fileInfo.IsReadOnly
            };
        }

        /// <summary>
        /// Safely deletes a file with optional secure wiping
        /// </summary>
        public async Task SafeDeleteFileAsync(string filePath, bool secureWipe = false)
        {
            if (!File.Exists(filePath))
                return;

            if (secureWipe)
            {
                await SecureWipeFileAsync(filePath);
            }

            File.Delete(filePath);
        }

        /// <summary>
        /// Securely wipes file contents before deletion
        /// </summary>
        private async Task SecureWipeFileAsync(string filePath)
        {
            const int bufferSize = 4096;
            var buffer = new byte[bufferSize];

            using (var fileStream = File.Open(filePath, FileMode.Open, FileAccess.Write))
            {
                for (long i = 0; i < fileStream.Length; i += bufferSize)
                {
                    await fileStream.WriteAsync(buffer, 0, bufferSize);
                }
            }
        }

        /// <summary>
        /// Creates a directory if it doesn't exist
        /// </summary>
        public DirectoryInfo EnsureDirectoryExists(string dirPath)
        {
            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);

            return new DirectoryInfo(dirPath);
        }

        /// <summary>
        /// Validates file path is safe (no path traversal attacks)
        /// </summary>
        public bool IsValidFilePath(string filePath, string baseDirectory)
        {
            try
            {
                var fullPath = Path.GetFullPath(filePath);
                var baseFullPath = Path.GetFullPath(baseDirectory);

                return fullPath.StartsWith(baseFullPath);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a unique file name by appending numbers if file exists
        /// </summary>
        public string GetUniqueFileName(string filePath)
        {
            if (!File.Exists(filePath))
                return filePath;

            var directory = Path.GetDirectoryName(filePath);
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var extension = Path.GetExtension(filePath);
            var counter = 1;

            while (File.Exists(filePath))
            {
                filePath = Path.Combine(directory, $"{fileName}_{counter}{extension}");
                counter++;
            }

            return filePath;
        }

        /// <summary>
        /// Reads file content asynchronously with encoding detection
        /// </summary>
        public async Task<string> ReadFileAsync(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not found: {filePath}");

            using (var reader = new StreamReader(filePath, Encoding.UTF8, detectEncodingFromByteOrderMarks: true))
            {
                return await reader.ReadToEndAsync();
            }
        }

        /// <summary>
        /// Writes content to file atomically
        /// </summary>
        public async Task WriteFileAtomicAsync(string filePath, string content)
        {
            var tempPath = filePath + ".tmp";

            try
            {
                using (var writer = new StreamWriter(tempPath, false, Encoding.UTF8))
                {
                    await writer.WriteAsync(content);
                }

                // Atomic rename
                if (File.Exists(filePath))
                    File.Delete(filePath);

                File.Move(tempPath, filePath);
            }
            catch
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);

                throw;
            }
        }
    }

    public class FileMetadata
    {
        public string Name { get; set; }
        public string FullPath { get; set; }
        public long SizeBytes { get; set; }
        public string SizeFormatted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public string Extension { get; set; }
        public bool IsReadOnly { get; set; }
    }
}

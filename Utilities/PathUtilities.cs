// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GpuImageProcessing.Utilities
{
    /// <summary>
    /// Utility methods for file path operations, normalization, and directory management.
    /// Provides cross-platform path handling and safe file operations.
    /// </summary>
    public static class PathUtilities
    {
        /// <summary>
        /// Normalizes a file path to use platform-specific separators.
        /// </summary>
        public static string NormalizePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return null;

            path = Path.GetFullPath(path); // Resolve to absolute path
            return path;
        }

        /// <summary>
        /// Gets the relative path from one directory to another.
        /// </summary>
        public static string GetRelativePath(string fromPath, string toPath)
        {
            try
            {
                Uri fromUri = new Uri(NormalizePath(fromPath));
                Uri toUri = new Uri(NormalizePath(toPath));
                return Uri.UnescapeDataString(fromUri.MakeRelativeUri(toUri).ToString());
            }
            catch
            {
                return toPath;
            }
        }

        /// <summary>
        /// Combines multiple path segments safely.
        /// </summary>
        public static string CombinePaths(params string[] segments)
        {
            if (segments == null || segments.Length == 0)
                return null;

            try
            {
                return Path.Combine(segments);
            }
            catch
            {
                return segments[0];
            }
        }

        /// <summary>
        /// Gets absolute path from potentially relative path.
        /// </summary>
        public static string GetAbsolutePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return null;

            try
            {
                return Path.GetFullPath(path);
            }
            catch
            {
                return path;
            }
        }

        /// <summary>
        /// Ensures a directory exists, creating it if necessary.
        /// Returns true if directory exists or was created successfully.
        /// </summary>
        public static bool EnsureDirectoryExists(string directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath))
                return false;

            try
            {
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Safely deletes a directory and all its contents.
        /// </summary>
        public static bool SafeDeleteDirectory(string directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath) || !Directory.Exists(directoryPath))
                return false;

            try
            {
                Directory.Delete(directoryPath, recursive: true);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Clears all files in a directory without deleting the directory itself.
        /// </summary>
        public static bool ClearDirectory(string directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath) || !Directory.Exists(directoryPath))
                return false;

            try
            {
                var directory = new DirectoryInfo(directoryPath);

                foreach (FileInfo file in directory.GetFiles())
                {
                    file.Delete();
                }

                foreach (DirectoryInfo subDir in directory.GetDirectories())
                {
                    subDir.Delete(recursive: true);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the size of a directory and all its contents in bytes.
        /// </summary>
        public static long GetDirectorySize(string directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath) || !Directory.Exists(directoryPath))
                return 0;

            try
            {
                var directory = new DirectoryInfo(directoryPath);
                long size = 0;

                foreach (var file in directory.GetFiles())
                {
                    size += file.Length;
                }

                foreach (var subDir in directory.GetDirectories())
                {
                    size += GetDirectorySize(subDir.FullPath);
                }

                return size;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Counts total files in a directory recursively.
        /// </summary>
        public static int CountFiles(string directoryPath, string searchPattern = "*.*")
        {
            if (string.IsNullOrWhiteSpace(directoryPath) || !Directory.Exists(directoryPath))
                return 0;

            try
            {
                var directory = new DirectoryInfo(directoryPath);
                int count = directory.GetFiles(searchPattern).Length;

                foreach (var subDir in directory.GetDirectories())
                {
                    count += CountFiles(subDir.FullPath, searchPattern);
                }

                return count;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Gets all files in directory matching pattern recursively.
        /// </summary>
        public static List<string> GetFilesRecursive(string directoryPath, string searchPattern = "*.*")
        {
            var files = new List<string>();

            if (string.IsNullOrWhiteSpace(directoryPath) || !Directory.Exists(directoryPath))
                return files;

            try
            {
                var directory = new DirectoryInfo(directoryPath);

                foreach (var file in directory.GetFiles(searchPattern))
                {
                    files.Add(file.FullPath);
                }

                foreach (var subDir in directory.GetDirectories())
                {
                    files.AddRange(GetFilesRecursive(subDir.FullPath, searchPattern));
                }
            }
            catch
            {
            }

            return files;
        }

        /// <summary>
        /// Generates a unique filename if file already exists.
        /// Appends number suffix before extension.
        /// </summary>
        public static string GenerateUniqueFilename(string filePath)
        {
            if (!File.Exists(filePath))
                return filePath;

            string directory = Path.GetDirectoryName(filePath);
            string nameWithoutExt = Path.GetFileNameWithoutExtension(filePath);
            string extension = Path.GetExtension(filePath);

            int counter = 1;
            while (true)
            {
                string newFilename = $"{nameWithoutExt}_{counter}{extension}";
                string newPath = Path.Combine(directory, newFilename);

                if (!File.Exists(newPath))
                    return newPath;

                counter++;

                if (counter > 10000) // Safety limit
                    return filePath;
            }
        }

        /// <summary>
        /// Gets recently modified files in directory.
        /// </summary>
        public static List<string> GetRecentFiles(string directoryPath, TimeSpan withinTimespan, string pattern = "*.*")
        {
            var files = new List<string>();

            if (string.IsNullOrWhiteSpace(directoryPath) || !Directory.Exists(directoryPath))
                return files;

            try
            {
                var cutoffTime = DateTime.Now - withinTimespan;
                var directory = new DirectoryInfo(directoryPath);

                files.AddRange(
                    directory.GetFiles(pattern)
                    .Where(f => f.LastWriteTime > cutoffTime)
                    .Select(f => f.FullPath)
                );

                foreach (var subDir in directory.GetDirectories())
                {
                    files.AddRange(GetRecentFiles(subDir.FullPath, withinTimespan, pattern));
                }
            }
            catch
            {
            }

            return files;
        }

        /// <summary>
        /// Safely moves a file with overwrite handling.
        /// </summary>
        public static bool SafeMoveFile(string sourcePath, string destinationPath, bool overwrite = false)
        {
            if (string.IsNullOrWhiteSpace(sourcePath) || string.IsNullOrWhiteSpace(destinationPath))
                return false;

            if (!File.Exists(sourcePath))
                return false;

            try
            {
                if (File.Exists(destinationPath))
                {
                    if (!overwrite)
                        return false;

                    File.Delete(destinationPath);
                }

                File.Move(sourcePath, destinationPath);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Safely copies a file with error handling.
        /// </summary>
        public static bool SafeCopyFile(string sourcePath, string destinationPath, bool overwrite = false)
        {
            if (string.IsNullOrWhiteSpace(sourcePath) || string.IsNullOrWhiteSpace(destinationPath))
                return false;

            if (!File.Exists(sourcePath))
                return false;

            try
            {
                File.Copy(sourcePath, destinationPath, overwrite);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets path size information formatted for display.
        /// </summary>
        public static string GetPathSizeInfo(string path)
        {
            if (File.Exists(path))
            {
                long size = new FileInfo(path).Length;
                return ImageUtilities.FormatFileSize(size);
            }
            else if (Directory.Exists(path))
            {
                long size = GetDirectorySize(path);
                return ImageUtilities.FormatFileSize(size);
            }

            return "N/A";
        }
    }
}

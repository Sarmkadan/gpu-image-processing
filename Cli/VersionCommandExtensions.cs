#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace GpuImageProcessing.Cli
{
    /// <summary>
    /// Extension methods for <see cref="VersionCommand"/>.
    /// </summary>
    public static class VersionCommandExtensions
    {
        /// <summary>
        /// Gets the application version string in the form <c>major.minor.build.revision</c>.
        /// </summary>
        /// <param name="command">The <see cref="VersionCommand"/> instance.</param>
        /// <returns>A version string.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="command"/> is <c>null</c>.</exception>
        public static string GetVersionString(this VersionCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            var version = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 0, 0);
            return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }

        /// <summary>
        /// Returns the list of command‑line options supported by the <c>version</c> command.
        /// </summary>
        /// <param name="command">The <see cref="VersionCommand"/> instance.</param>
        /// <returns>An immutable list of option names.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="command"/> is <c>null</c>.</exception>
        public static IReadOnlyList<string> GetSupportedOptions(this VersionCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            return new[] { "--full", "--check", "--json" };
        }

        /// <summary>
        /// Serialises version information to a JSON string.
        /// </summary>
        /// <param name="command">The <see cref="VersionCommand"/> instance.</param>
        /// <returns>A JSON representation of the version data.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="command"/> is <c>null</c>.</exception>
        public static string ToJson(this VersionCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            var version = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 0, 0);
            var fileInfo = new FileInfo(Assembly.GetExecutingAssembly().Location);
            var buildDate = fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

            var info = new
            {
                Name = "GPU Image Processing System",
                Version = $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}",
                BuildDate = buildDate,
                TargetFramework = ".NET 10.0",
                RuntimeVersion = Environment.Version.ToString(),
                OS = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Windows"
                     : RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "Linux"
                     : RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "macOS"
                     : "Unknown",
                Architecture = RuntimeInformation.ProcessArchitecture.ToString(),
                Debug = IsDebugBuild()
            };

            return JsonSerializer.Serialize(info, new JsonSerializerOptions { WriteIndented = true });
        }

        private static bool IsDebugBuild()
        {
#if DEBUG
            return true;
#else
            return false;
#endif
        }
    }
}

#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
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

            var targetFramework = GetTargetFramework();

            var osPlatform = GetOSPlatform();

            var info = new
            {
                Name = "GPU Image Processing System",
                Version = $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}",
                BuildDate = buildDate,
                TargetFramework = targetFramework,
                RuntimeVersion = Environment.Version.ToString(),
                OS = osPlatform,
                Architecture = RuntimeInformation.ProcessArchitecture.ToString(),
                Debug = IsDebugBuild()
            };

            return JsonSerializer.Serialize(info, new JsonSerializerOptions { WriteIndented = true });
        }

        /// <summary>
        /// Gets the target framework moniker from assembly metadata.
        /// </summary>
        /// <returns>The target framework identifier.</returns>
        private static string GetTargetFramework()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var targetFrameworkAttribute = assembly.GetCustomAttribute<TargetFrameworkAttribute>();
            return targetFrameworkAttribute?.FrameworkName ?? ".NET";
        }

        /// <summary>
        /// Determines the current operating system platform.
        /// </summary>
        /// <returns>The OS platform name.</returns>
        private static string GetOSPlatform()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return "Windows";
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return "Linux";
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return "macOS";
            }

            return "Unknown";
        }

        /// <summary>
        /// Checks if the application was compiled in Debug mode.
        /// </summary>
        /// <returns><c>true</c> if in Debug mode; otherwise, <c>false</c>.</returns>
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
// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Support
{
    using Microsoft.Extensions.FileSystemGlobbing;
    using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Represents a filter for files and directories using glob patterns.
    /// </summary>
    /// <remarks>
    /// The filter supports the following glob patterns:
    /// <list type="bullet">
    ///   <item><description>Use '..' to represent a parent directory.</description></item>
    ///   <item><description>Use '**' to represent arbitrary directory depth.</description></item>
    ///   <item><description>Use '*' to represent wildcards in file and directory names.</description></item>
    ///   <item><description>Use '!' in the beginning of a pattern to exclude files that match the pattern.</description></item>
    ///   <item><description>Use the forward slash '/' to represent directory separator.</description></item>
    /// </list>
    /// The matching is case-insensitive.
    /// </remarks>
    /// <seealso href="https://learn.microsoft.com/en-us/dotnet/core/extensions/file-globbing"/>
    public class FileGlobFilter : List<string>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileGlobFilter"/> class.
        /// </summary>
        public FileGlobFilter()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileGlobFilter"/> class with the specified patterns.
        /// </summary>
        /// <param name="patterns">The glob patterns to use for filtering files and directories.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="patterns"/> is <see langword="null"/>.</exception>
        public FileGlobFilter(IEnumerable<string> patterns)
            : base(patterns ?? throw new ArgumentNullException(nameof(patterns)))
        {
        }

        /// <summary>
        /// Searches for files in the specified directory that match the filter.
        /// </summary>
        /// <param name="directory">The directory to search for files.</param>
        /// <param name="defaultExtension">The file extension to use when a pattern does not specify an extension.</param>
        /// <returns>An enumerable collection of full file paths that match the filter.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="directory"/> is <see langword="null"/>.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown when the specified directory does not exist.</exception>
        public IEnumerable<string> FindMatchingFiles(string directory, string? defaultExtension = null)
        {
            ArgumentNullException.ThrowIfNull(directory);

            if (!Directory.Exists(directory))
                throw new DirectoryNotFoundException($"Directory '{directory}' does not exist.");

            var matcher = BuildMatcher(defaultExtension);
            var searchDirectory = new DirectoryInfoWrapper(new DirectoryInfo(directory));
            return matcher.Execute(searchDirectory).Files.Select(match => Path.GetFullPath(Path.Combine(directory, match.Path)));
        }

        /// <summary>
        /// Creates a <see cref="Matcher"/> instance based on the patterns in the filter.
        /// </summary>
        /// <param name="defaultExtension">The file extension to use when a pattern does not specify an extension.</param>
        /// <returns>A <see cref="Matcher"/> instance based on the patterns in the filter.</returns>
        protected Matcher BuildMatcher(string? defaultExtension = null)
        {
            var matcher = new Matcher();
            foreach (var pattern in this)
            {
                if (string.IsNullOrEmpty(pattern))
                    continue;

                if (pattern.StartsWith('!'))
                    matcher.AddExclude(AddExtensionIfMissing(pattern[1..], defaultExtension));
                else
                    matcher.AddInclude(AddExtensionIfMissing(pattern, defaultExtension));
            }
            return matcher;
        }

        /// <summary>
        /// Appends the specified file extension to the pattern if it does not already have an extension.
        /// </summary>
        /// <param name="pattern">The pattern to process.</param>
        /// <param name="extension">The file extension to append to the pattern.</param>
        /// <returns>The pattern with the specified file extension correctly applied.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="pattern"/> is <see langword="null"/>.</exception>
        public static string AddExtensionIfMissing(string pattern, string? extension)
        {
            ArgumentNullException.ThrowIfNull(pattern);

            if (string.IsNullOrEmpty(extension))
                return pattern;

            if (pattern.EndsWith('/'))
                return pattern + "*" + EnsureLeadingDot(extension);

            if (pattern.EndsWith("**"))
                return pattern + "/*" + EnsureLeadingDot(extension);

            if (pattern.IndexOf('.', pattern.LastIndexOf('/') + 1) == -1)
                return pattern + EnsureLeadingDot(extension);

            return pattern;

            static string EnsureLeadingDot(string extension) => extension.StartsWith('.') ? extension : '.' + extension;
        }
    }
}

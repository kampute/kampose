// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Outputs
{
    using Kampute.DocToolkit.Support;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;

    /// <summary>
    /// Represents the set of files produced in the documentation output directory during a single run.
    /// </summary>
    /// <remarks>
    /// Paths are stored relative to <see cref="OutputDirectory"/> with forward slashes as separators and compared case-sensitively,
    /// as URL paths are, regardless of the case sensitivity of the local file system. The set is safe for concurrent use.
    /// </remarks>
    public sealed class GeneratedFileSet
    {
        private readonly ConcurrentDictionary<string, byte> paths = new(StringComparer.Ordinal);
        private readonly ConcurrentDictionary<string, IReadOnlySet<string>> anchors = new(StringComparer.Ordinal);

        /// <summary>
        /// Initializes a new instance of the <see cref="GeneratedFileSet"/> class.
        /// </summary>
        /// <param name="outputDirectory">The documentation output directory.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="outputDirectory"/> is <see langword="null"/> or empty.</exception>
        public GeneratedFileSet(string outputDirectory)
        {
            ArgumentException.ThrowIfNullOrEmpty(outputDirectory);

            OutputDirectory = Path.GetFullPath(outputDirectory);
        }

        /// <summary>
        /// Gets the full path of the documentation output directory.
        /// </summary>
        /// <value>
        /// The full path of the documentation output directory.
        /// </value>
        public string OutputDirectory { get; }

        /// <summary>
        /// Gets the number of files in the set.
        /// </summary>
        /// <value>
        /// The number of distinct files produced so far.
        /// </value>
        public int Count => paths.Count;

        /// <summary>
        /// Adds a produced file to the set.
        /// </summary>
        /// <param name="path">The path of the produced file, either absolute or relative to <see cref="OutputDirectory"/>.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="path"/> is <see langword="null"/>, empty, or outside <see cref="OutputDirectory"/>.</exception>
        public void Add(string path)
        {
            ArgumentException.ThrowIfNullOrEmpty(path);

            var key = GetKey(path)
                ?? throw new ArgumentException($"The path '{path}' is outside the output directory '{OutputDirectory}'.", nameof(path));

            paths.TryAdd(key, 0);
        }

        /// <summary>
        /// Determines whether the specified file was produced.
        /// </summary>
        /// <param name="path">The path of the file, either absolute or relative to <see cref="OutputDirectory"/>.</param>
        /// <returns><see langword="true"/> if the file was produced; otherwise, <see langword="false"/>.</returns>
        public bool Contains(string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            var key = GetKey(path);
            return key is not null && paths.ContainsKey(key);
        }

        /// <summary>
        /// Records the anchors that a produced file defines.
        /// </summary>
        /// <param name="path">The path of the produced file, either absolute or relative to <see cref="OutputDirectory"/>.</param>
        /// <param name="anchorNames">The names that a URL fragment can use to refer to a location in the file. Names are compared case-sensitively.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="path"/> is <see langword="null"/>, empty, or outside <see cref="OutputDirectory"/>.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="anchorNames"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// Anchors are recorded only for files whose anchors can be determined from their content, such as HTML pages. Recording the
        /// anchors of a file again replaces the anchors recorded before.
        /// </remarks>
        public void SetAnchors(string path, IEnumerable<string> anchorNames)
        {
            ArgumentException.ThrowIfNullOrEmpty(path);
            ArgumentNullException.ThrowIfNull(anchorNames);

            var key = GetKey(path)
                ?? throw new ArgumentException($"The path '{path}' is outside the output directory '{OutputDirectory}'.", nameof(path));

            anchors[key] = new HashSet<string>(anchorNames, StringComparer.Ordinal);
        }

        /// <summary>
        /// Attempts to get the anchors that a produced file defines.
        /// </summary>
        /// <param name="path">The path of the file, either absolute or relative to <see cref="OutputDirectory"/>.</param>
        /// <param name="anchorNames">When this method returns, contains the anchors recorded for the file, if any; otherwise, <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if anchors were recorded for the file; otherwise, <see langword="false"/>.</returns>
        public bool TryGetAnchors(string path, [NotNullWhen(true)] out IReadOnlySet<string>? anchorNames)
        {
            anchorNames = null;
            if (string.IsNullOrEmpty(path))
                return false;

            var key = GetKey(path);
            return key is not null && anchors.TryGetValue(key, out anchorNames);
        }

        /// <summary>
        /// Converts the specified path to the key under which it is stored.
        /// </summary>
        /// <param name="path">The path, either absolute or relative to <see cref="OutputDirectory"/>.</param>
        /// <returns>The path relative to <see cref="OutputDirectory"/> with forward slashes, or <see langword="null"/> if the path is outside it.</returns>
        private string? GetKey(string path)
        {
            var fullPath = Path.GetFullPath(Path.Combine(OutputDirectory, path));
            var relativePath = Path.GetRelativePath(OutputDirectory, fullPath);

            if (Path.IsPathRooted(relativePath) || PathHelper.StartsWithDotSegment(relativePath))
                return null;

            return relativePath.Replace('\\', '/');
        }
    }
}

// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Outputs
{
    using Kampose.Reporters;
    using System.IO;

    /// <summary>
    /// Writes the documentation files to the output directory on the file system.
    /// </summary>
    public sealed class FileSystemDocumentationOutput : DocumentationOutput
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileSystemDocumentationOutput"/> class.
        /// </summary>
        /// <param name="outputDirectory">The documentation output directory.</param>
        /// <param name="clean">Indicates whether the output directory is emptied before any file is written.</param>
        /// <exception cref="System.ArgumentException">Thrown when <paramref name="outputDirectory"/> is <see langword="null"/> or empty.</exception>
        public FileSystemDocumentationOutput(string outputDirectory, bool clean = false)
            : base(outputDirectory)
        {
            Clean = clean;
        }

        /// <summary>
        /// Gets a value indicating whether the output directory is emptied before any file is written.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if existing files and subdirectories of the output directory are deleted by <see cref="Prepare"/>;
        /// otherwise, <see langword="false"/>.
        /// </value>
        public bool Clean { get; }

        /// <inheritdoc />
        /// <remarks>
        /// When <see cref="Clean"/> is <see langword="true"/>, deletes all files and subdirectories within the output directory.
        /// </remarks>
        public override void Prepare(IActivityReporter reporter)
        {
            if (!Clean || !Directory.Exists(OutputDirectory))
                return;

            using var _ = reporter.BeginActivity("Cleaning output directory");
            foreach (var dir in Directory.EnumerateDirectories(OutputDirectory))
                Directory.Delete(dir, true);
            foreach (var file in Directory.EnumerateFiles(OutputDirectory))
                File.Delete(file);
        }

        /// <inheritdoc />
        protected override TextWriter CreatePageWriter(string targetPath)
        {
            EnsureDirectory(targetPath);
            return File.CreateText(targetPath);
        }

        /// <inheritdoc />
        protected override void WriteFileCore(string targetPath, string content)
        {
            EnsureDirectory(targetPath);
            File.WriteAllText(targetPath, content);
        }

        /// <inheritdoc />
        protected override void CopyFileCore(string sourcePath, string targetPath)
        {
            EnsureDirectory(targetPath);
            File.Copy(sourcePath, targetPath, true);
        }

        /// <summary>
        /// Creates the directory of the specified file if it does not exist.
        /// </summary>
        /// <param name="filePath">The full path of the file.</param>
        private static void EnsureDirectory(string filePath)
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);
        }
    }
}

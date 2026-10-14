// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Outputs
{
    using Kampose.Reporters;
    using Kampute.DocToolkit;
    using Kampute.DocToolkit.Support;
    using System;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Represents the destination of the files produced by documentation generation.
    /// </summary>
    /// <remarks>
    /// Every file produced through this class is recorded in <see cref="GeneratedFiles"/>, which is the authority on which files
    /// the documentation consists of, whether or not the derived class stores their content.
    /// </remarks>
    public abstract class DocumentationOutput
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentationOutput"/> class.
        /// </summary>
        /// <param name="outputDirectory">The documentation output directory.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="outputDirectory"/> is <see langword="null"/> or empty.</exception>
        protected DocumentationOutput(string outputDirectory)
        {
            GeneratedFiles = new GeneratedFileSet(outputDirectory);
        }

        /// <summary>
        /// Gets the full path of the documentation output directory.
        /// </summary>
        /// <value>
        /// The full path of the documentation output directory.
        /// </value>
        public string OutputDirectory => GeneratedFiles.OutputDirectory;

        /// <summary>
        /// Gets the files produced so far.
        /// </summary>
        /// <value>
        /// The set of files produced through this output.
        /// </value>
        public GeneratedFileSet GeneratedFiles { get; }

        /// <summary>
        /// Prepares the output for documentation generation.
        /// </summary>
        /// <param name="reporter">The activity reporter for logging.</param>
        /// <remarks>
        /// This method is called after all validations that can stop the generation have passed, and before any file is produced.
        /// The default implementation does nothing.
        /// </remarks>
        public virtual void Prepare(IActivityReporter reporter)
        {
        }

        /// <summary>
        /// Creates the writer factory used to produce documentation pages.
        /// </summary>
        /// <returns>An <see cref="IDocumentWriterFactory"/> that produces pages through this output.</returns>
        /// <remarks>
        /// When an HTML page is complete, the anchors it defines are recorded in <see cref="GeneratedFiles"/>, so that links to
        /// locations within the page can be verified.
        /// </remarks>
        public IDocumentWriterFactory CreatePageWriterFactory() => new PageWriterFactory(this);

        /// <summary>
        /// Produces a file with the specified text content.
        /// </summary>
        /// <param name="targetPath">The full path of the file to produce.</param>
        /// <param name="content">The content of the file.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="targetPath"/> is <see langword="null"/>, empty, or outside <see cref="OutputDirectory"/>.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="content"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// The file is recorded in <see cref="GeneratedFiles"/> only when it is produced without an exception.
        /// </remarks>
        public void WriteFile(string targetPath, string content)
        {
            ArgumentException.ThrowIfNullOrEmpty(targetPath);
            ArgumentNullException.ThrowIfNull(content);

            WriteFileCore(targetPath, content);
            GeneratedFiles.Add(targetPath);
        }

        /// <summary>
        /// Produces a file with the content of the specified source file.
        /// </summary>
        /// <param name="sourcePath">The full path of the source file.</param>
        /// <param name="targetPath">The full path of the file to produce.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="sourcePath"/> or <paramref name="targetPath"/> is <see langword="null"/> or empty, or <paramref name="targetPath"/> is outside <see cref="OutputDirectory"/>.</exception>
        /// <remarks>
        /// The file is recorded in <see cref="GeneratedFiles"/> only when it is produced without an exception.
        /// </remarks>
        public void CopyFile(string sourcePath, string targetPath)
        {
            ArgumentException.ThrowIfNullOrEmpty(sourcePath);
            ArgumentException.ThrowIfNullOrEmpty(targetPath);

            CopyFileCore(sourcePath, targetPath);
            GeneratedFiles.Add(targetPath);
        }

        /// <summary>
        /// Creates a <see cref="TextWriter"/> that receives the content of a documentation page.
        /// </summary>
        /// <param name="targetPath">The full path of the page file.</param>
        /// <returns>A <see cref="TextWriter"/> that receives the page content.</returns>
        protected abstract TextWriter CreatePageWriter(string targetPath);

        /// <summary>
        /// Produces a file with the specified text content.
        /// </summary>
        /// <param name="targetPath">The full path of the file to produce.</param>
        /// <param name="content">The content of the file.</param>
        protected abstract void WriteFileCore(string targetPath, string content);

        /// <summary>
        /// Produces a file with the content of the specified source file.
        /// </summary>
        /// <param name="sourcePath">The full path of the source file.</param>
        /// <param name="targetPath">The full path of the file to produce.</param>
        protected abstract void CopyFileCore(string sourcePath, string targetPath);

        /// <summary>
        /// Produces documentation pages through a <see cref="DocumentationOutput"/>.
        /// </summary>
        /// <remarks>
        /// Path resolution, minification, and URL scoping are inherited from <see cref="FileSystemDocumentWriterFactory"/>; only the
        /// destination of the page content is delegated to the output.
        /// </remarks>
        private sealed class PageWriterFactory(DocumentationOutput output) : FileSystemDocumentWriterFactory(output.OutputDirectory)
        {
            /// <inheritdoc />
            public override TextWriter CreateTextWriter(string relativePath)
            {
                ArgumentException.ThrowIfNullOrEmpty(relativePath);

                var targetPath = Path.GetFullPath(Path.Combine(OutputFolder, relativePath));
                var writer = output.CreatePageWriter(targetPath);
                output.GeneratedFiles.Add(targetPath);
                return IsHtmlFile(targetPath)
                    ? new AnchorRecordingWriter(writer, output.GeneratedFiles, targetPath)
                    : writer;
            }

            /// <summary>
            /// Determines whether the specified file is an HTML page.
            /// </summary>
            /// <param name="path">The path of the file.</param>
            /// <returns><see langword="true"/> if the file has an HTML extension; otherwise, <see langword="false"/>.</returns>
            private static bool IsHtmlFile(string path)
            {
                return FileExtensions.HtmlExtensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase);
            }
        }
    }
}

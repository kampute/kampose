// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Outputs
{
    using System.IO;

    /// <summary>
    /// Records the documentation files that would be produced without writing anything.
    /// </summary>
    /// <remarks>
    /// Page and bundle content is discarded, and copying a file only checks that its source can be opened for reading, so the
    /// failures of reading sources still surface. Failures that occur only when writing, such as missing permissions or a full disk,
    /// cannot be detected.
    /// </remarks>
    public sealed class DryRunDocumentationOutput : DocumentationOutput
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DryRunDocumentationOutput"/> class.
        /// </summary>
        /// <param name="outputDirectory">The documentation output directory.</param>
        /// <exception cref="System.ArgumentException">Thrown when <paramref name="outputDirectory"/> is <see langword="null"/> or empty.</exception>
        public DryRunDocumentationOutput(string outputDirectory)
            : base(outputDirectory)
        {
        }

        /// <inheritdoc />
        protected override TextWriter CreatePageWriter(string targetPath) => TextWriter.Null;

        /// <inheritdoc />
        protected override void WriteFileCore(string targetPath, string content)
        {
        }

        /// <inheritdoc />
        protected override void CopyFileCore(string sourcePath, string targetPath)
        {
            using var _ = File.OpenRead(sourcePath);
        }
    }
}

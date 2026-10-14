// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Outputs
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    /// A text writer that passes an HTML page through to another writer and records the anchors the page defines.
    /// </summary>
    /// <remarks>
    /// The anchors are the values of <c>id</c> attributes and of <c>name</c> attributes on <c>a</c> elements, which are the targets a URL
    /// fragment can refer to. They are recorded in the <see cref="GeneratedFileSet"/> when the writer is disposed.
    /// </remarks>
    internal sealed partial class AnchorRecordingWriter : TextWriter
    {
        private readonly TextWriter writer;
        private readonly GeneratedFileSet generatedFiles;
        private readonly string path;
        private readonly StringBuilder content = new();
        private bool disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnchorRecordingWriter"/> class.
        /// </summary>
        /// <param name="writer">The writer that receives the page content.</param>
        /// <param name="generatedFiles">The set in which the anchors of the page are recorded.</param>
        /// <param name="path">The path of the page, either absolute or relative to the output directory.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="writer"/> or <paramref name="generatedFiles"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="path"/> is <see langword="null"/> or empty.</exception>
        public AnchorRecordingWriter(TextWriter writer, GeneratedFileSet generatedFiles, string path)
        {
            ArgumentNullException.ThrowIfNull(writer);
            ArgumentNullException.ThrowIfNull(generatedFiles);
            ArgumentException.ThrowIfNullOrEmpty(path);

            this.writer = writer;
            this.generatedFiles = generatedFiles;
            this.path = path;
            NewLine = writer.NewLine;
        }

        /// <inheritdoc />
        public override Encoding Encoding => writer.Encoding;

        /// <inheritdoc />
        public override void Write(char value)
        {
            writer.Write(value);
            content.Append(value);
        }

        /// <inheritdoc />
        public override void Write(char[] buffer, int index, int count)
        {
            writer.Write(buffer, index, count);
            content.Append(buffer, index, count);
        }

        /// <inheritdoc />
        public override void Write(string? value)
        {
            writer.Write(value);
            content.Append(value);
        }

        /// <inheritdoc />
        public override void Flush() => writer.Flush();

        /// <summary>
        /// Records the anchors of the page and releases the underlying writer.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && !disposed)
            {
                disposed = true;
                generatedFiles.SetAnchors(path, FindAnchors(content.ToString()));
                content.Clear();
                writer.Dispose();
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Finds the anchors defined in the specified HTML content.
        /// </summary>
        /// <param name="html">The HTML content.</param>
        /// <returns>An enumerable collection of the anchor names, with character references decoded.</returns>
        private static IEnumerable<string> FindAnchors(string html)
        {
            foreach (Match match in IdAttributeMatcher().Matches(html))
                yield return WebUtility.HtmlDecode(match.Groups["value"].Value);

            foreach (Match match in AnchorNameAttributeMatcher().Matches(html))
                yield return WebUtility.HtmlDecode(match.Groups["value"].Value);
        }

        [GeneratedRegex("""\sid\s*=\s*(?:"(?<value>[^"]*)"|'(?<value>[^']*)'|(?<value>[^\s"'=<>`]+))""", RegexOptions.IgnoreCase)]
        private static partial Regex IdAttributeMatcher();

        [GeneratedRegex("""<a\s(?:[^>]*?\s)?name\s*=\s*(?:"(?<value>[^"]*)"|'(?<value>[^']*)'|(?<value>[^\s"'=<>`]+))""", RegexOptions.IgnoreCase)]
        private static partial Regex AnchorNameAttributeMatcher();
    }
}

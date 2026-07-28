// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Support
{
    using Kampute.DocToolkit.Formatters;
    using Kampute.DocToolkit.Routing;
    using Markdig;
    using Markdig.Extensions.Alerts;
    using Markdig.Syntax;
    using System;
    using System.IO;
    using StringBuilderPool = Kampute.DocToolkit.Support.StringBuilderPool;

    /// <summary>
    /// Transforms Markdown content for Markdown output.
    /// </summary>
    /// <remarks>
    /// Applies output-specific compatibility transformations while preserving portable Markdown. GitHub alert
    /// directives are converted to titled blockquotes, and Markdown URLs are transformed when needed.
    /// </remarks>
    public sealed class MarkdownToMarkdownTransformer : ITextTransformer
    {
        private static readonly MarkdownPipeline alertPipeline = new MarkdownPipelineBuilder().UseAlertBlocks().Build();
        private readonly MarkdownLinkTransformer linkTransformer = new();

        /// <summary>
        /// Transforms Markdown from the specified reader into Markdown suitable for the target output.
        /// </summary>
        /// <param name="reader">The reader containing Markdown content.</param>
        /// <param name="writer">The writer that receives the transformed Markdown content.</param>
        /// <param name="urlTransformer">The optional URL transformer to apply to Markdown links.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="reader"/> or <paramref name="writer"/> is <see langword="null"/>.</exception>
        public void Transform(TextReader reader, TextWriter writer, IUrlTransformer? urlTransformer = null)
        {
            ArgumentNullException.ThrowIfNull(reader);
            ArgumentNullException.ThrowIfNull(writer);

            var markdown = reader.ReadToEnd();
            var portableMarkdown = ConvertAlertsToBlockquotes(markdown);
            using var portableReader = new StringReader(portableMarkdown);
            linkTransformer.Transform(portableReader, writer, urlTransformer);
        }

        /// <summary>
        /// Converts recognized GitHub alert directives to the titled blockquote form used for XML documentation notes.
        /// </summary>
        /// <param name="markdown">The Markdown source.</param>
        /// <returns>The Markdown source with GitHub alerts represented as portable titled blockquotes.</returns>
        private static string ConvertAlertsToBlockquotes(string markdown)
        {
            if (markdown.Length == 0)
                return markdown;

            using var reusable = StringBuilderPool.Shared.GetBuilder();
            var result = reusable.Builder;
            var hasAlerts = false;
            var copiedUntil = 0;
            var document = Markdown.Parse(markdown, alertPipeline);

            foreach (var alert in document.Descendants<AlertBlock>())
            {
                var kind = alert.Kind.ToString();
                if (!IsSupportedAlertKind(kind))
                    continue;

                var directiveStart = alert.Span.Start;
                var newlineIndex = markdown.IndexOf('\n', directiveStart);
                var directiveEnd = newlineIndex < 0 ? markdown.Length : newlineIndex + 1;
                var title = char.ToUpperInvariant(kind[0]) + kind[1..].ToLowerInvariant();

                hasAlerts = true;
                result.Append(markdown, copiedUntil, directiveStart - copiedUntil);
                result.Append("> **");
                result.Append(title);
                result.Append("** \\");
                if (newlineIndex >= 0)
                {
                    if (newlineIndex > directiveStart && markdown[newlineIndex - 1] == '\r')
                        result.Append('\r');

                    result.Append('\n');
                }

                copiedUntil = directiveEnd;
            }

            if (!hasAlerts)
                return markdown;

            result.Append(markdown, copiedUntil, markdown.Length - copiedUntil);
            return result.ToString();
        }

        /// <summary>
        /// Determines whether the alert kind is defined by GitHub's alert syntax.
        /// </summary>
        private static bool IsSupportedAlertKind(string kind)
        {
            return kind.Equals("NOTE", StringComparison.OrdinalIgnoreCase) ||
                   kind.Equals("TIP", StringComparison.OrdinalIgnoreCase) ||
                   kind.Equals("IMPORTANT", StringComparison.OrdinalIgnoreCase) ||
                   kind.Equals("WARNING", StringComparison.OrdinalIgnoreCase) ||
                   kind.Equals("CAUTION", StringComparison.OrdinalIgnoreCase);
        }
    }
}

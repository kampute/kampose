// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Transformers
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
    /// Provides the base class for transforming Markdown content to the Markdown of a publishing target.
    /// </summary>
    /// <remarks>
    /// The base class finds the Markdown constructs whose syntax differs between publishing targets and transforms the
    /// Markdown URLs when needed. Each derived class rewrites those constructs in the syntax of its target, such as by
    /// implementing <see cref="RewriteAlertDirective(string, string)"/>.
    /// </remarks>
    public abstract class MarkdownToMarkdownTransformer : ITextTransformer
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
            var portableMarkdown = RewriteAlerts(markdown);
            using var portableReader = new StringReader(portableMarkdown);
            linkTransformer.Transform(portableReader, writer, urlTransformer);
        }

        /// <summary>
        /// Rewrites the directive line of a GitHub alert for the target output.
        /// </summary>
        /// <param name="directiveLine">The line of the alert directive as written in the source, without its line terminator, such as <c>&gt; [!NOTE]</c>.</param>
        /// <param name="kind">The kind of the alert as written in the source, such as <c>NOTE</c>.</param>
        /// <returns>The line that replaces <paramref name="directiveLine"/> in the output.</returns>
        protected abstract string RewriteAlertDirective(string directiveLine, string kind);

        /// <summary>
        /// Rewrites the directive lines of the recognized GitHub alerts for the target output.
        /// </summary>
        /// <param name="markdown">The Markdown source.</param>
        /// <returns>The Markdown source with the alert directive lines rewritten by <see cref="RewriteAlertDirective(string, string)"/>.</returns>
        private string RewriteAlerts(string markdown)
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
                var directiveEnd = markdown.IndexOf('\n', directiveStart);
                if (directiveEnd < 0)
                    directiveEnd = markdown.Length;
                if (directiveEnd > directiveStart && markdown[directiveEnd - 1] == '\r')
                    directiveEnd--;

                hasAlerts = true;
                result.Append(markdown, copiedUntil, directiveStart - copiedUntil);
                result.Append(RewriteAlertDirective(markdown[directiveStart..directiveEnd], kind));
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

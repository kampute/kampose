// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Support
{
    using Kampute.DocToolkit.Formatters;
    using Kampute.DocToolkit.Routing;
    using Markdig;
    using Markdig.Renderers;
    using Markdig.Renderers.Html.Inlines;
    using Markdig.Syntax.Inlines;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Converts Markdown content to HTML.
    /// </summary>
    public sealed partial class MarkdownToHtmlTransformer : ITextTransformer
    {
        private readonly MarkdownPipeline pipeline;
        private readonly UrlReplacementExtension urlReplacementExtension = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkdownToHtmlTransformer"/> class.
        /// </summary>
        public MarkdownToHtmlTransformer()
        {
            var pipelineBuilder = new MarkdownPipelineBuilder().UseAdvancedExtensions();
            pipelineBuilder.Extensions.Add(urlReplacementExtension);
            pipeline = pipelineBuilder.Build();
        }

        /// <summary>
        /// Transforms the specified Markdown content to HTML.
        /// </summary>
        /// <param name="markdown">The Markdown content to transform.</param>
        /// <param name="urlTransformer">The optional URL mapper to use for replacing URLs in Markdown links.</param>
        /// <returns>The HTML representation of the Markdown content.</returns>
        public string Transform(string markdown, IUrlTransformer? urlTransformer = null)
        {
            if (string.IsNullOrWhiteSpace(markdown))
                return string.Empty;

            // Protect handlebars expressions from Markdig processing
            var id = Random.Shared.Next(100000, 999999);
            var placeholders = new Dictionary<string, string>(StringComparer.Ordinal);
            var processedMarkdown = GetHandlebarsRegex().Replace(markdown, match =>
            {
                var placeholder = $"%%{id}!HBS{placeholders.Count}%%";
                placeholders[placeholder] = match.Value;
                return placeholder;
            });

            // Transform Markdown to HTML
            urlReplacementExtension.UrlTransformer = urlTransformer;
            using var writer = Kampute.DocToolkit.Support.StringBuilderPool.Shared.GetWriter();
            Markdown.ToHtml(processedMarkdown, writer, pipeline);
            var html = writer.ToString();

            // Restore handlebars expressions
            if (placeholders.Count > 0)
            {
                foreach (var placeholder in placeholders)
                    html = html.Replace(placeholder.Key, placeholder.Value);
            }

            return html;
        }

        /// <summary>
        /// Transforms the Markdown content from the specified reader to HTML and writes it to the specified writer.
        /// </summary>
        /// <param name="reader">The reader to read the Markdown content from.</param>
        /// <param name="writer">The writer to write the HTML content to.</param>
        /// <param name="urlTransformer">The optional URL mapper to use for replacing URLs in Markdown links.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="reader"/> or <paramref name="writer"/> is <see langword="null"/>.</exception>
        public void Transform(TextReader reader, TextWriter writer, IUrlTransformer? urlTransformer = null)
        {
            ArgumentNullException.ThrowIfNull(reader);
            ArgumentNullException.ThrowIfNull(writer);

            writer.Write(Transform(reader.ReadToEnd(), urlTransformer));
        }

        /// <summary>
        /// Extension for replacing URLs in Markdown links.
        /// </summary>
        private sealed class UrlReplacementExtension : IMarkdownExtension
        {
            /// <summary>
            /// The URL mapper to use for replacing URLs in Markdown links.
            /// </summary>
            public IUrlTransformer? UrlTransformer { get; set; }

            /// <summary>
            /// No setup needed at pipeline building stage.
            /// </summary>
            /// <param name="pipeline">The pipeline builder to configure.</param>
            public void Setup(MarkdownPipelineBuilder pipeline) { }

            /// <summary>
            /// Replaces the default link renderer with a custom renderer that handles URL replacements.
            /// </summary>
            /// <param name="pipeline">The pipeline to configure.</param>
            /// <param name="renderer">The Markdown renderer to configure.</param>
            public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
            {
                if (UrlTransformer is not null && UrlTransformer.MayTransformUrls && renderer is HtmlRenderer htmlRenderer)
                {
                    htmlRenderer.ObjectRenderers.Replace<LinkInlineRenderer>(new CustomInlineLinkRenderer(UrlTransformer));
                    htmlRenderer.ObjectRenderers.Replace<AutolinkInlineRenderer>(new CustomAutoLinkInlineRenderer(UrlTransformer));
                }
            }
        }

        /// <summary>
        /// Custom renderer for replacing URLs in inline Markdown links.
        /// </summary>
        /// <param name="urlTransformer">The URL mapper to use for replacing URLs.</param>
        private sealed class CustomInlineLinkRenderer(IUrlTransformer urlTransformer) : LinkInlineRenderer
        {
            /// <summary>
            /// Writes the specified link to the renderer, replacing the URL if necessary.
            /// </summary>
            /// <param name="renderer">The HTML renderer to write to.</param>
            /// <param name="link">The link to write.</param>
            protected override void Write(HtmlRenderer renderer, LinkInline link)
            {
                if (urlTransformer.TryTransformUrl(link.Url!, out var replacementUrl))
                    link.Url = replacementUrl.ToString();

                base.Write(renderer, link);
            }
        }

        /// <summary>
        /// Custom renderer for replacing URLs in Markdown auto-links.
        /// </summary>
        /// <param name="urlTransformer">The URL mapper to use for replacing URLs.</param>
        private sealed class CustomAutoLinkInlineRenderer(IUrlTransformer urlTransformer) : AutolinkInlineRenderer
        {
            /// <summary>
            /// Writes the specified link to the renderer, replacing the URL if necessary.
            /// </summary>
            /// <param name="renderer">The HTML renderer to write to.</param>
            /// <param name="link">The link to write.</param>
            protected override void Write(HtmlRenderer renderer, AutolinkInline link)
            {
                if (urlTransformer.TryTransformUrl(link.Url, out var replacementUrl))
                    link.Url = replacementUrl.ToString();

                base.Write(renderer, link);
            }
        }

        [GeneratedRegex(@"\{{2,}[^}]+\}{2,}", RegexOptions.NonBacktracking | RegexOptions.Compiled)]
        private static partial Regex GetHandlebarsRegex();
    }
}

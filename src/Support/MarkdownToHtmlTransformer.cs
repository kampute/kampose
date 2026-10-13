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
    using Markdig.Renderers;
    using Markdig.Renderers.Html;
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
        private readonly HtmlRenderingExtension htmlRenderingExtension = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkdownToHtmlTransformer"/> class.
        /// </summary>
        public MarkdownToHtmlTransformer()
        {
            var pipelineBuilder = new MarkdownPipelineBuilder().UseAdvancedExtensions();
            pipelineBuilder.Extensions.Add(htmlRenderingExtension);
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
            htmlRenderingExtension.UrlTransformer = urlTransformer;
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
        /// Extension for customizing HTML rendering of Markdown content.
        /// </summary>
        private sealed class HtmlRenderingExtension : IMarkdownExtension
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
            /// Replaces the default alert and link renderers with Kampose-specific renderers.
            /// </summary>
            /// <param name="pipeline">The pipeline to configure.</param>
            /// <param name="renderer">The Markdown renderer to configure.</param>
            public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
            {
                if (renderer is not HtmlRenderer htmlRenderer)
                    return;

                htmlRenderer.ObjectRenderers.Replace<AlertBlockRenderer>(new NoteAlertBlockRenderer());

                if (UrlTransformer is not null && UrlTransformer.MayTransformUrls)
                {
                    htmlRenderer.ObjectRenderers.Replace<LinkInlineRenderer>(new CustomInlineLinkRenderer(UrlTransformer));
                    htmlRenderer.ObjectRenderers.Replace<AutolinkInlineRenderer>(new CustomAutoLinkInlineRenderer(UrlTransformer));
                }
            }
        }

        /// <summary>
        /// Renders GitHub alerts using the same HTML contract as XML documentation note elements.
        /// </summary>
        private sealed class NoteAlertBlockRenderer : HtmlObjectRenderer<AlertBlock>
        {
            /// <summary>
            /// Writes the alert using the same blockquote structure as an XML documentation note.
            /// </summary>
            /// <param name="renderer">The HTML renderer to write to.</param>
            /// <param name="alert">The alert block to render.</param>
            protected override void Write(HtmlRenderer renderer, AlertBlock alert)
            {
                var kind = alert.Kind.ToString().ToLowerInvariant();
                var title = char.ToUpperInvariant(kind[0]) + kind[1..];
                var attributes = CreateNoteAttributes(alert, kind, title);

                renderer.EnsureLine();
                renderer.Write("<blockquote");
                renderer.WriteAttributes(attributes);
                renderer.WriteLine(">");
                renderer.Write("<div class=\"note-title\" aria-hidden=\"true\">");
                renderer.WriteEscape(title);
                renderer.WriteLine("</div>");
                renderer.WriteLine("<div class=\"note-content\">");
                renderer.WriteChildren(alert);
                renderer.WriteLine("</div>");
                renderer.WriteLine("</blockquote>");
            }

            /// <summary>
            /// Creates note attributes while preserving custom attributes attached to the alert.
            /// </summary>
            private static HtmlAttributes CreateNoteAttributes(AlertBlock alert, string kind, string title)
            {
                var attributes = new HtmlAttributes();
                var sourceAttributes = alert.TryGetAttributes();

                if (sourceAttributes is not null)
                {
                    attributes.Id = sourceAttributes.Id;

                    if (sourceAttributes.Classes is not null)
                    {
                        foreach (var className in sourceAttributes.Classes)
                        {
                            if (!className.Equals("markdown-alert", StringComparison.Ordinal) &&
                                !className.Equals($"markdown-alert-{kind}", StringComparison.Ordinal))
                            {
                                attributes.AddClass(className);
                            }
                        }
                    }

                    if (sourceAttributes.Properties is not null)
                    {
                        foreach (var property in sourceAttributes.Properties)
                        {
                            if (!property.Key.Equals("role", StringComparison.OrdinalIgnoreCase) &&
                                !property.Key.Equals("data-type", StringComparison.OrdinalIgnoreCase) &&
                                !property.Key.Equals("aria-label", StringComparison.OrdinalIgnoreCase))
                            {
                                attributes.AddProperty(property.Key, property.Value ?? string.Empty);
                            }
                        }
                    }
                }

                attributes.AddClass("note");
                attributes.AddProperty("role", "note");
                attributes.AddProperty("data-type", kind);
                attributes.AddProperty("aria-label", title);
                return attributes;
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

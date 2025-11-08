// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Models
{
    using Kampose.Services;
    using Kampute.DocToolkit;
    using Kampute.DocToolkit.Topics.Abstracts;
    using System;
    using System.IO;

    /// <summary>
    /// Represents a topic source that uses a template to render its content.
    /// </summary>
    public sealed class TemplatedTopic : TopicSource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TemplatedTopic"/> class with the specified name and template
        /// name.
        /// </summary>
        /// <param name="name">The name of the topic</param>
        /// <param name="templateName">The name of the template associated with the topic.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> or <paramref name="templateName"/> is <see langword="null"/> or whitespace.</exception>
        public TemplatedTopic(string name, string templateName)
            : base(name)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(templateName);

            TemplateName = templateName;
        }

        /// <summary>
        /// Gets the name of the template associated with this topic.
        /// </summary>
        /// <value>
        /// The name of the template used to render this topic's content.
        /// </value>
        public string TemplateName { get; }

        /// <summary>
        /// Renders the specified template to the provided <see cref="TextWriter"/> using the given documentation context.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to which the rendered output will be written.</param>
        /// <param name="context">The <see cref="IDocumentationContext"/> containing the data and context required for rendering</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="writer"/> or <paramref name="context"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">Thrown if no template renderer is available for rendering this topic.</exception>
        public override void Render(TextWriter writer, IDocumentationContext context)
        {
            ArgumentNullException.ThrowIfNull(writer);
            ArgumentNullException.ThrowIfNull(context);

            var renderer = DocumentationService.CurrentRenderer
                ?? throw new InvalidOperationException($"No template renderer is available for rendering topic '{Id}' with template '{TemplateName}'.");

            renderer.RenderTemplate(writer, TemplateName, context);
        }
    }
}

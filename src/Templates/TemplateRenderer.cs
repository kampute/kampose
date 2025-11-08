// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Templates
{
    using HandlebarsDotNet;
    using Kampose.Reporters;
    using Kampute.DocToolkit;
    using System;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Provides functionality to manage and render Handlebars templates.
    /// </summary>
    public sealed class TemplateRenderer : DocumentPageRenderer
    {
        private readonly IActivityReporter reporter;
        private readonly IHandlebars handlebars;
        private readonly Dictionary<string, HandlebarsTemplate<TextWriter, object, object>> templates = new(StringComparer.Ordinal);

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateRenderer"/> class with the specified reporter and Handlebars environment.
        /// </summary>>
        /// <param name="reporter">The activity reporter to use for reporting page rendering.</param>
        /// <param name="handlebars">The Handlebars environment to use for rendering templates.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="reporter"/> or <paramref name="handlebars"/> is <see langword="null"/>.</exception>
        public TemplateRenderer(IActivityReporter reporter, IHandlebars handlebars)
        {
            this.reporter = reporter ?? throw new ArgumentNullException(nameof(reporter));
            this.handlebars = handlebars ?? throw new ArgumentNullException(nameof(handlebars));
        }

        /// <summary>
        /// Gets the common data shared across all templates.
        /// </summary>
        /// <value>
        /// The common data shared across all templates.
        /// </value>
        public Dictionary<string, object?> CommonData { get; } = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Determines if a template with the specified name exists in the renderer.
        /// </summary>
        /// <param name="templateName">The name of the template to check.</param>
        /// <returns><see langword="true"/> if the template exists; otherwise, <see langword="false"/>.</returns>
        public bool HasTemplate(string templateName) => templateName is not null && templates.ContainsKey(templateName);

        /// <summary>
        /// Adds a Handlebars template to the renderer.
        /// </summary>
        /// <param name="templateName">The name of the template.</param>
        /// <param name="templatePath">The path to the template file.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="templateName"/> or <paramref name="templatePath"/> is <see langword="null"/> or empty.</exception>
        /// <exception cref="FileNotFoundException">Thrown when the specified template file does not exist.</exception>
        /// <exception cref="HandlebarsCompilerException">Thrown when the template fails to compile.</exception>
        /// <remarks>
        /// This method loads a Handlebars template from the specified file path and registers it under the specified name.
        /// The template is also available as a partial with the specified name, allowing it to be reused within other templates.
        /// </remarks>
        public void AddTemplate(string templateName, string templatePath)
        {
            ArgumentException.ThrowIfNullOrEmpty(templateName);
            ArgumentException.ThrowIfNullOrEmpty(templatePath);

            using var reader = new StreamReader(templatePath);
            try
            {
                var template = handlebars.Compile(reader);
                handlebars.RegisterTemplate(templateName, template);
                templates[templateName] = template;
            }
            catch (Exception error)
            {
                throw new HandlebarsCompilerException($"Failed to compile template '{templatePath}'.", error);
            }
        }

        /// <summary>
        /// Adds an inline Handlebars template to the renderer.
        /// </summary>
        /// <param name="templateName">The name of the template.</param>
        /// <param name="templateContent">The content of the template.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="templateName"/> or <paramref name="templateContent"/> is <see langword="null"/> or empty.</exception>
        /// <exception cref="HandlebarsCompilerException">Thrown when the template fails to compile.</exception>
        public void AddInlineTemplate(string templateName, string templateContent)
        {
            ArgumentException.ThrowIfNullOrEmpty(templateName);
            ArgumentException.ThrowIfNullOrEmpty(templateContent);

            try
            {
                handlebars.RegisterTemplate(templateName, templateContent);
            }
            catch (Exception error)
            {
                throw new HandlebarsCompilerException($"Failed to compile inline template '{templateName}'.", error);
            }
        }

        /// <summary>
        /// Renders a template with the specified name using the provided data and writes the result to the specified writer.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to write the rendered output to.</param>
        /// <param name="templateName">The name of the template to render.</param>
        /// <param name="data">The data to use when rendering the template.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="writer"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="templateName"/> is <see langword="null"/> or empty.</exception>
        /// <exception cref="KeyNotFoundException">Thrown when a template with the specified name is not found.</exception>
        /// <exception cref="HandlebarsRuntimeException">Thrown when the template fails to render.</exception>
        public void RenderTemplate(TextWriter writer, string templateName, object data)
        {
            ArgumentNullException.ThrowIfNull(writer);
            ArgumentException.ThrowIfNullOrEmpty(templateName);

            if (!templates.TryGetValue(templateName, out var template))
                throw new KeyNotFoundException($"Template '{templateName}' does not exist.");

            try
            {
                template(writer, data);
            }
            catch (Exception error)
            {
                throw new HandlebarsRuntimeException($"Failed to render template '{templateName}'.", error);
            }
        }

        /// <summary>
        /// Renders a documentation page for a model and writes the result to the specified writer.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to write the rendered output to.</param>
        /// <param name="category">The category of the documentation page to render.</param>
        /// <param name="model">The documentation model to render.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="writer"/> or <paramref name="model"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="category"/> is not a valid documentation page type.</exception>
        protected override void Render(TextWriter writer, PageCategory category, IDocumentModel model)
        {
            ArgumentNullException.ThrowIfNull(writer);
            ArgumentNullException.ThrowIfNull(model);

            var templateName = category switch
            {
                PageCategory.Topic => TemplateNames.TopicPage,
                PageCategory.Namespace => TemplateNames.NamespacePage,
                PageCategory.Class => TemplateNames.ClassPage,
                PageCategory.ClassWithMembers => TemplateNames.ClassWithMembersPage,
                PageCategory.Struct => TemplateNames.StructPage,
                PageCategory.StructWithMembers => TemplateNames.StructWithMembersPage,
                PageCategory.Interface => TemplateNames.InterfacePage,
                PageCategory.InterfaceWithMembers => TemplateNames.InterfaceWithMembersPage,
                PageCategory.Enum => TemplateNames.EnumPage,
                PageCategory.Delegate => TemplateNames.DelegatePage,
                PageCategory.Constructor => TemplateNames.ConstructorPage,
                PageCategory.ConstructorOverloads => TemplateNames.ConstructorOverloadsPage,
                PageCategory.Field => TemplateNames.FieldPage,
                PageCategory.Event => TemplateNames.EventPage,
                PageCategory.Property => TemplateNames.PropertyPage,
                PageCategory.PropertyOverloads => TemplateNames.PropertyOverloadsPage,
                PageCategory.Method => TemplateNames.MethodPage,
                PageCategory.MethodOverloads => TemplateNames.MethodOverloadsPage,
                PageCategory.Operator => TemplateNames.OperatorPage,
                PageCategory.OperatorOverloads => TemplateNames.OperatorOverloadsPage,
                _ => throw new ArgumentOutOfRangeException(nameof(category), category, $"Invalid documentation page category '{category}'.")
            };

            using var _ = reporter.BeginStep($"{model.Name} {model.ModelType}");
            RenderTemplate(writer, templateName, new TemplateData(CommonData, model, nameof(model)));
        }
    }
}

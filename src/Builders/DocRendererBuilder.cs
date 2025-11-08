// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Builders
{
    using Kampose.Models;
    using Kampose.Reporters;
    using Kampose.Support;
    using Kampose.Templates;
    using Kampute.DocToolkit;
    using Kampute.DocToolkit.Support;
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// Builder responsible for creating and configuring template renderers for documentation generation.
    /// </summary>
    /// <remarks>
    /// The <see cref="DocRendererBuilder"/> class provides functionality for building template renderers
    /// that are configured with themes, templates, and documentation context data. It acts as a factory
    /// for creating fully configured <see cref="TemplateRenderer"/> instances that can be used to generate
    /// documentation pages with consistent theming and data.
    /// </remarks>
    public sealed class DocRendererBuilder
    {
        private readonly IActivityReporter reporter;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocRendererBuilder"/> class.
        /// </summary>
        /// <param name="reporter">The activity reporter to use for tracking progress.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="reporter"/> is <see langword="null"/>.</exception>
        public DocRendererBuilder(IActivityReporter reporter)
        {
            this.reporter = reporter ?? throw new ArgumentNullException(nameof(reporter));
        }

        /// <summary>
        /// Creates a template renderer configured with the specified configuration, theme, and context.
        /// </summary>
        /// <param name="context">The documentation context to use for rendering.</param>
        /// <param name="theme">The theme to apply to the renderer.</param>
        /// <param name="parameters">A dictionary of custom parameters to add to the template renderer.</param>
        /// <returns>A fully configured template renderer ready for documentation generation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is <see langword="null"/>.</exception>
        /// <remarks>
        /// This method creates a new <see cref="TemplateRenderer"/> instance and configures it with:
        /// <list type="bullet">
        ///   <item><description>Template files from the specified theme</description></item>
        ///   <item><description>Common data derived from the documentation context</description></item>
        ///   <item><description>Theme bundle information (scripts and styles)</description></item>
        ///   <item><description>Default and custom theme settings</description></item>
        /// </list>
        /// The resulting renderer is ready to generate documentation pages with consistent theming.
        /// </remarks>
        public TemplateRenderer Build(DocContext context, Theme theme, IReadOnlyDictionary<string, object?> themeSettings)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(theme);
            ArgumentNullException.ThrowIfNull(themeSettings);

            var handlebars = HandlebarsFactory.CreateEnvironment(context);
            var renderer = new TemplateRenderer(reporter, handlebars);

            LoadTemplateFiles(renderer, theme);

            AddCommonData(renderer, context);
            AddThemeData(renderer, theme);
            AddThemeSettings(renderer, context, theme, themeSettings);

            return renderer;
        }

        /// <summary>
        /// Loads template files from the specified theme into the renderer.
        /// </summary>
        /// <param name="renderer">The template renderer to load templates into.</param>
        /// <param name="theme">The theme containing template files.</param>
        private void LoadTemplateFiles(TemplateRenderer renderer, Theme theme)
        {
            using var _ = reporter.BeginActivity("Loading theme templates", theme.TemplateFiles.Count);
            foreach (var (name, path) in theme.TemplateFiles)
            {
                using var __ = reporter.BeginStep(path);
                renderer.AddTemplate(name, path);
            }
        }

        /// <summary>
        /// Adds common data to the renderer based on the context.
        /// </summary>
        /// <param name="renderer">The template renderer to assign common data to.</param>
        /// <param name="context">The documentation context.</param>
        private static void AddCommonData(TemplateRenderer renderer, DocContext context)
        {
            renderer.CommonData["language"] = context.Language;
            renderer.CommonData["generator"] = $"{nameof(Kampose)} [Version {Assembly.GetExecutingAssembly().GetName().Version}]";
            renderer.CommonData["absoluteUrls"] = context.AddressProvider.ActiveScope.RootUrl.IsAbsoluteUri;
            renderer.CommonData["hasNamespacePages"] = context.Assemblies.Count > 0 && context.AddressProvider.Granularity.HasFlag(PageGranularity.Namespace);
            renderer.CommonData["hasTypePages"] = context.Assemblies.Count > 0 && context.AddressProvider.Granularity.HasFlag(PageGranularity.Type);
            renderer.CommonData["hasMemberPages"] = context.Assemblies.Count > 0 && context.AddressProvider.Granularity.HasFlag(PageGranularity.Member);

            if (context.Topics.TryGetById(SpecialTopicIdentifiers.Home, out var homeTopic))
                renderer.CommonData["homePageTitle"] = homeTopic.Name;

            if (context.Topics.TryGetById(SpecialTopicIdentifiers.Api, out var apiTopic))
                renderer.CommonData["apiPageTitle"] = apiTopic.Name;
        }

        /// <summary>
        /// Adds theme information and default settings to the renderer's common data.
        /// </summary>
        /// <param name="renderer">The template renderer to assign common data to.</param>
        /// <param name="theme">The theme containing bundle information.</param>
        private static void AddThemeData(TemplateRenderer renderer, Theme theme)
        {
            if (theme.Metadata is not null)
                renderer.CommonData["theme"] = theme.Metadata;

            foreach (var (name, setting) in theme.Parameters)
            {
                if (setting.DefaultValue is not null)
                    renderer.CommonData[name] = setting.DefaultValue;
            }

            renderer.CommonData["scripts"] = theme.ScriptFiles.Keys;
            renderer.CommonData["styles"] = theme.StyleFiles.Keys;
        }

        /// <summary>
        /// Adds custom theme settings to the renderer's common data.
        /// </summary>
        /// <param name="renderer">The template renderer to assign common data to.</param>
        /// <param name="context">The documentation context used for transforming Markdown values.</param>
        /// <param name="theme">The theme defining the custom settings.</param>
        /// <param name="settings">A dictionary of custom theme settings to add to the template renderer.</param>
        private void AddThemeSettings(TemplateRenderer renderer, DocContext context, Theme theme, IReadOnlyDictionary<string, object?> settings)
        {
            foreach (var (name, value) in settings)
            {
                if (value is null)
                    continue;

                if (!theme.Parameters.TryGetValue(name, out var setting))
                {
                    renderer.CommonData[name] = value;
                    continue;
                }

                try
                {
                    var validatedValue = GetValidatedThemeValue(context, setting, value);
                    renderer.CommonData[name] = validatedValue;
                    if (setting.Type is ThemeParameterType.Markdown)
                        renderer.AddInlineTemplate($"{name}_partial", validatedValue!.ToString()!);
                }
                catch (Exception error)
                {
                    reporter.LogWarning($"Invalid value for theme parameter '{name}'. {error.Message}");
                }
            }
        }

        /// <summary>
        /// Validates the value of a theme parameter based on its type.
        /// </summary>
        /// <param name="context">The documentation context used for validation.</param>
        /// <param name="setting">The theme parameter to validate.</param>
        /// <param name="value">The value to validate.</param>
        /// <returns>The validated value, potentially transformed if it is a Markdown type.</returns>
        /// <exception cref="FormatException">Thrown when the value cannot be validated.</exception>
        private static object? GetValidatedThemeValue(DocContext context, ThemeParameter setting, object? value)
        {
            var validatedValue = setting.ValidateValue(value);
            if
            (
                setting.Type is ThemeParameterType.Markdown &&
                validatedValue is string markdown &&
                context.TryTransformText(FileExtensions.Markdown, markdown, out var transformedValue)
            )
            {
                return transformedValue;
            }

            return validatedValue;
        }
    }
}

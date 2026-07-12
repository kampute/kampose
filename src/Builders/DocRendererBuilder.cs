// Copyright (C) Kampute
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
    using System;
    using System.Collections.Generic;
    using System.Linq;

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
        /// <param name="themeSettings">A dictionary of custom parameters to add to the template renderer.</param>
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
        /// The resulting renderer is ready to generate documentation pages.
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
            AddThemeSettings(renderer, theme, themeSettings);

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
            renderer.CommonData["generator"] = $"{nameof(Kampose)} v{Program.Version}";
            renderer.CommonData["absoluteUrls"] = context.AddressProvider.ActiveScope.DocumentationRootUrl.IsAbsoluteUri;
            renderer.CommonData["hasNamespacePages"] = context.Assemblies.Count > 0 && context.AddressProvider.Granularity.HasFlag(PageGranularity.Namespace);
            renderer.CommonData["hasTypePages"] = context.Assemblies.Count > 0 && context.AddressProvider.Granularity.HasFlag(PageGranularity.Type);
            renderer.CommonData["hasMemberPages"] = context.Assemblies.Count > 0 && context.AddressProvider.Granularity.HasFlag(PageGranularity.Member);
            renderer.CommonData["hasTopics"] = context.Topics.Any(static topic => !SpecialTopicIdentifiers.IsSpecialTopic(topic.Id));

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

            renderer.CommonData["scripts"] = theme.ScriptFiles.Keys;
            renderer.CommonData["styles"] = theme.StyleFiles.Keys;
        }

        /// <summary>
        /// Adds custom theme settings to the renderer's common data.
        /// </summary>
        /// <param name="renderer">The template renderer to assign common data to.</param>
        /// <param name="theme">The theme defining the custom settings.</param>
        /// <param name="settings">A dictionary of custom theme settings to add to the template renderer.</param>
        private void AddThemeSettings(TemplateRenderer renderer, Theme theme, IReadOnlyDictionary<string, object?> settings)
        {
            foreach (var (name, setting) in theme.Parameters)
            {
                var value = settings.TryGetValue(name, out var configuredValue) && configuredValue is not null
                    ? configuredValue
                    : setting.DefaultValue;

                if (value is null)
                    continue;

                try
                {
                    var validatedValue = setting.ValidateValue(value);
                    renderer.CommonData[name] = validatedValue;
                    if (setting.Type is ThemeParameterType.Markdown)
                        renderer.AddInlineTemplate($"{name}_partial", $"{{{{#markdown}}}}{validatedValue}{{{{/markdown}}}}");
                }
                catch (Exception error)
                {
                    reporter.LogWarning($"Invalid value for theme parameter '{name}'. {error.Message}");
                }
            }

            foreach (var (name, value) in settings)
            {
                if (value is not null && !theme.Parameters.ContainsKey(name))
                    renderer.CommonData[name] = value;
            }
        }
    }
}

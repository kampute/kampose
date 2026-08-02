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
        /// <param name="config">The configuration containing the convention and custom theme parameters.</param>
        /// <param name="theme">The theme to apply to the renderer.</param>
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
        public TemplateRenderer Build(DocContext context, Configuration config, Theme theme)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(config);
            ArgumentNullException.ThrowIfNull(theme);

            var handlebars = HandlebarsFactory.CreateEnvironment(context);
            var renderer = new TemplateRenderer(reporter, handlebars);

            LoadTemplateFiles(renderer, theme);
            ConfigureCommonData(renderer, context, config, theme);

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
        /// Configures the common data exposed to templates and theme scripts.
        /// </summary>
        /// <param name="renderer">The template renderer to configure.</param>
        /// <param name="context">The documentation context.</param>
        /// <param name="config">The documentation configuration.</param>
        /// <param name="theme">The selected theme.</param>
        private void ConfigureCommonData(TemplateRenderer renderer, DocContext context, Configuration config, Theme theme)
        {
            var globalData = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            CollectContextData(globalData, context);
            CollectConfigData(globalData, config);
            CollectThemeData(globalData, theme);
            AddThemeSettings(globalData, renderer, theme, config.ThemeSettings);
            PublishGlobalData(renderer, globalData);
        }

        /// <summary>
        /// Collects common data derived from the documentation context.
        /// </summary>
        /// <param name="globalData">The collection to which common data is added.</param>
        /// <param name="context">The documentation context.</param>
        private static void CollectContextData(IDictionary<string, object?> globalData, DocContext context)
        {
            globalData["language"] = context.Language;
            globalData["generator"] = $"{nameof(Kampose)} v{Program.Version}";
            globalData["absoluteUrls"] = context.AddressProvider.ActiveScope.DocumentationRootUrl.IsAbsoluteUri;
            globalData["hasNamespacePages"] = context.Assemblies.Count > 0 && context.AddressProvider.Granularity.HasFlag(PageGranularity.Namespace);
            globalData["hasTypePages"] = context.Assemblies.Count > 0 && context.AddressProvider.Granularity.HasFlag(PageGranularity.Type);
            globalData["hasMemberPages"] = context.Assemblies.Count > 0 && context.AddressProvider.Granularity.HasFlag(PageGranularity.Member);
            globalData["hasTopics"] = context.Topics.Any(static topic => !SpecialTopicIdentifiers.IsSpecialTopic(topic.Id));
            globalData["homePageTitle"] = context.Topics.TryGetById(SpecialTopicIdentifiers.Home, out var homeTopic) ? homeTopic.Name : null;
            globalData["apiPageTitle"] = context.Topics.TryGetById(SpecialTopicIdentifiers.Api, out var apiTopic) ? apiTopic.Name : null;
        }

        /// <summary>
        /// Collects global data derived from the configuration.
        /// </summary>
        /// <param name="globalData">The collection to which configuration data is added.</param>
        /// <param name="config">The configuration to extract data from.</param>
        private static void CollectConfigData(IDictionary<string, object?> globalData, Configuration config)
        {
            globalData["convention"] = config.Convention;
        }

        /// <summary>
        /// Collects global theme information and bundle data.
        /// </summary>
        /// <param name="globalData">The collection to which theme data is added.</param>
        /// <param name="theme">The theme containing bundle information.</param>
        private static void CollectThemeData(IDictionary<string, object?> globalData, Theme theme)
        {
            globalData["theme"] = theme.Metadata;
            globalData["scripts"] = theme.ScriptFiles.Keys;
            globalData["styles"] = theme.StyleFiles.Keys;
        }

        /// <summary>
        /// Adds custom theme settings to the renderer's common data.
        /// </summary>
        /// <param name="globalData">The global data whose names are reserved.</param>
        /// <param name="renderer">The template renderer to assign common data to.</param>
        /// <param name="theme">The theme defining the custom settings.</param>
        /// <param name="settings">A dictionary of custom theme settings to add to the template renderer.</param>
        private void AddThemeSettings(
            IReadOnlyDictionary<string, object?> globalData,
            TemplateRenderer renderer,
            Theme theme,
            Dictionary<string, object?> settings)
        {
            foreach (var (name, setting) in theme.Parameters)
            {
                var value = settings.TryGetValue(name, out var configuredValue) && configuredValue is not null
                    ? configuredValue
                    : setting.DefaultValue;

                if (value is null)
                    continue;

                if (ConflictsWithGlobalData(name, globalData))
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
                if (value is null || theme.Parameters.ContainsKey(name) || ConflictsWithGlobalData(name, globalData))
                    continue;

                renderer.CommonData[name] = value;
            }
        }

        /// <summary>
        /// Reports whether a theme setting conflicts with global template data.
        /// </summary>
        /// <param name="name">The theme setting name.</param>
        /// <param name="globalData">The global data whose names are reserved.</param>
        /// <returns><see langword="true"/> when the setting conflicts with global data; otherwise, <see langword="false"/>.</returns>
        private bool ConflictsWithGlobalData(string name, IReadOnlyDictionary<string, object?> globalData)
        {
            if (!globalData.ContainsKey(name))
                return false;

            reporter.LogWarning($"Theme setting '{name}' conflicts with a built-in global value and was ignored.");
            return true;
        }

        /// <summary>
        /// Publishes non-null global data to the renderer.
        /// </summary>
        /// <param name="renderer">The template renderer to assign global data to.</param>
        /// <param name="globalData">The global data to publish.</param>
        private static void PublishGlobalData(TemplateRenderer renderer, IReadOnlyDictionary<string, object?> globalData)
        {
            foreach (var (name, value) in globalData)
            {
                if (value is not null)
                    renderer.CommonData[name] = value;
            }
        }
    }
}

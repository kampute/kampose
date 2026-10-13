// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Models
{
    using Kampose.Support;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Configures the inputs, routing, theme, output, and quality checks used to generate a documentation site.
    /// </summary>
    public sealed class Configuration
    {
        /// <summary>
        /// Gets or sets the directory used to resolve relative input and output paths.
        /// </summary>
        /// <value>
        /// The directory used to resolve relative assembly, XML documentation, topic, asset, and output paths.
        /// When the value is empty, the directory containing the configuration file is used.
        /// </value>
        public string BaseDirectory { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the absolute root URI of the generated documentation site.
        /// </summary>
        /// <value>
        /// The absolute root URI used to generate documentation links, or <see langword="null"/> to generate
        /// relative links that can be hosted at any location.
        /// </value>
        public Uri? BaseUrl { get; set; }

        /// <summary>
        /// Gets the glob patterns for locating assembly files to be documented.
        /// </summary>
        /// <value>
        /// A <see cref="FileGlobFilter"/> containing case-insensitive patterns, relative to
        /// <see cref="BaseDirectory"/>, that select .NET assemblies whose types and members are documented.
        /// </value>
        /// <remarks>
        /// Patterns without an extension are treated as <c>.dll</c> patterns. For each matched assembly, a
        /// same-named <c>.xml</c> file in the same directory is loaded automatically when present. This filter
        /// may be empty when <see cref="Topics"/> contains at least one pattern.
        /// </remarks>
        public FileGlobFilter Assemblies { get; } = [];

        /// <summary>
        /// Gets the glob patterns for locating XML documentation files.
        /// </summary>
        /// <value>
        /// A <see cref="FileGlobFilter"/> containing case-insensitive patterns, relative to
        /// <see cref="BaseDirectory"/>, that select additional XML documentation files.
        /// </value>
        /// <remarks>
        /// Patterns without an extension are treated as <c>.xml</c> patterns. Use this filter when XML files are
        /// not located beside, or are not named after, their assemblies. Matching files are loaded in addition to
        /// XML documentation discovered beside assemblies.
        /// </remarks>
        public FileGlobFilter XmlDocs { get; } = [];

        /// <summary>
        /// Gets the rules for linking code elements from external assemblies.
        /// </summary>
        /// <value>
        /// Rules that associate external namespace patterns with a documentation site and URL convention.
        /// </value>
        /// <remarks>
        /// These rules are used only for referenced types and members that are not part of the documented assemblies.
        /// </remarks>
        public List<DocReference> References { get; } = [];

        /// <summary>
        /// Gets the glob patterns for locating topic files to be included in the documentation output.
        /// </summary>
        /// <value>
        /// A <see cref="FileGlobFilter"/> containing case-insensitive patterns, relative to
        /// <see cref="BaseDirectory"/>, that select Markdown topic files such as guides and tutorials.
        /// </value>
        /// <remarks>
        /// Patterns without an extension are treated as <c>.md</c> patterns. Matching Markdown files are converted
        /// to the selected output format and included alongside API reference pages.
        /// <para>
        /// The default pattern is <c>*.md</c>, which matches Markdown files directly inside
        /// <see cref="BaseDirectory"/>.
        /// </para>
        /// </remarks>
        [JsonConverter(typeof(OverwritingCollectionJsonConverter<FileGlobFilter, string>))]
        public FileGlobFilter Topics { get; init; } = ["*.md"];

        /// <summary>
        /// Gets the explicit ordering list for topic files in the documentation.
        /// </summary>
        /// <value>
        /// Topic paths that are placed first, in the configured order.
        /// </value>
        /// <remarks>
        /// Matching is case-insensitive and accepts relative paths or filenames with or without extensions.
        /// Unlisted topics follow in alphabetical title order, and entries that do not match a topic are ignored.
        /// When this list is empty, all topics are sorted alphabetically by title.
        /// </remarks>
        public List<string> TopicOrder { get; } = [];

        /// <summary>
        /// Gets or sets the method used to derive parent-child relationships between topics.
        /// </summary>
        /// <value>
        /// The method used to organize topic files into a hierarchy.
        /// </value>
        /// <remarks>
        /// The available methods are:
        /// <list type="bullet">
        ///   <item>
        ///   <term><see cref="FileTopicHierarchyMethod.None"/></term>
        ///   <description>No hierarchy is constructed; all topics are treated as top-level items.</description>
        ///   </item>
        ///   <item>
        ///   <term><see cref="FileTopicHierarchyMethod.Directory"/></term>
        ///   <description>Files with names matching directory names become parent topics for files within those directories.</description>
        ///   </item>
        ///   <item>
        ///   <term><see cref="FileTopicHierarchyMethod.Index"/></term>
        ///   <description>Files named "overview" are designated as parent topics for other files in the same directory.</description>
        ///   </item>
        ///   <item>
        ///   <term><see cref="FileTopicHierarchyMethod.Prefix"/></term>
        ///   <description>Files with fewer dot-separated segments in their names become parent topics for files with more segments that share the same prefix.</description>
        ///   </item>
        /// </list>
        /// The default value is <see cref="FileTopicHierarchyMethod.None"/>.
        /// </remarks>
        public FileTopicHierarchyMethod TopicHierarchy { get; set; } = FileTopicHierarchyMethod.None;

        /// <summary>
        /// Gets the rules for copying static assets into the documentation output.
        /// </summary>
        /// <value>
        /// Rules that select static files such as images, stylesheets, and scripts and assign their output directories.
        /// </value>
        /// <remarks>
        /// Assets are copied unchanged. Each rule flattens its matched files into its configured target directory by
        /// retaining only each source filename. If multiple files resolve to the same destination, the last collected
        /// asset replaces the earlier one.
        /// </remarks>
        public List<FileTransferFilter> Assets { get; } = [];

        /// <summary>
        /// Gets or sets the directory that receives the generated documentation.
        /// </summary>
        /// <value>
        /// The directory that receives generated pages, theme bundles, and copied assets. A relative path is resolved
        /// against <see cref="BaseDirectory"/>.
        /// </value>
        /// <remarks>
        /// This value must not be empty. The directory is created when needed and may be cleared before generation when
        /// the build command uses <c>--clean</c>.
        /// </remarks>
        public required string OutputDirectory { get; set; }

        /// <summary>
        /// Gets or sets the documentation convention used when generating pages.
        /// </summary>
        /// <value>
        /// The convention that determines output format, page grouping, URL layout, and the built-in theme directory.
        /// The default is <see cref="DocConvention.DocFx"/>.
        /// </value>
        /// <remarks>
        /// <see cref="DocConvention.DotNet"/> and <see cref="DocConvention.DocFx"/> generate HTML.
        /// <see cref="DocConvention.DevOps"/> generates Markdown for Azure DevOps Wiki.
        /// </remarks>
        public DocConvention Convention { get; set; } = DocConvention.DocFx;

        /// <summary>
        /// Gets or sets the theme to be used for the documentation.
        /// </summary>
        /// <value>
        /// The directory name of a built-in theme or the absolute path of a custom theme directory.
        /// </value>
        /// <remarks>
        /// A relative value is resolved beneath Kampose's theme directory for the selected <see cref="Convention"/>.
        /// The directory must contain a <c>theme.json</c> file. The default value is <c>classic</c>.
        /// </remarks>
        public string Theme { get; set; } = "classic";

        /// <summary>
        /// Gets or sets values supplied to the selected theme.
        /// </summary>
        /// <value>
        /// A case-insensitive dictionary of theme parameter names and configured values.
        /// </value>
        /// <remarks>
        /// A non-<see langword="null"/> value overrides the parameter's default from <c>theme.json</c>.
        /// Unknown names are still exposed to templates and theme scripts for custom use.
        /// </remarks>
        public Dictionary<string, object?> ThemeSettings { get; set; } = [];

        /// <summary>
        /// Gets the settings for documentation completeness checks and link validation.
        /// </summary>
        /// <value>
        /// The settings applied when inspecting XML documentation and validating referenced URLs.
        /// </value>
        /// <remarks>
        /// Issues are reported as warnings unless <see cref="AuditConfiguration.StopOnIssues"/> is enabled.
        /// </remarks>
        public AuditConfiguration Audit { get; } = new();

        /// <summary>
        /// Returns a collection of validation errors for the configuration.
        /// </summary>
        /// <returns>An enumerable collection of validation errors, if any.</returns>
        public IEnumerable<(string Key, string Message)> GetValidationErrors()
        {
            if (!Directory.Exists(BaseDirectory))
                yield return (nameof(BaseDirectory), $"Directory '{BaseDirectory}' does not exist.");

            if (BaseUrl is not null && !BaseUrl.IsAbsoluteUri)
                yield return (nameof(BaseUrl), "The base URL must be an absolute URI.");

            if (Assemblies.Count == 0 && Topics.Count == 0)
                yield return ($"{nameof(Assemblies)}", "The assemblies filter is required when no filter for topics is specified.");

            for (var i = 0; i < References.Count; i++)
            {
                var reference = References[i];
                var referenceKey = $"{nameof(References)}[{i}]";

                if (reference.Namespaces.Count == 0)
                    yield return ($"{referenceKey}.{nameof(reference.Namespaces)}", "At least one namespace is required.");

                if (reference.Url is null)
                    yield return ($"{referenceKey}.{nameof(reference.Url)}", "The URL is required.");
                else if (!reference.Url.IsAbsoluteUri)
                    yield return ($"{referenceKey}.{nameof(reference.Url)}", "The URL cannot be relative.");
            }

            if (string.IsNullOrWhiteSpace(OutputDirectory))
                yield return (nameof(OutputDirectory), "The output directory is required.");

            if (string.IsNullOrWhiteSpace(Theme))
                yield return (nameof(Theme), "The theme is required.");
        }

        /// <summary>
        /// Loads a configuration from the specified file path.
        /// </summary>
        /// <param name="path">The path to the configuration file.</param>
        /// <returns>A new instance of <see cref="Configuration"/> loaded from the specified file path.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="path"/> is <see langword="null"/>.</exception>
        /// <exception cref="FileNotFoundException">Thrown when the file does not exist.</exception>
        /// <exception cref="ValidationException">Thrown when the file is empty or invalid.</exception>
        public static Configuration LoadFromFile(string path)
        {
            var configuration = Json.ReadFileWithSchemaValidation<Configuration>(path);

            configuration.BaseDirectory = string.IsNullOrEmpty(configuration.BaseDirectory)
                ? Path.GetDirectoryName(Path.GetFullPath(path))!
                : Path.GetFullPath(configuration.BaseDirectory);

            var errors = configuration.GetValidationErrors();
            if (errors.Any())
                throw new ValidationException($"Configuration file is invalid: {path}", errors.Select(static e => $"{e.Key}: {e.Message}"));

            configuration.OutputDirectory = Path.GetFullPath(Path.Combine(configuration.BaseDirectory, configuration.OutputDirectory));
            foreach (var filter in configuration.Assets)
                filter.TargetPath = Path.GetFullPath(Path.Combine(configuration.OutputDirectory, filter.TargetPath));

            return configuration;
        }
    }
}

// Copyright (C) 2025 Kampute
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

    /// <summary>
    /// Represents the configuration for the documentation generation process.
    /// </summary>
    public sealed class Configuration
    {
        /// <summary>
        /// Gets or sets the base directory of the relative paths referenced in the configuration.
        /// </summary>
        /// <value>
        /// The base directory path used to resolve relative paths in the configuration. When not specified,
        /// the directory containing the configuration file is used as the base directory.
        /// </value>
        public string BaseDirectory { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the base URI for the documentation site.
        /// </summary>
        /// <value>
        /// The base URL for the documentation site. This URL is used as the root for all links in the generated
        /// documentation. If not specified, the URLs will be relative to the output directory.
        /// </value>
        public Uri? BaseUrl { get; set; }

        /// <summary>
        /// Gets the glob patterns for locating assembly files to be documented.
        /// </summary>
        /// <value>
        /// A <see cref="FileGlobFilter"/> containing glob patterns for locating .NET assembly (.dll) files
        /// whose types and members will be included in the generated documentation. At least one pattern
        /// must be specified.
        /// </value>
        /// <remarks>
        /// The assembly files are searched within the <see cref="BaseDirectory"/> and its subdirectories.
        /// For each assembly, the tool will attempt to locate a corresponding XML documentation file with the same name.
        /// </remarks>
        public FileGlobFilter Assemblies { get; } = [];

        /// <summary>
        /// Gets the glob patterns for locating XML documentation files.
        /// </summary>
        /// <value>
        /// A <see cref="FileGlobFilter"/> containing glob patterns for locating XML documentation files that may be in
        /// different directories or have different naming conventions than the assemblies being documented.
        /// </value>
        /// <remarks>
        /// This property is optional and only needed when XML documentation files cannot be automatically located alongside
        /// their corresponding assemblies. XML files specified here will be loaded in addition to any XML files found
        /// automatically.
        /// </remarks>
        public FileGlobFilter XmlDocs { get; } = [];

        /// <summary>
        /// Gets the list of settings for external documentation references.
        /// </summary>
        /// <value>
        /// A list of <see cref="DocReference"/> objects that define how to resolve documentation URLs for external
        /// code elements (types and members) referenced by the assemblies being documented.
        /// </value>
        /// <remarks>
        /// External references allow linking from your documentation to other documentation sites (like .NET API Browser or
        /// your organization's other API documentation) when your code references types or members from external libraries.
        /// </remarks>
        public List<DocReference> References { get; } = [];

        /// <summary>
        /// Gets the glob patterns for locating topic files to be included in the documentation output.
        /// </summary>
        /// <value>
        /// A <see cref="FileGlobFilter"/> containing glob patterns for locating topic files (such as guides, tutorials, or
        /// conceptual documentation) that will be converted to the format of documentation pages and included in the output.
        /// </value>
        /// <remarks>
        /// Topic files provide additional documentation content beyond API reference documentation. These files will be processed
        /// by appropriate converters based on their file extension (e.g., Markdown to HTML) before being included in the documentation
        /// output.
        /// <para>
        /// The default pattern is "*.md", which matches Markdown files in the base directory.
        /// </para>
        /// </remarks>
        public FileGlobFilter Topics { get; } = ["*.md"];

        /// <summary>
        /// Gets the explicit ordering list for topic files in the documentation.
        /// </summary>
        /// <value>
        /// A list of filenames or file paths that define the explicit ordering of topics.
        /// </value>
        /// <remarks>
        /// The values can be relative file paths with or without extensions (e.g., "getting-started.md" or "guides/advanced").
        /// If this list is empty, topics will maintain the order in which they were discovered by the file system.
        /// </remarks>
        public List<string> TopicOrder { get; } = [];

        /// <summary>
        /// Gets or sets the method used to organize topics in a hierarchy structure.
        /// </summary>
        /// <value>
        /// The <see cref="FileTopicHierarchyMethod"/> value that determines how topics are organized into a hierarchy.
        /// </value>
        /// <remarks>
        /// This property specifies the strategy for constructing parent-child relationships among topic files.
        /// The available methods include:
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
        /// Gets the collection of filters for asset files to be included in the documentation output.
        /// </summary>
        /// <value>
        /// A list of <see cref="FileTransferFilter"/> objects that define glob patterns for locating static asset files
        /// (such as images, stylesheets, or scripts) and specify their destination directories in the output.
        /// </value>
        /// <remarks>
        /// Unlike topic files, assets are copied to the output directory as-is without any content transformation. Each <see cref="FileTransferFilter"/>
        /// specifies both the source glob patterns and a target directory within the output directory.
        /// </remarks>
        public List<FileTransferFilter> Assets { get; } = [];

        /// <summary>
        /// Gets or sets the output directory where the generated documentation files should be stored.
        /// </summary>
        /// <value>
        /// The relative or absolute path to the directory where all generated documentation files will be written.
        /// If a relative path is provided, it is resolved against the <see cref="BaseDirectory"/>.
        /// </value>
        /// <remarks>
        /// This property is required for configuration to be valid. If the directory does not exist, it will be created
        /// during the documentation generation process.
        /// </remarks>
        public required string OutputDirectory { get; set; }

        /// <summary>
        /// Gets or sets the documentation convention used when generating pages.
        /// </summary>
        /// <value>
        /// The <see cref="DocConvention"/> value that determines the site URL patterns and page grouping used when generating
        /// documentation.
        /// </value>
        /// <remarks>
        /// This property is required for configuration to be valid. The chosen convention affects how documentation pages
        /// are organized, how URLs are constructed, and the overall structure of the documentation site.
        /// </remarks>
        public required DocConvention Convention { get; set; }

        /// <summary>
        /// Gets or sets the theme to be used for the documentation.
        /// </summary>
        /// <value>
        /// The identifier of a theme that controls the visual appearance of the generated documentation.
        /// </value>
        /// <remarks>
        /// The identifier of a theme is its directory name within the themes directory corresponding to the chosen
        /// <see cref="Convention"/> (e.g. "html" or "md"). The default theme is "classic".
        /// </remarks>
        public string Theme { get; set; } = "classic";

        /// <summary>
        /// Gets or sets the settings for the selected theme to customize its appearance and behavior.
        /// </summary>
        /// <value>
        /// A dictionary of key-value pairs where keys are theme-specific setting names and values are the
        /// corresponding setting values that customize the theme's appearance and behavior.
        /// </value>
        /// <remarks>
        /// Available settings depend on the chosen theme. Use the <c>--show-theme-params</c> command-line option
        /// to see the available parameters for a specific theme.
        /// </remarks>
        public Dictionary<string, object?> ThemeSettings { get; set; } = [];

        /// <summary>
        /// Gets the settings for XML documentation auditing.
        /// </summary>
        /// <value>
        /// An <see cref="AuditConfiguration"/> object that contains configuration for XML documentation analysis
        /// to detect missing or incomplete elements.
        /// </value>
        /// <remarks>
        /// The audit process checks XML documentation for missing or incomplete elements based on configurable options.
        /// If issues are found, they will be reported and may optionally cause the documentation generation to stop.
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

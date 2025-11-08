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

    /// <summary>
    /// Represents a theme for generating documentation.
    /// </summary>
    public sealed class Theme
    {
        private readonly Dictionary<string, ThemeParameter> parameters = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> templateFiles = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> assetFiles = new(StringComparer.OrdinalIgnoreCase);
        private readonly SortedDictionary<string, IReadOnlyList<string>> scriptFiles = new(StringComparer.OrdinalIgnoreCase);
        private readonly SortedDictionary<string, IReadOnlyList<string>> styleFiles = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets the unique identifier for the theme.
        /// </summary>
        /// <value>
        /// The unique identifier for the theme, which is typically the directory name of the theme.
        /// </value>
        public required string Id { get; init; }

        /// <summary>
        /// Gets the metadata for the theme.
        /// </summary>
        /// <value>
        /// The metadata for the theme, or <see langword="null"/> if no metadata is provided.
        /// </value>
        public ThemeMetadata? Metadata { get; private set; }

        /// <summary>
        /// Gets the configuration parameters for the theme.
        /// </summary>
        /// <value>
        /// A dictionary containing all theme parameters with their associated metadata.
        /// </value>
        public IReadOnlyDictionary<string, ThemeParameter> Parameters => parameters;

        /// <summary>
        /// Gets the template files of the theme.
        /// </summary>
        /// <value>
        /// A read-only dictionary containing the template files of the theme, where the key is the template name and the value is the full path to the template file.
        /// </value>
        public IReadOnlyDictionary<string, string> TemplateFiles => templateFiles;

        /// <summary>
        /// Gets the JavaScript files of the theme.
        /// summary>
        /// <value>
        /// A read-only dictionary containing the JavaScript files of the theme, where the key is the path of the minified file and the value is a collection of full
        /// paths to the JavaScript files that will be included in the minified file.
        public IReadOnlyDictionary<string, IReadOnlyList<string>> ScriptFiles => scriptFiles;

        /// <summary>
        /// Gets the stylesheet files of the theme.
        /// <summary>
        /// <value>
        /// A read-only dictionary containing the stylesheet files of the theme, where the key is the path of the minified file and the value is a collection of full
        /// paths to the stylesheet files that will be included in the minified file.
        /// </value>
        public IReadOnlyDictionary<string, IReadOnlyList<string>> StyleFiles => styleFiles;

        /// <summary>
        /// Gets the asset files of the theme.
        /// </summary>
        /// <value>
        /// A read-only dictionary containing the asset files of the theme, where the key is the relative path to the asset file and the value is the full path to the
        /// asset file. The files in this collection will be copied to the output directory as-is.
        /// </value>
        public IReadOnlyDictionary<string, string> AssetFiles => assetFiles;

        /// <summary>
        /// Merges the settings, templates, and assets of the specified theme configuration with this theme while preserving the existing content.
        /// </summary>
        /// <param name="config">The theme configuration to merge with this theme.</param>
        /// <param name="directory">The directory where the theme configuration files are located.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="config"/> or <paramref name="directory"/> is <see langword="null"/>.</exception>
        private void Merge(ThemeConfiguration config, string directory)
        {
            ArgumentNullException.ThrowIfNull(config);
            ArgumentNullException.ThrowIfNull(directory);

            MergeFileBundles(directory, config.Scripts, scriptFiles, ".js");
            MergeFileBundles(directory, config.Styles, styleFiles, ".css");

            foreach (var fullPath in config.Templates.FindMatchingFiles(directory, ".hbs"))
            {
                var templateName = Path.GetFileNameWithoutExtension(fullPath);
                templateFiles.TryAdd(templateName, fullPath);
            }

            foreach (var fullPath in config.Assets.FindMatchingFiles(directory))
            {
                var relativePath = Path.GetRelativePath(directory, fullPath);
                assetFiles.TryAdd(relativePath, fullPath);
            }

            foreach (var (name, value) in config.Parameters)
                parameters.TryAdd(name, value);

            static void MergeFileBundles(
                string directory,
                FileTransferFilter filter,
                SortedDictionary<string, IReadOnlyList<string>> target,
                string defaultExtension)
            {
                var files = new List<string>();
                var fileSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                if (target.TryGetValue(filter.TargetPath, out var existing))
                {
                    files.AddRange(existing);
                    fileSet.UnionWith(existing);
                }

                foreach (var fullPath in filter.Source.FindMatchingFiles(directory, defaultExtension))
                {
                    if (fileSet.Add(fullPath))
                        files.Add(fullPath);
                }

                if (files.Count > 0)
                    target[filter.TargetPath] = files;
            }
        }

        /// <summary>
        /// Loads a theme configuration from the specified theme name and documentation convention.
        /// </summary>
        /// <param name="name">The name of a built-in theme or the path to a custom theme.</param>
        /// <param name="docConvention">The documentation convention to get the theme for.</param>
        /// <returns>An instance of <see cref="Theme"/> containing the configuration for the specified theme.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is <see langword="null"/> or empty.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown when the theme's directory does not exist.</exception>
        /// <exception cref="FileNotFoundException">Thrown when the theme configuration file does not exist.</exception>
        /// <exception cref="ValidationException">Thrown when the theme's configuration file is empty or invalid.</exception>
        public static Theme Load(string name, DocConvention docConvention)
        {
            ArgumentException.ThrowIfNullOrEmpty(name, nameof(name));

            var loadedConfiguration = new HashSet<string>();
            var themesDirectory = GetThemesDirectory(docConvention);
            var theme = new Theme { Id = name };

            var themeName = name;
            while (!string.IsNullOrEmpty(themeName) && loadedConfiguration.Add(themeName))
            {
                var directory = Path.Combine(themesDirectory, themeName);
                var config = ThemeConfiguration.LoadFromFile(Path.Combine(directory, "theme.json"));

                if (themeName == name)
                    theme.Metadata = config.Metadata;

                theme.Merge(config, directory);
                themeName = config.Base;
            }

            return theme;
        }

        /// <summary>
        /// Gets the themes directory for themes in the specified documentation convention.
        /// </summary>
        /// <param name="convention">The documentation convention to get the themes directory for.</param>
        /// <returns>The directory of themes for the specified documentation convention.</returns>
        /// <exception cref="NotSupportedException">Thrown when the <paramref name="convention"/> is not supported.</exception>
        private static string GetThemesDirectory(DocConvention convention)
        {
            var appDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)!;
            return Path.Combine(appDirectory, "themes", convention switch
            {
                DocConvention.DevOps => "md",
                DocConvention.DotNet or DocConvention.DocFx => "html",
                _ => throw new NotSupportedException($"The specified convention '{convention}' is not supported.")
            });
        }
    }
}

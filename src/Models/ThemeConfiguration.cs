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

    /// <summary>
    /// Defines a theme's inheritance, metadata, templates, resources, and configurable parameters.
    /// </summary>
    public sealed class ThemeConfiguration
    {
        /// <summary>
        /// Gets or sets the parent theme inherited by this theme.
        /// </summary>
        /// <value>
        /// The directory name of the parent theme, or <see langword="null"/> if this theme is standalone.
        /// </value>
        /// <remarks>
        /// Templates, bundles, assets, and parameter definitions in this theme take precedence over inherited
        /// entries with the same name or target path. A standalone theme must define at least one
        /// <see cref="Templates"/> pattern.
        /// </remarks>
        public string? Base { get; set; }

        /// <summary>
        /// Gets descriptive metadata for the theme.
        /// </summary>
        /// <value>
        /// Information about the theme that is exposed to templates.
        /// </value>
        /// <remarks>
        /// Metadata does not control theme loading or output selection.
        /// </remarks>
        public ThemeMetadata Metadata { get; } = new();

        /// <summary>
        /// Gets the parameter definitions supported by the theme.
        /// </summary>
        /// <value>
        /// A case-insensitive dictionary that defines parameter types, descriptions, and fallback values.
        /// </value>
        /// <remarks>
        /// Resolved non-<see langword="null"/> values are exposed by name to Handlebars templates and, when
        /// the theme produces a script bundle, through <c>window.kampose.config</c>.
        /// </remarks>
        public Dictionary<string, ThemeParameter> Parameters { get; } = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets the glob patterns that select the theme's Handlebars templates.
        /// </summary>
        /// <value>
        /// Case-insensitive patterns relative to the theme directory.
        /// </value>
        /// <remarks>
        /// Patterns without an extension are treated as <c>.hbs</c> patterns. Each matching template is
        /// registered by filename without its extension. Prefix a pattern with <c>!</c> to exclude matches.
        /// </remarks>
        public FileGlobFilter Templates { get; } = [];

        /// <summary>
        /// Gets the rule used to build the theme's JavaScript bundle.
        /// </summary>
        /// <value>
        /// The source patterns and output-relative target path for the JavaScript bundle. The target path defaults
        /// to <c>script.js</c>.
        /// </value>
        /// <remarks>
        /// Source patterns are relative to the theme directory and default to the <c>.js</c> extension when
        /// none is specified. Matching files are concatenated and minified into
        /// <see cref="FileTransferFilter.TargetPath"/>. The first generated script bundle also receives the
        /// global <c>window.kampose</c> data used by theme scripts.
        /// </remarks>
        public FileTransferFilter Scripts { get; } = new()
        {
            TargetPath = "script.js"
        };

        /// <summary>
        /// Gets the rule used to build the theme's stylesheet bundle.
        /// </summary>
        /// <value>
        /// The source patterns and output-relative target path for the stylesheet bundle. The target path defaults
        /// to <c>styles.css</c>.
        /// </value>
        /// <remarks>
        /// Source patterns are relative to the theme directory and default to the <c>.css</c> extension when
        /// none is specified. Matching files are concatenated and minified into
        /// <see cref="FileTransferFilter.TargetPath"/>.
        /// </remarks>
        public FileTransferFilter Styles { get; } = new()
        {
            TargetPath = "styles.css"
        };

        /// <summary>
        /// Gets the glob patterns that select the theme's static assets.
        /// </summary>
        /// <value>
        /// Case-insensitive patterns relative to the theme directory.
        /// </value>
        /// <remarks>
        /// Matching files are copied unchanged while preserving paths relative to the theme directory.
        /// This theme takes precedence when an inherited theme provides the same relative path.
        /// </remarks>
        public FileGlobFilter Assets { get; } = [];

        /// <summary>
        /// Loads a theme configuration from the specified file path.
        /// </summary>
        /// <param name="path">The path to the theme configuration file.</param>
        /// <returns>An instance of <see cref="ThemeConfiguration"/> loaded from the specified file path.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="path"/> is <see langword="null"/>.</exception>
        /// <exception cref="FileNotFoundException">Thrown when the file does not exist.</exception>
        /// <exception cref="ValidationException">Thrown when the file is empty or invalid.</exception>
        public static ThemeConfiguration LoadFromFile(string path)
        {
            var themeConfiguration = Json.ReadFileWithSchemaValidation<ThemeConfiguration>(path);

            if (string.IsNullOrEmpty(themeConfiguration.Base) && themeConfiguration.Templates.Count == 0)
                throw new ValidationException($"Theme configuration file is invalid: {path}", ["Templates: A standalone theme must define template patterns."]);

            return themeConfiguration;
        }
    }
}

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
    /// Represents the configuration for a theme.
    /// </summary>
    public sealed class ThemeConfiguration
    {
        /// <summary>
        /// Gets or sets the theme that this theme is based on.
        /// </summary>
        /// <value>
        /// The identifier of the theme that this theme is based on, or <see langword="null"/> if the theme is standalone.
        /// </value>
        /// <remarks>
        /// The identifier of a theme is its directory name, which is typically the name of the theme also.
        /// Use this to implement theme inheritance or to extend existing themes with customizations.
        /// </remarks>
        public string? Base { get; set; }

        /// <summary>
        /// Gets the metadata for the theme.
        /// </summary>
        /// <value>
        /// The metadata for the theme.
        /// </value>
        /// <remarks>
        /// Metadata is informational only and not used by the application logic. Use it to provide details
        /// such as author, version, and description for documentation or UI display.
        /// </remarks>
        public ThemeMetadata Metadata { get; } = new();

        /// <summary>
        /// Gets the configuration parameters for the theme.
        /// </summary>
        /// <value>
        /// A dictionary of theme parameters and their metadata.
        /// </value>
        /// <remarks>
        /// Parameters are accessible as variables in the theme's templates and as properties of the global
        /// <c>kampose.config</c> object in the theme's JavaScript files. Use parameters to make your theme
        /// customizable.
        /// </remarks>
        public Dictionary<string, ThemeParameter> Parameters { get; } = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets the glob patterns for the template files of the theme.
        /// </summary>
        /// <value>
        /// The glob patterns for the template files of the theme.
        /// </value>
        /// <remarks>
        /// These patterns are used to locate template files in the theme's directory. You can use exclusion
        /// patterns by prefixing them with an exclamation mark (!).
        /// </remarks>
        public FileGlobFilter Templates { get; } = [];

        /// <summary>
        /// Gets the glob patterns for the JavaScript files of the theme.
        /// </summary>
        /// <value>
        /// The glob patterns for the JavaScript files (.js) of the theme.
        /// </value>
        /// <remarks>
        /// All matching JavaScript files will be minified and bundled into a single output file.
        /// The <c>TargetPath</c> property specifies the name of the output file.
        /// </remarks>
        public FileTransferFilter Scripts { get; } = new()
        {
            TargetPath = "script.js"
        };

        /// <summary>
        /// Gets the glob patterns for the stylesheet files of the theme.
        /// </summary>
        /// <value>
        /// The patterns for the stylesheet files (.css) of the theme.
        /// </value>
        /// <remarks>
        /// All matching CSS files will be minified and bundled into a single output file.
        /// The <c>TargetPath</c> property specifies the name of the output file.
        /// </remarks>
        public FileTransferFilter Styles { get; } = new()
        {
            TargetPath = "styles.css"
        };

        /// <summary>
        /// Gets the glob patterns for the asset files of the theme.
        /// </summary>
        /// <value>
        /// The glob patterns for the asset files of the theme.
        /// </value>
        /// <remarks>
        /// Asset files matching these patterns will be copied to the output directory as-is, preserving
        /// their original folder structure.
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
        public static ThemeConfiguration LoadFromFile(string path) => Json.ReadFileWithSchemaValidation<ThemeConfiguration>(path);
    }
}

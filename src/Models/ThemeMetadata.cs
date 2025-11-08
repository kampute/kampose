// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Models
{
    using System;

    /// <summary>
    /// Represents metadata for a Kampose theme, as defined in the theme configuration schema.
    /// </summary>
    public sealed class ThemeMetadata
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ThemeMetadata"/> class.
        /// </summary>
        public ThemeMetadata() { }

        /// <summary>
        /// The format of content that the theme is designed to render.
        /// </summary>
        /// <value>
        /// The format string indicating the type of content the theme is designed to render, such as "html", "md", etc.
        /// </value>
        public string? Format { get; set; }

        /// <summary>
        /// The display name of the theme.
        /// <summary>
        /// <value>
        /// The display name of the theme, which is typically used in user interfaces or documentation.
        /// </value>
        public string? Name { get; set; }

        /// <summary>
        /// The version of the theme.
        /// </summary>
        /// <value>
        /// The theme version, which can be used for versioning purposes.
        /// </value>
        public string? Version { get; set; }

        /// <summary>
        /// A brief description of the theme.
        /// </summary>
        /// <value>
        /// The theme description, explaining its purpose and features, or <see langword="null"/> if not provided.
        /// </value>
        public string? Description { get; set; }

        /// <summary>
        /// The name of the theme's author.
        /// </summary>
        /// <value>
        /// The name of the person or organization that created the theme, or <see langword="null"/> if not specified.
        /// </value>
        public string? Author { get; set; }

        /// <summary>
        /// The license under which the theme is distributed, if applicable.
        /// </summary>
        /// <value>
        /// The license string indicating the terms under which the theme can be used, or <see langword="null"/> if not specified.
        /// </value>
        public string? License { get; set; }

        /// <summary>
        /// The URL of the theme's homepage or documentation.
        /// </summary>
        /// <value>
        /// The URI pointing to the theme's homepage or documentation, or <see langword="null"/> if not provided.
        /// </value>
        public Uri? Homepage { get; set; }
    }
}

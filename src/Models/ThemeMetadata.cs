// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Models
{
    using System;

    /// <summary>
    /// Provides descriptive information about a Kampose theme.
    /// </summary>
    public sealed class ThemeMetadata
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ThemeMetadata"/> class.
        /// </summary>
        public ThemeMetadata() { }

        /// <summary>
        /// Gets or sets the output format that the theme is intended to render.
        /// </summary>
        /// <value>
        /// A format identifier such as <c>html</c> or <c>md</c>, or <see langword="null"/> when unspecified.
        /// </value>
        /// <remarks>
        /// This value is informational. The documentation convention selects the actual output format.
        /// </remarks>
        public string? Format { get; set; }

        /// <summary>
        /// Gets or sets the human-readable name of the theme.
        /// </summary>
        /// <value>
        /// The name displayed in documentation or user interfaces, or <see langword="null"/> when unspecified.
        /// </value>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the theme's release version.
        /// </summary>
        /// <value>
        /// The version displayed as metadata, or <see langword="null"/> when unspecified.
        /// </value>
        /// <remarks>
        /// Kampose does not interpret or compare this value.
        /// </remarks>
        public string? Version { get; set; }

        /// <summary>
        /// Gets or sets a brief summary of the theme.
        /// </summary>
        /// <value>
        /// A summary of the theme's intended use or distinguishing features, or <see langword="null"/> when unspecified.
        /// </value>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the person or organization that maintains the theme.
        /// </summary>
        /// <value>
        /// The maintainer's name, or <see langword="null"/> when unspecified.
        /// </value>
        public string? Author { get; set; }

        /// <summary>
        /// Gets or sets the license under which the theme is distributed.
        /// </summary>
        /// <value>
        /// A license name or identifier such as <c>MIT</c>, or <see langword="null"/> when unspecified.
        /// </value>
        public string? License { get; set; }

        /// <summary>
        /// Gets or sets the theme's homepage, repository, or documentation URI.
        /// </summary>
        /// <value>
        /// An absolute URI containing more information about the theme, or <see langword="null"/> when unspecified.
        /// </value>
        public Uri? Homepage { get; set; }
    }
}

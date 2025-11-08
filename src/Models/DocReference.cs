// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Models
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents the settings for resolving the documentation URLs of external code elements
    /// referenced by the assemblies to be documented.
    /// </summary>
    public sealed class DocReference
    {
        /// <summary>
        /// Gets the collection of namespaces that identify the code elements for which
        /// documentation URLs are to be resolved.
        /// </summary>
        /// <value>
        /// The collection of namespaces that identify the code elements for which documentation
        /// URLs are to be resolved. The namespaces are case-sensitive and may contain a single
        /// wildcard (*) as the last segment.
        /// </value>
        public HashSet<string> Namespaces { get; } = new(StringComparer.Ordinal);

        /// <summary>
        /// Gets or sets the strategy used for resolving the documentation URLs of code elements.
        /// </summary>
        /// <value>
        /// The strategy used for resolving the documentation URLs of code elements. This value
        /// determines the format and structure of the URLs.
        /// </value>
        public required DocReferenceStrategy Strategy { get; set; }

        /// <summary>
        /// Gets or sets the full URL of the documentation site.
        /// </summary>
        /// <value>
        /// The full URL of the documentation site where the documentation for the code elements
        /// can be found.
        /// </value>
        public required Uri Url { get; set; }

        /// <summary>
        /// Gets or sets the file extension used in the documentation URLs.
        /// </summary>
        /// <value>
        /// The file extension used in the documentation URLs if the URLs are file-based and have a
        /// file extension other than the default; otherwise, <see langword="null"/>.
        /// </value>
        public string? Extension { get; set; }
    }
}

// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Models
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Maps external namespace patterns to a documentation site and URL convention.
    /// </summary>
    public sealed class DocReference
    {
        /// <summary>
        /// Gets the namespace patterns whose external code elements use this reference.
        /// </summary>
        /// <value>
        /// A non-empty, case-sensitive set of namespace patterns. A pattern may end with a single
        /// wildcard (<c>*</c>) segment to match descendant namespaces.
        /// </value>
        public HashSet<string> Namespaces { get; } = new(StringComparer.Ordinal);

        /// <summary>
        /// Gets or sets the URL convention used for matching external code elements.
        /// </summary>
        /// <value>
        /// The required strategy that constructs a documentation URL or online search query for a referenced type
        /// or member.
        /// </value>
        public required DocReferenceStrategy Strategy { get; set; }

        /// <summary>
        /// Gets or sets the absolute base URI of the external documentation site.
        /// </summary>
        /// <value>
        /// The required absolute documentation-site URI, or the search endpoint when <see cref="Strategy"/> is
        /// <see cref="DocReferenceStrategy.OnlineSearch"/>.
        /// </value>
        public required Uri Url { get; set; }

        /// <summary>
        /// Gets or sets an override for the page extension produced by the selected strategy.
        /// </summary>
        /// <value>
        /// The page extension to use; an empty or whitespace-only string to omit page extensions; or
        /// <see langword="null"/> to retain the strategy's default. This value has no effect when
        /// <see cref="Strategy"/> is <see cref="DocReferenceStrategy.OnlineSearch"/>.
        /// </value>
        public string? Extension { get; set; }
    }
}

// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Represents a navigation element in the sitemap hierarchy.
    /// </summary>
    /// <remarks>
    /// The <see cref="SitemapNode"/> can be either a link to a specific documentation page or a group containing child nodes.
    /// Each node has a title for display purposes, and may optionally have a URL if it represents a page.
    /// </remarks>
    public class SitemapNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SitemapNode"/> class as a navigation link.
        /// </summary>
        /// <param name="title">The title of the page.</param>
        /// <param name="url">The URL of the page.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="title"/> is <see langword="null"/> or whitespace.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="url"/> is <see langword="null"/>.</exception>
        public SitemapNode(string title, Uri url)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(title);
            ArgumentNullException.ThrowIfNull(url);

            Title = title;
            Url = url;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SitemapNode"/> class as a navigation group.
        /// </summary>
        /// <param name="title">The name of the group.</param>
        /// <param name="children">The child nodes of the group.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="title"/> is <see langword="null"/> or whitespace.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="children"/> is <see langword="null"/>.</exception>
        public SitemapNode(string title, IEnumerable<SitemapNode> children)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(title);
            ArgumentNullException.ThrowIfNull(children);

            Title = title;
            Items = [.. children];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SitemapNode"/> class as a navigation group with an associated URL.
        /// </summary>
        /// <param name="title">The name of the group.</param>
        /// <param name="url">The URL of the group's documentation page.</param>
        /// <param name="children">The child nodes of the group.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="title"/> is <see langword="null"/> or whitespace.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="children"/> or <paramref name="url"/> is <see langword="null"/>.</exception>
        public SitemapNode(string title, Uri url, IEnumerable<SitemapNode> children)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(title);
            ArgumentNullException.ThrowIfNull(url);
            ArgumentNullException.ThrowIfNull(children);

            Title = title;
            Url = url;
            Items = children.Any() ? [.. children] : null;
        }

        /// <summary>
        /// Gets the display text of the sitemap node.
        /// </summary>
        /// <value>
        /// The display text of the sitemap node.
        /// </value>
        public string Title { get; }

        /// <summary>
        /// Gets the URL of the documentation page relative to the documentation root.
        /// </summary>
        /// <value>
        /// The <see cref="Uri"/> pointing to the documentation page (relative to the documentation root),
        /// or <see langword="null"/> if this is a group without an associated page.
        /// </value>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Uri? Url { get; }

        /// <summary>
        /// Gets the child nodes of this sitemap group.
        /// </summary>
        /// <value>
        /// A read-only collection of child <see cref="SitemapNode"/> objects, or <see langword="null"/> if this is a leaf node.
        /// </value>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IReadOnlyList<SitemapNode>? Items { get; }

        /// <summary>
        /// Calculates the total number of pages represented by the current object and its child items.
        /// </summary>
        /// <returns>The total number of pages, including the current page and all pages in the <see cref="Items"/> hierarchy.</returns>
        public int CountPages()
        {
            var count = 0;

            if (Url is not null)
                count++;

            if (Items is not null)
                count += Items.Sum(static node => node.CountPages());

            return count;
        }

        /// <summary>
        /// Returns a string representation of the current object.
        /// </summary>
        /// <returns>The value of the <see cref="Title"/> property.</returns>
        public override string ToString() => Title;
    }
}

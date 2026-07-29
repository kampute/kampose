// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Models
{
    /// <summary>
    /// Specifies how parent-child relationships are derived from topic paths and filenames.
    /// </summary>
    public enum FileTopicHierarchyMethod
    {
        /// <summary>
        /// Keeps every topic at the top level.
        /// </summary>
        /// <remarks>
        /// Topic ordering still follows the configured explicit order and alphabetical fallback.
        /// </remarks>
        None,

        /// <summary>
        /// Makes a topic whose filename matches a directory name the parent of topics in that directory.
        /// </summary>
        /// <remarks>
        /// Filename and directory-name matching is case-insensitive.
        /// </remarks>
        Directory,

        /// <summary>
        /// Makes an <c>overview</c> topic the parent of other topics in the same directory.
        /// </summary>
        /// <remarks>
        /// The <c>overview</c> filename is matched case-insensitively.
        /// </remarks>
        Index,

        /// <summary>
        /// Uses dot-separated filename prefixes to derive parent-child relationships.
        /// </summary>
        /// <remarks>
        /// A topic with fewer dot-separated filename segments becomes the parent of topics with more segments
        /// that share its prefix.
        /// </remarks>
        Prefix
    }
}

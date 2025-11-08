// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Models
{
    /// <summary>
    /// Specifies the method used to construct a file topic hierarchy.
    /// </summary>
    public enum FileTopicHierarchyMethod
    {
        /// <summary>
        /// No hierarchy is constructed.
        /// </summary>
        /// <remarks>
        /// This option is used when no parent-child relationships are desired among the file topics.
        /// </remarks>
        None,

        /// <summary>
        /// Constructs the hierarchy by matching file names to directory names.
        /// </summary>
        /// <remarks>
        /// Files with names matching directory names (case-insensitive) become parent topics for all files within those directories.
        /// </remarks>
        Directory,

        /// <summary>
        /// Constructs the hierarchy using "<c>overview</c>" as the index file name within directories.
        /// </summary>
        /// <remarks>
        /// Files with the name "overview" (case-insensitive) are designated as parent topics for other files in the same directory.
        /// </remarks>
        Index,

        /// <summary>
        /// Constructs the hierarchy using '<c>.</c>' as the delimiter for prefixes in file names.
        /// </summary>
        /// <remarks>
        /// Files with fewer dot-separated segments in their names become parent topics for files with more segments that share the same prefix.
        /// </remarks>
        Prefix
    }
}

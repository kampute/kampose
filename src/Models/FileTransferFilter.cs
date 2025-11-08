// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Models
{
    using Kampose.Support;

    /// <summary>
    /// Represents a filter for transferring files based on glob patterns.
    /// </summary>
    public sealed class FileTransferFilter
    {
        /// <summary>
        /// Gets the glob patterns for filtering files to be transferred.
        /// </summary>
        /// <value>
        /// The glob patterns used to filter files to be transferred.
        /// </value>
        public FileGlobFilter Source { get; } = [];

        /// <summary>
        /// Gets or sets the path where the filtered files will be transferred.
        /// </summary>
        /// <value>
        /// The path where the filtered files will be transferred. Depends on the context of the transfer operation,
        /// it could be an absolute or a relative file or directory path.
        /// </value>
        public string TargetPath { get; set; } = string.Empty;
    }
}

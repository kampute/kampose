// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Models
{
    using Kampose.Support;

    /// <summary>
    /// Selects files using glob patterns and assigns their transfer destination.
    /// </summary>
    public sealed class FileTransferFilter
    {
        /// <summary>
        /// Gets the glob patterns that select source files.
        /// </summary>
        /// <value>
        /// Case-insensitive glob patterns relative to the transfer operation's source directory.
        /// A pattern prefixed with <c>!</c> excludes matching files.
        /// </value>
        public FileGlobFilter Source { get; } = [];

        /// <summary>
        /// Gets or sets the destination path for matching files.
        /// </summary>
        /// <value>
        /// A destination whose interpretation depends on the operation. Configuration assets use an output directory,
        /// while theme script and style bundles use an output-relative file path.
        /// </value>
        public string TargetPath { get; set; } = string.Empty;
    }
}

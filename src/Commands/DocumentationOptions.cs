// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Commands
{
    /// <summary>
    /// Represents the options shared by the commands that process the documentation configuration.
    /// </summary>
    public class DocumentationOptions
    {
        /// <summary>
        /// The configuration file used when none is specified.
        /// </summary>
        public const string DefaultConfigFile = "kampose.json";

        /// <summary>
        /// Gets or sets the configuration file path.
        /// </summary>
        /// <value>
        /// The path to the configuration file.
        /// </value>
        public string ConfigPath { get; set; } = DefaultConfigFile;

        /// <summary>
        /// Gets or sets a value indicating whether detailed logging is enabled.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if detailed logging is enabled; otherwise, <see langword="false"/>.
        /// </value>
        public bool Debug { get; set; } = false;
    }
}

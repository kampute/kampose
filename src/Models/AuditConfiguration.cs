// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Models
{
    using Kampute.DocToolkit.XmlDoc;
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Configures XML documentation completeness checks and link validation.
    /// </summary>
    public sealed class AuditConfiguration
    {
        /// <summary>
        /// Gets the XML documentation elements and rule groups to inspect.
        /// </summary>
        /// <value>
        /// A case-insensitive set containing individual XML documentation element names or the
        /// <c>required</c>, <c>recommended</c>, or <c>all</c> rule groups. The default set contains
        /// <c>required</c>. An empty set disables XML documentation inspection.
        /// </value>
        [JsonConverter(typeof(Support.OverwritingCollectionJsonConverter<HashSet<string>, string>))]
        public HashSet<string> Options { get; init; } = new(StringComparer.OrdinalIgnoreCase)
        {
            nameof(XmlDocInspectionOptions.Required)
        };

        /// <summary>
        /// Gets or sets a value indicating whether compiler-generated public constructors are inspected.
        /// </summary>
        /// <value>
        /// <see langword="true"/> to require implicit constructors to satisfy the selected documentation checks;
        /// otherwise, <see langword="false"/> to omit them. The default is <see langword="false"/>.
        /// </value>
        public bool IncludeImplicitConstructors { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether link validation checks external URLs.
        /// </summary>
        /// <value>
        /// <see langword="true"/> to allow network requests that verify external URLs found in XML comments and
        /// Markdown topics; otherwise, <see langword="false"/>. Internal references are validated regardless of
        /// this value. The default is <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// Enabling this option can increase build time and requires network access.
        /// </remarks>
        public bool VerifyExternalLinks { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether documentation issues fail the build.
        /// </summary>
        /// <value>
        /// <see langword="true"/> to fail the build when XML documentation is missing, an inspection issue is
        /// reported, or a referenced URL is invalid; otherwise, <see langword="false"/> to report warnings and
        /// continue. The default is <see langword="false"/>.
        /// </value>
        public bool StopOnIssues { get; set; } = false;

        /// <summary>
        /// Gets or sets the combined inspection flags represented by this configuration.
        /// </summary>
        /// <value>
        /// A <see cref="XmlDocInspectionOptions"/> value representing the configured audit options.
        /// </value>
        public XmlDocInspectionOptions InspectionOptions
        {
            get
            {
                var result = IncludeImplicitConstructors ? XmlDocInspectionOptions.None : XmlDocInspectionOptions.OmitImplicitlyCreatedConstructors;
                foreach (var option in Options)
                {
                    if (Enum.TryParse<XmlDocInspectionOptions>(option, true, out var parsedOption))
                        result |= parsedOption;
                }
                return result;
            }
            set
            {
                Options.Clear();
                foreach (var option in Enum.GetValues<XmlDocInspectionOptions>())
                {
                    if (value.HasFlag(option) && option is not (XmlDocInspectionOptions.None or XmlDocInspectionOptions.OmitImplicitlyCreatedConstructors))
                        Options.Add(option.ToString());
                }

                IncludeImplicitConstructors = !value.HasFlag(XmlDocInspectionOptions.OmitImplicitlyCreatedConstructors);
            }
        }
    }
}

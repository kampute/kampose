// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Models
{
    /// <summary>
    /// Specifies the output format, URL layout, and page grouping used by the documentation generator.
    /// </summary>
    public enum DocConvention
    {
        /// <summary>
        /// Generates HTML with separate pages for types and members, following .NET API Browser URL conventions.
        /// </summary>
        DotNet,

        /// <summary>
        /// Generates HTML with members rendered on their declaring type's page, following DocFX URL conventions.
        /// </summary>
        DocFx,

        /// <summary>
        /// Generates Markdown with members rendered on their declaring type's page, following Azure DevOps Wiki
        /// URL conventions.
        /// </summary>
        DevOps,
    }
}

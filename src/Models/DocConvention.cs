// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Models
{
    /// <summary>
    /// Specifies the site conventions such as URL patterns and page grouping used by the documentation generator.
    /// </summary>
    public enum DocConvention
    {
        /// <summary>
        /// HTML format with separate pages for types and members, following .NET API Browser URL conventions.
        /// </summary>
        DotNet,

        /// <summary>
        /// HTML format with types and members sharing the same page, following DocFX URL conventions.
        /// </summary>
        DocFx,

        /// <summary>
        /// Markdown format with types and members sharing the same page, following Azure DevOps Wiki URL conventions.
        /// </summary>
        DevOps,
    }
}

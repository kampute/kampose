// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Models
{
    /// <summary>
    /// Specifies how documentation URLs are constructed for external types and members.
    /// </summary>
    public enum DocReferenceStrategy
    {
        /// <summary>
        /// Constructs URLs using .NET API Browser addressing conventions.
        /// </summary>
        DotNet,

        /// <summary>
        /// Constructs URLs using DocFX addressing conventions.
        /// </summary>
        DocFx,

        /// <summary>
        /// Constructs URLs using Azure DevOps Wiki addressing conventions.
        /// </summary>
        DevOps,

        /// <summary>
        /// Constructs an online search query instead of a direct documentation-page URL.
        /// </summary>
        OnlineSearch,
    }
}

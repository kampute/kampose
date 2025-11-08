// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Models
{
    /// <summary>
    /// Specifies the strategy used to reference documentation for types and members in referenced libraries.
    /// </summary>
    public enum DocReferenceStrategy
    {
        /// <summary>
        /// Documentation follows the structure and conventions of the .NET API Browser.
        /// </summary>
        DotNet,

        /// <summary>
        /// Documentation follows the structure and conventions of the DocFX document generator.
        /// </summary>
        DocFx,

        /// <summary>
        /// Documentation is hosted on an Azure DevOps Wiki.
        /// </summary>
        DevOps,

        /// <summary>
        /// Documentation is not directly available but may be found through an Internet search.
        /// </summary>
        OnlineSearch,
    }
}

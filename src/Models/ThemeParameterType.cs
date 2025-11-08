// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Models
{
    /// <summary>
    /// Defines the data type and handling requirements for a theme parameter.
    /// </summary>
    public enum ThemeParameterType
    {
        /// <summary>
        /// A string value.
        /// </summary>
        String,

        /// <summary>
        /// A numeric value.
        /// </summary>
        Number,

        /// <summary>
        /// A boolean value.
        /// </summary>
        Boolean,

        /// <summary>
        /// A Markdown formatted text, which may contain template expressions.
        /// </summary>
        Markdown,

        /// <summary>
        /// An URI or resource path.
        /// </summary>
        Uri,

        /// <summary>
        /// An array of values.
        /// </summary>
        Array,

        /// <summary>
        /// A structured object with its own properties.
        /// </summary>
        Object,
    }
}

// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Models
{
    /// <summary>
    /// Specifies the expected representation and normalization of a theme parameter value.
    /// </summary>
    public enum ThemeParameterType
    {
        /// <summary>
        /// A JSON string preserved as text.
        /// </summary>
        String,

        /// <summary>
        /// A JSON number.
        /// </summary>
        Number,

        /// <summary>
        /// A JSON Boolean value.
        /// </summary>
        Boolean,

        /// <summary>
        /// Markdown text that may contain template expressions.
        /// </summary>
        /// <remarks>
        /// Accepts a string or a sequence of strings. Sequence items are joined using the platform newline.
        /// </remarks>
        Markdown,

        /// <summary>
        /// An absolute or relative URI reference, including a resource path.
        /// </summary>
        Uri,

        /// <summary>
        /// A JSON array.
        /// </summary>
        Array,

        /// <summary>
        /// A JSON object.
        /// </summary>
        Object,
    }
}

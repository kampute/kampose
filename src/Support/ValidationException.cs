// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Support
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents an exception that occurs during validation.
    /// </summary>
    public sealed class ValidationException : Exception
    {
        /// <summary>
        /// Creates a new instance of the <see cref="ValidationException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="errors">The validation errors.</param>
        public ValidationException(string? message, IEnumerable<string>? errors = null)
            : base(message)
        {
            Errors = errors is not null ? [.. errors] : [];
        }

        /// <summary>
        /// Gets the validation errors.
        /// </summary>
        /// <value>
        /// The validation errors.
        /// </value>
        public IReadOnlyCollection<string> Errors { get; }
    }
}

// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Templates.Formatters
{
    using HandlebarsDotNet;
    using HandlebarsDotNet.IO;
    using Kampute.DocToolkit.Models;
    using System;

    /// <summary>
    /// Provides functionality for formatting a <see cref="NamespaceModel"/> as a link.
    /// </summary>
    public sealed class NamespaceModelFormatter : IFormatter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NamespaceModelFormatter"/> class.
        /// </summary>
        public NamespaceModelFormatter()
        {
        }

        /// <summary>
        /// Formats the specified <see cref="NamespaceModel"/>.
        /// </summary>
        /// <typeparam name="T">The type of the value to format.</typeparam>
        /// <param name="value">The value to format, expected to be a <see cref="NamespaceModel"/>.</param>
        /// <param name="writer">The <see cref="EncodedTextWriter"/> to write the formatted output to.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is not a <see cref="NamespaceModel"/>.</exception>
        public void Format<T>(T value, in EncodedTextWriter writer)
        {
            if (value is not NamespaceModel ns)
                throw new ArgumentException("Invalid value type.", nameof(value));

            using var docWriter = writer.CreateMarkupWrapper(ns.Context);
            docWriter.WriteDocLink(ns);
        }
    }
}
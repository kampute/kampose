// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Templates.Formatters
{
    using HandlebarsDotNet;
    using HandlebarsDotNet.IO;
    using Kampute.DocToolkit;
    using Kampute.DocToolkit.Languages;
    using Kampute.DocToolkit.Metadata;
    using System;

    /// <summary>
    /// Provides functionality for formatting a custom attribute as links to documentation of its component parts.
    /// </summary>
    /// <remarks>
    /// The <see cref="AttributeFormatter"/> supports formatting any object that implements the <see cref="ICustomAttribute"/> interface.
    /// It generates links to the attribute's documentation, using the declaring type as the name qualifier. Any constructor arguments
    /// or property assignments within the attribute are also formatted as links to their respective documentation.
    /// </remarks>
    public sealed class AttributeFormatter : IFormatter
    {
        private readonly IDocumentationContext context;

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeFormatter"/> class.
        /// </summary>
        /// <param name="context">The documentation context used for resolving URLs and encoding.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is <see langword="null"/>.</exception>
        public AttributeFormatter(IDocumentationContext context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Formats the specified value as a documentation link.
        /// </summary>
        /// <typeparam name="T">The type of the value to format.</typeparam>
        /// <param name="value">The value to format, expected to be a <see cref="ICustomAttribute"/>.</param>
        /// <param name="writer">The <see cref="EncodedTextWriter"/> to write the formatted output to.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is not an <see cref="ICustomAttribute"/>.</exception>
        public void Format<T>(T value, in EncodedTextWriter writer)
        {
            if (value is not ICustomAttribute attribute)
                throw new ArgumentException("Invalid value type.", nameof(value));

            using var docWriter = writer.CreateMarkupWrapper(context);
            docWriter.WriteDocLink(attribute, context, NameQualifier.DeclaringType);
        }
    }
}
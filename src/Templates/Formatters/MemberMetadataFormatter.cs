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
    /// Provides functionality for formatting a member as a link to its documentation.
    /// </summary>
    /// <remarks>
    /// The <see cref="MemberMetadataFormatter"/> supports formatting any object that implements the <see cref="IMember"/> interface.
    /// It generates a link to the member's documentation, using the declaring type as the name qualifier.
    /// </remarks>
    public sealed class MemberMetadataFormatter : IFormatter
    {
        private readonly IDocumentationContext context;

        /// Initializes a new instance of the <see cref="MemberMetadataFormatter"/> class.
        /// <param name="context">The documentation context used for resolving URLs and encoding.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is <see langword="null"/>.</exception>
        public MemberMetadataFormatter(IDocumentationContext context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Formats the specified value as a documentation link to a code element.
        /// </summary>
        /// <typeparam name="T">The type of the value to format.</typeparam>
        /// <param name="value">The value to format, expected to be an <see cref="IMember"/> instance.</param>
        /// <param name="writer">The <see cref="EncodedTextWriter"/> to write the formatted output to.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is not an <see cref="IMember"/>.</exception>
        public void Format<T>(T value, in EncodedTextWriter writer)
        {
            if (value is not IMember member)
                throw new ArgumentException("Invalid value type.", nameof(value));

            using var docWriter = writer.CreateMarkupWrapper(context);
            docWriter.WriteDocLink(member, context, NameQualifier.DeclaringType);
        }
    }
}
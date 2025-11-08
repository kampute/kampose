// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Templates.Formatters
{
    using HandlebarsDotNet;
    using HandlebarsDotNet.IO;
    using Kampute.DocToolkit;
    using Kampute.DocToolkit.XmlDoc.Comments;
    using System;
    using System.Xml.Linq;

    /// <summary>
    /// Provides functionality for formatting an XML comment in the output format of the documentation context.
    /// </summary>
    /// <remarks>
    /// The <see cref="CommentFormatter"/> supports formatting either a <see cref="Comment"/> object or an <see cref="XElement"/>
    /// representing the XML content of a documentation comment. It transforms the XML into the appropriate output format.
    /// </remarks>
    public sealed class CommentFormatter : IFormatter
    {
        private readonly IDocumentationContext context;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommentFormatter"/> class.
        /// </summary>
        /// <param name="context">The documentation context used for resolving URLs and encoding.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is <see langword="null"/>.</exception>
        public CommentFormatter(IDocumentationContext context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Formats the specified value as a pre-formatted text.
        /// </summary>
        /// <typeparam name="T">The type of the value to format.</typeparam>
        /// <param name="value">The value to format, expected to be either a <see cref="Comment"/> or an <see cref="XElement"/>.</param>
        /// <param name="writer">The <see cref="EncodedTextWriter"/> to write the formatted output to.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is neither a <see cref="Comment"/> nor an <see cref="XElement"/>.</exception>
        /// <remarks>
        /// The <see cref="CommentFormatter"/> formats XML comments into the output format defined by the documentation context.
        /// </remarks>
        public void Format<T>(T value, in EncodedTextWriter writer)
        {
            if (value is not XElement element)
            {
                if (value is not Comment comment)
                    throw new ArgumentException("Invalid value type.", nameof(value));

                element = comment.Content;
            }

            using var textWriter = writer.CreateSuppressedWrapper();
            context.ContentFormatter.Transform(textWriter, element);
        }
    }
}
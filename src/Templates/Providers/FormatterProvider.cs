// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Templates.Providers
{
    using HandlebarsDotNet.IO;
    using Kampose.Templates.Formatters;
    using Kampute.DocToolkit;
    using Kampute.DocToolkit.Metadata;
    using Kampute.DocToolkit.Models;
    using Kampute.DocToolkit.XmlDoc.Comments;
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Xml.Linq;

    /// <summary>
    /// Provides functionality to create formatters for a template variable type.
    /// </summary>
    public sealed class FormatterProvider : IFormatterProvider
    {
        private readonly TopicModelFormatter topicModelFormatter;
        private readonly NamespaceModelFormatter namespaceModelFormatter;
        private readonly MemberModelFormatter memberModelFormatter;
        private readonly CommentFormatter commentFormatter;
        private readonly MemberMetadataFormatter memberMetadataFormatter;
        private readonly AttributeFormatter attributeFormatter;

        /// <summary>
        /// Initializes a new instance of the <see cref="FormatterProvider"/> class.
        /// </summary>
        /// <param name="context">The documentation context used for resolving URLs and encoding.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is <see langword="null"/>.</exception>
        public FormatterProvider(IDocumentationContext context)
        {
            topicModelFormatter = new();
            namespaceModelFormatter = new();
            memberModelFormatter = new();
            commentFormatter = new(context);
            memberMetadataFormatter = new(context);
            attributeFormatter = new(context);
        }

        /// <summary>
        /// Attempts to create a formatter for the specified type.
        /// </summary>
        /// <param name="type">The type for which to create a formatter.</param>
        /// <param name="formatter">When this method returns, contains the formatter for the specified type if successful; otherwise, <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if a formatter for <paramref name="type"/> was successfully created; otherwise, <see langword="false"/>.</returns>
        public bool TryCreateFormatter(Type type, [NotNullWhen(true)] out IFormatter? formatter)
        {
            if (typeof(XElement).IsAssignableFrom(type) || typeof(Comment).IsAssignableFrom(type))
            {
                formatter = commentFormatter;
                return true;
            }

            if (typeof(MemberModel).IsAssignableFrom(type))
            {
                formatter = memberModelFormatter;
                return true;
            }

            if (typeof(NamespaceModel).IsAssignableFrom(type))
            {
                formatter = namespaceModelFormatter;
                return true;
            }

            if (typeof(TopicModel).IsAssignableFrom(type))
            {
                formatter = topicModelFormatter;
                return true;
            }

            if (typeof(IMember).IsAssignableFrom(type))
            {
                formatter = memberMetadataFormatter;
                return true;
            }

            if (typeof(ICustomAttribute).IsAssignableFrom(type))
            {
                formatter = attributeFormatter;
                return true;
            }

            formatter = null;
            return false;
        }
    }
}

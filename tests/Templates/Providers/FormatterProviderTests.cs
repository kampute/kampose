// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Test.Templates.Providers
{
    using Kampose.Templates.Formatters;
    using Kampose.Templates.Providers;
    using Kampute.DocToolkit;
    using Kampute.DocToolkit.Metadata;
    using Kampute.DocToolkit.Models;
    using Kampute.DocToolkit.XmlDoc.Comments;
    using Moq;
    using NUnit.Framework;
    using System;
    using System.Xml.Linq;

    [TestFixture]
    public class FormatterProviderTests
    {
        [TestCase(typeof(IMember), ExpectedResult = typeof(MemberMetadataFormatter))]
        [TestCase(typeof(IType), ExpectedResult = typeof(MemberMetadataFormatter))]
        [TestCase(typeof(XElement), ExpectedResult = typeof(CommentFormatter))]
        [TestCase(typeof(Comment), ExpectedResult = typeof(CommentFormatter))]
        [TestCase(typeof(MemberModel), ExpectedResult = typeof(MemberModelFormatter))]
        [TestCase(typeof(TypeModel), ExpectedResult = typeof(MemberModelFormatter))]
        [TestCase(typeof(NamespaceModel), ExpectedResult = typeof(NamespaceModelFormatter))]
        [TestCase(typeof(TopicModel), ExpectedResult = typeof(TopicModelFormatter))]
        [TestCase(typeof(IAssembly), ExpectedResult = null)]
        public Type? TryCreateFormatter_CreatesFormatterForType(Type type)
        {
            var docContextMock = new Mock<IDocumentationContext>();
            var formatterProvider = new FormatterProvider(docContextMock.Object);

            return formatterProvider.TryCreateFormatter(type, out var formatter) ? formatter.GetType() : null;
        }
    }
}

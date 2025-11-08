// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Test
{
    using Kampute.DocToolkit;
    using Kampute.DocToolkit.Formatters;
    using Kampute.DocToolkit.Languages;
    using Kampute.DocToolkit.Metadata;
    using Kampute.DocToolkit.Routing;
    using Kampute.DocToolkit.Support;
    using Kampute.DocToolkit.Topics;
    using Kampute.DocToolkit.XmlDoc;
    using Moq;
    using System;
    using System.Xml.Linq;

    /// <summary>
    /// Provides helper methods for creating mock objects.
    /// </summary>
    internal static class MockHelper
    {
        /// <summary>
        /// Creates a documentation context without any assemblies or topics.
        /// </summary>
        /// <typeparam name="TFormat">The type of the document formatter.</typeparam>
        /// <returns>A mocked documentation context.</returns>
        public static IDocumentationContext CreateDocumentationContext<TFormat>()
            where TFormat : IDocumentFormatter, new()
        {
            return new DocumentationContext(new CSharp(), CreateAddressProvider(), CreateXmlDocProvider(), new TFormat(), [], []);
        }

        /// <summary>
        /// Creates a mocked address provider.
        /// </summary>
        /// <returns>A mocked address provider.</returns>
        public static IDocumentAddressProvider CreateAddressProvider()
        {
            var urlContext = new ContextAwareUrlNormalizer();
            var addressProviderMock = new Mock<IDocumentAddressProvider>();
            addressProviderMock.SetupGet(x => x.Granularity).Returns(PageGranularity.NamespaceTypeMember);
            addressProviderMock.SetupGet(x => x.ActiveScope).Returns(() => urlContext.ActiveScope);
            addressProviderMock.Setup(x => x.BeginScope(It.IsAny<string>(), It.IsAny<IDocumentModel?>()))
                .Returns((string directory, IDocumentModel? model) => urlContext.BeginScope(directory, model));
            addressProviderMock.Setup(x => x.TryGetNamespaceUrl(It.IsAny<string>(), out It.Ref<Uri?>.IsAny))
                .Returns((string ns, out Uri? url) =>
                {
                    url = new RawUri($"https://example.com/{ns.ToLowerInvariant()}", UriKind.Absolute);
                    return true;
                });
            addressProviderMock.Setup(x => x.TryGetMemberUrl(It.IsAny<IMember>(), out It.Ref<Uri?>.IsAny))
                .Returns((IMember member, out Uri? url) =>
                {
                    if (member.IsDirectDeclaration)
                    {
                        var resourceName = member.CodeReference[2..].ReplaceMany(['`', '#'], '-').ToLowerInvariant();
                        url = new RawUri($"https://example.com/{resourceName}", UriKind.Absolute);
                        return true;
                    }

                    url = null;
                    return false;
                });
            addressProviderMock.Setup(x => x.TryGetTopicUrl(It.IsAny<ITopic>(), out It.Ref<Uri?>.IsAny))
                .Returns((ITopic topic, out Uri? url) =>
                {
                    url = new RawUri($"~/{topic.Id.ToLowerInvariant()}", UriKind.Relative);
                    return true;
                });
            addressProviderMock.Setup(x => x.TryGetNamespaceFile(It.IsAny<string>(), out It.Ref<string?>.IsAny))
                .Returns((string ns, out string? path) =>
                {
                    path = ns.ToLowerInvariant();
                    return true;
                });
            addressProviderMock.Setup(x => x.TryGetMemberFile(It.IsAny<IMember>(), out It.Ref<string?>.IsAny))
                .Returns((IMember member, out string? path) =>
                {
                    if (member.IsDirectDeclaration)
                    {
                        path = member.CodeReference[2..].ReplaceMany(['`', '#'], '-').ToLowerInvariant();
                        return true;
                    }

                    path = null;
                    return false;
                });
            addressProviderMock.Setup(x => x.TryGetTopicFile(It.IsAny<ITopic>(), out It.Ref<string?>.IsAny))
                .Returns((ITopic topic, out string? path) =>
                {
                    path = topic.Id.ToLowerInvariant();
                    return false;
                });
            return addressProviderMock.Object;
        }

        /// <summary>
        /// Creates a mocked XML documentation provider.
        /// </summary>
        /// <returns>A mocked XML documentation provider.</returns>
        public static IXmlDocProvider CreateXmlDocProvider()
        {
            var contentProviderMock = new Mock<IXmlDocProvider>();

            contentProviderMock.SetupGet(x => x.HasDocumentation).Returns(true);
            contentProviderMock.Setup(x => x.TryGetDoc(It.IsAny<string>(), out It.Ref<XmlDocEntry?>.IsAny))
                .Returns((string cref, out XmlDocEntry? doc) =>
                {
                    if (CodeReference.IsValid(cref))
                    {
                        var xmlComment = XElement.Parse($"<member name=\"{cref}\"><summary>Description of <c>{cref[2..]}</c>.</summary></member>");
                        doc = new XmlDocEntry(xmlComment);
                        return true;
                    }

                    doc = null;
                    return false;
                });

            return contentProviderMock.Object;
        }
    }
}

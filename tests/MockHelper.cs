// Copyright (C) Kampute
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
            var addressProvider = CreateAddressProvider();
            var xmlDocProvider = new XmlDocProvider(CreateXmlDocResolver());
            return new DocumentationContext(new CSharp(), addressProvider, xmlDocProvider, new TFormat(), [], []);
        }

        /// <summary>
        /// Creates a mocked address provider.
        /// </summary>
        /// <returns>A mocked address provider.</returns>
        public static IDocumentAddressProvider CreateAddressProvider()
        {
            var urlContext = new ContextAwareUrlNormalizer();
            var addressProviderMock = new Mock<IDocumentAddressProvider>();
            var addressProvider = addressProviderMock.Object;

            addressProviderMock.SetupGet(x => x.Granularity).Returns(PageGranularity.NamespaceTypeMember);
            addressProviderMock.SetupGet(x => x.ActiveScope).Returns(() => urlContext.ActiveScope);

            addressProviderMock.Setup(x => x.BeginScope(It.IsAny<string>(), It.IsAny<IDocumentModel?>()))
                .Returns((string directory, IDocumentModel? model) => urlContext.BeginScope(directory, model));

            addressProviderMock.Setup(x => x.TryGetNamespaceFile(It.IsAny<string>(), out It.Ref<string?>.IsAny))
                .Returns((string ns, out string? path) =>
                {
                    path = ns.ToLowerInvariant();
                    if (!addressProvider.ActiveScope.IsRoot)
                        path = $"{addressProvider.ActiveScope.Directory}/{path}";

                    return true;
                });

            addressProviderMock.Setup(x => x.TryGetMemberFile(It.IsAny<IMember>(), out It.Ref<string?>.IsAny))
                .Returns((IMember member, out string? path) =>
                {
                    if (member.IsDirectDeclaration)
                    {
                        path = member.CodeReference[2..].ReplaceChars(['`', '#'], '-').ToLowerInvariant();
                        if (!addressProvider.ActiveScope.IsRoot)
                            path = $"{addressProvider.ActiveScope.Directory}/{path}";

                        return true;
                    }

                    path = null;
                    return false;
                });

            addressProviderMock.Setup(x => x.TryGetTopicFile(It.IsAny<ITopic>(), out It.Ref<string?>.IsAny))
                .Returns((ITopic topic, out string? path) =>
                {
                    var segments = new System.Collections.Generic.List<string>();

                    for (var current = topic; current is not null; current = current.ParentTopic)
                        segments.Add(current.Id);

                    if (!addressProvider.ActiveScope.IsRoot)
                        segments.Add(addressProvider.ActiveScope.Directory);

                    segments.Reverse();
                    path = string.Join('/', segments).ToLowerInvariant();
                    return true;
                });

            addressProviderMock.Setup(x => x.TryGetNamespaceUrl(It.IsAny<string>(), out It.Ref<Uri?>.IsAny))
                .Returns((string ns, out Uri? url) =>
                {
                    if (addressProvider.TryGetNamespaceFile(ns, out var path))
                    {
                        url = new RawUri($"https://example.com/{path}", UriKind.Absolute);
                        return true;
                    }

                    url = null;
                    return false;
                });

            addressProviderMock.Setup(x => x.TryGetMemberUrl(It.IsAny<IMember>(), out It.Ref<Uri?>.IsAny))
                .Returns((IMember member, out Uri? url) =>
                {
                    if (addressProvider.TryGetMemberFile(member, out var path))
                    {
                        url = new RawUri($"https://example.com/{path}", UriKind.Absolute);
                        return true;
                    }

                    url = null;
                    return false;
                });

            addressProviderMock.Setup(x => x.TryGetTopicUrl(It.IsAny<ITopic>(), out It.Ref<Uri?>.IsAny))
                .Returns((ITopic topic, out Uri? url) =>
                {
                    if (addressProvider.TryGetTopicFile(topic, out var path))
                    {
                        url = new RawUri($"https://example.com/{path}", UriKind.Absolute);
                        return true;
                    }

                    url = null;
                    return false;
                });

            return addressProvider;
        }

        /// <summary>
        /// Creates a mocked XML documentation resolver.
        /// </summary>
        /// <returns>A mocked XML documentation resolver.</returns>
        public static IXmlDocResolver CreateXmlDocResolver()
        {
            var xmlDocResolverMock = new Mock<IXmlDocResolver>();

            xmlDocResolverMock.SetupGet(x => x.HasDocumentation).Returns(true);
            xmlDocResolverMock.Setup(x => x.TryGetXmlDoc(It.IsAny<string>(), out It.Ref<XElement?>.IsAny))
                .Returns((string cref, out XElement? xmlDoc) =>
                {
                    if (CodeReference.IsValid(cref))
                    {
                        xmlDoc = XElement.Parse($"<member name=\"{cref}\"><summary>Description of <c>{cref[2..]}</c>.</summary></member>");
                        return true;
                    }

                    xmlDoc = null;
                    return false;
                });

            return xmlDocResolverMock.Object;
        }
    }
}

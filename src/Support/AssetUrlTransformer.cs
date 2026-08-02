// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Support
{
    using Kampose.Models;
    using Kampute.DocToolkit.Models;
    using Kampute.DocToolkit.Routing;
    using Kampute.DocToolkit.Support;
    using Kampute.DocToolkit.Topics;
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Net;

    /// <summary>
    /// Resolves source-relative references to registered assets before applying ordinary documentation URL rules.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="AssetUrlTransformer"/> class.
    /// </remarks>
    /// <param name="context">The documentation context containing the registered assets.</param>
    /// <param name="innerTransformer">The URL transformer used when a URL does not identify a registered asset.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> or <paramref name="innerTransformer"/> is <see langword="null"/>.</exception>
    public sealed class AssetUrlTransformer(DocContext context, IUrlTransformer innerTransformer) : IUrlTransformer
    {
        private readonly DocContext context = context ?? throw new ArgumentNullException(nameof(context));
        private readonly IUrlTransformer innerTransformer = innerTransformer ?? throw new ArgumentNullException(nameof(innerTransformer));

        /// <inheritdoc/>
        public bool MayTransformUrls => true;

        /// <inheritdoc/>
        public bool TryTransformUrl(string urlString, [NotNullWhen(true)] out Uri? transformedUrl)
        {
            var scope = this.context.AddressProvider.ActiveScope;
            if (!string.IsNullOrWhiteSpace(urlString)
                && !UriHelper.IsQueryOrFragmentOnly(urlString)
                && !UriHelper.IsAbsoluteOrRooted(urlString)
                && scope.Model is TopicModel currentTopic
                && currentTopic.Source is IFileBasedTopic sourceTopic)
            {
                var (urlPath, urlSuffix) = UriHelper.SplitPathAndSuffix(urlString);
                var topicDirectory = Path.GetDirectoryName(sourceTopic.FilePath) ?? string.Empty;
                string referencedPath;
                try
                {
                    referencedPath = Path.GetFullPath(Path.Combine(topicDirectory, WebUtility.UrlDecode(urlPath)));
                }
                catch (Exception error) when (error is ArgumentException or NotSupportedException or PathTooLongException)
                {
                    return this.innerTransformer.TryTransformUrl(urlString, out transformedUrl);
                }

                foreach (var asset in this.context.Assets)
                {
                    if (asset.GeneratedPath is not null
                        && string.Equals(Path.GetFullPath(asset.SourcePath), referencedPath, StringComparison.OrdinalIgnoreCase))
                    {
                        var generatedUrl = scope.ResolveFromDocumentationRoot(asset.GeneratedPath.Replace('\\', '/'));
                        transformedUrl = new RawUri(generatedUrl, UriKind.RelativeOrAbsolute).Combine(urlSuffix);
                        return true;
                    }
                }
            }

            return this.innerTransformer.TryTransformUrl(urlString, out transformedUrl);
        }
    }
}

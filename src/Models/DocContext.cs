// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Models
{
    using Kampose.Support;
    using Kampute.DocToolkit;
    using Kampute.DocToolkit.Formatters;
    using Kampute.DocToolkit.Languages;
    using Kampute.DocToolkit.Metadata;
    using Kampute.DocToolkit.Routing;
    using Kampute.DocToolkit.Support;
    using Kampute.DocToolkit.Topics;
    using Kampute.DocToolkit.XmlDoc;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Provides the context for generating documentation.
    /// </summary>
    public sealed class DocContext : DocumentationContext
    {
        private readonly Lazy<Sitemap> sitemap;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocContext"/> class.
        /// </summary>
        /// <param name="language">The programming language of the codebase being documented.</param>
        /// <param name="addressProvider">The provider responsible for resolving documentation URLs and file paths of elements.</param>
        /// <param name="contentProvider">The provider responsible for finding documentation content of elements.</param>
        /// <param name="contentFormatter">The object responsible for formatting the documentation content.</param>
        /// <param name="assemblies">The assemblies to document.</param>
        /// <param name="topics">The topics to include.</param>
        /// <param name="assets">The supplementary files to include in the documentation.</param>
        /// <exception cref="ArgumentNullException">Thrown when any of the parameters are <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="topics"/> contains duplicate topic names.</exception>
        public DocContext
        (
            IProgrammingLanguage language,
            IDocumentAddressProvider addressProvider,
            IXmlDocProvider contentProvider,
            IDocumentFormatter contentFormatter,
            IEnumerable<IAssembly> assemblies,
            IEnumerable<ITopic> topics,
            IEnumerable<AssetReference> assets,
            object? metadata = null
        ) : base(language, addressProvider, contentProvider, contentFormatter, assemblies, topics, metadata)
        {
            ArgumentNullException.ThrowIfNull(assets);

            Assets = [.. assets];
            sitemap = new(() => new Sitemap(this));

            if (ContentFormatter is HtmlFormat)
                ContentFormatter.TextTransformers.Register<MarkdownToHtmlTransformer>(FileExtensions.MarkdownExtensions);
        }

        /// <summary>
        /// Gets the sitemap for the documentation.
        /// </summary>
        /// <value>
        /// The <see cref="Sitemap"/> instance representing the sitemap for the documentation.
        /// </value>
        public Sitemap Sitemap => sitemap.Value;

        /// <summary>
        /// Gets the supplementary files to include in the documentation.
        /// </summary>
        /// <value>
        /// The enumerable collection of tuples representing the source and target paths of the supplementary files.
        /// </value>
        public IReadOnlyCollection<AssetReference> Assets { get; }
    }
}

// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Builders
{
    using Kampose.Models;
    using Kampose.Reporters;
    using Kampose.Support;
    using Kampose.Templates;
    using Kampute.DocToolkit;
    using Kampute.DocToolkit.Routing;
    using Kampute.DocToolkit.Support;
    using Kampute.DocToolkit.Topics;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Builder responsible for creating and configuring documentation contexts from configuration and theme data.
    /// </summary>
    public sealed class DocContextBuilder : DocumentationContextBuilder<DocContext>
    {
        private readonly IActivityReporter reporter;
        private readonly Dictionary<string, string> assets = [];
        private IDocumentAddressingStrategy? strategy;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocContextBuilder"/> class.
        /// </summary>
        /// <param name="reporter">The activity reporter to use for tracking progress.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="reporter"/> is <see langword="null"/>.</exception>
        public DocContextBuilder(IActivityReporter reporter)
        {
            this.reporter = reporter ?? throw new ArgumentNullException(nameof(reporter));
        }

        /// <summary>
        /// Gets the document addressing strategy.
        /// </summary>
        /// <value>
        /// The addressing strategy implementation.
        /// </value>
        public override IDocumentAddressingStrategy Strategy => strategy ?? throw new InvalidOperationException("The addressing strategy has not been configured.");

        /// <summary>
        /// Adds an asset to the documentation context.
        /// </summary>
        /// <param name="sourcePath">The full path to the source asset file.</param>
        /// <param name="targetPath">The full path to the target asset file.</param>
        /// <returns>The current builder instance for method chaining.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="sourcePath"/> or <paramref name="targetPath"/> is <see langword="null"/> or empty.</exception>
        public DocContextBuilder AddAsset(string sourcePath, string targetPath)
        {
            ArgumentException.ThrowIfNullOrEmpty(sourcePath);
            ArgumentException.ThrowIfNullOrEmpty(targetPath);

            assets[targetPath] = sourcePath;
            return this;
        }

        /// <summary>
        /// Configures the builder with the specified configuration and theme.
        /// </summary>
        /// <param name="config">The configuration to use.</param>
        /// <param name="theme">The theme to use.</param>
        /// <returns>The current builder instance for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is <see langword="null"/>.</exception>
        public DocContextBuilder Configure(Configuration config, Theme theme)
        {
            ArgumentNullException.ThrowIfNull(config);
            ArgumentNullException.ThrowIfNull(theme);

            strategy = CreateAddressingStrategy(config.Convention);

            BaseUrl = config.BaseUrl;
            XmlDocErrorHandler = new XmlDocErrorReporter(reporter);

            foreach (var reference in config.References)
                AddExternalReference(CreateExternalUrlResolver(reference));

            CollectAssemblies(config);
            CollectExtraXmlDocs(config);
            CollectTopics(config, AssemblyPaths.Count > 0 && strategy is HtmlAddressingStrategy);
            CollectAssets(config, theme);
            return this;
        }

        /// <summary>
        /// Creates a new instance of <see cref="DocContext"/> with the configured settings.
        /// </summary>
        /// <returns>A new instance of <see cref="DocContext"/> representing the documentation context.</returns>
        protected override DocContext CreateContext()
        {
            var universe = CreateMetadataUniverse();
            var assemblies = LoadAssembliesFromPaths(universe);
            var addressProvider = CreateAddressProvider(assemblies);
            var xmlProvider = CreateXmlDocProvider();
            var formatter = CreateFormatter();

            return new DocContext
            (
                Language ?? Kampute.DocToolkit.Languages.Language.Default,
                addressProvider,
                xmlProvider,
                formatter,
                assemblies,
                Topics,
                assets.Select(kvp => new AssetReference(kvp.Value, kvp.Key)),
                universe
            );
        }

        /// <summary>
        /// Creates a documentation addressing strategy based on the specified convention.
        /// </summary>
        /// <param name="convention">The documentation convention to create a strategy for.</param>
        /// <returns>An instance of <see cref="IDocumentAddressingStrategy"/> for the specified convention.</returns>
        /// <exception cref="NotSupportedException">Thrown when the specified convention is not supported.</exception>
        private static IDocumentAddressingStrategy CreateAddressingStrategy(DocConvention convention)
        {
            return convention switch
            {
                DocConvention.DevOps => new DevOpsWikiStrategy(CreateAzureWikiOptions()),
                DocConvention.DotNet => new DotNetApiStrategy(CreateHtmlAddressingOptions<DotNetApiOptions>()),
                DocConvention.DocFx => new DocFxStrategy(CreateHtmlAddressingOptions<DocFxOptions>()),
                _ => throw new NotSupportedException($"The specified convention '{convention}' is not supported.")
            };

            static DevOpsWikiOptions CreateAzureWikiOptions()
            {
                return new DevOpsWikiOptions
                {
                    MainTopicId = SpecialTopicIdentifiers.Home
                };
            }

            static TOptions CreateHtmlAddressingOptions<TOptions>()
                where TOptions : HtmlAddressingOptions, new()
            {
                var options = new TOptions();

                options.AddPinnedTopic(SpecialTopicIdentifiers.Home, string.Empty);
                if (!string.IsNullOrEmpty(options.ApiPath))
                    options.AddPinnedTopic(SpecialTopicIdentifiers.Api, options.ApiPath);

                return options;
            }
        }

        /// <summary>
        /// Create a documentation URL resolver based on the provided documentation reference settings.
        /// </summary>
        /// <param name="reference">The documentation reference settings.</param>
        /// <returns>An instance of <see cref="RemoteApiDocUrlResolver"/> for the specified reference.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="reference"/> is <see langword="null"/>.</exception>
        private RemoteApiDocUrlResolver CreateExternalUrlResolver(DocReference reference)
        {
            ArgumentNullException.ThrowIfNull(reference);

            var externalReference = reference.Strategy switch
            {
                DocReferenceStrategy.DotNet => CreateDotNetReference(),
                DocReferenceStrategy.DocFx => CreateDocFxReference(),
                DocReferenceStrategy.DevOps => CreateAzureWikiReference(),
                DocReferenceStrategy.OnlineSearch => CreateOnlineSearchReference(),
                _ => throw new NotSupportedException($"The documentation reference strategy '{reference.Strategy}' is not supported.")
            };

            foreach (var ns in reference.Namespaces)
                externalReference.NamespacePatterns.Add(ns);

            return externalReference;

            RemoteApiDocUrlResolver CreateOnlineSearchReference()
            {
                return new SearchBasedApiDocUrlResolver(reference.Url)
                {
                    Language = Language ?? Kampute.DocToolkit.Languages.Language.Default
                };
            }

            RemoteApiDocUrlResolver CreateDotNetReference()
            {
                var options = CreateOptions<DotNetApiOptions>(reference.Extension);
                var strategy = new DotNetApiStrategy(options);
                return new StrategyBasedApiDocUrlResolver(reference.Url, strategy);
            }

            RemoteApiDocUrlResolver CreateDocFxReference()
            {
                var options = CreateOptions<DocFxOptions>(reference.Extension);
                var strategy = new DocFxStrategy(options);
                return new StrategyBasedApiDocUrlResolver(reference.Url, strategy);
            }

            RemoteApiDocUrlResolver CreateAzureWikiReference()
            {
                var options = CreateOptions<DevOpsWikiOptions>(reference.Extension);
                options.Language = Language ?? Kampute.DocToolkit.Languages.Language.Default;
                var strategy = new DevOpsWikiStrategy(options);
                return new StrategyBasedApiDocUrlResolver(reference.Url, strategy);
            }

            static T CreateOptions<T>(string? fileExtension)
                where T : AddressingOptions, new()
            {
                var options = new T();
                if (fileExtension is not null)
                {
                    if (string.IsNullOrWhiteSpace(fileExtension))
                    {
                        options.OmitExtensionInUrls = true;
                    }
                    else
                    {
                        options.FileExtension = fileExtension;
                        options.OmitExtensionInUrls = false;
                    }
                }
                return options;
            }
        }

        /// <summary>
        /// Collects assemblies from the configuration and loads them into the context builder.
        /// </summary>
        /// <param name="config">The configuration containing assembly information.</param>
        private void CollectAssemblies(Configuration config)
        {
            if (config.Assemblies.Count == 0)
                return;

            reporter.BeginActivity("Collecting assemblies for documentation");

            foreach (var assemblyFilePath in config.Assemblies.FindMatchingFiles(config.BaseDirectory, ".dll"))
            {
                using (reporter.BeginStep(assemblyFilePath))
                {
                    AddAssembly(assemblyFilePath);

                    // Attempt to load the XML documentation file if it exists.
                    var xmlDocFilePath = Path.ChangeExtension(assemblyFilePath, ".xml");
                    if (File.Exists(xmlDocFilePath))
                        AddXmlDoc(xmlDocFilePath);
                }
            }
        }

        /// <summary>
        /// Collects additional XML documentation files specified in the configuration and adds them to the context builder.
        /// </summary>
        /// <param name="config">The configuration containing XML documentation information.</param>
        private void CollectExtraXmlDocs(Configuration config)
        {
            if (config.XmlDocs.Count == 0)
                return;

            reporter.BeginActivity("Collecting additional XML documentation files");

            foreach (var xmlDocFilePath in config.XmlDocs.FindMatchingFiles(config.BaseDirectory, ".xml"))
            {
                using (reporter.BeginStep(xmlDocFilePath))
                    AddXmlDoc(xmlDocFilePath);
            }
        }

        /// <summary>
        /// Collects topics from the configuration and adds them to the documentation context.
        /// </summary>
        /// <param name="config">The configuration containing topic information.</param>
        /// <param name="requiresApiPage">Indicates whether an API page is required.</param>
        private void CollectTopics(Configuration config, bool requiresApiPage)
        {
            if (config.Topics.Count == 0)
                return;

            reporter.BeginActivity("Collecting topic files");

            FileTopic? homeTopic = null;
            FileTopic? apiTopic = null;
            List<FileTopic> topics = [];

            foreach (var topicPath in config.Topics.FindMatchingFiles(config.BaseDirectory, ".md"))
            {
                using var _ = reporter.BeginStep(topicPath);

                var fileExtension = Path.GetExtension(topicPath);
                if (!FileExtensions.MarkdownExtensions.Contains(fileExtension, StringComparer.OrdinalIgnoreCase))
                {
                    reporter.LogWarning($"The format of topic file is not supported. The file '{topicPath}' is ignored.");
                    continue;
                }

                var topic = FileTopicFactory.Create(topicPath);
                if (SpecialTopicIdentifiers.Home.Equals(topic.Id, StringComparison.OrdinalIgnoreCase))
                    homeTopic = topic;
                else if (SpecialTopicIdentifiers.Api.Equals(topic.Id, StringComparison.OrdinalIgnoreCase))
                    apiTopic = topic;
                else if ("README".Equals(topic.Id, StringComparison.OrdinalIgnoreCase))
                    homeTopic ??= FileTopicFactory.Create(SpecialTopicIdentifiers.Home, topicPath);
                else
                    topics.Add(topic);
            }

            var sortedTopics = TopicSorter.SortTopics(topics, config.TopicOrder);
            var topLevelTopics = config.TopicHierarchy switch
            {
                FileTopicHierarchyMethod.None => sortedTopics,
                FileTopicHierarchyMethod.Directory => FileTopicHelper.ConstructHierarchyByDirectory(sortedTopics),
                FileTopicHierarchyMethod.Index => FileTopicHelper.ConstructHierarchyByIndexFile(sortedTopics, "overview"),
                FileTopicHierarchyMethod.Prefix => FileTopicHelper.ConstructHierarchyByFilenamePrefix(sortedTopics, '.'),
                _ => throw new NotSupportedException($"The topic hierarchy method '{config.TopicHierarchy}' is not supported.")
            };

            AddSpecialTopics(homeTopic, apiTopic, requiresApiPage);
            foreach (var topic in topLevelTopics)
                AddTopic(topic);
        }

        /// <summary>
        /// Adds special topics to the documentation context, ensuring that home API pages are present.
        /// </summary>
        /// <param name="homeTopic">The home topic, if available; otherwise, a warning is logged.</param>
        /// <param name="apiTopic">The API topic, if available; otherwise, a default API topic will be created.</param>
        /// <param name="requiresApiTopic">Indicates whether an API page is required.</param>
        private void AddSpecialTopics(FileTopic? homeTopic, FileTopic? apiTopic, bool requiresApiPage)
        {
            if (homeTopic is not null)
            {
                AddTopic(homeTopic);
            }
            else if (requiresApiPage)
            {
                reporter.LogWarning($"No home page found. Consider providing a topic file named '{SpecialTopicIdentifiers.Home}.md' or 'README.md' to serve as the home page.");
            }
            else
            {
                reporter.LogVerbose($"Used an auto-generated page as the home page because neither '{SpecialTopicIdentifiers.Home}.md' nor 'README.md' was found.");
                AddTopic(new TemplatedTopic(SpecialTopicIdentifiers.Home, TemplateNames.ApiPageContent)
                {
                    Title = "API"
                });
            }

            if (apiTopic is not null)
            {
                AddTopic(apiTopic);
            }
            else if (requiresApiPage)
            {
                reporter.LogVerbose($"Used an auto-generated page as the API page because '{SpecialTopicIdentifiers.Api}.md' was not found.");
                AddTopic(new TemplatedTopic(SpecialTopicIdentifiers.Api, TemplateNames.ApiPageContent)
                {
                    Title = "API"
                });
            }
        }

        /// <summary>
        /// Collects assets from the theme and configuration, adding them to the context builder.
        /// </summary>
        /// <param name="config">The configuration containing asset information.</param>
        /// <param name="theme">The theme containing asset files.</param>
        private void CollectAssets(Configuration config, Theme theme)
        {
            if (theme.AssetFiles.Count == 0 && config.Assets.Count == 0)
                return;

            reporter.BeginActivity("Collecting asset files");

            // Collect assets of the theme
            foreach (var (targetRelativePath, sourceFullPath) in theme.AssetFiles)
            {
                using var _ = reporter.BeginStep(sourceFullPath);
                var targetFullPath = Path.GetFullPath(Path.Combine(config.OutputDirectory, targetRelativePath));
                AddAsset(sourceFullPath, targetFullPath);
            }

            // Collect assets from the configuration
            foreach (var filter in config.Assets)
            {
                foreach (var sourceFullPath in filter.Source.FindMatchingFiles(config.BaseDirectory))
                {
                    using var _ = reporter.BeginStep(sourceFullPath);
                    var targetFullPath = Path.GetFullPath(Path.Combine(config.OutputDirectory, filter.TargetPath, Path.GetFileName(sourceFullPath)));
                    AddAsset(sourceFullPath, targetFullPath);
                }
            }
        }
    }
}
